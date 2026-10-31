namespace WhiteP.Host.Application;

/// <summary>
/// Fail-closed readiness policy for the declared model RAM requirement and minimum free system-drive space.
/// The caller must also have passed the configured inference backend health check.
/// </summary>
public static class HostReadinessEvaluator
{
    private const double MaximumDeclaredGiB = 1_048_576d;
    private const double BytesPerGiB = 1024d * 1024d * 1024d;

    public static HostReadinessResult Evaluate(
        double requiredModelMemoryGiB,
        ulong availablePhysicalMemoryBytes,
        double requiredFreeDiskGiB,
        long availableSystemDriveBytes)
    {
        if (!IsValidRequirement(requiredModelMemoryGiB))
        {
            return new HostReadinessResult(
                false,
                "Host is not Ready: enter a positive declared model RAM requirement in GiB.");
        }

        if (!IsValidRequirement(requiredFreeDiskGiB))
        {
            return new HostReadinessResult(
                false,
                "Host is not Ready: enter a positive minimum free system-drive requirement in GiB.");
        }

        if (availableSystemDriveBytes < 0)
        {
            return new HostReadinessResult(
                false,
                "Host is not Ready: current system-drive capacity could not be verified.");
        }

        var requiredMemoryBytes = checked((ulong)Math.Ceiling(requiredModelMemoryGiB * BytesPerGiB));
        if (requiredMemoryBytes > availablePhysicalMemoryBytes)
        {
            return new HostReadinessResult(
                false,
                "Host is not Ready: insufficient currently available physical RAM for the declared model requirement.");
        }

        var requiredDiskBytes = checked((long)Math.Ceiling(requiredFreeDiskGiB * BytesPerGiB));
        if (requiredDiskBytes > availableSystemDriveBytes)
        {
            return new HostReadinessResult(
                false,
                "Host is not Ready: free disk space on the Windows system drive is below the declared minimum.");
        }

        return new HostReadinessResult(
            true,
            "Local backend health passed; declared model RAM and minimum free system-drive capacity checks passed.");
    }

    private static bool IsValidRequirement(double requiredGiB) =>
        double.IsFinite(requiredGiB) &&
        requiredGiB > 0 &&
        requiredGiB <= MaximumDeclaredGiB;
}
