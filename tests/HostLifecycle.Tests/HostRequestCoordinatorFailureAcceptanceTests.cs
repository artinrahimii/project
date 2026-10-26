using WhiteP.Host.Domain;
using WhiteP.Host.Windows.Requests;

internal static class HostRequestCoordinatorFailureAcceptanceTests
{
    public static async Task RunAsync()
    {
        await DuplicateAdmissionIsNotInvokedTwiceAsync().ConfigureAwait(false);
        ExpiredDeadlineIsRejectedBeforeInference();
        await CancellationDuringExecutionWithholdsResultAsync().ConfigureAwait(false);
        await InferenceFailureReleasesCoordinatorAsync().ConfigureAwait(false);

        Console.WriteLine(
            "PASS: duplicate admission, expired deadline, in-flight cancellation, and inference failure are handled without a duplicate or late successful result.");
    }

    private static async Task DuplicateAdmissionIsNotInvokedTwiceAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var admission = ValidAdmission();
        var operationStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseOperation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var operationInvocations = 0;

        var first = coordinator.ExecuteAsync(admission, async cancellationToken =>
        {
            Interlocked.Increment(ref operationInvocations);
            operationStarted.TrySetResult(true);
            await releaseOperation.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return "first";
        });

        try
        {
            await operationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            var duplicate = await coordinator.TryExecuteAsync(admission, _ =>
            {
                Interlocked.Increment(ref operationInvocations);
                return Task.FromResult("duplicate-must-not-run");
            }).WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            Assert(duplicate.Status == HostRequestStatus.Busy && duplicate.Result is null,
                "A repeated admission while the original request is active must be rejected as Busy.");
            Assert(operationInvocations == 1,
                "The repeated request must not invoke the inference operation a second time.");
        }
        finally
        {
            releaseOperation.TrySetResult(true);
        }

        Assert(await first.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false) == "first",
            "The admitted request must retain its successful result.");
        Assert(!coordinator.IsBusy, "The coordinator gate must be released after the original request completes.");
    }

    private static void ExpiredDeadlineIsRejectedBeforeInference()
    {
        var now = DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000);
        var coordinator = new HostRequestCoordinator(() => now);
        var admission = ValidAdmission() with
        {
            DeadlineUnixMs = (ulong)now.AddMilliseconds(-1).ToUnixTimeMilliseconds()
        };
        var inferenceInvoked = false;

        var outcome = coordinator.TryExecuteAsync(admission, _ =>
        {
            inferenceInvoked = true;
            return Task.FromResult("must-not-run");
        }).GetAwaiter().GetResult();

        Assert(outcome.Status == HostRequestStatus.Rejected &&
               outcome.AdmissionFailure == HostRequestAdmissionFailure.DeadlineExpired,
            "An already-expired request must receive the explicit DeadlineExpired rejection.");
        Assert(!inferenceInvoked, "An expired request must not invoke inference.");
        Assert(!coordinator.IsBusy, "A request rejected before admission must not acquire the inference gate.");
    }

    private static async Task CancellationDuringExecutionWithholdsResultAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var operationStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseIgnoringCancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var active = coordinator.ExecuteAsync(ValidAdmission(), async _ =>
        {
            operationStarted.TrySetResult(true);
            // Simulate an engine that completes after cancellation instead of honoring its token.
            await releaseIgnoringCancellation.Task.ConfigureAwait(false);
            return "must-not-be-delivered";
        });

        await operationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(coordinator.IsBusy, "Coordinator must remain Busy while inference is active.");
        Assert(coordinator.CancelActiveRequest(), "Cancellation during inference must signal the active request.");
        releaseIgnoringCancellation.TrySetResult(true);

        var cancellationObserved = false;
        try
        {
            _ = await active.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancellationObserved = true;
        }

        Assert(cancellationObserved,
            "Coordinator must not return a successful result when inference returns after its request was cancelled.");
        Assert(!coordinator.IsBusy, "The inference gate must be released after cancellation.");
        Assert(!coordinator.CancelActiveRequest(), "The active cancellation reference must be cleared after completion.");
    }

    private static async Task InferenceFailureReleasesCoordinatorAsync()
    {
        var coordinator = new HostRequestCoordinator();
        var observedExpectedFailure = false;

        try
        {
            _ = await coordinator.ExecuteAsync<string>(
                ValidAdmission(),
                _ => Task.FromException<string>(new InvalidOperationException("Expected inference engine failure.")))
                .WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        }
        catch (InvalidOperationException exception) when (
            exception is not HostRequestBusyException &&
            exception is not HostRequestAdmissionException &&
            exception.Message == "Expected inference engine failure.")
        {
            observedExpectedFailure = true;
        }

        Assert(observedExpectedFailure, "Inference engine exceptions must propagate to the caller.");
        Assert(!coordinator.IsBusy, "Coordinator must release the inference gate after an inference failure.");
        Assert(!coordinator.CancelActiveRequest(),
            "Coordinator must clear the active cancellation reference after an inference failure.");

        var nextResult = await coordinator.ExecuteAsync(
            ValidAdmission(),
            _ => Task.FromResult("accepted-after-failure")).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        Assert(nextResult == "accepted-after-failure",
            "A new request must be accepted after an inference engine failure.");
    }

    private static HostRequestAdmissionContext ValidAdmission() =>
        new(
            true,
            HostState.Ready,
            "fixture/ready-model",
            "fixture/ready-model",
            HostCapacityState.Ready,
            (ulong)DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds());

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
