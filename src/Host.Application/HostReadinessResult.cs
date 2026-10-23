namespace WhiteP.Host.Application;

/// <summary>
/// Result of checking whether the configured local model and its declared host-RAM requirement are ready.
/// A failed or unknown check must not allow the host to report Ready.
/// </summary>
public sealed record HostReadinessResult(bool IsReady, string Message);
