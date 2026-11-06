namespace WhiteP.Host.Windows.Requests;

/// <summary>
/// Provides a single in-process response-path handoff for one request lifecycle.
/// The same gate instance must be shared by every terminal path for that request.
/// This does not guarantee network delivery exactly once or deduplicate across gate instances.
/// </summary>
public sealed class HostRequestResponseDeliveryGate
{
    private int _deliveryClaimed;

    public HostRequestResponseDeliveryGate(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("A request ID is required for response delivery.", nameof(requestId));
        }

        RequestId = requestId;
    }

    public string RequestId { get; }

    /// <summary>
    /// Indicates that one caller claimed the response path. It remains true if the callback throws,
    /// because the side effect may have occurred before the exception was observed.
    /// </summary>
    public bool HasClaimedDelivery => Volatile.Read(ref _deliveryClaimed) != 0;

    /// <summary>
    /// Invokes the supplied response path at most once for this gate instance.
    /// A false result means another call already claimed delivery. If the response path throws,
    /// the claim is not reset: retrying an ambiguous send could duplicate the response.
    /// </summary>
    public async Task<bool> TryDeliverAsync<TResponse>(
        TResponse response,
        Func<TResponse, CancellationToken, Task> responsePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(responsePath);
        cancellationToken.ThrowIfCancellationRequested();

        if (Interlocked.CompareExchange(ref _deliveryClaimed, 1, 0) != 0)
        {
            return false;
        }

        await responsePath(response, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
