using WhiteP.Host.Domain;

namespace WhiteP.Host.Windows.Requests;

public sealed record HostRequestAdmissionContext(
    bool HasActiveParticipation,
    HostState HostState,
    string? RequestedModelId,
    string? ReadyModelId,
    HostCapacityState CapacityState,
    ulong DeadlineUnixMs);

public enum HostRequestAdmissionFailure
{
    None,
    InactiveParticipation,
    ModelNotReady,
    ModelMismatch,
    CapacityUnknown,
    CapacityInsufficient,
    InvalidDeadline,
    DeadlineExpired
}

public sealed record HostRequestAdmissionDecision(
    bool IsAdmitted,
    HostRequestAdmissionFailure Failure,
    string Message)
{
    public static HostRequestAdmissionDecision Accepted() =>
        new(true, HostRequestAdmissionFailure.None, "Request admission checks passed.");

    public static HostRequestAdmissionDecision Rejected(
        HostRequestAdmissionFailure failure,
        string message) =>
        new(false, failure, message);
}

/// <summary>
/// Fail-closed preflight for active participation, model readiness, capacity and deadline.
/// The caller supplies current state from the live session/backend; missing or inconsistent
/// values must not be interpreted as Ready.
/// </summary>
public static class HostRequestAdmissionPolicy
{
    public static HostRequestAdmissionDecision Evaluate(
        HostRequestAdmissionContext context,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveParticipation)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.InactiveParticipation,
                "Request rejected: no active participation is attached to this session.");
        }

        if (context.HostState != HostState.Ready)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.ModelNotReady,
                "Request rejected: the host/model session is not Ready.");
        }

        if (string.IsNullOrWhiteSpace(context.RequestedModelId) ||
            string.IsNullOrWhiteSpace(context.ReadyModelId) ||
            !string.Equals(context.RequestedModelId, context.ReadyModelId, StringComparison.Ordinal))
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.ModelMismatch,
                "Request rejected: the requested model does not match the ready model.");
        }

        if (context.CapacityState == HostCapacityState.Unknown)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.CapacityUnknown,
                "Request rejected: host capacity is unknown.");
        }

        if (context.CapacityState != HostCapacityState.Ready)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.CapacityInsufficient,
                "Request rejected: verified host capacity is insufficient.");
        }

        return EvaluateDeadline(context.DeadlineUnixMs, nowUtc);
    }

    public static HostRequestAdmissionDecision EvaluateDeadline(
        ulong deadlineUnixMs,
        DateTimeOffset nowUtc)
    {
        var nowUnixMs = nowUtc.ToUnixTimeMilliseconds();
        var maximumDeadlineUnixMs = (ulong)DateTimeOffset.MaxValue.ToUnixTimeMilliseconds();
        if (deadlineUnixMs == 0 ||
            deadlineUnixMs > maximumDeadlineUnixMs ||
            nowUnixMs < 0)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.InvalidDeadline,
                "Request rejected: deadline is missing or invalid.");
        }

        if (deadlineUnixMs <= (ulong)nowUnixMs)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.DeadlineExpired,
                "Request rejected: deadline expired.");
        }

        return HostRequestAdmissionDecision.Accepted();
    }
}
