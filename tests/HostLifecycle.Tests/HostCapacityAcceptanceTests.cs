using WhiteP.Host.Domain;

internal static class HostCapacityAcceptanceTests
{
    public static void Run()
    {
        AssertState("Low RAM", Build(sufficientMemory: false), HostCapacityState.Insufficient);
        AssertState("Insufficient disk space", Build(sufficientDisk: false), HostCapacityState.Insufficient);
        AssertState("Unsupported inference backend", Build(backendSupported: false), HostCapacityState.Insufficient);

        // A known capacity failure must not be hidden by other unknown checks.
        AssertState("Low RAM with other checks unknown", new HostCapabilities { HasSufficientMemory = false }, HostCapacityState.Insufficient);
        AssertState("Insufficient disk with other checks unknown", new HostCapabilities { HasSufficientDiskSpace = false }, HostCapacityState.Insufficient);
        AssertState("Unsupported backend with other checks unknown", new HostCapabilities { IsInferenceBackendSupported = false }, HostCapacityState.Insufficient);

        AssertState("Missing checks and measurements", new HostCapabilities(), HostCapacityState.Unknown);
        AssertState("Missing disk check", Build() with { HasSufficientDiskSpace = null }, HostCapacityState.Unknown);
        AssertState("Inconsistent available RAM", Build() with { AvailablePhysicalMemoryBytes = 17UL * GiB }, HostCapacityState.Unknown);

        Console.WriteLine("PASS: low-RAM, low-disk, unsupported-backend, and unknown-capacity acceptance tests.");
    }

    private const ulong GiB = 1024UL * 1024UL * 1024UL;

    private static HostCapabilities Build(
        bool? sufficientMemory = true,
        bool? sufficientDisk = true,
        bool? backendSupported = true) => new()
    {
        OperatingSystemDescription = "Test host",
        OperatingSystemVersion = new Version(10, 0),
        LogicalProcessorCount = 8,
        DisplayAdapters = [new HostDisplayAdapterCapability("DISPLAY1", "Test display adapter", true)],
        TotalPhysicalMemoryBytes = 16UL * GiB,
        AvailablePhysicalMemoryBytes = 8UL * GiB,
        SystemDriveTotalBytes = 500L * 1024L * 1024L * 1024L,
        SystemDriveAvailableBytes = 100L * 1024L * 1024L * 1024L,
        HasSufficientMemory = sufficientMemory,
        HasSufficientDiskSpace = sufficientDisk,
        IsInferenceBackendSupported = backendSupported
    };

    private static void AssertState(string scenario, HostCapabilities capabilities, HostCapacityState expected)
    {
        if (capabilities.CapacityState != expected)
        {
            throw new InvalidOperationException($"{scenario}: expected {expected}, got {capabilities.CapacityState}.");
        }

        if (expected != HostCapacityState.Ready && capabilities.CanRunInference)
        {
            throw new InvalidOperationException($"{scenario}: host must not advertise that inference can run.");
        }
    }
}
