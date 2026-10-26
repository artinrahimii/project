using WhiteP.Host.Windows.Requests;

internal static class HostRequestCoordinatorTests
{
    public static async Task RunAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var busyOperationInvoked = false;
        var activeCount = 0;
        var maximumActiveCount = 0;

        var first = coordinator.ExecuteAsync(async cancellationToken =>
        {
            TrackActiveOperation();
            firstStarted.TrySetResult(true);
            try
            {
                await releaseFirst.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return "first";
            }
            finally
            {
                Interlocked.Decrement(ref activeCount);
            }
        });

        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(coordinator.IsBusy, "The coordinator must report busy while one inference owns the gate.");

        var busyOutcome = await coordinator.TryExecuteAsync(cancellationToken =>
        {
            busyOperationInvoked = true;
            TrackActiveOperation();
            return Task.FromResult("must-not-run");
        }).WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

        Assert(busyOutcome.Status == HostRequestStatus.Busy && busyOutcome.Result is null,
            "A concurrent request must receive an explicit Busy outcome immediately.");
        Assert(!busyOperationInvoked, "A rejected concurrent operation must not be invoked.");

        var busyExceptionObserved = false;
        try
        {
            await coordinator.ExecuteAsync(_ =>
            {
                busyOperationInvoked = true;
                return Task.FromResult("must-not-run");
            }).ConfigureAwait(false);
        }
        catch (HostRequestBusyException exception)
        {
            busyExceptionObserved = exception.Status == HostRequestStatus.Busy;
        }

        Assert(busyExceptionObserved, "ExecuteAsync must expose a typed Busy exception for a concurrent request.");
        Assert(!busyOperationInvoked, "No rejected operation may execute.");

        releaseFirst.TrySetResult(true);
        Assert(await first.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false) == "first",
            "The first inference result must be preserved.");
        var afterFirst = await coordinator.ExecuteAsync(cancellationToken =>
        {
            TrackActiveOperation();
            return Task.FromResult("after-first");
        }).ConfigureAwait(false);
        Assert(afterFirst == "after-first", "A new request must be accepted after the active inference finishes.");
        Assert(maximumActiveCount == 1, "No two coordinator operations may overlap.");
        Assert(!coordinator.IsBusy, "The coordinator gate must be released after all work completes.");

        var exceptionObserved = false;
        try
        {
            await coordinator.ExecuteAsync<string>(_ => Task.FromException<string>(new InvalidOperationException("Expected test failure."))).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception) when (exception is not HostRequestBusyException)
        {
            exceptionObserved = true;
        }

        Assert(exceptionObserved, "The coordinator must propagate an operation failure.");
        var afterFailure = await coordinator.ExecuteAsync(_ => Task.FromResult("released")).ConfigureAwait(false);
        Assert(afterFailure == "released", "The semaphore must be released after an operation throws.");

        Console.WriteLine("PASS: HostRequestCoordinator serializes active inference, rejects concurrent work as Busy without queuing, and releases its semaphore after success or failure.");

        void TrackActiveOperation()
        {
            var active = Interlocked.Increment(ref activeCount);
            while (true)
            {
                var observed = Volatile.Read(ref maximumActiveCount);
                if (active <= observed ||
                    Interlocked.CompareExchange(ref maximumActiveCount, active, observed) == observed)
                {
                    break;
                }
            }
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
