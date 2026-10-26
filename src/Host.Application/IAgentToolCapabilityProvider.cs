namespace WhiteP.Host.Application;

/// <summary>Unknown is fail-closed; text health and license metadata do not prove structured tool-call support.</summary>
public enum AgentToolCapabilitySupport { Unknown = 0, Unsupported = 1, Supported = 2 }

/// <summary>A time-bounded observation for one selected backend/model and its structured tool-call protocol.</summary>
public sealed record AgentToolCallingCapabilityReport(
    string BackendId, string ModelId,
    AgentToolCapabilitySupport BackendSupport,
    AgentToolCapabilitySupport ModelSupport,
    AgentToolCapabilitySupport StructuredProtocolSupport,
    DateTimeOffset CheckedAtUtc, string Evidence);

/// <summary>Only reports Supported when actual backend/model/protocol support has been checked.</summary>
public interface IAgentToolCapabilityProvider
{
    Task<AgentToolCallingCapabilityReport> CheckAgentToolCallingCapabilitiesAsync(CancellationToken cancellationToken = default);
}
