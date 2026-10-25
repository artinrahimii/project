using System.IO;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using System.Linq;
using WhiteP.Host.Windows.Models.Catalog;
using WhiteP.Host.Application;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Inference;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly LocalHostSettingsStore _settingsStore = new();
    private readonly LocalInferenceProcessLauncher _localInferenceProcessLauncher = new();
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
            backendFactory: LocalInferenceBackendFactory.Create,
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
        EngineExecutablePath.Text = _settings.EngineExecutablePath ?? string.Empty;
        ModelFilePath.Text = _settings.ModelFilePath ?? string.Empty;
        RenderStatus(_session.Status);
        RenderHostHardwareSummary();
        RenderDisplayAdapters();
        BackendCapabilityStatus.Text = "Backend: not checked; Connect will test the configured local llama.cpp endpoint.";
        LocalProcessStatus.Text = "Local engine process: not started by this app.";
        if (settingsLoadFailed)
        {
            LifecycleState.Text = "Error — local settings are invalid; correct the settings file before reconnecting.";
        }
    }

    private void BrowseCatalogButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Filter = "Model catalog JSON (*.json)|*.json|All files (*.*)|*.*",
            Title = "Select a versioned model catalog"
        };
        if (dialog.ShowDialog(this) == true)
        {
            CatalogFilePath.Text = dialog.FileName;
            CatalogStatus.Text = "Catalog file selected. Enter the trusted source-host allowlist, then load to validate its metadata.";
        }
    }

    private void LoadCatalogButton_Click(object sender, RoutedEventArgs e)
    {
        CatalogItems.ItemsSource = null;
        CatalogModelDetails.Text = "Select a model record to inspect its source, license metadata, size, checksum, backend and limitations.";
        try
        {
            var sourceHosts = ParseTrustedHostList(CatalogTrustedSourceHosts.Text);
            var licenseHosts = ParseTrustedHostList(CatalogTrustedLicenseHosts.Text);
            var cachePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WhiteP", "model-catalog-cache.json");
            var catalog = new OfflineModelCatalogStore().Load(
                CatalogFilePath.Text, cachePath, sourceHosts, LocalInferenceBackendFactory.SupportedBackendIds,
                licenseHosts.Length == 0 ? null : licenseHosts, out var usedCache);
            CatalogItems.ItemsSource = catalog.Models;
            var savedSelection = catalog.Models.FirstOrDefault(model =>
                string.Equals(model.Id, _settings.ActiveCatalogModelId, StringComparison.Ordinal));
            if (savedSelection is not null)
            {
                CatalogItems.SelectedItem = savedSelection;
            }

            CatalogStatus.Text = usedCache
                ? $"Internet or selected catalog unavailable. Showing {catalog.Models.Count} model metadata record(s) from the last validated local cache; no model was downloaded or installed."
                : catalog.Models.Count == 0
                    ? $"Catalog schema v{catalog.SchemaVersion} loaded successfully; it contains no model entries."
                    : $"Loaded {catalog.Models.Count} metadata record(s) from schema v{catalog.SchemaVersion}. No model was downloaded or installed.";
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            CatalogStatus.Text = "Catalog not loaded. Check the local file, JSON metadata, and explicit trusted source/license host allowlists.";
        }
    }

    private async void RemoveActiveModelButton_Click(object sender, RoutedEventArgs e)
    {
        if (_session.Status.State == HostState.Busy) { CatalogStatus.Text = "Cannot remove the active model while inference is running."; return; }
        var answer = MessageBox.Show(this, "Disconnect the active model and release backend resources? The model file on disk will not be deleted. If this app started llama.cpp, its process will be stopped.", "Remove active model", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        if (answer != MessageBoxResult.Yes) { CatalogStatus.Text = "Active model removal cancelled."; return; }
        RemoveActiveModelButton.IsEnabled = false; ConnectButton.IsEnabled = false; GenerateButton.IsEnabled = false;
        try
        {
            var status = await _session.DisconnectAsync();
            _localInferenceProcessLauncher.Stop();
            ModelId.Text = string.Empty;
            _settings = _settingsStore.Save(_settings with { ModelId = "unconfigured", ActiveCatalogModelId = null });
            CatalogItems.SelectedItem = null;
            RenderStatus(status);
            LocalProcessStatus.Text = "Local engine process stopped if it was started by this app.";
            CatalogStatus.Text = "Active model disconnected; backend resources released where owned. The model file remains on disk.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or OperationCanceledException or TimeoutException)
        { CatalogStatus.Text = "Active model removal did not complete. Check host state and local settings access."; RenderStatus(_session.Status); }
        finally { RemoveActiveModelButton.IsEnabled = true; ConnectButton.IsEnabled = true; GenerateButton.IsEnabled = _session.IsConnected && _capacityReady; }
    }

    private void CatalogItems_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CatalogItems.SelectedItem is not ModelCatalogEntry model)
        {
            CatalogModelDetails.Text = "Select a model record to inspect its source, license metadata, size, checksum, backend and limitations.";
            return;
        }

        var sizeMiB = model.SizeBytes / (1024d * 1024d);
        var sizeGiB = model.SizeBytes / (1024d * 1024d * 1024d);
        var licenseStatus = model.License.Status;
        var licenseLimit = model.License.RequiresReview
            ? "License status is unverified or requires review. Do not assume permission to use, modify, or redistribute this model."
            : "The catalog declares a license status, but the app has not independently verified legal terms or usage restrictions.";
        var licenseUrl = model.License.Url?.ToString() ?? "Not provided";
        var licenseId = model.License.Identifier ?? "Not provided";
        var backends = string.Join(", ", model.BackendIds);

        try
        {
            _settings = _settingsStore.Save(_settings with { ActiveCatalogModelId = model.Id });
            CatalogStatus.Text = $"Saved model selection: {model.Id}. Selection is a persistent preference; it does not download, install, or activate the model in the inference backend.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            CatalogStatus.Text = "Model selection is not persisted. Check access to the local settings file.";
        }

        CatalogModelDetails.Text =
            $"Name: {model.Name}\n" +
            $"Model ID: {model.Id}\n" +
            $"Format: {model.Format}\n" +
            $"Declared size: {model.SizeBytes:N0} bytes ({sizeMiB:N2} MiB / {sizeGiB:N3} GiB)\n" +
            $"Source: {model.SourceUrl}\n" +
            $"License status: {licenseStatus}\n" +
            $"License identifier: {licenseId}\n" +
            $"License terms URL: {licenseUrl}\n" +
            $"Supported backend IDs: {backends}\n" +
            $"Checksum: {model.ChecksumAlgorithm.ToUpperInvariant()} {model.ChecksumValue}\n" +
            $"Usage limitations: {licenseLimit} Catalog metadata does not establish model safety, performance, hardware compatibility, or the accuracy of the publisher's claims.";
    }

    private static string[] ParseTrustedHostList(string? value) =>
        (value ?? string.Empty)
            .Split(new[] { ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

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
                    _settings.RequestTimeoutSeconds,
                    EngineExecutablePath.Text,
                    ModelFilePath.Text,
                    _settings.ActiveCatalogModelId));
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

    private void StartLocalEngineButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Uri.TryCreate(ServerUrl.Text.Trim(), UriKind.Absolute, out var endpoint) ||
                (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
            {
                LocalProcessStatus.Text = "Local engine start failed: enter a valid HTTP(S) URL first.";
                return;
            }

            var process = _localInferenceProcessLauncher.Start(
                EngineExecutablePath.Text,
                ModelFilePath.Text,
                endpoint,
                ModelId.Text);
            LocalProcessStatus.Text = $"Local llama.cpp process started (PID {process.Id}). Press Connect to verify health and model availability.";
        }
        catch (Exception exception)
        {
            LocalProcessStatus.Text = "Local engine start failed: " + HostErrorPresentation.ToUserMessage(exception);
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
        CancelGenerationButton.IsEnabled = true;
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
        catch (OperationCanceledException)
        {
            // Closing the client request may not stop llama.cpp's in-flight computation.
            // Stop the managed process as a fallback so the host does not keep consuming resources.
            if (_localInferenceProcessLauncher.IsRunning)
            {
                _localInferenceProcessLauncher.Stop();
                LocalProcessStatus.Text = "Local engine process stopped after inference cancellation; reconnect before generating again.";
            }
            RenderStatus(_session.Status);
            OutputText.Text = "Inference cancelled. If this app started the local engine, that process was stopped. Reconnect before the next request.";
        }
        catch (Exception exception)
        {
            RenderStatus(_session.Status);
            OutputText.Text = HostErrorPresentation.ToUserMessage(exception);
        }
        finally
        {
            CancelGenerationButton.IsEnabled = false;
            RenderHostHardwareSummary();
            ConnectButton.IsEnabled = true;
            GenerateButton.IsEnabled = _session.IsConnected && _capacityReady;
        }
    }

    private void CancelGenerationButton_Click(object sender, RoutedEventArgs e)
    {
        CancelGenerationButton.IsEnabled = false;
        _operationCancellation?.Cancel();
        if (_localInferenceProcessLauncher.IsRunning)
        {
            _localInferenceProcessLauncher.Stop();
            LocalProcessStatus.Text = "Local engine process stopped to cancel inference; reconnect before generating again.";
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
        _localInferenceProcessLauncher.Dispose();
    }
}
