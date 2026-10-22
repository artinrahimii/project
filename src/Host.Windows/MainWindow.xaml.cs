using System.Windows;

namespace WhiteP.Host.Windows;

public partial class MainWindow : Window
{
    private readonly HostInferenceSession _session = new();
    private CancellationTokenSource? _operationCancellation;

    public MainWindow()
    {
        InitializeComponent();
        RenderStatus(_session.Status);
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

            _operationCancellation?.Dispose();
            _operationCancellation = new CancellationTokenSource();
            var status = await _session.ConnectAsync(endpoint, ModelId.Text.Trim(), _operationCancellation.Token);
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
