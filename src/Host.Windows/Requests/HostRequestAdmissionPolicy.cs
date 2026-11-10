using WhiteP.Host.Domain;
using WhiteP.Shared.Contracts;

namespace WhiteP.Host.Windows.Requests;

public sealed record HostRequestAdmissionContext(
    bool HasActiveParticipation,
    HostState HostState,
    string? RequestedModelId,
    string? ReadyModelId,
    HostCapacityState CapacityState,
    uint RequestedMaxOutputTokens,
    ulong ReadyModelMaxOutputTokens,
    ulong ReadyModelMaxContextTokens,
    ulong DeadlineUnixMs);

public enum HostRequestAdmissionFailure
{
    None,
    InactiveParticipation,
    ModelNotReady,
    ModelMismatch,
    InvalidModelCapability,
    InvalidOutputTokenLimit,
    OutputTokenLimitExceeded,
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
/// Fail-closed preflight for active participation, live model identity/capability,
/// output-token limits, capacity and deadline. Model limits must come from a fresh backend probe.
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

        // Preserve a known capacity failure even when that same condition has already
        // transitioned the lifecycle to Error. Unknown backend state is still reported below.
        if (context.CapacityState == HostCapacityState.Insufficient)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.CapacityInsufficient,
                "Request rejected: verified host capacity is insufficient.");
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

        if (context.ReadyModelMaxContextTokens == 0 ||
            context.ReadyModelMaxOutputTokens == 0 ||
            context.ReadyModelMaxOutputTokens > context.ReadyModelMaxContextTokens ||
            context.ReadyModelMaxOutputTokens > ProtocolMessageLimits.MaximumOutputTokens)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.InvalidModelCapability,
                "Request rejected: the live model capability is missing or inconsistent.");
        }

        if (context.RequestedMaxOutputTokens == 0)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.InvalidOutputTokenLimit,
                "Request rejected: maximum output tokens must be positive.");
        }

        var allowedOutputTokens = Math.Min(
            Math.Min(context.ReadyModelMaxOutputTokens, context.ReadyModelMaxContextTokens),
            Math.Min(
                (ulong)ProtocolMessageLimits.MaximumProductOutputTokens,
                (ulong)ProtocolMessageLimits.MaximumOutputTokens));

        if (context.RequestedMaxOutputTokens > allowedOutputTokens)
        {
            return HostRequestAdmissionDecision.Rejected(
                HostRequestAdmissionFailure.OutputTokenLimitExceeded,
                "Request rejected: maximum output tokens exceeds the live model or product limit.");
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
