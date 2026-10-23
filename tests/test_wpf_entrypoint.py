"""Repository guards for the real WPF startup path and its Windows-runner smoke test."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
APP_XAML = (ROOT / "src/Host.Windows/App.xaml").read_text(encoding="utf-8")
APP_CODE = (ROOT / "src/Host.Windows/App.xaml.cs").read_text(encoding="utf-8")
WINDOW_XAML = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
WINDOW_CODE = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
PROJECT = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
WORKFLOW = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")


class WpfEntryPointTests(unittest.TestCase):
    def test_application_uses_the_real_wpf_entrypoint_and_startup_uri(self):
        self.assertIn('<Application x:Class="WhiteP.Host.Windows.App"', APP_XAML)
        self.assertIn('StartupUri="MainWindow.xaml"', APP_XAML)
        self.assertIn("<OutputType>WinExe</OutputType>", PROJECT)
        self.assertIn("<TargetFramework>net10.0-windows</TargetFramework>", PROJECT)
        self.assertIn("<UseWPF>true</UseWPF>", PROJECT)

    def test_main_window_has_required_operational_controls_and_cleanup(self):
        for name in ("ServerUrl", "ModelId", "ConnectButton", "LifecycleState", "PromptInput", "OutputText", "GenerateButton"):
            with self.subTest(control=name):
                self.assertIn(f'x:Name="{name}"', WINDOW_XAML)
                self.assertIn(f'window.FindName("{name}")', APP_CODE)
        self.assertIn("InitializeComponent();", WINDOW_CODE)
        self.assertIn("RenderStatus(_session.Status);", WINDOW_CODE)
        self.assertIn("_session.Dispose();", WINDOW_CODE)

    def test_smoke_mode_waits_for_loaded_visible_main_window_then_exits(self):
        for marker in (
            '"--smoke-test"',
            "DispatcherPriority.ApplicationIdle",
            "window.IsLoaded",
            "window.IsVisible",
            "ShutdownMode.OnExplicitShutdown",
            "window.Close();",
            "Shutdown(0);",
            "Shutdown(1);",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, APP_CODE)

    def test_windows_ci_runs_a_bounded_process_startup_smoke_test(self):
        for marker in (
            "Smoke test WPF application startup",
            "WhiteP.Host.Windows.exe",
            '-ArgumentList "--smoke-test"',
            "WaitForExit(30000)",
            "$process.Kill()",
            "$process.ExitCode -ne 0",
            "PASS: WPF entry point displayed MainWindow",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, WORKFLOW)

    def test_status_panel_uses_live_backend_state_and_discloses_p2p_limit(self):
        self.assertIn('x:Name="NetworkState"', WINDOW_XAML)
        self.assertIn("HostStatusPresentation.FormatBackendStatus(status)", WINDOW_CODE)
        self.assertIn("HostStatusPresentation.FormatNetworkStatus(status)", WINDOW_CODE)
        self.assertIn("var connectTask = _session.ConnectAsync", WINDOW_CODE)
        self.assertIn("var generationTask = _session.GenerateAsync", WINDOW_CODE)
        presenter = (ROOT / "src/Host.Application/HostStatusPresentation.cs").read_text(encoding="utf-8")
        self.assertIn("Direct peer-to-peer transport: not implemented in this build.", presenter)

    def test_user_visible_errors_use_safe_mapping_not_exception_details(self):
        self.assertIn("HostErrorPresentation.ToUserMessage(exception)", WINDOW_CODE)
        self.assertNotIn('LifecycleState.Text = $"Error — {exception.Message}"', WINDOW_CODE)
        self.assertNotIn('OutputText.Text = exception.Message', WINDOW_CODE)
        presenter = (ROOT / "src/Host.Application/HostErrorPresentation.cs").read_text(encoding="utf-8")
        self.assertIn("Details are hidden to protect local paths and sensitive data.", presenter)
        self.assertIn("HttpRequestException", presenter)
        self.assertIn("OperationCanceledException", presenter)

    def test_window_status_starts_without_connecting_to_a_backend(self):
        self.assertIn('Text="Stopped — connect to a local backend."', WINDOW_XAML)
        self.assertNotIn(".ConnectAsync(", APP_CODE)


if __name__ == "__main__":
    unittest.main()
