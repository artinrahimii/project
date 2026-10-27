"""Regression guards for loopback-only listener exposure and user-visible endpoint status."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LISTENER = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")


class NetworkListenerExposureTests(unittest.TestCase):
    def test_listener_rejects_non_loopback_bind_addresses_before_binding(self):
        self.assertIn("IPAddress.IsLoopback(bindAddress)", LISTENER)
        self.assertIn("The initial host listener is restricted to loopback addresses.", LISTENER)

    def test_wpf_host_uses_loopback_and_displays_actual_port_and_status(self):
        self.assertIn("new(IPAddress.Loopback, 0)", WINDOW)
        self.assertIn("Loopback-only listener active at {endpoint.Address}:{endpoint.Port}", WINDOW)
        self.assertIn("Network participation is disabled; no listener is open.", WINDOW)
        self.assertIn("if (active && _networkParticipationLifecycle.LocalEndpoint is { } endpoint)", WINDOW)


    def test_participation_activation_failure_contains_rollback_failure_safely(self):
        start = WINDOW.index("private async void NetworkParticipationToggle_Changed")
        end = WINDOW.index("private async void MainWindow_Closing", start)
        handler = WINDOW[start:end]

        primary_catch = handler.index("catch (Exception exception)")
        rollback = handler.index("await _networkParticipationLifecycle.SetParticipationActiveAsync(false);", primary_catch)
        disposed_catch = handler.index("catch (ObjectDisposedException)", rollback)
        rollback_catch = handler.index("catch (Exception rollbackException)", disposed_catch)
        self.assertLess(primary_catch, rollback)
        self.assertLess(rollback, disposed_catch)
        self.assertLess(disposed_catch, rollback_catch)
        self.assertIn("string rollbackFailureMessage = string.Empty;", handler)
        self.assertIn("HostErrorPresentation.ToUserMessage(rollbackException)", handler)
        self.assertIn("Listener cleanup could not be confirmed. Restart the host before retrying.", handler)
        self.assertIn("{HostErrorPresentation.ToUserMessage(exception)}{rollbackFailureMessage}", handler)
        self.assertNotIn("rollbackException.Message", handler)
        self.assertNotIn("rollbackException.ToString()", handler)

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
