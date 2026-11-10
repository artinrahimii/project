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
PEER_HOST_LIFECYCLE = (ROOT / "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs").read_text(encoding="utf-8")
PEER_TRUST_STORE = (ROOT / "src/Shared.Security/PeerTrustStore.cs").read_text(encoding="utf-8")


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
        self.assertIn("await service.StartAsync(cancellationToken);", WINDOW)
        self.assertIn("PeerReachabilityConfirmed.IsChecked != true", WINDOW)
        self.assertIn("TrustPeerAfterExplicitConfirmation(expectedPeerIdentity, explicitlyConfirmed: true)", WINDOW)
        self.assertIn("this app does not independently verify public internet reachability", WINDOW.lower())
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
        cancel_local = closing.index("_operationCancellation?.Cancel();")
        cancel_update = closing.index("_modelUpdateCancellation?.Cancel();")
        dispose = closing.index("await peerService.DisposeAsync();")
        stop_owned_engine = closing.index("_localInferenceProcessLauncher.Stop();")
        dispose_session = closing.index("await _session.DisposeAsync();")
        wait_update = closing.index("await _modelUpdateTask;")
        wait_generation = closing.index("await _generationTask;")
        retry_close = closing.index("_ = Dispatcher.BeginInvoke(")
        self.assertLess(cancel_local, dispose)
        self.assertLess(cancel_update, dispose)
        self.assertLess(dispose, stop_owned_engine)
        self.assertLess(stop_owned_engine, dispose_session)
        self.assertLess(dispose_session, wait_update)
        self.assertLess(wait_update, wait_generation)
        self.assertLess(wait_generation, retry_close)
        self.assertIn("_peerParticipationService = null;", closing)
        self.assertIn("_allowWindowClose = true;", closing)
        self.assertIn("Close();", closing)

    def test_window_closing_cancels_and_awaits_local_work_before_async_session_disposal(self):
        closing = WINDOW.split("private async void MainWindow_Closing", 1)[1].split(
            "private void MainWindow_Closed", 1
        )[0]
        self.assertIn("_windowClosing = true;", closing)
        self.assertIn("_operationCancellation?.Cancel();", closing)
        self.assertIn("_modelUpdateCancellation?.Cancel();", closing)
        self.assertIn("await _modelUpdateTask;", closing)
        self.assertIn("await _generationTask;", closing)
        self.assertIn("await _session.DisposeAsync();", closing)
        self.assertNotIn("_session.Dispose();", closing)

    def test_window_closed_cancels_pending_work_and_releases_owned_resources(self):
        start = WINDOW.index("private void MainWindow_Closed")
        closed = WINDOW[start:]
        self.assertNotIn("_operationCancellation?.Cancel();", closed)
        self.assertNotIn("_modelUpdateCancellation?.Cancel();", closed)
        self.assertIn("_operationCancellation?.Dispose();", closed)
        self.assertIn("_modelUpdateCancellation?.Dispose();", closed)
        self.assertNotIn("_session.Dispose();", closed)
        self.assertIn("_localInferenceProcessLauncher.Dispose();", closed)


    def test_window_closing_cancels_and_drains_peer_participation_transition(self):
        self.assertIn("_peerParticipationTask", WINDOW)
        self.assertIn("_peerParticipationCancellation?.Cancel();", WINDOW)
        self.assertIn("if (_suppressParticipationToggleChanged || _windowClosing || _peerParticipationTask is not null)", WINDOW)
        closing = WINDOW.split("private async void MainWindow_Closing", 1)[1].split(
            "private void MainWindow_Closed", 1
        )[0]
        cancel_transition = closing.index("_peerParticipationCancellation?.Cancel();")
        wait_transition = closing.index("await _peerParticipationTask;")
        dispose_service = closing.index("await peerService.DisposeAsync();")
        self.assertLess(cancel_transition, wait_transition)
        self.assertLess(wait_transition, dispose_service)

        transition = WINDOW.split("private async Task ChangeNetworkParticipationAsync", 1)[1].split(
            "private async Task<WhiteP.Shared.Contracts.V1.HostStatus> GetPeerRuntimeStatusAsync", 1
        )[0]
        self.assertIn("await service.StartAsync(cancellationToken);", transition)
        self.assertIn("await previous.DisposeAsync();", transition)
        self.assertIn("cancellationToken.ThrowIfCancellationRequested();", transition)

    def test_peer_shutdown_closes_request_admission_before_cancelling_and_draining(self):
        stop = PEER_HOST_LIFECYCLE.split("public async Task StopAsync()", 1)[1].split(
            "private async Task ReceiveLoopAsync", 1
        )[0]
        self.assertIn("lock (_requestAdmissionSync)", stop)
        self.assertIn("Volatile.Write(ref _active, 0);", stop)
        close_admission = stop.index("Volatile.Write(ref _active, 0);")
        cancel_receive = stop.index("lifetime?.Cancel();")
        stop_publisher = stop.index("await publisher.StopAsync()")
        self.assertLess(close_admission, cancel_receive)
        self.assertLess(cancel_receive, stop_publisher)

        receive = PEER_HOST_LIFECYCLE.split("private async Task ReceiveLoopAsync", 1)[1].split(
            "private static async Task SendAndClearMessageAsync", 1
        )[0]
        admission_lock = receive.index("lock (_requestAdmissionSync)")
        closed_guard = receive.index("if (!IsActive || cancellationToken.IsCancellationRequested)")
        slot_check = receive.index("!_requestHandlerSlots.Wait(0)")
        request_register = receive.index("_activeRequestCancellations.TryAdd(key, requestCancellation)")
        handler_register = receive.index("_requestHandlerTasks.TryAdd(key, responseTask)")
        self.assertLess(admission_lock, closed_guard)
        self.assertLess(closed_guard, slot_check)
        self.assertLess(slot_check, request_register)
        self.assertLess(request_register, handler_register)

    def test_windows_peer_trust_revocation_requires_stopped_host_and_explicit_confirmation(self):
        self.assertIn('x:Name="RevokePeerTrustButton"', WINDOW_XAML)
        self.assertIn('Click="RevokePeerTrustButton_Click"', WINDOW_XAML)
        self.assertIn("public bool RevokeTrust(string peerId)", PEER_TRUST_STORE)
        start = WINDOW.index("private void RevokePeerTrustButton_Click")
        end = WINDOW.index("private async Task<WhiteP.Shared.Contracts.V1.HostStatus>", start)
        handler = WINDOW[start:end]
        self.assertIn("_peerParticipationTask is not null", handler)
        self.assertIn("_peerParticipationService is not null", handler)
        self.assertIn("NetworkParticipationToggle.IsChecked == true", handler)
        self.assertIn("MessageBoxResult.No", handler)
        self.assertIn("trustStore.RevokeTrust(peerId)", handler)
        self.assertNotIn("exception.Message", handler)
        self.assertNotIn("exception.ToString()", handler)

if __name__ == "__main__":
    unittest.main()
