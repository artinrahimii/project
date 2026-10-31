using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Nethermind.Libp2p.Core;
using WhiteP.Host.Infrastructure;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;
using WhiteP.Shared.Contracts.Networking;
using WhiteP.Shared.Security;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Owns a single explicitly trusted inbound libp2p peer session and its renewable rendezvous lease.
/// The caller must confirm that the advertised address is reachable before enabling publication.
/// This service never infers public reachability from a successful local bind.
/// </summary>
public sealed class PeerAnnouncementHostLifecycle : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly PeerTrustStore _trustStore;
    private readonly Identity _identity;
    private readonly string _expectedPeerIdentity;
    private readonly IPAddress _bindAddress;
    private readonly IPAddress _advertisedAddress;
    private readonly int _port;
    private readonly Func<byte[], CancellationToken, Task<byte[]>> _requestHandler;
    private readonly HttpClient _httpClient;
    private readonly PeerAnnouncementClient _announcementClient;
    private readonly TimeSpan _timeToLive;
    private readonly TimeSpan _renewalInterval;
    private readonly AuthenticatedRequestReplayGuard _replayGuard;

    private Libp2pPeerTransport? _transport;
    private PeerAnnouncementLeasePublisher? _publisher;
    private CancellationTokenSource? _lifetime;
    private Task? _receiveLoop;
    private bool _started;
    private bool _disposed;
    private int _active;
    private string? _lastFailureType;

    public PeerAnnouncementHostLifecycle(
        PeerTrustStore trustStore,
        Identity localIdentity,
        string expectedPeerIdentity,
        Uri rendezvousUri,
        IPAddress bindAddress,
        IPAddress advertisedAddress,
        int port,
        bool advertisedAddressVerified,
        Func<byte[], CancellationToken, Task<byte[]>> requestHandler,
        TimeSpan? timeToLive = null,
        TimeSpan? renewalInterval = null,
        AuthenticatedRequestReplayGuard? replayGuard = null)
    {
        ArgumentNullException.ThrowIfNull(trustStore);
        ArgumentNullException.ThrowIfNull(localIdentity);
        ArgumentNullException.ThrowIfNull(rendezvousUri);
        ArgumentNullException.ThrowIfNull(bindAddress);
        ArgumentNullException.ThrowIfNull(advertisedAddress);
        ArgumentNullException.ThrowIfNull(requestHandler);

        if (!advertisedAddressVerified)
            throw new ArgumentException("The advertised address must be verified by an independent peer before it can be announced.", nameof(advertisedAddressVerified));
        if (IPAddress.Any.Equals(bindAddress) || IPAddress.IPv6Any.Equals(bindAddress))
            throw new ArgumentException("A specific interface address is required; wildcard binding is not allowed.", nameof(bindAddress));
        if (IPAddress.Any.Equals(advertisedAddress) || IPAddress.IPv6Any.Equals(advertisedAddress))
            throw new ArgumentException("A specific peer-reachable advertised address is required.", nameof(advertisedAddress));
        if (bindAddress.AddressFamily != advertisedAddress.AddressFamily)
            throw new ArgumentException("Bind and advertised addresses must use the same address family.", nameof(advertisedAddress));
        if (bindAddress.AddressFamily == AddressFamily.InterNetworkV6 &&
            (bindAddress.ScopeId != 0 || advertisedAddress.ScopeId != 0))
            throw new ArgumentException("IPv6 scoped addresses are not supported as portable peer addresses.", nameof(advertisedAddress));
        if (port is < 1 or > IPEndPoint.MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port), "Peer hosting requires a fixed, explicitly configured TCP/UDP port.");
        if (!rendezvousUri.IsAbsoluteUri || !string.IsNullOrEmpty(rendezvousUri.UserInfo) ||
            !string.IsNullOrEmpty(rendezvousUri.Query) || !string.IsNullOrEmpty(rendezvousUri.Fragment))
            throw new ArgumentException("Rendezvous URI must be absolute and must not include credentials, query parameters or a fragment.", nameof(rendezvousUri));
        if (!string.Equals(rendezvousUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            !(string.Equals(rendezvousUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && rendezvousUri.IsLoopback))
            throw new ArgumentException("Rendezvous requires HTTPS except for loopback-only tests.", nameof(rendezvousUri));

        _trustStore = trustStore;
        _identity = localIdentity;
        _expectedPeerIdentity = expectedPeerIdentity;
        _bindAddress = bindAddress;
        _advertisedAddress = advertisedAddress;
        _port = port;
        _requestHandler = requestHandler;
        _timeToLive = timeToLive ?? PeerAnnouncementLeasePublisher.DefaultTimeToLive;
        _renewalInterval = renewalInterval ?? PeerAnnouncementLeasePublisher.DefaultRenewalInterval;
        _replayGuard = replayGuard ?? new AuthenticatedRequestReplayGuard();
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        try
        {
            _announcementClient = new PeerAnnouncementClient(_httpClient, rendezvousUri);
        }
        catch
        {
            _httpClient.Dispose();
            throw;
        }

        if (_timeToLive <= TimeSpan.Zero || _timeToLive > PeerAnnouncement.MaximumLifetime)
        {
            _httpClient.Dispose();
            throw new ArgumentOutOfRangeException(nameof(timeToLive));
        }
        if (_renewalInterval <= TimeSpan.Zero || _renewalInterval >= _timeToLive)
        {
            _httpClient.Dispose();
            throw new ArgumentOutOfRangeException(nameof(renewalInterval));
        }

        LocalPeerIdentity = new PeerId(_identity.PublicKey).ToString();
    }

    public string LocalPeerIdentity { get; }

    public bool IsActive => Volatile.Read(ref _active) == 1;

    /// <summary>Type-only diagnostics; never retains request, response, or exception message content.</summary>
    public string? LastFailureType => Volatile.Read(ref _lastFailureType);

    /// <summary>Starts the authenticated listener before publishing a renewable metadata lease.</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_started)
                throw new InvalidOperationException("This host lifecycle instance can only be started once.");
            _started = true;

            _trustStore.EnsureTrusted(_expectedPeerIdentity);
            var transport = new Libp2pPeerTransport(
                _trustStore,
                expectedRemotePeerIdentity: _expectedPeerIdentity,
                localIdentity: _identity);
            PeerAnnouncementLeasePublisher? publisher = null;
            try
            {
                await transport.StartListeningAsync(_bindAddress, _port, cancellationToken).ConfigureAwait(false);

                string prefix = _advertisedAddress.AddressFamily == AddressFamily.InterNetwork
                    ? $"/ip4/{_advertisedAddress}"
                    : $"/ip6/{_advertisedAddress}";
                string[] addresses =
                [
                    $"{prefix}/tcp/{_port}/p2p/{LocalPeerIdentity}",
                    $"{prefix}/udp/{_port}/quic-v1/p2p/{LocalPeerIdentity}"
                ];
                var template = new PeerAnnouncement(
                    LocalPeerIdentity,
                    addresses,
                    ["inference.v1"],
                    DateTimeOffset.UtcNow.Add(_timeToLive));

                publisher = new PeerAnnouncementLeasePublisher(
                    _announcementClient,
                    template,
                    _timeToLive,
                    _renewalInterval);
                await publisher.StartAsync(cancellationToken).ConfigureAwait(false);

                _transport = transport;
                _publisher = publisher;
                _lifetime = new CancellationTokenSource();
                Volatile.Write(ref _lastFailureType, null);
                Volatile.Write(ref _active, 1);
                _receiveLoop = ReceiveLoopAsync(transport, publisher, _lifetime.Token);
            }
            catch
            {
                if (publisher is not null)
                {
                    try { await publisher.DisposeAsync().ConfigureAwait(false); }
                    catch (Exception) { }
                }
                try { await transport.DisposeAsync().ConfigureAwait(false); }
                catch (Exception) { }
                throw;
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>
    /// Stops renewal before closing the peer transport. The last remote lease is allowed to expire naturally.
    /// </summary>
    public async Task StopAsync()
    {
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            Volatile.Write(ref _active, 0);
            PeerAnnouncementLeasePublisher? publisher = _publisher;
            Libp2pPeerTransport? transport = _transport;
            CancellationTokenSource? lifetime = _lifetime;
            Task? receiveLoop = _receiveLoop;

            _publisher = null;
            _transport = null;
            _lifetime = null;
            _receiveLoop = null;

            if (publisher is not null)
            {
                try { await publisher.StopAsync().ConfigureAwait(false); }
                catch (Exception exception) { Volatile.Write(ref _lastFailureType, exception.GetType().Name); }
            }

            lifetime?.Cancel();
            if (transport is not null)
            {
                try { await transport.DisconnectAsync().ConfigureAwait(false); }
                catch (Exception exception) { Volatile.Write(ref _lastFailureType, exception.GetType().Name); }
            }

            if (receiveLoop is not null)
            {
                try { await receiveLoop.ConfigureAwait(false); }
                catch (Exception exception) { Volatile.Write(ref _lastFailureType, exception.GetType().Name); }
            }

            if (publisher is not null)
            {
                try { await publisher.DisposeAsync().ConfigureAwait(false); }
                catch (Exception exception) { Volatile.Write(ref _lastFailureType, exception.GetType().Name); }
            }

            if (transport is not null)
            {
                try { await transport.DisposeAsync().ConfigureAwait(false); }
                catch (Exception exception) { Volatile.Write(ref _lastFailureType, exception.GetType().Name); }
            }

            lifetime?.Dispose();
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(
        Libp2pPeerTransport transport,
        PeerAnnouncementLeasePublisher publisher,
        CancellationToken cancellationToken)
    {
        try
        {
            // StartListenAsync opens the listener before any remote session exists. Wait for the
            // explicit peer-identity gate and negotiated secure protocol to become connected
            // before calling ReceiveMessageAsync, which intentionally fails closed otherwise.
            while (!cancellationToken.IsCancellationRequested && !transport.IsConnected)
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken).ConfigureAwait(false);

            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] requestWire = await transport.ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);
                var parsedRequest = ProtocolMessageParser.ParseInferenceRequest(requestWire);
                if (!parsedRequest.IsValid || parsedRequest.Message is null)
                    throw new InvalidDataException("The peer sent a malformed or invalid inference request.");

                InferenceRequest request = parsedRequest.Message;
                string authenticatedPeerIdentity = transport.RemotePeerIdentity
                    ?? throw new InvalidDataException("The secure peer session has no authenticated remote identity.");
                if (!StringComparer.Ordinal.Equals(authenticatedPeerIdentity, _expectedPeerIdentity))
                    throw new InvalidDataException("The secure peer session identity changed unexpectedly.");

                DateTimeOffset requestExpiry = DateTimeOffset.FromUnixTimeMilliseconds(checked((long)request.DeadlineUnixMs));
                var replayDecision = _replayGuard.TryRegister(
                    authenticatedPeerIdentity, request.RequestId, request.AttemptId, requestExpiry, DateTimeOffset.UtcNow);

                if (replayDecision != AuthenticatedRequestReplayDecision.Accepted)
                {
                    InferenceResponse rejected = CreateReplayRejection(request);
                    PeerResponseCorrelationValidator.Validate(request, rejected, LocalPeerIdentity);
                    await transport.SendMessageAsync(rejected.ToByteArray(), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                byte[] responseWire = await _requestHandler(requestWire, cancellationToken).ConfigureAwait(false);
                if (responseWire is null || responseWire.Length == 0)
                    throw new InvalidDataException("The peer request handler returned no response.");
                if (responseWire.Length > ProtocolMessageLimits.MaximumResponseMessageBytes)
                    throw new InvalidDataException("The peer response exceeds the protocol message limit.");

                InferenceResponse parsedResponse;
                try
                {
                    parsedResponse = InferenceResponse.Parser.ParseFrom(responseWire);
                }
                catch (InvalidProtocolBufferException exception)
                {
                    throw new InvalidDataException("The peer request handler produced malformed response data.", exception);
                }

                PeerResponseCorrelationValidator.Validate(request, parsedResponse, LocalPeerIdentity);
                await transport.SendMessageAsync(responseWire, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected during explicit participation stop or application shutdown.
        }
        catch (Exception exception)
        {
            Volatile.Write(ref _lastFailureType, exception.GetType().Name);
            Volatile.Write(ref _active, 0);
            try { await publisher.StopAsync().ConfigureAwait(false); }
            catch (Exception stopException) { Volatile.Write(ref _lastFailureType, stopException.GetType().Name); }
            try { await transport.DisconnectAsync().ConfigureAwait(false); }
            catch (Exception disconnectException) { Volatile.Write(ref _lastFailureType, disconnectException.GetType().Name); }
        }
    }

    private InferenceResponse CreateReplayRejection(InferenceRequest request) =>
        new()
        {
            RequestId = request.RequestId,
            AttemptId = request.AttemptId,
            EphemeralHostId = LocalPeerIdentity,
            Outcome = InferenceOutcome.Rejected,
            OutputText = string.Empty,
            CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            ModelId = request.ModelId,
            ErrorMessage = "A duplicate or untracked request was rejected."
        };

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;
            _disposed = true;
            _httpClient.Dispose();
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }
}
