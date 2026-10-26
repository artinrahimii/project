using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WhiteP.Host.Windows.Tools;

namespace WhiteP.Host.Windows;

public partial class App : System.Windows.Application
{
    private void ExitAfterWindowCleanup(Window? window, int exitCode)
    {
        if (window is null)
        {
            Shutdown(exitCode);
            return;
        }

        EventHandler closedHandler = null!;
        closedHandler = (_, _) =>
        {
            window.Closed -= closedHandler;
            Shutdown(exitCode);
        };
        window.Closed += closedHandler;
        window.Close();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var workerMode = Array.Exists(
            e.Args,
            argument => string.Equals(argument, "--agent-tool-worker", StringComparison.Ordinal));

        if (workerMode)
        {
            // Do not open the WPF window in worker mode. No tools are registered by default.
            StartupUri = null!;
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            base.OnStartup(e);

            var exitCode = AgentToolWorkerHost.RunAsync(
                Console.OpenStandardInput(),
                Console.OpenStandardOutput(),
                new AgentToolRegistry()).GetAwaiter().GetResult();
            Shutdown(exitCode);
            return;
        }

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
                    window.FindName("GenerateButton") is not Button ||
                    window.FindName("ChatMessages") is not System.Windows.Controls.ListBox ||
                    window.FindName("GpuStatus") is not TextBlock ||
                    window.FindName("CpuStatus") is not TextBlock ||
                    window.FindName("MemoryStatus") is not TextBlock ||
                    window.FindName("CapacityStatus") is not TextBlock ||
                    window.FindName("BackendCapabilityStatus") is not TextBlock ||
                    window.FindName("EngineExecutablePath") is not TextBox ||
                    window.FindName("ModelFilePath") is not TextBox ||
                    window.FindName("StartLocalEngineButton") is not Button ||
                    window.FindName("LocalProcessStatus") is not TextBlock)
                {
                    ExitAfterWindowCleanup(window, 2);
                    return;
                }

                var displayAdapters = WindowsHostHardwareDiscovery.DiscoverDisplayAdapters();
                if (displayAdapters.Any(adapter => string.IsNullOrWhiteSpace(adapter.DeviceName) || string.IsNullOrWhiteSpace(adapter.Description)))
                {
                    ExitAfterWindowCleanup(window, 4);
                    return;
                }

                // This runs on a real Windows CI host and verifies the OS, RAM and system-drive APIs.
                var hardware = WindowsHostHardwareDiscovery.Discover();
                if (string.IsNullOrWhiteSpace(hardware.OperatingSystemDescription) ||
                    hardware.OperatingSystemVersion.Major < 1 ||
                    string.IsNullOrWhiteSpace(hardware.ProcessorName) ||
                    hardware.LogicalProcessorCount <= 0 ||
                    string.IsNullOrWhiteSpace(hardware.SystemDriveName) ||
                    hardware.TotalPhysicalMemoryBytes == 0 ||
                    hardware.AvailablePhysicalMemoryBytes > hardware.TotalPhysicalMemoryBytes ||
                    hardware.SystemDriveTotalBytes <= 0 ||
                    hardware.SystemDriveAvailableBytes < 0 ||
                    hardware.SystemDriveAvailableBytes > hardware.SystemDriveTotalBytes)
                {
                    ExitAfterWindowCleanup(window, 3);
                    return;
                }

                ExitAfterWindowCleanup(window, 0);
            }
            catch
            {
                // CI consumes the non-zero exit code; exceptions must never be treated as success.
                ExitAfterWindowCleanup(MainWindow, 1);
            }
        }));
    }
}
