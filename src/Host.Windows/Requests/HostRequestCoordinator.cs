namespace WhiteP.Host.Windows.Requests;

public enum HostRequestStatus
{
    Completed,
    Busy
}

public sealed record HostRequestOutcome<T>(HostRequestStatus Status, T? Result)
{
    public static HostRequestOutcome<T> Completed(T result) =>
        new(HostRequestStatus.Completed, result);

    public static HostRequestOutcome<T> Busy() =>
        new(HostRequestStatus.Busy, default);
}

public sealed class HostRequestBusyException : InvalidOperationException
{
    public HostRequestBusyException()
        : base("Host is Busy; another inference request is already active.")
    {
    }

    public HostRequestStatus Status => HostRequestStatus.Busy;
}

public sealed class HostRequestCoordinator
{
    private readonly SemaphoreSlim _inferenceSemaphore = new(1, 1);

    public bool IsBusy => _inferenceSemaphore.CurrentCount == 0;

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var outcome = await TryExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
        if (outcome.Status == HostRequestStatus.Busy)
        {
            throw new HostRequestBusyException();
        }

        return outcome.Result!;
    }

    public async Task<HostRequestOutcome<T>> TryExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        // Never queue a second request behind an active inference.
        if (!await _inferenceSemaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return HostRequestOutcome<T>.Busy();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await operation(cancellationToken).ConfigureAwait(false);
            return HostRequestOutcome<T>.Completed(result);
        }
        finally
        {
            _inferenceSemaphore.Release();
        }
    }
}
