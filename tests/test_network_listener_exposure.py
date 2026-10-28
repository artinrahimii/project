"""Regression guards for authenticated peer-host exposure and safe shutdown status."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LISTENER = (ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
WINDOW_XAML = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
RECOVERY = (ROOT / "src/Host.Application/ParticipationActivationRecovery.cs").read_text(encoding="utf-8")
RECOVERY_TESTS = (ROOT / "tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs").read_text(encoding="utf-8")
TEST_ENTRYPOINT = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")


class NetworkListenerExposureTests(unittest.TestCase):
    def test_legacy_listener_remains_loopback_only(self):
        self.assertIn("IPAddress.IsLoopback(bindAddress)", LISTENER)
        self.assertIn("The initial host listener is restricted to loopback addresses.", LISTENER)

    def test_wpf_exposes_authenticated_peer_host_configuration(self):
        for name in (
            "PeerRendezvousUrl",
            "PeerBindAddress",
            "PeerAdvertisedAddress",
            "PeerPort",
            "PeerTrustedIdentity",
            "PeerReachabilityConfirmed",
        ):
            with self.subTest(control=name):
                self.assertIn(f'x:Name="{name}"', WINDOW_XAML)

        self.assertIn("new PeerAnnouncementHostLifecycle(", WINDOW)
        self.assertIn("await service.StartAsync();", WINDOW)
        self.assertIn("PeerReachabilityConfirmed.IsChecked != true", WINDOW)
        self.assertIn("TrustPeerAfterExplicitConfirmation(expectedPeerIdentity, explicitlyConfirmed: true)", WINDOW)
        self.assertIn("this app does not independently verify public Internet reachability", WINDOW.lower())
        self.assertIn("Peer hosting is disabled. The last rendezvous lease is no longer renewed", WINDOW)
        self.assertIn("loopback http is accepted only for integration testing", WINDOW_XAML.lower())
        # The legacy listener class remains loopback-only; the WPF peer-host path no longer uses its random loopback endpoint.
        self.assertNotIn("new(IPAddress.Loopback, 0)", WINDOW)

    def test_peer_host_activation_failure_stops_partial_service_and_hides_exception_details(self):
        start = WINDOW.index("private async void NetworkParticipationToggle_Changed")
        end = WINDOW.index("private async Task<byte[]> HandlePeerInferenceMessageAsync", start)
        handler = WINDOW[start:end]

        self.assertIn("catch (Exception exception)", handler)
        self.assertIn("var failedService = _peerParticipationService;", handler)
        self.assertIn("await failedService.DisposeAsync();", handler)
        self.assertIn("NetworkParticipationToggle.IsChecked = false;", handler)
        self.assertIn("HostErrorPresentation.ToUserMessage(exception)", handler)
        self.assertNotIn("exception.Message", handler)
        self.assertNotIn("exception.ToString()", handler)

        # Keep the older recovery helper's own behaviorally tested rollback path covered while
        # it remains in the application layer; the WPF peer-host path has its own disposal boundary.
        self.assertIn("public static async Task<string> RecoverAsync", RECOVERY)
        self.assertIn("catch (ObjectDisposedException)", RECOVERY)
        self.assertIn("catch (Exception rollbackException)", RECOVERY)
        self.assertIn("HostErrorPresentation.ToUserMessage(rollbackException)", RECOVERY)
        self.assertNotIn("rollbackException.Message", RECOVERY)
        self.assertNotIn("rollbackException.ToString()", RECOVERY)
        self.assertIn("await ParticipationActivationRecoveryTests.RunAsync();", TEST_ENTRYPOINT)
        self.assertIn("A failed rollback must produce safe recovery guidance.", RECOVERY_TESTS)
        self.assertIn("A disposed lifecycle during shutdown must not report a second cleanup failure.", RECOVERY_TESTS)

    def test_window_closing_waits_for_peer_service_disposal_before_closing_again(self):
        start = WINDOW.index("private async void MainWindow_Closing")
        end = WINDOW.index("private void MainWindow_Closed", start)
        closing = WINDOW[start:end]

        self.assertIn("if (_allowWindowClose)", closing)
        self.assertIn("e.Cancel = true;", closing)
        dispose = closing.index("await _peerParticipationService.DisposeAsync();")
        self.assertIn("_ = Dispatcher.BeginInvoke(", closing)
        retry_close = closing.index("_ = Dispatcher.BeginInvoke(")
        self.assertLess(dispose, retry_close)
        self.assertIn("_peerParticipationService = null;", closing)
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
