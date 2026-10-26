using WhiteP.Host.Windows.Requests;

internal static class HostRequestCoordinatorTests
{
    public static async Task RunAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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

        var second = coordinator.ExecuteAsync(cancellationToken =>
        {
            TrackActiveOperation();
            secondStarted.TrySetResult(true);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult("second");
            }
            finally
            {
                Interlocked.Decrement(ref activeCount);
            }
        });

        Assert(!secondStarted.Task.IsCompleted, "The second inference must not start before the first releases the single-capacity gate.");

        releaseFirst.TrySetResult(true);
        Assert(await first.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false) == "first", "The first inference result must be preserved.");
        Assert(await second.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false) == "second", "The waiting inference must start after the first finishes.");
        Assert(await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false), "The second inference must eventually start.");
        Assert(maximumActiveCount == 1, "No two coordinator operations may overlap.");
        Assert(!coordinator.IsBusy, "The coordinator gate must be released after all work completes.");

        var exceptionObserved = false;
        try
        {
            await coordinator.ExecuteAsync<string>(_ => Task.FromException<string>(new InvalidOperationException("Expected test failure."))).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            exceptionObserved = true;
        }

        Assert(exceptionObserved, "The coordinator must propagate an operation failure.");
        var afterFailure = await coordinator.ExecuteAsync(_ => Task.FromResult("released")).ConfigureAwait(false);
        Assert(afterFailure == "released", "The semaphore must be released after an operation throws.");

        Console.WriteLine("PASS: HostRequestCoordinator limits active operations to one and releases its semaphore after success or failure.");

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
