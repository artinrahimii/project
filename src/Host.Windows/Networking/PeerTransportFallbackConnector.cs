using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace WhiteP.Host.Windows.Networking;

/// <summary>A safe summary of one failed direct-address attempt. Payloads and exception messages are never captured.</summary>
public sealed record PeerTransportAttemptFailure(string Address, string FailureType);

/// <summary>Thrown when none of the supplied peer addresses can establish a verified direct connection.</summary>
public sealed class PeerConnectionUnavailableException : IOException
{
    public PeerConnectionUnavailableException(IEnumerable<PeerTransportAttemptFailure> attempts)
        : this(attempts?.ToArray() ?? throw new ArgumentNullException(nameof(attempts)))
    {
    }

    private PeerConnectionUnavailableException(PeerTransportAttemptFailure[] attempts)
        : base($"No authenticated direct peer path was established across {attempts.Length} candidate address(es).")
    {
        Attempts = Array.AsReadOnly(attempts);
    }

    public IReadOnlyList<PeerTransportAttemptFailure> Attempts { get; }
}

/// <summary>
/// Owns the successful transport returned by the fallback connector. Dispose this result to
/// close the selected peer connection and release its native/network resources.
/// </summary>
public sealed class PeerTransportConnectionResult : IAsyncDisposable
{
    internal PeerTransportConnectionResult(IPeerTransport transport, PeerTransportEndpoint selectedEndpoint, int failedAttemptCount)
    {
        Transport = transport ?? throw new ArgumentNullException(nameof(transport));
        SelectedEndpoint = selectedEndpoint ?? throw new ArgumentNullException(nameof(selectedEndpoint));
        FailedAttemptCount = failedAttemptCount;
    }

    public IPeerTransport Transport { get; }
    public PeerTransportEndpoint SelectedEndpoint { get; }
    public int FailedAttemptCount { get; }
    public ValueTask DisposeAsync() => Transport.DisposeAsync();
}

/// <summary>
/// Tries previously advertised addresses for the same expected peer, in order. The address list
/// may contain TCP and QUIC paths. This connector accepts connection metadata only and never
/// accepts inference prompts/responses; application messages use the returned authenticated
/// IPeerTransport directly to the selected peer.
/// </summary>
public static class PeerTransportFallbackConnector
{
    public static async Task<PeerTransportConnectionResult> ConnectAsync(
        IEnumerable<PeerTransportEndpoint> candidateEndpoints,
        string expectedPeerIdentity,
        Func<IPeerTransport> transportFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidateEndpoints);
        ArgumentNullException.ThrowIfNull(transportFactory);
        ValidateIdentityText(expectedPeerIdentity);

        PeerTransportEndpoint[] candidates = candidateEndpoints.ToArray();
        if (candidates.Length == 0)
            throw new ArgumentException("At least one direct peer address is required.", nameof(candidateEndpoints));

        var unique = new List<PeerTransportEndpoint>(candidates.Length);
        var seenAddresses = new HashSet<string>(StringComparer.Ordinal);
        foreach (PeerTransportEndpoint? candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            if (!string.Equals(candidate.ExpectedPeerIdentity, expectedPeerIdentity, StringComparison.Ordinal))
                throw new ArgumentException("Every fallback address must pin the same expected peer identity.", nameof(candidateEndpoints));
            if (seenAddresses.Add(candidate.Address))
                unique.Add(candidate);
        }

        var failures = new List<PeerTransportAttemptFailure>(unique.Count);
        foreach (PeerTransportEndpoint candidate in unique)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IPeerTransport? transport = null;
            try
            {
                transport = transportFactory()
                    ?? throw new InvalidOperationException("The transport factory returned null.");
                await transport.ConnectSecureAsync(candidate, cancellationToken).ConfigureAwait(false);
                if (!transport.IsConnected ||
                    !string.Equals(transport.RemotePeerIdentity, expectedPeerIdentity, StringComparison.Ordinal))
                    throw new AuthenticationException("The candidate did not establish the pinned authenticated peer identity.");
                return new PeerTransportConnectionResult(transport, candidate, failures.Count);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await DisposeQuietlyAsync(transport).ConfigureAwait(false);
                throw;
            }
            catch (Exception exception)
            {
                await DisposeQuietlyAsync(transport).ConfigureAwait(false);
                if (!IsRetryableConnectionFailure(exception))
                    throw;
                // Deliberately record only the exception type, never the exception message or message bytes.
                failures.Add(new PeerTransportAttemptFailure(candidate.Address, exception.GetType().Name));
            }
        }

        throw new PeerConnectionUnavailableException(failures);
    }

    private static bool IsRetryableConnectionFailure(Exception exception)
    {
        if (exception is IOException or AuthenticationException or SocketException or TimeoutException
            or OperationCanceledException or InvalidOperationException)
        {
            return true;
        }

        // libp2p wraps socket failures in PeerConnectionException; inspect nested causes so
        // a refused first address does not prevent trying the next authenticated candidate.
        if (exception is AggregateException aggregate)
        {
            return aggregate.InnerExceptions.Any(IsRetryableConnectionFailure);
        }

        return exception.InnerException is not null &&
            IsRetryableConnectionFailure(exception.InnerException);
    }

    private static async ValueTask DisposeQuietlyAsync(IPeerTransport? transport)
    {
        if (transport is null) return;
        try { await transport.DisposeAsync().ConfigureAwait(false); }
        catch (Exception) { /* Preserve the connection failure that caused this candidate to be abandoned. */ }
    }

    private static void ValidateIdentityText(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity) ||
            !string.Equals(identity, identity.Trim(), StringComparison.Ordinal) ||
            identity.Any(char.IsControl))
            throw new ArgumentException("A normalized expected peer identity is required.", nameof(identity));
    }
}
