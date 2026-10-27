using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Multiformats.Address;
using Nethermind.Libp2p;
using Nethermind.Libp2p.Core;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Concrete message transport backed by Nethermind.Libp2p. Each instance represents one
/// remote peer; create a new instance to establish another peer session. Listening is opt-in
/// and requires a pinned expected peer identity.
/// </summary>
public sealed class Libp2pPeerTransport : IPeerTransport
{
    public const int DefaultMaximumMessageBytes = 4 * 1024 * 1024;
    public const int DefaultInboundQueueCapacity = 8;
    private static readonly TimeSpan ProtocolStartupTimeout = TimeSpan.FromSeconds(15);

    private readonly ServiceProvider _services;
    private readonly ILocalPeer _peer;
    private readonly Libp2pPeerMessageProtocol _protocol;
    private readonly object _sessionGate = new();
    private readonly int _maximumMessageBytes;
    private ISession? _session;
    private string? _expectedRemotePeerIdentity;
    private string? _remotePeerIdentity;
    private string[] _listenAddresses = Array.Empty<string>();
    private Task? _dialTask;
    private int _connectStarted;
    private int _listenStarted;
    private int _disconnectStarted;
    private int _disposeStarted;

    public Libp2pPeerTransport(
        string? expectedRemotePeerIdentity = null,
        Identity? localIdentity = null,
        int maximumMessageBytes = DefaultMaximumMessageBytes,
        int inboundQueueCapacity = DefaultInboundQueueCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumMessageBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(inboundQueueCapacity);
        if (expectedRemotePeerIdentity is not null)
        {
            ValidatePeerIdentity(expectedRemotePeerIdentity);
        }

        _maximumMessageBytes = maximumMessageBytes;
        _expectedRemotePeerIdentity = expectedRemotePeerIdentity;
        _protocol = new Libp2pPeerMessageProtocol(maximumMessageBytes, inboundQueueCapacity);
        _services = new ServiceCollection()
            .AddLogging()
            .AddLibp2p(builder => builder.WithQuic().AddProtocol(_protocol))
            .BuildServiceProvider();
        try
        {
            _peer = _services.GetRequiredService<IPeerFactory>().Create(localIdentity);
            LocalPeerIdentity = new PeerId(_peer.Identity.PublicKey).ToString();
            _peer.OnConnected += OnPeerConnected;
        }
        catch
        {
            _services.Dispose();
            _protocol.Dispose();
            throw;
        }
    }

    /// <summary>The local libp2p PeerId derived from the public key.</summary>
    public string LocalPeerIdentity { get; }

    /// <summary>Multiaddresses that include this peer's authenticated identity.</summary>
    public IReadOnlyList<string> ListenAddresses => Array.AsReadOnly(Volatile.Read(ref _listenAddresses).ToArray());

    public bool IsConnected => Volatile.Read(ref _disconnectStarted) == 0 && _protocol.IsConnected && RemotePeerIdentity is not null;

    public string? RemotePeerIdentity => Volatile.Read(ref _remotePeerIdentity);

