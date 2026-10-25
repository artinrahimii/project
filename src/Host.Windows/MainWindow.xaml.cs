using System.IO;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;
using System.Linq;
using WhiteP.Host.Windows.Models.Catalog;
using WhiteP.Host.Application;
using WhiteP.Host.Application.Chat;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;
using WhiteP.Host.Windows.Inference;
using WhiteP.Host.Windows.Models;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly LocalHostSettingsStore _settingsStore = new();
    private readonly LocalInferenceProcessLauncher _localInferenceProcessLauncher = new();
    private LocalHostSettings _settings = LocalHostSettings.Defaults;
    private readonly HostInferenceSession _session;
    private readonly ChatConversation _conversation = new();
    private string? _requestedModelMemoryRequirementText;
    private string? _requestedMinimumFreeDiskRequirementText;
    private CancellationTokenSource? _operationCancellation;
    private CancellationTokenSource? _modelUpdateCancellation;
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

    private async void UpdateSelectedModelButton_Click(object sender, RoutedEventArgs e)
    {
        if (_session.Status.State is HostState.Busy or HostState.Starting)
        {
            CatalogStatus.Text = "Wait for the current host operation to finish before updating a model.";
            return;
        }

        if (CatalogItems.SelectedItem is not ModelCatalogEntry model)
        {
            CatalogStatus.Text = "Select a validated catalog entry before updating an installed model.";
            return;
        }

        var sourceHosts = ParseTrustedHostList(CatalogTrustedSourceHosts.Text);
        if (!sourceHosts.Contains(model.SourceUrl.IdnHost, StringComparer.OrdinalIgnoreCase))
        {
            CatalogStatus.Text = "Update refused: the selected model source host is not in the explicit trusted-source allowlist.";
            return;
        }

        var destination = ModelFilePath.Text.Trim();
        if (string.IsNullOrWhiteSpace(destination) || !File.Exists(destination))
        {
            CatalogStatus.Text = "Update refused: select the existing installed .gguf file in the Model file field first.";
            return;
        }

        string fullDestinationPath;
        try
        {
            fullDestinationPath = Path.GetFullPath(destination);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            CatalogStatus.Text = "Update refused: the configured model path is invalid.";
            return;
        }

        var licenseWarning = model.License.UsageDisclaimer;
        var answer = MessageBox.Show(
            this,
            $"Download {model.Name} ({model.SizeBytes:N0} bytes), verify its SHA-256 and GGUF header, then atomically replace:\n{fullDestinationPath}\n\nThe current file is retained until validation succeeds. The host will be disconnected, and only a local engine process started by this app will be stopped. An external process may keep the file locked and cause the update to be refused.\n\n{licenseWarning}\n\nContinue?",
            "Update installed model",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (answer != MessageBoxResult.Yes)
        {
            CatalogStatus.Text = "Model update cancelled before downloading; the installed file was not changed.";
            return;
        }

        var cancellation = new CancellationTokenSource();
        _modelUpdateCancellation?.Dispose();
        _modelUpdateCancellation = cancellation;
        UpdateSelectedModelButton.IsEnabled = false;
        CancelModelUpdateButton.IsEnabled = true;
        CancelModelUpdateButton.Visibility = Visibility.Visible;
        RemoveActiveModelButton.IsEnabled = false;
        LoadCatalogButton.IsEnabled = false;
        BrowseCatalogButton.IsEnabled = false;
        ConnectButton.IsEnabled = false;
        StartLocalEngineButton.IsEnabled = false;
        GenerateButton.IsEnabled = false;
        CatalogItems.IsEnabled = false;
        var ownedEngineWasRunning = _localInferenceProcessLauncher.IsRunning;

        try
        {
            // Disconnect before touching an active model file. DisconnectAsync shares the same
            // operation gate as generation/reconnect; the owned server is then stopped if needed.
            await _session.DisconnectAsync();
            if (ownedEngineWasRunning)
            {
                _localInferenceProcessLauncher.Stop();
                LocalProcessStatus.Text = "App-owned local engine stopped before model replacement.";
            }

            RenderStatus(_session.Status);
            using var downloader = new ModelStreamingDownloader(sourceHosts);
            var installer = new VerifiedModelInstaller(downloader);
            var progress = new Progress<ModelDownloadProgress>(value =>
            {
                if (!ReferenceEquals(_modelUpdateCancellation, cancellation))
                {
                    return;
                }

                var speedMiB = value.BytesPerSecond / (1024d * 1024d);
                CatalogStatus.Text =
                    $"Downloading verified update for {model.Name}: {value.ProgressPercentage:F1}% " +
                    $"({value.BytesDownloaded:N0}/{value.TotalBytes:N0} bytes; {speedMiB:F2} MiB/s). " +
                    "The installed file has not been replaced yet.";
            });

            var result = await installer.DownloadAndUpdateAsync(
                model,
                fullDestinationPath,
                progress,
                cancellation.Token);

            ModelId.Text = model.Id;
            ModelFilePath.Text = result.InstalledPath;
            try
            {
                _settings = _settingsStore.Save(_settings with
                {
                    ModelId = model.Id,
                    ModelFilePath = result.InstalledPath,
                    ActiveCatalogModelId = model.Id
                });
                CatalogStatus.Text =
                    $"Model update succeeded: SHA-256 verified and the installed file was atomically replaced. " +
                    $"Size: {result.SizeBytes:N0} bytes; SHA-256: {result.Sha256}. " +
                    "The host remains disconnected; restart the engine/service so it loads the updated file before generating again.";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                CatalogStatus.Text =
                    $"Model file update succeeded and SHA-256 was verified, but local settings could not be saved. " +
                    $"The current UI shows the updated path; save settings access before closing. {HostErrorPresentation.ToUserMessage(exception)}";
            }
        }
        catch (OperationCanceledException)
        {
            CatalogStatus.Text = "Model update cancelled before atomic replacement. The previously installed model was preserved; reconnect/restart the host before using it again.";
            RenderStatus(_session.Status);
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or
            NotSupportedException or InvalidOperationException or TimeoutException or System.Net.Http.HttpRequestException)
        {
            CatalogStatus.Text =
                "Model update failed. Downloaded content is not promoted until size, SHA-256 and GGUF checks pass; " +
                "the previous destination is preserved on validation or atomic-replacement failure. " +
                HostErrorPresentation.ToUserMessage(exception);
            RenderStatus(_session.Status);
        }
        finally
        {
            if (ReferenceEquals(_modelUpdateCancellation, cancellation))
            {
                _modelUpdateCancellation = null;
            }

            cancellation.Dispose();
            CancelModelUpdateButton.IsEnabled = false;
            CancelModelUpdateButton.Visibility = Visibility.Collapsed;
            UpdateSelectedModelButton.IsEnabled = true;
            RemoveActiveModelButton.IsEnabled = true;
            LoadCatalogButton.IsEnabled = true;
            BrowseCatalogButton.IsEnabled = true;
            ConnectButton.IsEnabled = true;
            StartLocalEngineButton.IsEnabled = true;
            CatalogItems.IsEnabled = true;
            GenerateButton.IsEnabled = _session.IsConnected && _capacityReady;
        }
    }

    private void CancelModelUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        CancelModelUpdateButton.IsEnabled = false;
        _modelUpdateCancellation?.Cancel();
        CatalogStatus.Text = "Cancelling model download. The installed file is not replaced until validation finishes.";
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
        var licenseLimit = model.License.UsageDisclaimer;
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
            $"License classification: {model.License.Classification}\n" +
            $"License disclosure: {model.License.ReviewLabel}\n" +
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

    private void PromptInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter ||
            System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
        {
            return;
        }

        e.Handled = true;
        GenerateButton_Click(sender, new RoutedEventArgs());
    }

    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var prompt = PromptInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            LifecycleState.Text = "Error — enter a message first.";
            return;
        }

        if (!_session.IsConnected || !_capacityReady)
        {
            LifecycleState.Text = "Connect to a ready local backend and verify host capacity before sending a message.";
            return;
        }

        _requestedModelMemoryRequirementText = ModelMemoryRequirementGiB.Text;
        _requestedMinimumFreeDiskRequirementText = MinimumFreeDiskGiB.Text;
        AppendConversationMessage(ConversationMessageRole.User, prompt);
        ChatMessages.ScrollIntoView(ChatMessages.Items[ChatMessages.Items.Count - 1]);
        PromptInput.Clear();
        GenerateButton.IsEnabled = false;
        ConnectButton.IsEnabled = false;
        CancelGenerationButton.IsEnabled = true;
        try
        {
            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            var generationTask = _session.GenerateAsync(prompt, 1024, _operationCancellation.Token);
            RenderStatus(_session.Status);
            var response = await generationTask;
            AppendConversationMessage(ConversationMessageRole.Assistant, response);
            RenderStatus(_session.Status);
        }
        catch (OperationCanceledException)
        {
            if (_localInferenceProcessLauncher.IsRunning)
            {
                _localInferenceProcessLauncher.Stop();
                LocalProcessStatus.Text = "Local engine process stopped after inference cancellation; reconnect before generating again.";
            }
            RenderStatus(_session.Status);
            AppendConversationMessage(ConversationMessageRole.System, "Inference cancelled. Reconnect before the next request.");
        }
        catch (Exception exception)
        {
            RenderStatus(_session.Status);
            AppendConversationMessage(ConversationMessageRole.System, HostErrorPresentation.ToUserMessage(exception));
        }
        finally
        {
            if (ChatMessages.Items.Count > 0)
            {
                ChatMessages.ScrollIntoView(ChatMessages.Items[ChatMessages.Items.Count - 1]);
            }
            CancelGenerationButton.IsEnabled = false;
            RenderHostHardwareSummary();
            ConnectButton.IsEnabled = true;
            GenerateButton.IsEnabled = _session.IsConnected && _capacityReady;
        }
    }

    private void AppendConversationMessage(ConversationMessageRole role, string content)
    {
        var message = _conversation.Add(role, content);
        var label = role switch
        {
            ConversationMessageRole.User => "You",
            ConversationMessageRole.Assistant => "Local AI",
            _ => "System"
        };
        ChatMessages.Items.Add($"{label}: {message.Content}");
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
        _modelUpdateCancellation?.Cancel();
        _operationCancellation?.Dispose();
        _session.Dispose();
        _localInferenceProcessLauncher.Dispose();
    }
}
