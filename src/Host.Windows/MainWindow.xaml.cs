using System.IO;
using System.Windows;
using WhiteP.Host.Application;
using WhiteP.Host.Domain;
using WhiteP.Host.Infrastructure;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly LocalHostSettingsStore _settingsStore = new();
    private LocalHostSettings _settings = LocalHostSettings.Defaults;
    private readonly HostInferenceSession _session = new(
        backendFactory: static (address, model) => new LlamaCppServerBackend(address, model));
    private CancellationTokenSource? _operationCancellation;

    public MainWindow()
    {
        InitializeComponent();
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
        if (settingsLoadFailed)
        {
            LifecycleState.Text = "Error — local settings are invalid; correct the settings file before reconnecting.";
        }
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
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
                LifecycleState.Text = $"Error — {exception.Message}";
                return;
            }

            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            var status = await _session.ConnectAsync(new Uri(_settings.ServerUrl), _settings.ModelId, _operationCancellation.Token);
            RenderStatus(status);
            GenerateButton.IsEnabled = status.State == HostState.Ready;
        }
        catch (OperationCanceledException)
        {
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

        GenerateButton.IsEnabled = false;
        ConnectButton.IsEnabled = false;
        OutputText.Clear();
        try
        {
            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            OutputText.Text = await _session.GenerateAsync(PromptInput.Text, 1024, _operationCancellation.Token);
            RenderStatus(_session.Status);
        }
        catch (Exception)
        {
            LifecycleState.Text = _session.Status.Message;
            OutputText.Text = "Generation failed. Check the local server status and reconnect; prompt/response contents are not logged by the backend client.";
        }
        finally
        {
            ConnectButton.IsEnabled = true;
            GenerateButton.IsEnabled = _session.IsConnected;
        }
    }

    private void RenderStatus(HostStatus status) => LifecycleState.Text = $"{status.State} — {status.Message}";

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        _operationCancellation?.Cancel();
        _operationCancellation?.Dispose();
        _session.Dispose();
    }
}
