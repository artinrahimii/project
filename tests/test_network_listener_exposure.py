"""Regression guards for loopback-only listener exposure and user-visible endpoint status."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LISTENER = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
RECOVERY = (ROOT / "src/Host.Application/ParticipationActivationRecovery.cs").read_text(encoding="utf-8")
RECOVERY_TESTS = (ROOT / "tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs").read_text(encoding="utf-8")
TEST_ENTRYPOINT = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")


class NetworkListenerExposureTests(unittest.TestCase):
    def test_listener_rejects_non_loopback_bind_addresses_before_binding(self):
        self.assertIn("IPAddress.IsLoopback(bindAddress)", LISTENER)
        self.assertIn("The initial host listener is restricted to loopback addresses.", LISTENER)

    def test_wpf_host_uses_loopback_and_displays_actual_port_and_status(self):
        self.assertIn("new(IPAddress.Loopback, 0)", WINDOW)
        self.assertIn("Loopback-only listener active at {endpoint.Address}:{endpoint.Port}", WINDOW)
        self.assertIn("Network participation is disabled; no listener is open.", WINDOW)
        self.assertIn("if (active && _networkParticipationLifecycle.LocalEndpoint is { } endpoint)", WINDOW)


    def test_participation_activation_failure_uses_behaviorally_tested_recovery(self):
        start = WINDOW.index("private async void NetworkParticipationToggle_Changed")
        end = WINDOW.index("private async void MainWindow_Closing", start)
        handler = WINDOW[start:end]

        self.assertIn("await ParticipationActivationRecovery.RecoverAsync(", handler)
        self.assertIn("exception,", handler)
        self.assertIn("() => _networkParticipationLifecycle.SetParticipationActiveAsync(false)", handler)
        self.assertIn("ParticipationNetworkStatus.Text = failureMessage;", handler)
        self.assertNotIn("exception.Message", handler)
        self.assertNotIn("exception.ToString()", handler)

        self.assertIn("public static async Task<string> RecoverAsync", RECOVERY)
        self.assertIn("catch (ObjectDisposedException)", RECOVERY)
        self.assertIn("catch (Exception rollbackException)", RECOVERY)
        self.assertIn("HostErrorPresentation.ToUserMessage(rollbackException)", RECOVERY)
        self.assertNotIn("rollbackException.Message", RECOVERY)
        self.assertNotIn("rollbackException.ToString()", RECOVERY)

        self.assertIn("await ParticipationActivationRecoveryTests.RunAsync();", TEST_ENTRYPOINT)
        self.assertIn("A failed rollback must produce safe recovery guidance.", RECOVERY_TESTS)
        self.assertIn("A disposed lifecycle during shutdown must not report a second cleanup failure.", RECOVERY_TESTS)

    def test_window_closing_waits_for_listener_disposal_before_closing_again(self):
        start = WINDOW.index("private async void MainWindow_Closing")
        end = WINDOW.index("private void MainWindow_Closed", start)
        closing = WINDOW[start:end]

        self.assertIn("if (_allowWindowClose)", closing)
        self.assertIn("e.Cancel = true;", closing)
        dispose = closing.index("await _networkParticipationLifecycle.DisposeAsync();")
        self.assertIn("_ = Dispatcher.BeginInvoke(", closing)
        retry_close = closing.index("_ = Dispatcher.BeginInvoke(")
        self.assertLess(dispose, retry_close)
        self.assertIn("_allowWindowClose = true;", closing)
        self.assertIn("Close();", closing)

    def test_window_closed_cancels_pending_work_and_releases_owned_resources(self):
        start = WINDOW.index("private void MainWindow_Closed")
        closed = WINDOW[start:]

        self.assertIn("_operationCancellation?.Cancel();", closed)
        self.assertIn("_modelUpdateCancellation?.Cancel();", closed)
        self.assertIn("_operationCancellation?.Dispose();", closed)
        self.assertIn("_session.Dispose();", closed)
        self.assertIn("_localInferenceProcessLauncher.Dispose();", closed)


if __name__ == "__main__":
    unittest.main()
