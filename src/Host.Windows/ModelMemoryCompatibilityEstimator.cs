namespace WhiteP.Host.Windows;

public sealed record ModelMemoryCompatibilityResult(bool IsCompatible, long RequiredMemoryBytes, ulong AvailablePhysicalMemoryBytes, string Message);

/// <summary>Compares a declared model memory requirement with currently available Windows physical RAM.</summary>
public static class ModelMemoryCompatibilityEstimator
{
    public static ModelMemoryCompatibilityResult Estimate(long requiredMemoryBytes, ulong availablePhysicalMemoryBytes)
    {
        if (requiredMemoryBytes <= 0) throw new ArgumentOutOfRangeException(nameof(requiredMemoryBytes), "The declared model memory requirement must be positive.");
        var compatible = (ulong)requiredMemoryBytes <= availablePhysicalMemoryBytes;
        var message = compatible
            ? "Likely compatible with currently available physical RAM. This estimate does not validate GPU/VRAM, context length, model format, or backend-specific overhead."
            : "Insufficient currently available physical RAM for the declared requirement. GPU/VRAM is not evaluated.";
        return new ModelMemoryCompatibilityResult(compatible, requiredMemoryBytes, availablePhysicalMemoryBytes, message);
    }
}
