using WhiteP.Host.Application;

namespace WhiteP.Host.Windows.Chat;

public sealed record AgentToolCapabilityDecision(bool IsSupported, string Reason);

/// <summary>Fail-closed policy for exposure and execution of model-proposed tools.</summary>
public static class AgentToolCapabilityPolicy
{
    public static TimeSpan MaximumReportAge { get; } = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaximumFutureClockSkew = TimeSpan.FromMinutes(1);

    public static async Task<AgentToolCapabilityDecision> EvaluateAsync(IInferenceBackend? backend, CancellationToken cancellationToken = default)
    {
        if (backend is null) return new(false, "No inference backend is configured.");
        try { if (!backend.IsAvailable) return new(false, "The inference backend is not currently available."); }
        catch (Exception) { return new(false, "Backend availability could not be verified."); }
        if (backend is not IAgentToolCapabilityProvider provider)
            return new(false, "The backend has no explicit tool-calling capability provider.");
        AgentToolCallingCapabilityReport report;
        try { report = await provider.CheckAgentToolCallingCapabilitiesAsync(cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { return new(false, "The backend/model tool-calling capability check failed."); }
        return Evaluate(report, DateTimeOffset.UtcNow);
    }

    public static AgentToolCapabilityDecision Evaluate(AgentToolCallingCapabilityReport? report, DateTimeOffset nowUtc)
    {
        if (report is null || string.IsNullOrWhiteSpace(report.BackendId) ||
            string.IsNullOrWhiteSpace(report.ModelId) || string.IsNullOrWhiteSpace(report.Evidence))
            return new(false, "A complete backend/model capability report with evidence is required.");
        if (report.CheckedAtUtc > nowUtc.Add(MaximumFutureClockSkew) || nowUtc - report.CheckedAtUtc > MaximumReportAge)
            return new(false, "The backend/model capability report is stale or has an invalid timestamp.");
        if (report.BackendSupport != AgentToolCapabilitySupport.Supported ||
            report.ModelSupport != AgentToolCapabilitySupport.Supported ||
            report.StructuredProtocolSupport != AgentToolCapabilitySupport.Supported)
            return new(false, $"Tool calling is not verified for backend '{report.BackendId}' and model '{report.ModelId}'; backend={report.BackendSupport}, model={report.ModelSupport}, protocol={report.StructuredProtocolSupport}.");
        return new(true, "Backend, model, and structured tool-call protocol are explicitly supported.");
    }
}

public sealed class AgentToolCapabilityUnavailableException : InvalidOperationException
{
    public AgentToolCapabilityUnavailableException(string reason)
        : base("Agent-tool execution is disabled because capability support was not verified. " + reason) { }
}
