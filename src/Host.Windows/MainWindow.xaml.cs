using System.IO;
using System.Globalization;
using System.Windows;
using WhiteP.Host.Application;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly LocalHostSettingsStore _settingsStore = new();
    private LocalHostSettings _settings = LocalHostSettings.Defaults;
    private readonly HostInferenceSession _session;
    private string? _requestedModelMemoryRequirementText;
    private string? _requestedMinimumFreeDiskRequirementText;
    private CancellationTokenSource? _operationCancellation;
    private bool _capacityReady;

    public MainWindow()
    {
        InitializeComponent();
        _session = new HostInferenceSession(
            backendFactory: static (address, model) => new LlamaCppServerBackend(address, model),
            readinessEvaluator: EvaluateHostReadinessAsync);
        var settingsLoadFailed = false;
        try
        {
            _settings = _settingsStore.LoadOrDefault();
        }
        catch (InvalidDataException)
        {
            settingsLoadFailed = true;
        }

        ServerUrl.Text = _settings.ServerUrl;
        ModelId.Text = _settings.ModelId;
        RenderStatus(_session.Status);
        RenderHostHardwareSummary();
        RenderDisplayAdapters();
        BackendCapabilityStatus.Text = "Backend: not checked; Connect will test the configured local llama.cpp endpoint.";
        if (settingsLoadFailed)
        {
            LifecycleState.Text = "Error — local settings are invalid; correct the settings file before reconnecting.";
        }
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        _requestedModelMemoryRequirementText = ModelMemoryRequirementGiB.Text;
        _requestedMinimumFreeDiskRequirementText = MinimumFreeDiskGiB.Text;
        ConnectButton.IsEnabled = false;
        GenerateButton.IsEnabled = false;
        try
        {
            if (!Uri.TryCreate(ServerUrl.Text.Trim(), UriKind.Absolute, out var endpoint) ||
                (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
            {
                LifecycleState.Text = "Error — enter a valid absolute HTTP(S) URL.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ModelId.Text))
            {
                LifecycleState.Text = "Error — enter the model ID configured in the local server.";
                return;
            }

            try
            {
                _settings = _settingsStore.Save(new LocalHostSettings(
                    endpoint.ToString(),
                    ModelId.Text,
                    _settings.RequestTimeoutSeconds));
            }
            catch (ArgumentException exception)
            {
                LifecycleState.Text = $"Error — {HostErrorPresentation.ToUserMessage(exception)}";
                return;
            }

            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            var connectTask = _session.ConnectAsync(new Uri(_settings.ServerUrl), _settings.ModelId, _operationCancellation.Token);
            RenderStatus(_session.Status);
            var status = await connectTask;
            RenderStatus(status);
            RenderHostHardwareSummary();
            GenerateButton.IsEnabled = status.State == HostState.Ready && _capacityReady;
        }
        catch (OperationCanceledException)
        {
            RenderStatus(_session.Status);
            LifecycleState.Text = "Connection cancelled.";
        }
        finally
        {
            ConnectButton.IsEnabled = true;
        }
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PromptInput.Text))
        {
            LifecycleState.Text = "Error — enter a prompt first.";
            return;
        }

        _requestedModelMemoryRequirementText = ModelMemoryRequirementGiB.Text;
        _requestedMinimumFreeDiskRequirementText = MinimumFreeDiskGiB.Text;
        GenerateButton.IsEnabled = false;
        ConnectButton.IsEnabled = false;
        OutputText.Clear();
        try
        {
            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            var generationTask = _session.GenerateAsync(PromptInput.Text, 1024, _operationCancellation.Token);
            RenderStatus(_session.Status);
            OutputText.Text = await generationTask;
            RenderStatus(_session.Status);
        }
        catch (Exception exception)
        {
            RenderStatus(_session.Status);
            OutputText.Text = HostErrorPresentation.ToUserMessage(exception);
        }
        finally
        {
            RenderHostHardwareSummary();
            ConnectButton.IsEnabled = true;
            GenerateButton.IsEnabled = _session.IsConnected && _capacityReady;
        }
    }

    private void RenderStatus(HostStatus status)
    {
        var capacityState = RenderCapacityStatus(status);
        LifecycleState.Text = status.State == HostState.Ready && capacityState != HostCapacityState.Ready
            ? capacityState == HostCapacityState.Insufficient
                ? "Not Ready — measured host capacity is insufficient."
                : "Unknown — host capacity could not be verified; the host is not Ready."
            : HostStatusPresentation.FormatBackendStatus(status);
        NetworkState.Text = HostStatusPresentation.FormatNetworkStatus(status);
        BackendCapabilityStatus.Text = status.State switch
        {
            HostState.Ready => "Backend: available — the configured local llama.cpp endpoint passed its health check.",
            HostState.Error => "Backend: unavailable or not ready — " + status.Message,
            HostState.Starting => "Backend: checking the configured local endpoint…",
            HostState.Busy => "Backend: available — one local inference request is running.",
            _ => "Backend: not checked or disconnected."
        };
        GenerateButton.IsEnabled = status.State == HostState.Ready && capacityState == HostCapacityState.Ready;
    }

    private HostCapacityState RenderCapacityStatus(HostStatus status)
    {
        try
        {
            var hardware = WindowsHostHardwareDiscovery.Discover();
            var capacity = new HostCapabilities
            {
                OperatingSystemDescription = hardware.OperatingSystemDescription,
                OperatingSystemVersion = hardware.OperatingSystemVersion,
                LogicalProcessorCount = hardware.LogicalProcessorCount,
                TotalPhysicalMemoryBytes = hardware.TotalPhysicalMemoryBytes,
                AvailablePhysicalMemoryBytes = hardware.AvailablePhysicalMemoryBytes,
                SystemDriveTotalBytes = hardware.SystemDriveTotalBytes,
                SystemDriveAvailableBytes = hardware.SystemDriveAvailableBytes,
                HasSufficientMemory = CheckMemoryRequirement(ModelMemoryRequirementGiB.Text, hardware.AvailablePhysicalMemoryBytes),
                HasSufficientDiskSpace = CheckDiskRequirement(MinimumFreeDiskGiB.Text, hardware.SystemDriveAvailableBytes),
                IsInferenceBackendSupported = status.State is HostState.Ready or HostState.Busy ? true : (bool?)null
            };

            var state = capacity.CapacityState;
            _capacityReady = state == HostCapacityState.Ready;
            CapacityStatus.Text = state switch
            {
                HostCapacityState.Ready => "Capacity: Ready for the configured local backend and the declared RAM/disk checks. GPU compute/VRAM capability is not verified.",
                HostCapacityState.Insufficient => "Capacity: Insufficient — measured RAM or system-drive space does not meet the declared requirement. Host is not Ready.",
                _ => "Capacity: Unknown — required hardware measurements or checks are unavailable/unverified. Host is not Ready."
            };
            return state;
        }
        catch (Exception)
        {
            _capacityReady = false;
            CapacityStatus.Text = "Capacity: Unknown — live hardware capacity could not be verified; host is not Ready.";
            return HostCapacityState.Unknown;
        }
    }

    private static bool? CheckMemoryRequirement(string? requirementText, ulong availableMemoryBytes)
    {
        if (!TryParseCapacityRequirement(requirementText, out var requiredGiB))
        {
            return null;
        }

        var requiredBytes = checked((ulong)Math.Ceiling(requiredGiB * 1024d * 1024d * 1024d));
        return requiredBytes <= availableMemoryBytes;
    }

    private static bool? CheckDiskRequirement(string? requirementText, long availableDiskBytes)
    {
        if (availableDiskBytes < 0 || !TryParseCapacityRequirement(requirementText, out var requiredGiB))
        {
            return null;
        }

        var requiredBytes = checked((long)Math.Ceiling(requiredGiB * 1024d * 1024d * 1024d));
        return requiredBytes <= availableDiskBytes;
    }

    private static bool TryParseCapacityRequirement(string? requirementText, out double requiredGiB)
    {
        return double.TryParse(
                requirementText?.Trim(),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out requiredGiB) &&
            double.IsFinite(requiredGiB) &&
            requiredGiB > 0 &&
            requiredGiB <= 1_048_576;
    }

    private void RenderHostHardwareSummary()
    {
        try
        {
            var hardware = WindowsHostHardwareDiscovery.Discover();
            const double gib = 1024d * 1024d * 1024d;
            CpuStatus.Text = $"CPU: {hardware.ProcessorName}; {hardware.LogicalProcessorCount} logical processors available to this process.";
            MemoryStatus.Text = $"Physical memory: {(hardware.AvailablePhysicalMemoryBytes / gib).ToString("F2", CultureInfo.InvariantCulture)} GiB available of {(hardware.TotalPhysicalMemoryBytes / gib).ToString("F2", CultureInfo.InvariantCulture)} GiB total.";
        }
        catch (Exception)
        {
            CpuStatus.Text = "CPU: processor details could not be verified.";
            MemoryStatus.Text = "Physical memory: live measurements are unavailable.";
        }
    }

    private void RenderDisplayAdapters()
    {
        try
        {
            var adapters = WindowsHostHardwareDiscovery.DiscoverDisplayAdapters();
            GpuStatus.Text = adapters.Count == 0
                ? "GPU/display adapters: Windows reported no display adapters."
                : "GPU/display adapters (display inventory only; compute support is not inferred): " +
                  string.Join("; ", adapters.Select(adapter => adapter.Description + (adapter.AttachedToDesktop ? " (active)" : " (not attached)")));
        }
        catch (Exception)
        {
            GpuStatus.Text = "GPU/display adapters: discovery failed; compute capability is unknown.";
        }
    }


    private Task<HostReadinessResult> EvaluateHostReadinessAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!double.TryParse(
                _requestedModelMemoryRequirementText?.Trim(),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var requiredModelMemoryGiB) ||
            !double.IsFinite(requiredModelMemoryGiB) ||
            requiredModelMemoryGiB <= 0 ||
            requiredModelMemoryGiB > 1_048_576)
        {
            return Task.FromResult(new HostReadinessResult(
                false,
                "Host is not Ready: enter a positive declared model RAM requirement in GiB."));
        }

        if (!double.TryParse(
                _requestedMinimumFreeDiskRequirementText?.Trim(),
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var requiredFreeDiskGiB) ||
            !double.IsFinite(requiredFreeDiskGiB) ||
            requiredFreeDiskGiB <= 0 ||
            requiredFreeDiskGiB > 1_048_576)
        {
            return Task.FromResult(new HostReadinessResult(
                false,
                "Host is not Ready: enter a positive minimum free system-drive requirement in GiB."));
        }

        try
        {
            var hardware = WindowsHostHardwareDiscovery.Discover();
            return Task.FromResult(HostReadinessEvaluator.Evaluate(
                requiredModelMemoryGiB,
                hardware.AvailablePhysicalMemoryBytes,
                requiredFreeDiskGiB,
                hardware.SystemDriveAvailableBytes));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Task.FromResult(new HostReadinessResult(
                false,
                "Host is not Ready: current hardware capacity could not be verified."));
        }
    }

    private void EstimateModelCompatibility_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(ModelMemoryRequirementGiB.Text.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var requiredGiB) ||
            !double.IsFinite(requiredGiB) || requiredGiB <= 0 || requiredGiB > 1_048_576)
        {
            ModelCompatibilityStatus.Text = "Enter a positive model memory requirement in GiB using a decimal point (for example, 6.5).";
            return;
        }
        try
        {
            var hardware = WindowsHostHardwareDiscovery.Discover();
            var requiredBytes = checked((long)Math.Ceiling(requiredGiB * 1024d * 1024d * 1024d));
            var result = ModelMemoryCompatibilityEstimator.Estimate(requiredBytes, hardware.AvailablePhysicalMemoryBytes);
            var availableGiB = hardware.AvailablePhysicalMemoryBytes / (1024d * 1024d * 1024d);
            ModelCompatibilityStatus.Text = $"{result.Message} Available RAM now: {availableGiB:F2} GiB; declared model requirement: {requiredGiB:F2} GiB.";
        }
        catch (Exception)
        {
            ModelCompatibilityStatus.Text = "Compatibility estimate unavailable: Windows could not report current hardware memory.";
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _operationCancellation?.Cancel();
        _operationCancellation?.Dispose();
        _session.Dispose();
    }
}
