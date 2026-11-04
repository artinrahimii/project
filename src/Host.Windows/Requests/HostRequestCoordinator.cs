using WhiteP.Host.Domain;

namespace WhiteP.Host.Windows.Requests;

public enum HostRequestStatus
{
    Completed,
    Busy,
    Rejected
}

public sealed record HostRequestOutcome<T>(
    HostRequestStatus Status,
    T? Result,
    HostRequestAdmissionFailure? AdmissionFailure = null,
    string? Message = null)
{
    public static HostRequestOutcome<T> Completed(T result) =>
        new(HostRequestStatus.Completed, result);

    public static HostRequestOutcome<T> Busy() =>
        new(HostRequestStatus.Busy, default);

    public static HostRequestOutcome<T> Rejected(
        HostRequestAdmissionFailure failure,
        string message) =>
        new(HostRequestStatus.Rejected, default, failure, message);
}

public sealed class HostRequestBusyException : InvalidOperationException
{
    public HostRequestBusyException()
        : base("Host is Busy; another inference request is already active.")
    {
    }

    public HostRequestStatus Status => HostRequestStatus.Busy;
}

public sealed class HostRequestAdmissionException : InvalidOperationException
{
    public HostRequestAdmissionException(
        HostRequestAdmissionFailure failure,
        string message)
        : base(message)
    {
        Failure = failure;
    }

    public HostRequestAdmissionFailure Failure { get; }

    public HostRequestStatus Status => HostRequestStatus.Rejected;
}

public sealed class HostRequestCoordinator
{
    private readonly SemaphoreSlim _inferenceSemaphore = new(1, 1);
    private readonly object _activeCancellationSync = new();
    private readonly Func<DateTimeOffset> _utcNow;
    private const long MaximumDeadlineTimerSliceMilliseconds = 24L * 60L * 60L * 1000L;
    private CancellationTokenSource? _activeRequestCancellation;

    public HostRequestCoordinator(Func<DateTimeOffset>? utcNow = null)
    {
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public bool IsBusy => _inferenceSemaphore.CurrentCount == 0;

    /// <summary>Requests cancellation of the currently active coordinator operation.</summary>
    /// <returns>True when an active request was signalled; false when no request is active.</returns>
    public bool CancelActiveRequest()
    {
        lock (_activeCancellationSync)
        {
            if (_activeRequestCancellation is null)
            {
                return false;
            }

            try
            {
                _activeRequestCancellation.Cancel();
                return true;
            }
            catch (ObjectDisposedException)
            {
                // Completion raced the request; the operation is no longer cancellable.
                return false;
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(
        HostRequestAdmissionContext admission,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var outcome = await TryExecuteAsync(admission, operation, cancellationToken).ConfigureAwait(false);
        if (outcome.Status == HostRequestStatus.Busy)
        {
            throw new HostRequestBusyException();
        }

        if (outcome.Status == HostRequestStatus.Rejected)
        {
            throw new HostRequestAdmissionException(
                outcome.AdmissionFailure ?? HostRequestAdmissionFailure.InvalidDeadline,
                outcome.Message ?? "Request rejected by admission policy.");
        }

        return outcome.Result!;
    }

    public async Task<HostRequestOutcome<T>> TryExecuteAsync<T>(
        HostRequestAdmissionContext admission,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(admission);
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        // Snapshot the absolute deadline at the coordinator boundary; do not allow later mutation to extend it.
        var receivedAtUtc = _utcNow();
        var receivedDeadlineUnixMs = admission.DeadlineUnixMs;
        var receivedAdmission = admission with { DeadlineUnixMs = receivedDeadlineUnixMs };
        var decision = HostRequestAdmissionPolicy.Evaluate(receivedAdmission, receivedAtUtc);
        if (!decision.IsAdmitted)
        {
            return HostRequestOutcome<T>.Rejected(decision.Failure, decision.Message);
        }

        if (!await _inferenceSemaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return HostRequestOutcome<T>.Busy();
        }

        CancellationTokenSource? deadlineCancellation = null;
        CancellationTokenSource? deadlineMonitorCancellation = null;
        CancellationTokenSource? activeCancellation = null;
        Task? deadlineMonitor = null;
        try
        {
            deadlineCancellation = new CancellationTokenSource();
            activeCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                deadlineCancellation.Token);
            lock (_activeCancellationSync)
            {
                _activeRequestCancellation = activeCancellation;
            }

            activeCancellation.Token.ThrowIfCancellationRequested();

            // Recheck readiness/deadline immediately before inference is invoked.
            var finalDecision = HostRequestAdmissionPolicy.Evaluate(receivedAdmission, _utcNow());
            if (!finalDecision.IsAdmitted)
            {
                return HostRequestOutcome<T>.Rejected(finalDecision.Failure, finalDecision.Message);
            }

            // An absolute request deadline must cancel long-running work, not merely reject its result later.
            deadlineMonitorCancellation = new CancellationTokenSource();
            deadlineMonitor = MonitorDeadlineAsync(
                deadlineCancellation,
                receivedDeadlineUnixMs,
                deadlineMonitorCancellation.Token);

            var result = await operation(activeCancellation.Token).ConfigureAwait(false);

            // Preserve caller/deadline cancellation even when a callback incorrectly returns after its token was signalled.
            activeCancellation.Token.ThrowIfCancellationRequested();

            // Do not deliver a result when inference finished after its request deadline.
            var responseDeadline = HostRequestAdmissionPolicy.EvaluateDeadline(receivedDeadlineUnixMs, _utcNow());
            if (!responseDeadline.IsAdmitted)
            {
                return HostRequestOutcome<T>.Rejected(
                    responseDeadline.Failure,
                    "Request deadline expired before result delivery; the late result was withheld.");
            }

            return HostRequestOutcome<T>.Completed(result);
        }
        catch (OperationCanceledException) when (
            deadlineCancellation?.IsCancellationRequested == true &&
            !cancellationToken.IsCancellationRequested)
        {
            return HostRequestOutcome<T>.Rejected(
                HostRequestAdmissionFailure.DeadlineExpired,
                "Request deadline expired while inference was active; cancellation was signalled.");
        }
        finally
        {
            if (deadlineMonitorCancellation is not null)
            {
                deadlineMonitorCancellation.Cancel();
                if (deadlineMonitor is not null)
                {
                    await deadlineMonitor.ConfigureAwait(false);
                }

                deadlineMonitorCancellation.Dispose();
            }

            if (activeCancellation is not null)
            {
                lock (_activeCancellationSync)
                {
                    if (ReferenceEquals(_activeRequestCancellation, activeCancellation))
                    {
                        _activeRequestCancellation = null;
                    }
                }

                activeCancellation.Dispose();
            }

            deadlineCancellation?.Dispose();
            _inferenceSemaphore.Release();
        }
    }
    private async Task MonitorDeadlineAsync(
        CancellationTokenSource deadlineCancellation,
        ulong deadlineUnixMs,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = _utcNow();
            var decision = HostRequestAdmissionPolicy.EvaluateDeadline(deadlineUnixMs, nowUtc);
            if (!decision.IsAdmitted)
            {
                try
                {
                    deadlineCancellation.Cancel();
                }
                catch (AggregateException)
                {
                    // Cancellation is already signalled; callback failures must not hide deadline expiry.
                }

                return;
            }

            var remainingMilliseconds = checked(
                (long)(deadlineUnixMs - (ulong)nowUtc.ToUnixTimeMilliseconds()));
            var delayMilliseconds = Math.Min(
                remainingMilliseconds,
                MaximumDeadlineTimerSliceMilliseconds);

            try
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(delayMilliseconds),
                    stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

}
