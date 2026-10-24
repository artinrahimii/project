using WhiteP.Host.Application;
using WhiteP.Host.Infrastructure;

namespace WhiteP.Host.Windows.Inference;

/// <summary>
/// Composition boundary for choosing the host's local inference backend.
/// Conversation, lifecycle, readiness and request-validation logic depend on IInferenceBackend,
/// not on the concrete llama.cpp HTTP adapter.
/// </summary>
public static class LocalInferenceBackendFactory
{
    public static IInferenceBackend Create(Uri serverAddress, string modelId)
    {
        ArgumentNullException.ThrowIfNull(serverAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        return new LlamaCppServerBackend(serverAddress, modelId);
    }
}
