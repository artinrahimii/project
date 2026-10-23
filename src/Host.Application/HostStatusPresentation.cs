using WhiteP.Host.Domain;

namespace WhiteP.Host.Application;

/// <summary>
/// Formats the most recently observed backend session state for the Windows status panel.
/// It never claims that the not-yet-implemented direct peer transport is connected.
/// </summary>
public static class HostStatusPresentation
{
    public static string FormatBackendStatus(HostStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        return $"Backend — {status.State}: {status.Message}";
    }

    public static string FormatNetworkStatus(HostStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        var endpoint = status.State switch
        {
            HostState.Stopped => "Local endpoint: not checked.",
            HostState.Starting => "Local endpoint: checking backend health.",
            HostState.Ready => "Local endpoint: healthy; backend health check passed.",
            HostState.Busy => "Local endpoint: previously healthy; inference is in progress.",
            HostState.Error => "Local endpoint/backend status: error; see backend status above.",
            _ => "Local endpoint: status unknown."
        };
        return $"{endpoint} Direct peer-to-peer transport: not implemented in this build.";
    }
}
