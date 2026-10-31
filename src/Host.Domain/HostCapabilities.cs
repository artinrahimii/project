namespace WhiteP.Host.Domain;

/// <summary>
/// The state of the host's capacity checks. Unknown is deliberately not equivalent to Ready.
/// </summary>
public enum HostCapacityState
{
    Unknown,
    Insufficient,
    Ready
}

/// <summary>A platform-neutral display-adapter record; inventory does not imply compute support.</summary>
public sealed record HostDisplayAdapterCapability(
    string DeviceName,
    string Description,
    bool AttachedToDesktop);

/// <summary>
/// Platform-neutral hardware measurements and explicit readiness checks for the local inference host.
/// Unknown measurements remain unknown; the domain model never infers readiness from missing data.
/// </summary>
public sealed record HostCapabilities
{
    public string? OperatingSystemDescription { get; init; }

    public Version? OperatingSystemVersion { get; init; }

    public int? LogicalProcessorCount { get; init; }

    public IReadOnlyList<HostDisplayAdapterCapability>? DisplayAdapters { get; init; }

    public ulong? TotalPhysicalMemoryBytes { get; init; }

    public ulong? AvailablePhysicalMemoryBytes { get; init; }

    public long? SystemDriveTotalBytes { get; init; }

    public long? SystemDriveAvailableBytes { get; init; }

    /// <summary>True/false only after checking the configured model requirement against measured RAM.</summary>
    public bool? HasSufficientMemory { get; init; }

    /// <summary>True/false only after checking the applicable model/storage requirement.</summary>
    public bool? HasSufficientDiskSpace { get; init; }

    /// <summary>True/false only after identifying and checking an inference backend.</summary>
    public bool? IsInferenceBackendSupported { get; init; }

    public HostCapacityState CapacityState
    {
        get
        {
            if (HasSufficientMemory is false ||
                HasSufficientDiskSpace is false ||
                IsInferenceBackendSupported is false)
            {
                return HostCapacityState.Insufficient;
            }

            if (HasSufficientMemory is not true ||
                HasSufficientDiskSpace is not true ||
                IsInferenceBackendSupported is not true ||
                !HasValidPhysicalMemoryMeasurements ||
                !HasValidSystemDriveMeasurements)
            {
                return HostCapacityState.Unknown;
            }

            return HostCapacityState.Ready;
        }
    }

    /// <summary>Only a fully measured host with every required check passing may run inference.</summary>
    public bool CanRunInference => CapacityState == HostCapacityState.Ready;

    private bool HasValidPhysicalMemoryMeasurements =>
        TotalPhysicalMemoryBytes is ulong totalMemory &&
        totalMemory > 0 &&
        AvailablePhysicalMemoryBytes is ulong availableMemory &&
        availableMemory <= totalMemory;

    private bool HasValidSystemDriveMeasurements =>
        SystemDriveTotalBytes is long totalDrive &&
        totalDrive > 0 &&
        SystemDriveAvailableBytes is long availableDrive &&
        availableDrive >= 0 &&
        availableDrive <= totalDrive;
}
