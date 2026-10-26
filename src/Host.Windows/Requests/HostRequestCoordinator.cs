namespace WhiteP.Host.Windows.Requests;

public sealed class HostRequestCoordinator
{
    private readonly SemaphoreSlim _inferenceSemaphore = new(1, 1);

    public bool IsBusy => _inferenceSemaphore.CurrentCount == 0;

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await _inferenceSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operation(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _inferenceSemaphore.Release();
        }
    }
}
