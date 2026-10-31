using System;
using System.Threading;
using System.Threading.Tasks;

namespace WhiteP.Host.Windows.Networking;

/// <summary>
/// Host-side abstraction for a message-oriented peer connection.
/// A successful <see cref="ConnectSecureAsync"/> call is a security boundary: the implementation
/// must have established an encrypted channel and authenticated the remote peer against the
/// expected identity before returning. Implementations must fail closed and must not downgrade
/// to plaintext. This interface is only a contract; it does not provide a secure transport by itself.
/// </summary>
public interface IPeerTransport : IAsyncDisposable
{
    /// <summary>True only while an authenticated, encrypted peer channel is established.</summary>
    bool IsConnected { get; }

    /// <summary>Authenticated remote identity, or null until a secure connection has been verified.</summary>
    string? RemotePeerIdentity { get; }

    /// <summary>
    /// Establish an encrypted, authenticated connection to the requested peer.
    /// Completes only when the verified remote identity matches
    /// <see cref="PeerTransportEndpoint.ExpectedPeerIdentity"/>. If encryption or identity
    /// verification is unavailable or fails, this operation must fail closed.
    /// </summary>
    Task ConnectSecureAsync(
        PeerTransportEndpoint endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one complete application-level message. The memory is consumed before this task
    /// completes. Implementations must not log message contents.
    /// </summary>
    Task SendMessageAsync(
        ReadOnlyMemory<byte> message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives one complete application-level message and returns an owned byte array.
    /// Cancellation is honored; connection loss is reported as an I/O failure rather than an
    /// empty successful message.
    /// </summary>
    Task<byte[]> ReceiveMessageAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes the secure connection and releases transport resources. Repeated calls are safe.</summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