    /// <summary>
    /// Starts listening on one explicit local address. Wildcard addresses are rejected, and
    /// inbound connections are refused unless the constructor pins an expected peer identity.
    /// </summary>
    public async Task StartListeningAsync(
        IPAddress bindAddress,
        int port,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bindAddress);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeStarted) != 0, this);
        if (_expectedRemotePeerIdentity is null)
        {
            throw new InvalidOperationException("Listening requires an expected remote peer identity.");
        }

        if (IPAddress.Any.Equals(bindAddress) || IPAddress.IPv6Any.Equals(bindAddress))
        {
            throw new ArgumentException("A specific interface address is required; wildcard binding is not allowed.", nameof(bindAddress));
        }

        if (bindAddress.AddressFamily == AddressFamily.InterNetworkV6 && bindAddress.ScopeId != 0)
        {
            throw new ArgumentException("IPv6 scoped addresses are not supported as a portable libp2p endpoint.", nameof(bindAddress));
        }

        if (port is < 1 or > IPEndPoint.MaxPort)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Listening requires an explicitly selected TCP port.");
        }

        if (Interlocked.CompareExchange(ref _listenStarted, 1, 0) != 0)
        {
            throw new InvalidOperationException("This peer transport instance has already started listening.");
        }

        string prefix = bindAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => $"/ip4/{bindAddress}",
            AddressFamily.InterNetworkV6 => $"/ip6/{bindAddress}",
            _ => throw new ArgumentException("Only IP addresses can be used for listening.", nameof(bindAddress))
        };
        // Expose two direct paths for the same authenticated PeerId: TCP with negotiated
        // Noise/TLS, and QUIC over UDP with libp2p's authenticated QUIC stack.
        // Discovery shares addresses only; inference payloads remain on the peer connection.
        Multiaddress tcpListenAddress = Multiaddress.Decode($"{prefix}/tcp/{port}");
        Multiaddress quicListenAddress = Multiaddress.Decode($"{prefix}/udp/{port}/quic-v1");
        await _peer.StartListenAsync([tcpListenAddress, quicListenAddress], cancellationToken).ConfigureAwait(false);

        string[] addresses = _peer.ListenAddresses.Count > 0
            ? _peer.ListenAddresses.Select(AddLocalPeerIdentity).Distinct(StringComparer.Ordinal).ToArray()
            : [AddLocalPeerIdentity(tcpListenAddress.ToString()), AddLocalPeerIdentity(quicListenAddress.ToString())];
        if (addresses.Length == 0)
        {
            throw new IOException("The libp2p node started without publishing a listen address.");
        }

        Volatile.Write(ref _listenAddresses, addresses);
    }

    public async Task ConnectSecureAsync(
        PeerTransportEndpoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeStarted) != 0, this);
        if (Interlocked.CompareExchange(ref _connectStarted, 1, 0) != 0)
        {
            throw new InvalidOperationException("This peer transport instance can connect to only one remote peer.");
        }

        Multiaddress remoteAddress;
        try
        {
            ValidatePeerIdentity(endpoint.ExpectedPeerIdentity);
            remoteAddress = Multiaddress.Decode(endpoint.Address);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or InvalidOperationException)
        {
            throw new AuthenticationException("The remote peer address or expected identity is invalid.", exception);
        }

        string? addressPeerIdentity = remoteAddress.GetPeerId()?.ToString();
        if (addressPeerIdentity is null)
        {
            throw new AuthenticationException("The remote multiaddress must include a /p2p peer identity.");
        }

        if (!string.Equals(addressPeerIdentity, endpoint.ExpectedPeerIdentity, StringComparison.Ordinal))
        {
            throw new AuthenticationException("The peer identity in the multiaddress does not match the expected peer identity.");
        }

        lock (_sessionGate)
        {
            _expectedRemotePeerIdentity = endpoint.ExpectedPeerIdentity;
        }

        ISession? session = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            session = await _peer.DialAsync(remoteAddress, cancellationToken).ConfigureAwait(false);
            string actualPeerIdentity = session.RemoteAddress.GetPeerId()?.ToString()
                ?? throw new AuthenticationException("The libp2p session did not expose an authenticated peer identity.");
            if (!string.Equals(actualPeerIdentity, endpoint.ExpectedPeerIdentity, StringComparison.Ordinal))
            {
                await session.DisconnectAsync().ConfigureAwait(false);
                throw new AuthenticationException("The authenticated libp2p peer identity did not match the expected identity.");
            }

            ISession? existing = Interlocked.CompareExchange(ref _session, session, null);
            if (existing is not null && !ReferenceEquals(existing, session))
            {
                await session.DisconnectAsync().ConfigureAwait(false);
                throw new IOException("This transport instance already owns a different peer session.");
            }

            Volatile.Write(ref _remotePeerIdentity, actualPeerIdentity);
            Task dialTask = session.DialAsync<Libp2pPeerMessageProtocol>(cancellationToken);
            Volatile.Write(ref _dialTask, dialTask);

            using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            startupCancellation.CancelAfter(ProtocolStartupTimeout);
            Task readyTask = _protocol.Ready.WaitAsync(startupCancellation.Token);
            Task completed = await Task.WhenAny(readyTask, dialTask).ConfigureAwait(false);
            if (completed == dialTask)
            {
                await dialTask.ConfigureAwait(false);
                throw new IOException("The peer protocol ended before it became ready.");
            }

            await readyTask.ConfigureAwait(false);
            if (!IsConnected)
            {
                throw new IOException("The libp2p secure channel did not reach the connected state.");
            }
        }
        catch
        {
            if (session is not null)
            {
                await DisconnectQuietlyAsync(session).ConfigureAwait(false);
            }
            else
            {
                await DisconnectQuietlyAsync().ConfigureAwait(false);
            }

            throw;
        }
    }

    public async Task SendMessageAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeStarted) != 0, this);
        if (!IsConnected)
        {
            throw new IOException("A verified peer connection is not active.");
        }

        if (message.Length > _maximumMessageBytes)
        {
            throw new InvalidDataException("The peer message exceeds the configured size limit.");
        }

        await _protocol.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    public async Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposeStarted) != 0, this);
        if (!IsConnected && Volatile.Read(ref _dialTask) is null)
        {
            throw new IOException("A verified peer connection is not active.");
        }

        return await _protocol.ReceiveMessageAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.Exchange(ref _disconnectStarted, 1) != 0)
        {
            return;
        }

        ISession? session = Volatile.Read(ref _session);
        if (session is not null)
        {
            await session.DisconnectAsync().ConfigureAwait(false);
        }
        else
        {
            await _protocol.DisconnectCurrentSessionAsync().ConfigureAwait(false);
        }

        Task? dialTask = Volatile.Read(ref _dialTask);
        if (dialTask is not null)
        {
            try
            {
                await dialTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // Session disconnect remains authoritative even if the library task takes longer to unwind.
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Expected: the libp2p session cancels its long-running protocol dial task
                // when the caller intentionally disconnects. Caller-requested cancellation
                // is not swallowed because the filter excludes a cancelled caller token.
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
        {
            return;
        }

        Exception? disposalError = null;
        try
        {
            await DisconnectQuietlyAsync(Volatile.Read(ref _session)).ConfigureAwait(false);
            _peer.OnConnected -= OnPeerConnected;
            await _peer.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            disposalError = exception;
        }
        finally
        {
            _protocol.Dispose();
            _services.Dispose();
        }

        if (disposalError is not null)
        {
            throw new IOException("The libp2p transport failed while releasing its resources.", disposalError);
        }
    }

    private void OnPeerConnected(ISession session)
    {
        string identity;
        try
        {
            identity = session.RemoteAddress.GetPeerId()?.ToString()
                ?? throw new AuthenticationException("The libp2p session did not expose an authenticated peer identity.");
        }
        catch (Exception)
        {
            _ = DisconnectQuietlyAsync(session);
            return;
        }

        string? expected;
        lock (_sessionGate)
        {
            expected = _expectedRemotePeerIdentity;
        }

        if (expected is null || !string.Equals(expected, identity, StringComparison.Ordinal))
        {
            _ = DisconnectQuietlyAsync(session);
            return;
        }

        ISession? existing = Interlocked.CompareExchange(ref _session, session, null);
        if (existing is not null && !ReferenceEquals(existing, session))
        {
            _ = DisconnectQuietlyAsync(session);
            return;
        }

        Volatile.Write(ref _remotePeerIdentity, identity);
    }

    private string AddLocalPeerIdentity(Multiaddress address) => AddLocalPeerIdentity(address.ToString());

    private string AddLocalPeerIdentity(string address)
    {
        Multiaddress parsed = Multiaddress.Decode(address);
        string? peerIdentity = parsed.GetPeerId()?.ToString();
        if (peerIdentity is not null)
        {
            if (!string.Equals(peerIdentity, LocalPeerIdentity, StringComparison.Ordinal))
            {
                throw new IOException("The libp2p listen address exposed an unexpected peer identity.");
            }

            return parsed.ToString();
        }

        return $"{parsed}/p2p/{LocalPeerIdentity}";
    }

    private static void ValidatePeerIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity) ||
            !string.Equals(identity, identity.Trim(), StringComparison.Ordinal) ||
            identity.Any(char.IsControl))
        {
            throw new AuthenticationException("A valid, normalized peer identity is required.");
        }

        try
        {
            _ = new PeerId(identity);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or IndexOutOfRangeException)
        {
            throw new AuthenticationException("The supplied peer identity is not a valid libp2p PeerId.", exception);
        }
    }

    private static async Task DisconnectQuietlyAsync(ISession? session = null)
    {
        if (session is null)
        {
            return;
        }

        try
        {
            await session.DisconnectAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Teardown is best-effort while preserving the original connection failure.
        }
    }
}
