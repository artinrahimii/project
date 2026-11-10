using WhiteP.Host.Domain;
using WhiteP.Host.Windows.Requests;

internal static class HostRequestAdmissionTests
{
    public static async Task RunAsync()
    {
        var now = DateTimeOffset.FromUnixTimeMilliseconds(1_800_000_000_000);
        var valid = new HostRequestAdmissionContext(
            true,
            HostState.Ready,
            "fixture/model-a",
            "fixture/model-a",
            HostCapacityState.Ready,
            1024,
            4096,
            8192,
            (ulong)now.AddMinutes(1).ToUnixTimeMilliseconds());

        Check(valid, now, HostRequestAdmissionFailure.None);
        Check(valid with { HostState = HostState.Error, CapacityState = HostCapacityState.Insufficient }, now,
            HostRequestAdmissionFailure.CapacityInsufficient);
        Check(valid with { HasActiveParticipation = false }, now, HostRequestAdmissionFailure.InactiveParticipation);
        Check(valid with { HostState = HostState.Busy }, now, HostRequestAdmissionFailure.ModelNotReady);
        Check(valid with { HostState = HostState.Stopped }, now, HostRequestAdmissionFailure.ModelNotReady);
        Check(valid with { RequestedModelId = "fixture/model-b" }, now, HostRequestAdmissionFailure.ModelMismatch);
        Check(valid with { ReadyModelId = null }, now, HostRequestAdmissionFailure.ModelMismatch);
        Check(valid with { ReadyModelMaxOutputTokens = 0 }, now, HostRequestAdmissionFailure.InvalidModelCapability);
        Check(valid with { ReadyModelMaxContextTokens = 0 }, now, HostRequestAdmissionFailure.InvalidModelCapability);
        Check(valid with { ReadyModelMaxOutputTokens = 8193 }, now, HostRequestAdmissionFailure.InvalidModelCapability);
        Check(valid with { RequestedMaxOutputTokens = 0 }, now, HostRequestAdmissionFailure.InvalidOutputTokenLimit);
        Check(valid with { RequestedMaxOutputTokens = 4097 }, now, HostRequestAdmissionFailure.OutputTokenLimitExceeded);
        Check(valid with { RequestedMaxOutputTokens = 2049, ReadyModelMaxOutputTokens = 2048 }, now,
            HostRequestAdmissionFailure.OutputTokenLimitExceeded);
        Check(valid with { CapacityState = HostCapacityState.Unknown }, now, HostRequestAdmissionFailure.CapacityUnknown);
        Check(valid with { CapacityState = HostCapacityState.Insufficient }, now, HostRequestAdmissionFailure.CapacityInsufficient);
        Check(valid with { DeadlineUnixMs = 0 }, now, HostRequestAdmissionFailure.InvalidDeadline);
        Check(valid with { DeadlineUnixMs = (ulong)now.ToUnixTimeMilliseconds() }, now, HostRequestAdmissionFailure.DeadlineExpired);
        Check(valid with { DeadlineUnixMs = (ulong)DateTimeOffset.MaxValue.ToUnixTimeMilliseconds() + 1UL }, now,
            HostRequestAdmissionFailure.InvalidDeadline);

        var coordinator = new HostRequestCoordinator();
        var invoked = false;
        var rejected = await coordinator.TryExecuteAsync(
            valid with { HasActiveParticipation = false },
            _ => { invoked = true; return Task.FromResult("no"); });
        Assert(rejected.Status == HostRequestStatus.Rejected &&
               rejected.AdmissionFailure == HostRequestAdmissionFailure.InactiveParticipation &&
               !invoked,
            "Inactive participation must reject before invocation.");

        var capacityRejected = false;
        try
        {
            await coordinator.ExecuteAsync(
                valid with { CapacityState = HostCapacityState.Unknown },
                _ => { invoked = true; return Task.FromResult("no"); });
        }
        catch (HostRequestAdmissionException exception)
        {
            capacityRejected = exception.Failure == HostRequestAdmissionFailure.CapacityUnknown;
        }

        Assert(capacityRejected && !invoked, "Unknown capacity must fail closed.");

        var insufficientInvoked = false;
        var insufficient = await coordinator.TryExecuteAsync(
            valid with { CapacityState = HostCapacityState.Insufficient },
            _ => { insufficientInvoked = true; return Task.FromResult("no"); });
        Assert(insufficient.Status == HostRequestStatus.Rejected &&
               insufficient.AdmissionFailure == HostRequestAdmissionFailure.CapacityInsufficient &&
               !insufficientInvoked,
            "Known insufficient capacity must produce a typed rejection before inference.");

        var live = valid with
        {
            RequestedMaxOutputTokens = 1024,
            DeadlineUnixMs = (ulong)DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeMilliseconds()
        };
        Assert(await coordinator.ExecuteAsync(live, _ => Task.FromResult("accepted")) == "accepted",
            "Valid model identity, limits and admission state should pass.");
        Console.WriteLine("PASS: admission validates participation, exact model identity, live model context/output ceilings, product/protocol output limits, capacity and deadlines before inference.");
    }

    private static void Check(
        HostRequestAdmissionContext context,
        DateTimeOffset now,
        HostRequestAdmissionFailure expected)
    {
        var decision = HostRequestAdmissionPolicy.Evaluate(context, now);
        Assert(
            decision.IsAdmitted == (expected == HostRequestAdmissionFailure.None) &&
            decision.Failure == expected,
            $"Expected {expected}, actual {decision.Failure}.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
