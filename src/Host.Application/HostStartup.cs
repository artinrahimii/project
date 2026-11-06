using WhiteP.Host.Domain;

namespace WhiteP.Host.Application;

public sealed class HostStartup(HostLifecycle lifecycle, IInferenceBackend backend)
{
    public HostStatus Start()
    {
        lifecycle.Transition(HostState.Starting, "Checking local inference backend.");
        try
        {
            if (backend.IsAvailable)
            {
                return lifecycle.Transition(HostState.Ready, "Local inference backend is ready.");
            }

            var reason = backend.UnavailableReason;
            return lifecycle.Transition(
                HostState.Error,
                string.IsNullOrWhiteSpace(reason)
                    ? "Local inference backend is unavailable. Host is not ready."
                    : reason.Trim());
        }
        catch (Exception)
        {
            return lifecycle.Transition(HostState.Error, "Local inference backend check failed. Host is not ready.");
        }
    }
}
