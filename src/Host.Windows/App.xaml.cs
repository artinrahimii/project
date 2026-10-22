using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WhiteP.Host.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        var smokeTest = Array.Exists(
            e.Args,
            argument => string.Equals(argument, "--smoke-test", StringComparison.Ordinal));

        if (smokeTest)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        base.OnStartup(e);

        if (!smokeTest)
        {
            return;
        }

        // Check after WPF processes StartupUri and loads the real MainWindow.
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            try
            {
                var window = MainWindow;
                if (window is null ||
                    !window.IsLoaded ||
                    !window.IsVisible ||
                    window.Title != "White-P Local AI Host" ||
                    window.FindName("ServerUrl") is not TextBox ||
                    window.FindName("ModelId") is not TextBox ||
                    window.FindName("ConnectButton") is not Button ||
                    window.FindName("LifecycleState") is not TextBlock status ||
                    string.IsNullOrWhiteSpace(status.Text) ||
                    window.FindName("PromptInput") is not TextBox ||
                    window.FindName("OutputText") is not TextBox ||
                    window.FindName("GenerateButton") is not Button)
                {
                    Shutdown(2);
                    return;
                }

                window.Close();
                Shutdown(0);
            }
            catch
            {
                // CI consumes the non-zero exit code; exceptions must never be treated as success.
                Shutdown(1);
            }
        }));
    }
}
