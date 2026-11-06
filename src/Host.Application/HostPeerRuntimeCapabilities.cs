using WhiteP.Host.Domain;

namespace WhiteP.Host.Application;

/// <summary>
/// Model limits verified from the active inference backend, not inferred from catalog metadata.
/// MaxContextTokens is the backend's reported context window; MaxOutputTokens is bounded by both
/// that window and the shared protocol ceiling.
/// </summary>
public sealed record InferenceModelCapabilitySnapshot(
    string ModelId,
    string DisplayName,
    ulong MaxContextTokens,
    ulong MaxOutputTokens,
    bool SupportsStreaming);

/// <summary>
/// Implemented only by backends that can report their active model and runtime limits.
/// A missing snapshot means the peer must not advertise a selectable model.
/// </summary>
public interface IInferenceModelCapabilityProvider
{
    Task<InferenceModelCapabilitySnapshot?> GetModelCapabilityAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>A fresh host lifecycle status and its optional live backend capability snapshot.</summary>
public sealed record HostPeerRuntimeSnapshot(
    HostStatus Status,
    InferenceModelCapabilitySnapshot? ModelCapability);
