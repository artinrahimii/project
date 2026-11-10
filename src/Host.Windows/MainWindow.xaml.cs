using System.Security.Cryptography;
using System.IO;
using System.Globalization;
using System.Net;
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
using WhiteP.Host.Windows.Networking;
using WhiteP.Host.Windows.Requests;
using WhiteP.Host.Windows.Security;
using Nethermind.Libp2p.Core;
using WhiteP.Shared.Contracts;
using WhiteP.Shared.Contracts.V1;
using Google.Protobuf;
using HostCapabilities = WhiteP.Host.Domain.HostCapabilities;
using HostStatus = WhiteP.Host.Domain.HostStatus;
using WhiteP.Shared.Security;
using WhiteP.Host.Windows.Chat;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly LocalHostSettingsStore _settingsStore = new();
    private readonly LocalInferenceProcessLauncher _localInferenceProcessLauncher = new();
    private LocalHostSettings _settings = LocalHostSettings.Defaults;
    private readonly HostInferenceSession _session;
    private ChatConversation _conversation = new(HostConversationRetentionPolicy.Default);
    private readonly WindowsHostConversationStore _conversationStore = new();
    private bool _conversationPersistenceAvailable = true;
    private bool _suppressConversationRetentionToggle;
    private readonly HostRequestCoordinator _peerRequestCoordinator = new();
    private readonly AuthenticatedRequestReplayGuard _peerRequestReplayGuard = new();
    private PeerAnnouncementHostLifecycle? _peerParticipationService;
    private string? _requestedModelMemoryRequirementText;
    private string? _requestedMinimumFreeDiskRequirementText;
    private CancellationTokenSource? _operationCancellation;
    private CancellationTokenSource? _modelUpdateCancellation;
    private Task? _generationTask;
    private Task? _modelUpdateTask;
    private Task? _peerParticipationTask;
    private CancellationTokenSource? _peerParticipationCancellation;
    private volatile bool _capacityReady;
    private bool _activeCatalogModelVerified;
    private bool _clearSessionRequested;
    private bool _suppressParticipationToggleChanged;
    private bool _windowCloseCleanupStarted;
    private bool _allowWindowClose;
    private bool _windowClosing;

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
        catch (Exception exception) when (
            exception is InvalidDataException or ArgumentException or IOException or UnauthorizedAccessException)
        {
            // Invalid persisted values and unreadable settings must not escape the WPF constructor.
            // Keep initialized safe defaults in memory and avoid exposing file paths or exception details.
            settingsLoadFailed = true;
        }

        ServerUrl.Text = _settings.ServerUrl;
        ModelId.Text = _settings.ModelId;
        EngineExecutablePath.Text = _settings.EngineExecutablePath ?? string.Empty;
        ModelFilePath.Text = _settings.ModelFilePath ?? string.Empty;
        ModelMemoryRequirementGiB.Text = _settings.RequiredModelMemoryGiB.ToString("0.###", CultureInfo.InvariantCulture);
        MinimumFreeDiskGiB.Text = _settings.MinimumFreeDiskGiB.ToString("0.###", CultureInfo.InvariantCulture);
        _requestedModelMemoryRequirementText = ModelMemoryRequirementGiB.Text;
        _requestedMinimumFreeDiskRequirementText = MinimumFreeDiskGiB.Text;
        _conversation.UpdateRetentionPolicy(
            HostConversationRetentionPolicy.FromOwnerConsent(_settings.PersistHostConversation));
        _suppressConversationRetentionToggle = true;
        HostConversationRetentionToggle.IsChecked = _settings.PersistHostConversation;
        _suppressConversationRetentionToggle = false;
        if (_settings.PersistHostConversation)
        {
            try
            {
                foreach (var savedMessage in _conversationStore.Load())
                {
                    var restoredMessage = _conversation.Add(savedMessage.Role, savedMessage.Content);
                    RenderConversationMessage(restoredMessage);
                }
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or CryptographicException or InvalidDataException)
            {
                _conversationPersistenceAvailable = false;
            }
        }
        RenderStatus(_session.Status);
        RenderHostHardwareSummary();
        RenderDisplayAdapters();
        BackendCapabilityStatus.Text = "Backend: not checked; Connect will test the configured local llama.cpp endpoint.";
        LocalProcessStatus.Text = "Local engine process: not started by this app.";
        ParticipationNetworkStatus.Text = "Network participation is disabled; no listener is open.";
        if (settingsLoadFailed)
        {
            LifecycleState.Text = "Error — local settings could not be loaded or validated; check the settings file and permissions before reconnecting.";
        }
        else if (_settings.PersistHostConversation && !_conversationPersistenceAvailable)
        {
            LifecycleState.Text = "Optional local conversation history could not be opened. It was not overwritten; turn the setting off to stop restoring it.";
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
            _settings = _settingsStore.Save(_settings with
            {
                ModelId = "unconfigured",
                ActiveCatalogModelId = null,
                ActiveCatalogModelSha256 = null
            });
            _activeCatalogModelVerified = false;
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
        if (_windowClosing || _modelUpdateTask is not null) return;
        var operationTask = UpdateSelectedModelAsync();
        _modelUpdateTask = operationTask;
        try
        {
            await operationTask;
        }
        finally
        {
            if (ReferenceEquals(_modelUpdateTask, operationTask))
            {
                _modelUpdateTask = null;
            }
        }
    }

    private async Task UpdateSelectedModelAsync()
    {
        if (_windowClosing) return;
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
                if (_windowClosing || !ReferenceEquals(_modelUpdateCancellation, cancellation))
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
                    ActiveCatalogModelId = model.Id,
                    ActiveCatalogModelSha256 = result.Sha256
                });
                _activeCatalogModelVerified = false;
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
            if (!_windowClosing) CatalogStatus.Text = "Model update cancelled before atomic replacement. The previously installed model was preserved; reconnect/restart the host before using it again.";
            if (!_windowClosing) RenderStatus(_session.Status);
        }
        catch (Exception exception) when (
            exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or
            NotSupportedException or InvalidOperationException or TimeoutException or System.Net.Http.HttpRequestException)
        {
            if (!_windowClosing) CatalogStatus.Text =
                "Model update failed. Downloaded content is not promoted until size, SHA-256 and GGUF checks pass; " +
                "the previous destination is preserved on validation or atomic-replacement failure. " +
                HostErrorPresentation.ToUserMessage(exception);
            if (!_windowClosing) RenderStatus(_session.Status);
        }
        finally
        {
            if (ReferenceEquals(_modelUpdateCancellation, cancellation))
            {
                _modelUpdateCancellation = null;
            }

            cancellation.Dispose();
            if (!_windowClosing)
            {
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
            var sameSelectedModel = string.Equals(_settings.ActiveCatalogModelId, model.Id, StringComparison.Ordinal);
            _settings = _settingsStore.Save(_settings with
            {
                ActiveCatalogModelId = model.Id,
                ActiveCatalogModelSha256 = sameSelectedModel ? _settings.ActiveCatalogModelSha256 : null
            });
            _activeCatalogModelVerified = false;
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
        ConnectButton.IsEnabled = false;
        GenerateButton.IsEnabled = false;
        try
        {
            if (!TryPersistCapacityRequirements())
            {
                return;
            }

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
                _settings = _settingsStore.Save(_settings with
                {
                    ServerUrl = endpoint.ToString(),
                    ModelId = ModelId.Text,
                    EngineExecutablePath = EngineExecutablePath.Text,
                    ModelFilePath = ModelFilePath.Text
                });
            }
            catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
            {
                LifecycleState.Text = $"Error — {HostErrorPresentation.ToUserMessage(exception)}";
                return;
            }

            _activeCatalogModelVerified = await InstalledCatalogModelVerifier.VerifyAsync(
                _settings.ActiveCatalogModelId,
                _settings.ModelId,
                _settings.ModelFilePath,
                _settings.ActiveCatalogModelSha256,
                CancellationToken.None).ConfigureAwait(true);
            if (!_activeCatalogModelVerified)
            {
                BackendCapabilityStatus.Text =
                    "Local inference may be available, but peer hosting is not READY: select a model from a validated catalog, install/update it, and verify its SHA-256 before sharing.";
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
        if (_windowClosing || _generationTask is not null) return;
        var operationTask = GeneratePromptAsync();
        _generationTask = operationTask;
        try
        {
            await operationTask;
        }
        finally
        {
            if (ReferenceEquals(_generationTask, operationTask))
            {
                _generationTask = null;
            }
        }
    }

    private async Task GeneratePromptAsync()
    {
        var prompt = PromptInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            LifecycleState.Text = "Error — enter a message first.";
            return;
        }

        if (!_session.IsConnected)
        {
            LifecycleState.Text = "Connect to a ready local backend and verify host capacity before sending a message.";
            return;
        }

        if (!TryPersistCapacityRequirements())
        {
            return;
        }

        RenderStatus(_session.Status);
        if (!_capacityReady)
        {
            LifecycleState.Text = "The configured RAM/disk requirements are not satisfied by current measured host capacity.";
            return;
        }
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
            if (!_clearSessionRequested && !_windowClosing)
            {
                AppendConversationMessage(ConversationMessageRole.Assistant, response);
            }
            if (!_windowClosing) RenderStatus(_session.Status);
        }
        catch (OperationCanceledException)
        {
            if (_localInferenceProcessLauncher.IsRunning)
            {
                _localInferenceProcessLauncher.Stop();
                if (!_windowClosing)
                {
                    LocalProcessStatus.Text = "Local engine process stopped after inference cancellation; reconnect before generating again.";
                }
            }
            if (!_windowClosing) RenderStatus(_session.Status);
            if (!_clearSessionRequested && !_windowClosing)
            {
                AppendConversationMessage(ConversationMessageRole.System, "Inference cancelled. Reconnect before the next request.");
            }
        }
        catch (Exception exception)
        {
            if (!_windowClosing) RenderStatus(_session.Status);
            if (!_clearSessionRequested && !_windowClosing)
            {
                AppendConversationMessage(ConversationMessageRole.System, HostErrorPresentation.ToUserMessage(exception));
            }
        }
        finally
        {
            if (!_windowClosing)
            {
                if (ChatMessages.Items.Count > 0)
                {
                    ChatMessages.ScrollIntoView(ChatMessages.Items[ChatMessages.Items.Count - 1]);
                }
                CancelGenerationButton.IsEnabled = false;
                _clearSessionRequested = false;
                RenderHostHardwareSummary();
                ConnectButton.IsEnabled = true;
                GenerateButton.IsEnabled = _session.IsConnected && _capacityReady;
            }
        }
    }

    private void AppendConversationMessage(ConversationMessageRole role, string content)
    {
        var message = _conversation.Add(role, content);
        RenderConversationMessage(message);
        if (_settings.PersistHostConversation && _conversationPersistenceAvailable)
        {
            try
            {
                _conversationStore.Save(_conversation.Messages);
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or CryptographicException or InvalidDataException or ArgumentException)
            {
                _conversationPersistenceAvailable = false;
                LifecycleState.Text = "Local conversation history could not be updated. Further saves are paused for this session; the last successfully saved copy may remain encrypted on this device.";
            }
        }
    }

    private void RenderConversationMessage(ConversationMessage message)
    {
        var label = message.Role switch
        {
            ConversationMessageRole.User => "You",
            ConversationMessageRole.Assistant => "Local AI",
            _ => "System"
        };
        ChatMessages.Items.Add($"{label}: {message.Content}");
    }

    private void HostConversationRetentionToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressConversationRetentionToggle)
            return;
        if (HostConversationRetentionToggle.IsChecked == true)
            EnableHostConversationRetention();
        else
            DisableHostConversationRetention();
    }

    private void EnableHostConversationRetention()
    {
        if (_settings.PersistHostConversation)
            return;
        var choice = MessageBox.Show(
            "Enable optional local host conversation history?\n\nIf you continue, the prompts, responses and system messages already shown in this session, plus future messages, will be saved on this Windows device. The file is encrypted with Windows DPAPI for your current Windows account. This does not protect it from malicious code running as your account, Windows backups, paging files or screenshots. You can turn off this setting or clear the session at any time.\n\nDo you consent to this local storage?",
            "Confirm optional local conversation history",
            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        if (choice != MessageBoxResult.Yes)
        {
            SetConversationRetentionToggle(false);
            return;
        }

        try
        {
            _conversationStore.Delete();
            _settings = _settingsStore.Save(_settings with { PersistHostConversation = true });
            _conversation.UpdateRetentionPolicy(HostConversationRetentionPolicy.FromOwnerConsent(ownerConsent: true));
            _conversationPersistenceAvailable = true;
            _conversationStore.Save(_conversation.Messages);
            LifecycleState.Text = "Optional local conversation history is enabled and protected for this Windows account.";
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or CryptographicException or InvalidDataException or ArgumentException)
        {
            _conversationPersistenceAvailable = false;
            _conversation.UpdateRetentionPolicy(HostConversationRetentionPolicy.Default);
            try
            {
                _settings = _settingsStore.Save(_settings with { PersistHostConversation = false });
                SetConversationRetentionToggle(false);
            }
            catch (Exception rollbackException) when (
                rollbackException is IOException or UnauthorizedAccessException or ArgumentException or InvalidDataException)
            {
                HostConversationRetentionToggle.IsEnabled = false;
            }
            LifecycleState.Text = "Optional local conversation history could not be enabled. The non-persistent in-memory mode remains active.";
        }
    }

    private void DisableHostConversationRetention()
    {
        try
        {
            _settings = _settingsStore.Save(_settings with { PersistHostConversation = false });
            _conversation.UpdateRetentionPolicy(HostConversationRetentionPolicy.Default);
            _conversationPersistenceAvailable = false;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or ArgumentException or InvalidDataException)
        {
            SetConversationRetentionToggle(true);
            LifecycleState.Text = "The retention preference could not be saved. The previous setting remains active.";
            return;
        }

        try
        {
            _conversationStore.Delete();
            LifecycleState.Text = "Optional local conversation history is off; the protected local history file was deleted.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            LifecycleState.Text = "Optional local conversation history is off, but the previous encrypted file could not be deleted. It will not be restored while this setting remains off.";
        }
    }

    private void SetConversationRetentionToggle(bool enabled)
    {
        _suppressConversationRetentionToggle = true;
        HostConversationRetentionToggle.IsChecked = enabled;
        _suppressConversationRetentionToggle = false;
    }

    private void ClearSessionButton_Click(object sender, RoutedEventArgs e)
    {
        var generationWasRunning = CancelGenerationButton.IsEnabled;
        if (generationWasRunning)
        {
            _clearSessionRequested = true;
            _operationCancellation?.Cancel();
            if (_localInferenceProcessLauncher.IsRunning)
            {
                _localInferenceProcessLauncher.Stop();
                LocalProcessStatus.Text = "Local engine process stopped to clear the active session; reconnect before generating again.";
            }
        }

        _conversation.Clear();
        ChatMessages.Items.Clear();
        PromptInput.Clear();
        CancelGenerationButton.IsEnabled = false;
        var historyDeleted = true;
        try
        {
            _conversationStore.Delete();
        }
        catch (IOException)
        {
            historyDeleted = false;
        }
        catch (UnauthorizedAccessException)
        {
            historyDeleted = false;
        }

        LifecycleState.Text = !historyDeleted
            ? "The in-memory session was cleared, but the previous encrypted local history file could not be deleted."
            : generationWasRunning
                ? "Session cleared; active inference cancellation requested. Reconnect before generating again."
                : "Session cleared.";
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
            HostState.Error => "Backend: unavailable or not ready — details are hidden.",
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


    private bool TryPersistCapacityRequirements()
    {
        _requestedModelMemoryRequirementText = ModelMemoryRequirementGiB.Text;
        _requestedMinimumFreeDiskRequirementText = MinimumFreeDiskGiB.Text;

        if (!TryParseCapacityRequirement(_requestedModelMemoryRequirementText, out var requiredModelMemoryGiB) ||
            !TryParseCapacityRequirement(_requestedMinimumFreeDiskRequirementText, out var minimumFreeDiskGiB))
        {
            RenderStatus(_session.Status);
            LifecycleState.Text = "Not Ready — enter positive, finite RAM and minimum free-disk requirements in GiB.";
            return false;
        }

        try
        {
            _settings = _settingsStore.Save(_settings with
            {
                RequiredModelMemoryGiB = requiredModelMemoryGiB,
                MinimumFreeDiskGiB = minimumFreeDiskGiB
            });
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
        {
            _capacityReady = false;
            GenerateButton.IsEnabled = false;
            CapacityStatus.Text = "Capacity: Unknown — configured resource limits could not be saved locally.";
            LifecycleState.Text = "Resource limits could not be saved; inference and network admission are disabled until settings can be written.";
            return false;
        }
    }

    private void CapacityRequirement_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_windowClosing)
        {
            return;
        }

        if (TryPersistCapacityRequirements())
        {
            RenderStatus(_session.Status);
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

    private async void NetworkParticipationToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (_suppressParticipationToggleChanged || _windowClosing || _peerParticipationTask is not null)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _peerParticipationCancellation = cancellation;
        var operationTask = ChangeNetworkParticipationAsync(sender, e, cancellation.Token);
        _peerParticipationTask = operationTask;
        try
        {
            await operationTask;
        }
        finally
        {
            if (ReferenceEquals(_peerParticipationTask, operationTask))
            {
                _peerParticipationTask = null;
            }

            if (ReferenceEquals(_peerParticipationCancellation, cancellation))
            {
                _peerParticipationCancellation = null;
            }

            cancellation.Dispose();
        }
    }

    private async Task ChangeNetworkParticipationAsync(
        object sender,
        RoutedEventArgs e,
        CancellationToken cancellationToken)
    {
        if (_suppressParticipationToggleChanged || _windowClosing)
        {
            return;
        }

        var active = NetworkParticipationToggle.IsChecked == true;
        NetworkParticipationToggle.IsEnabled = false;
        try
        {
            if (!active)
            {
                var previous = _peerParticipationService;
                _peerParticipationService = null;
                if (previous is not null)
                {
                    await previous.DisposeAsync();
                }

                ParticipationNetworkStatus.Text =
                    "Peer hosting is disabled. The last rendezvous lease is no longer renewed and expires naturally.";
                return;
            }

            if (!TryPersistCapacityRequirements())
                throw new InvalidOperationException("The configured RAM and disk requirements could not be validated and saved.");

            RenderStatus(_session.Status);
            if (!_session.IsConnected || !_capacityReady)
                throw new InvalidOperationException("Connect to a healthy local inference backend and verify host capacity before enabling peer inference.");

            if (!Uri.TryCreate(PeerRendezvousUrl.Text.Trim(), UriKind.Absolute, out var rendezvousUri))
                throw new ArgumentException("Enter an absolute HTTPS rendezvous URL. HTTP is permitted only for loopback testing.");

            if (!IPAddress.TryParse(PeerBindAddress.Text.Trim(), out var bindAddress))
                throw new ArgumentException("Enter a specific local interface IP address for the peer listener.");

            if (!IPAddress.TryParse(PeerAdvertisedAddress.Text.Trim(), out var advertisedAddress))
                throw new ArgumentException("Enter the IP address that the remote peer has independently verified as reachable.");

            if (!int.TryParse(PeerPort.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var peerPort) ||
                peerPort is < 1 or > IPEndPoint.MaxPort)
                throw new ArgumentException("Enter a fixed TCP/UDP peer port from 1 through 65535.");

            if (PeerReachabilityConfirmed.IsChecked != true)
                throw new ArgumentException("Confirm that this advertised address and port were tested from the remote peer before publishing.");

            var expectedPeerIdentity = PeerTrustedIdentity.Text.Trim();
            if (string.IsNullOrWhiteSpace(expectedPeerIdentity))
                throw new ArgumentException("Enter the exact PeerId of the remote peer you explicitly trust.");

            var privacyConsent = MessageBox.Show(
                this,
                WhiteP.Shared.Security.PeerInferencePrivacyDisclosure.BuildHostParticipationMessage(expectedPeerIdentity),
                WhiteP.Shared.Security.PeerInferencePrivacyDisclosure.HostParticipationTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);
            if (privacyConsent != MessageBoxResult.Yes)
            {
                _suppressParticipationToggleChanged = true;
                NetworkParticipationToggle.IsChecked = false;
                _suppressParticipationToggleChanged = false;
                ParticipationNetworkStatus.Text = "Peer hosting remains disabled because the privacy notice was not accepted.";
                return;
            }

            var localIdentity = new WindowsHostIdentityProvider().LoadOrCreate();
            var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localData))
                throw new InvalidOperationException("The current user's local application data directory is unavailable.");

            var trustStore = new PeerTrustStore(
                Path.Combine(localData, "WhiteP", "security", "peer-trust-v1.dpapi"),
                new WindowsDpapiSecretProtector());

            if (!trustStore.IsTrusted(expectedPeerIdentity))
            {
                var trustAnswer = MessageBox.Show(
                    this,
                    $"Trust this exact peer identity for authenticated incoming inference?\n\n{expectedPeerIdentity}\n\nOnly continue if you verified this PeerId through a separate trusted channel.",
                    "Confirm peer identity",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (trustAnswer != MessageBoxResult.Yes)
                    throw new InvalidOperationException("Peer hosting was cancelled because the remote identity was not approved.");

                trustStore.TrustPeerAfterExplicitConfirmation(expectedPeerIdentity, explicitlyConfirmed: true);
            }

            var service = new PeerAnnouncementHostLifecycle(
                trustStore,
                localIdentity,
                expectedPeerIdentity,
                rendezvousUri,
                bindAddress,
                advertisedAddress,
                peerPort,
                advertisedAddressVerified: true,
                HandlePeerInferenceMessageAsync,
                replayGuard: _peerRequestReplayGuard,
                statusProvider: GetPeerRuntimeStatusAsync);
            _peerParticipationService = service;
            await service.StartAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            ParticipationNetworkStatus.Text =
                $"Authenticated peer host active. Local PeerId: {service.LocalPeerIdentity}. " +
                $"Lease published to rendezvous and renewed periodically. The advertised address was operator-confirmed; this app does not independently verify public Internet reachability.";
        }
        catch (Exception exception)
        {
            var failedService = _peerParticipationService;
            _peerParticipationService = null;
            if (failedService is not null)
            {
                try { await failedService.DisposeAsync(); }
                catch (Exception) { }
            }

            _suppressParticipationToggleChanged = true;
            NetworkParticipationToggle.IsChecked = false;
            _suppressParticipationToggleChanged = false;

            if (!_windowClosing)
            {
                ParticipationNetworkStatus.Text =
                    "Peer hosting could not be enabled. Check the backend, explicit peer trust, HTTPS rendezvous, bind address, port forwarding/firewall and independently tested advertised address. " +
                    HostErrorPresentation.ToUserMessage(exception);
            }
        }
        finally
        {
            if (!_windowClosing)
                NetworkParticipationToggle.IsEnabled = true;
        }
    }

    private void RevokePeerTrustButton_Click(object sender, RoutedEventArgs e)
    {
        if (_windowClosing)
            return;

        if (_peerParticipationTask is not null ||
            _peerParticipationService is not null ||
            NetworkParticipationToggle.IsChecked == true)
        {
            ParticipationNetworkStatus.Text =
                "Stop peer hosting and wait for shutdown to finish before revoking the saved peer trust decision.";
            return;
        }

        string peerId = PeerTrustedIdentity.Text.Trim();
        if (string.IsNullOrWhiteSpace(peerId))
        {
            ParticipationNetworkStatus.Text = "Enter the exact PeerId whose saved trust decision should be revoked.";
            return;
        }

        try
        {
            string localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(localData))
                throw new InvalidOperationException("The current user's local application data directory is unavailable.");

            var trustStore = new PeerTrustStore(
                Path.Combine(localData, "WhiteP", "security", "peer-trust-v1.dpapi"),
                new WindowsDpapiSecretProtector());
            if (!trustStore.IsTrusted(peerId))
            {
                ParticipationNetworkStatus.Text = "The entered PeerId does not have a saved trust decision.";
                return;
            }

            var answer = MessageBox.Show(
                this,
                $"Revoke the saved trust decision for this exact PeerId?\n\n{peerId}\n\nFuture connections from this identity will be rejected until you explicitly trust it again.",
                "Revoke peer trust",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);
            if (answer != MessageBoxResult.Yes)
                return;

            if (trustStore.RevokeTrust(peerId))
                ParticipationNetworkStatus.Text =
                    "Peer trust was revoked. The identity is no longer eligible to connect; a new explicit confirmation is required to trust it again.";
            else
                ParticipationNetworkStatus.Text = "The PeerId was no longer present in the saved trust list.";
        }
        catch (Exception exception)
        {
            ParticipationNetworkStatus.Text =
                "Peer trust could not be revoked. " + HostErrorPresentation.ToUserMessage(exception);
        }
    }

    private async Task<WhiteP.Shared.Contracts.V1.HostStatus> GetPeerRuntimeStatusAsync(
        CancellationToken cancellationToken)
    {
        var snapshot = await _session.GetFreshPeerRuntimeSnapshotAsync(cancellationToken).ConfigureAwait(false);
        HostStatus current = snapshot.Status;
        var state = current.State switch
        {
            HostState.Starting => (WhiteP.Shared.Contracts.V1.HostRuntimeState)1,
            HostState.Ready when _capacityReady &&
                snapshot.ModelCapability is not null &&
                _activeCatalogModelVerified &&
                string.Equals(_settings.ActiveCatalogModelId, snapshot.ModelCapability.ModelId, StringComparison.Ordinal) =>
                (WhiteP.Shared.Contracts.V1.HostRuntimeState)2,
            HostState.Busy => (WhiteP.Shared.Contracts.V1.HostRuntimeState)3,
            _ => (WhiteP.Shared.Contracts.V1.HostRuntimeState)5
        };

        var peerStatus = new WhiteP.Shared.Contracts.V1.HostStatus
        {
            State = state,
            ActiveRequests = current.State == HostState.Busy ? 1U : 0U,
            ProtocolVersion = ProtocolVersionPolicy.CurrentVersion
        };

        if (state == (WhiteP.Shared.Contracts.V1.HostRuntimeState)2 &&
            snapshot.ModelCapability is { } capability)
        {
            peerStatus.Models.Add(new ModelCapability
            {
                ModelId = capability.ModelId,
                DisplayName = capability.DisplayName,
                MaxContextTokens = capability.MaxContextTokens,
                MaxOutputTokens = capability.MaxOutputTokens,
                SupportsStreaming = capability.SupportsStreaming
            });
        }

        return peerStatus;
    }

    private async Task<byte[]> HandlePeerInferenceMessageAsync(byte[] wireBytes, CancellationToken cancellationToken)
    {
        var parsed = ProtocolMessageParser.ParseInferenceRequest(wireBytes);
        if (!parsed.IsValid || parsed.Message is null)
            throw new InvalidDataException("The peer sent a malformed or invalid inference request.");

        var request = parsed.Message;
        if (!_activeCatalogModelVerified ||
            !string.Equals(_settings.ActiveCatalogModelId, request.ModelId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The requested model is not the catalog-verified installed model currently selected by the host owner.");
        }

        if (string.IsNullOrWhiteSpace(request.RoundId))
            throw new InvalidDataException("The peer request is missing its dispatch round identifier.");

        var currentService = _peerParticipationService
            ?? throw new InvalidOperationException("Peer hosting is no longer active.");

        var admission = new HostRequestAdmissionContext(
            currentService.IsActive,
            _session.Status.State,
            request.ModelId,
            ModelId.Text.Trim(),
            _capacityReady ? HostCapacityState.Ready : HostCapacityState.Unknown,
            request.DeadlineUnixMs);

        HostRequestOutcome<string> outcome;
        try
        {
            outcome = await _peerRequestCoordinator.TryExecuteAsync(
                admission,
                token => _session.GenerateAsync(request, token),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException)
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= (long)request.DeadlineUnixMs)
                throw;
            return CreatePeerInferenceResponse(
                request,
                currentService.LocalPeerIdentity,
                InferenceOutcome.TimedOut,
                string.Empty,
                "The host inference backend timed out before the request deadline.",
                ProtocolErrorCode.RequestExpired).ToByteArray();
        }
        catch (Exception)
        {
            return CreatePeerInferenceResponse(
                request,
                currentService.LocalPeerIdentity,
                InferenceOutcome.Failed,
                string.Empty,
                "The host could not process the request.",
                ProtocolErrorCode.Internal).ToByteArray();
        }

        if (outcome.Status == HostRequestStatus.Completed && outcome.Result is not null)
        {
            return CreatePeerInferenceResponse(
                request,
                currentService.LocalPeerIdentity,
                InferenceOutcome.Succeeded,
                outcome.Result,
                string.Empty).ToByteArray();
        }

        if (outcome.Status == HostRequestStatus.Rejected &&
            outcome.AdmissionFailure == HostRequestAdmissionFailure.DeadlineExpired)
            throw new TimeoutException("The logical request deadline expired before admission.");

        var busy = outcome.Status == HostRequestStatus.Busy ||
            (outcome.Status == HostRequestStatus.Rejected &&
             (outcome.AdmissionFailure == HostRequestAdmissionFailure.InactiveParticipation ||
              outcome.AdmissionFailure == HostRequestAdmissionFailure.ModelNotReady));
        var errorCode = busy
            ? ProtocolErrorCode.HostBusy
            : MapAdmissionErrorCode(outcome.AdmissionFailure ?? HostRequestAdmissionFailure.InvalidDeadline);
        return CreatePeerInferenceResponse(
            request,
            currentService.LocalPeerIdentity,
            busy ? InferenceOutcome.Failed : InferenceOutcome.Rejected,
            string.Empty,
            busy ? "The host is busy or temporarily unavailable; retry later." : "The host cannot currently admit this request.",
            errorCode).ToByteArray();
    }

    private static ProtocolErrorCode MapAdmissionErrorCode(HostRequestAdmissionFailure failure) =>
        failure switch
        {
            HostRequestAdmissionFailure.ModelMismatch => ProtocolErrorCode.UnsupportedModel,
            HostRequestAdmissionFailure.ModelNotReady or HostRequestAdmissionFailure.InactiveParticipation =>
                ProtocolErrorCode.HostBusy,
            HostRequestAdmissionFailure.CapacityUnknown or HostRequestAdmissionFailure.CapacityInsufficient =>
                ProtocolErrorCode.ResourceLimit,
            HostRequestAdmissionFailure.DeadlineExpired => ProtocolErrorCode.RequestExpired,
            HostRequestAdmissionFailure.InvalidDeadline => ProtocolErrorCode.InvalidMessage,
            _ => ProtocolErrorCode.Internal
        };

    private static InferenceResponse CreatePeerInferenceResponse(
        InferenceRequest request,
        string localPeerIdentity,
        InferenceOutcome outcome,
        string outputText,
        string errorMessage,
        ProtocolErrorCode errorCode = ProtocolErrorCode.Unspecified)
    {
        var response = new InferenceResponse
        {
            RequestId = request.RequestId,
            AttemptId = request.AttemptId,
            RoundId = request.RoundId,
            EphemeralHostId = localPeerIdentity,
            Outcome = outcome,
            OutputText = outputText,
            CompletedAtUnixMs = (ulong)Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
            ModelId = request.ModelId,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };

        PeerResponseCorrelationValidator.Validate(request, response, localPeerIdentity);

        return response;
    }

    private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_allowWindowClose)
        {
            return;
        }

        e.Cancel = true;
        if (_windowCloseCleanupStarted)
        {
            return;
        }

        _windowCloseCleanupStarted = true;
        _windowClosing = true;
        IsEnabled = false;
        NetworkParticipationToggle.IsEnabled = false;

        // Stop new local UI work immediately; the peer lifecycle below independently
        // closes its listener, cancels peer requests, and awaits its handlers.
        _peerParticipationCancellation?.Cancel();
        _operationCancellation?.Cancel();
        _modelUpdateCancellation?.Cancel();

        Exception? cleanupFailure = null;
        if (_peerParticipationTask is not null)
        {
            try
            {
                await _peerParticipationTask;
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
        }

        var peerService = _peerParticipationService;
        if (peerService is not null)
        {
            try
            {
                await peerService.DisposeAsync();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
            finally
            {
                if (ReferenceEquals(_peerParticipationService, peerService))
                {
                    _peerParticipationService = null;
                }
            }
        }

        // Stop only the engine process owned by this app. External backends are never killed.
        if (_localInferenceProcessLauncher.IsRunning)
        {
            try
            {
                _localInferenceProcessLauncher.Stop();
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
        }

        try
        {
            // DisposeAsync closes session admission, cancels linked operation tokens, and waits
            // for the shared operation gate before disposing the backend.
            await _session.DisposeAsync();
        }
        catch (Exception exception)
        {
            cleanupFailure ??= exception;
        }

        if (_modelUpdateTask is not null)
        {
            try
            {
                await _modelUpdateTask;
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
        }

        if (_generationTask is not null)
        {
            try
            {
                await _generationTask;
            }
            catch (Exception exception)
            {
                cleanupFailure ??= exception;
            }
        }

        if (cleanupFailure is not null)
        {
            MessageBox.Show(
                this,
                $"Host shutdown reported a cleanup error. Active work was cancelled and resource release was attempted. {HostErrorPresentation.ToUserMessage(cleanupFailure)}",
                "Host shutdown warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Defer the second Close until the original Closing event has returned and all
        // owned session/network work has been drained.
        _ = Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() =>
            {
                _allowWindowClose = true;
                Close();
            }));
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _operationCancellation?.Dispose();
        _modelUpdateCancellation?.Dispose();
        _localInferenceProcessLauncher.Dispose();
    }
}
