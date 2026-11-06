namespace WhiteP.Host.Application.Inference;

/// <summary>
/// The result of an actual runtime discovery attempt for one configured inference backend.
/// Availability must be based on an observation, not on the existence of configuration alone.
/// </summary>
/// <param name="BackendId">A backend identifier suitable for diagnostics; not a user identity.</param>
/// <param name="DisplayName">A user-safe name for the backend.</param>
/// <param name="IsAvailable">Whether the backend was observed to be reachable and usable.</param>
/// <param name="UnavailableReason">A safe explanation when the backend is not available; must not contain prompts, responses, credentials, or secrets.</param>
public sealed record InferenceEngineDiscoveryResult(
    string BackendId,
    string DisplayName,
    bool IsAvailable,
    string? UnavailableReason);

/// <summary>
/// Application-layer contract for discovering and operating a local inference engine.
/// Implementations must not report a model as loaded until their real backend confirms readiness.
/// Long-running operations must observe their cancellation token and must not log prompt or response content.
/// </summary>
public interface IInferenceEngine : IAsyncDisposable
{
    /// <summary>Checks the configured backend using a real runtime probe.</summary>
    Task<InferenceEngineDiscoveryResult> DiscoverAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads or selects the requested model and confirms it is usable.
    /// This operation must fail explicitly when the backend cannot confirm the selected model.
    /// </summary>
    Task LoadModelAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>Generates text from the loaded model; implementations must honor cancellation and token bounds.</summary>
    Task<string> GenerateAsync(string prompt, uint maxOutputTokens, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests cancellation of the currently active generation. The operation should be safe when no generation is active.
    /// It does not replace passing a CancellationToken to each long-running operation.
    /// </summary>
    Task CancelAsync();

    /// <summary>Unloads the selected model and releases model-specific resources when supported by the backend.</summary>
    Task UnloadModelAsync(CancellationToken cancellationToken = default);
}
