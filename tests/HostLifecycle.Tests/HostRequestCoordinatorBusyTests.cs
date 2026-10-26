using WhiteP.Host.Windows.Requests;

internal static class HostRequestCoordinatorBusyTests
{
    public static async Task RunAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var invocationCount = 0;

        var first = coordinator.ExecuteAsync(async cancellationToken =>
        {
            Interlocked.Increment(ref invocationCount);
            started.TrySetResult(true);
            await release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return "first";
        });
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        var rejected = new List<HostRequestOutcome<string>>();
        for (var index = 0; index < 32; index++)
        {
            rejected.Add(await coordinator.TryExecuteAsync(_ =>
            {
                Interlocked.Increment(ref invocationCount);
                return Task.FromResult("unexpected");
            }).WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false));
        }

        Assert(rejected.All(result => result.Status == HostRequestStatus.Busy && result.Result is null),
            "Every concurrent request must immediately receive a Busy outcome.");
        Assert(invocationCount == 1,
            "Concurrent requests must not queue and must not invoke their operations.");

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var cancellationObserved = false;
        try
        {
            await coordinator.TryExecuteAsync(_ => Task.FromResult("unexpected"), cancelled.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancellationObserved = true;
        }
        Assert(cancellationObserved, "An already-cancelled request must preserve cancellation rather than masquerade as Busy.");

        release.TrySetResult(true);
        Assert(await first.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false) == "first",
            "The first request must complete normally.");
        Assert(await coordinator.ExecuteAsync(_ => Task.FromResult("next")).ConfigureAwait(false) == "next",
            "A new request must be accepted after the current inference releases the gate.");

        Console.WriteLine("PASS: busy admission is fail-fast, does not queue 32 requests, preserves cancellation, and resumes after release.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
