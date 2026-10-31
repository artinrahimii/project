using WhiteP.Host.Windows.Requests;

internal static class HostRequestCoordinatorConcurrencyTests
{
    public static async Task RunAsync()
    {
        const int requestCount = 64;
        var coordinator = new HostRequestCoordinator();
        var launchContenders = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstInferenceStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseInference = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var allBusyResponsesObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var operationInvocationCount = 0;
        var busyResponseCount = 0;
        var activeInferenceCount = 0;
        var maximumActiveInferenceCount = 0;

        var contenders = Enumerable.Range(0, requestCount)
            .Select(RunContenderAsync)
            .ToArray();

        var observedEveryLosingRequest = false;
        try
        {
            launchContenders.TrySetResult(true);
            await firstInferenceStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            observedEveryLosingRequest = await Task.WhenAny(
                allBusyResponsesObserved.Task,
                Task.Delay(TimeSpan.FromSeconds(3))).ConfigureAwait(false) == allBusyResponsesObserved.Task;
        }
        finally
        {
            // Release any admitted operation even when an assertion precondition times out.
            releaseInference.TrySetResult(true);
        }

        var outcomes = await Task.WhenAll(contenders).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        var completedCount = outcomes.Count(outcome => outcome.Status == HostRequestStatus.Completed);
        var busyCount = outcomes.Count(outcome => outcome.Status == HostRequestStatus.Busy);

        Assert(observedEveryLosingRequest,
            "All 63 competing requests must receive Busy while the single admitted inference remains active.");
        Assert(operationInvocationCount == 1,
            $"Exactly one operation may be invoked for {requestCount} simultaneous contenders; observed {operationInvocationCount}.");
        Assert(maximumActiveInferenceCount == 1,
            $"At most one inference may be active on one coordinator; observed a maximum of {maximumActiveInferenceCount}.");
        Assert(completedCount == 1 && busyCount == requestCount - 1,
            $"Expected one completed request and {requestCount - 1} Busy outcomes; observed {completedCount} completed and {busyCount} Busy.");
        Assert(activeInferenceCount == 0, "All admitted inference work must have exited before this test completes.");
        Assert(!coordinator.IsBusy, "The coordinator must release its inference gate after all contenders finish.");

        Console.WriteLine(
            $"PASS: one coordinator admitted exactly one of {requestCount} simultaneous requests; {requestCount - 1} returned Busy and maximum active inference was {maximumActiveInferenceCount}.");

        async Task<HostRequestOutcome<string>> RunContenderAsync(int requestIndex)
        {
            await launchContenders.Task.ConfigureAwait(false);
            var outcome = await coordinator.TryExecuteAsync(
                ValidAdmission(),
                async cancellationToken =>
                {
                    var active = Interlocked.Increment(ref activeInferenceCount);
                    UpdateMaximumActiveCount(active);
                    Interlocked.Increment(ref operationInvocationCount);
                    firstInferenceStarted.TrySetResult(true);
                    try
                    {
                        await releaseInference.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                        return $"completed-{requestIndex}";
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeInferenceCount);
                    }
                }).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            if (outcome.Status == HostRequestStatus.Busy &&
                Interlocked.Increment(ref busyResponseCount) == requestCount - 1)
            {
                allBusyResponsesObserved.TrySetResult(true);
            }

            return outcome;
        }

        void UpdateMaximumActiveCount(int active)
        {
            while (true)
            {
                var observed = Volatile.Read(ref maximumActiveInferenceCount);
                if (active <= observed ||
                    Interlocked.CompareExchange(ref maximumActiveInferenceCount, active, observed) == observed)
                {
                    return;
                }
            }
        }
    }

    private static HostRequestAdmissionContext ValidAdmission() =>
        new(
            true,
            WhiteP.Host.Domain.HostState.Ready,
            "fixture/ready-model",
            "fixture/ready-model",
            WhiteP.Host.Domain.HostCapacityState.Ready,
            (ulong)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds());

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
