using WhiteP.Host.Windows.Requests;

internal static class HostRequestResponseDeliveryTests
{
    public static async Task RunAsync()
    {
        await SequentialDuplicateDeliveryIsSuppressedAsync().ConfigureAwait(false);
        await ConcurrentDeliveryRaceInvokesCallbackOnceAsync().ConfigureAwait(false);
        await AmbiguousFailureIsNotRetriedAsync().ConfigureAwait(false);
        await PreCancelledCallDoesNotClaimTheGateAsync().ConfigureAwait(false);
        InvalidRequestIdIsRejected();
        Console.WriteLine("PASS: HostRequestResponseDeliveryGate invokes a request response path at most once.");
    }

    private static async Task SequentialDuplicateDeliveryIsSuppressedAsync()
    {
        var gate = new HostRequestResponseDeliveryGate("request-sequential");
        var calls = 0;
        string? delivered = null;

        var first = await gate.TryDeliverAsync(
            "first-result",
            (response, _) =>
            {
                Interlocked.Increment(ref calls);
                delivered = response;
                return Task.CompletedTask;
            }).ConfigureAwait(false);

        var second = await gate.TryDeliverAsync(
            "duplicate-result",
            (response, _) =>
            {
                Interlocked.Increment(ref calls);
                delivered = response;
                return Task.CompletedTask;
            }).ConfigureAwait(false);

        Assert(first, "The first response-path handoff must claim delivery.");
        Assert(!second, "A sequential duplicate handoff must be suppressed.");
        Assert(calls == 1 && delivered == "first-result", "Only the first result may reach the response path.");
        Assert(gate.HasClaimedDelivery, "A successful handoff must leave the gate claimed.");
    }

    private static async Task ConcurrentDeliveryRaceInvokesCallbackOnceAsync()
    {
        const int attemptCount = 64;
        var gate = new HostRequestResponseDeliveryGate("request-concurrent");
        var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var callbackCalls = 0;

        var attempts = Enumerable.Range(0, attemptCount)
            .Select(async value =>
            {
                await start.Task.ConfigureAwait(false);
                return await gate.TryDeliverAsync(
                    value,
                    async (_, cancellationToken) =>
                    {
                        Interlocked.Increment(ref callbackCalls);
                        await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken).ConfigureAwait(false);
                    }).ConfigureAwait(false);
            })
            .ToArray();

        start.TrySetResult(true);
        var results = await Task.WhenAll(attempts).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(results.Count(delivered => delivered) == 1, "Exactly one concurrent caller may claim delivery.");
        Assert(callbackCalls == 1, "The response-path callback must run once under a concurrent race.");
        Assert(gate.HasClaimedDelivery, "A concurrent successful claim must be observable.");
    }

    private static async Task AmbiguousFailureIsNotRetriedAsync()
    {
        var gate = new HostRequestResponseDeliveryGate("request-ambiguous-send");
        var callbackCalls = 0;
        var failed = false;

        try
        {
            await gate.TryDeliverAsync(
                "possibly-sent",
                (_, _) =>
                {
                    Interlocked.Increment(ref callbackCalls);
                    throw new InvalidOperationException("Simulated ambiguous response-path failure.");
                }).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception) when (
            exception.Message == "Simulated ambiguous response-path failure.")
        {
            failed = true;
        }

        var retried = await gate.TryDeliverAsync(
            "retry-must-not-send",
            (_, _) =>
            {
                Interlocked.Increment(ref callbackCalls);
                return Task.CompletedTask;
            }).ConfigureAwait(false);

        Assert(failed, "The initial response-path failure must propagate to its caller.");
        Assert(!retried, "A claimed response path must not be retried after an ambiguous failure.");
        Assert(callbackCalls == 1, "A failed callback must not be invoked again and risk duplicate delivery.");
        Assert(gate.HasClaimedDelivery, "An ambiguous failure must leave the delivery claim in place.");
    }

    private static async Task PreCancelledCallDoesNotClaimTheGateAsync()
    {
        var gate = new HostRequestResponseDeliveryGate("request-cancelled-before-handoff");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = false;

        try
        {
            await gate.TryDeliverAsync(
                "not-sent",
                (_, _) => Task.CompletedTask,
                cancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }

        Assert(cancelled, "A pre-cancelled handoff must surface cancellation.");
        Assert(!gate.HasClaimedDelivery, "Cancellation before claiming must leave the response gate available.");

        var delivered = await gate.TryDeliverAsync(
            "valid-result",
            (_, _) => Task.CompletedTask).ConfigureAwait(false);
        Assert(delivered, "A later valid handoff must succeed when the cancelled call never claimed the gate.");
    }

    private static void InvalidRequestIdIsRejected()
    {
        var rejected = false;
        try
        {
            _ = new HostRequestResponseDeliveryGate("  ");
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        Assert(rejected, "The response gate must require a non-empty request ID.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
