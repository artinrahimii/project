"""Source-contract checks for explicit peer privacy consent in both product UIs."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ANDROID_MAIN = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt"
).read_text(encoding="utf-8")
WINDOW_CODE = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
SHARED_DISCLOSURE = (
    ROOT / "src/Shared.Security/PeerInferencePrivacyDisclosure.cs"
).read_text(encoding="utf-8")
SECURITY_DOC = (ROOT / "docs/security.md").read_text(encoding="utf-8")
HOST_TESTS = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")


class PeerInferencePrivacyDisclosureTests(unittest.TestCase):
    def test_android_trust_dialog_discloses_the_selected_peer_before_saving_trust(self):
        dialog = ANDROID_MAIN.split("pendingTrustPeerId?.let", 1)[1]
        self.assertIn("Peer ID: $peerId", dialog)
        self.assertIn("context capsule", dialog)
        self.assertIn("after transport decryption", dialog)
        self.assertIn("does not hide request content", dialog)
        self.assertIn("Saving trust does not send a prompt or context now", dialog)
        self.assertLess(
            dialog.index("after transport decryption"),
            dialog.index("requestViewModel.trustPeerAfterExplicitConfirmation(peerId)"),
        )
        self.assertNotIn("sendRequest(", dialog)

    def test_windows_privacy_consent_is_required_before_peer_service_creation(self):
        start = WINDOW_CODE.index("private async void NetworkParticipationToggle_Changed")
        end = WINDOW_CODE.index("private async Task<", start)
        handler = WINDOW_CODE[start:end]
        self.assertIn("PeerInferencePrivacyDisclosure.BuildHostParticipationMessage(expectedPeerIdentity)", handler)
        self.assertIn("MessageBoxButton.YesNo", handler)
        self.assertIn("MessageBoxImage.Warning", handler)
        self.assertIn("MessageBoxResult.No", handler)
        self.assertLess(
            handler.index("PeerInferencePrivacyDisclosure.BuildHostParticipationMessage(expectedPeerIdentity)"),
            handler.index("new PeerAnnouncementHostLifecycle("),
        )
        self.assertLess(
            handler.index("if (privacyConsent != MessageBoxResult.Yes)"),
            handler.index("new PeerAnnouncementHostLifecycle("),
        )
        self.assertIn("NetworkParticipationToggle.IsChecked = false;", handler)
        self.assertIn("Peer hosting remains disabled because the privacy notice was not accepted.", handler)

    def test_shared_windows_notice_and_host_acceptance_test_cover_payload_visibility(self):
        self.assertIn("HostParticipationTitle", SHARED_DISCLOSURE)
        for marker in (
            "BuildHostParticipationMessage",
            "context capsule",
            "after transport decryption",
            "does not hide request content",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, SHARED_DISCLOSURE)
                self.assertIn(marker, HOST_TESTS)

    def test_security_documentation_matches_the_ui_consent_boundary(self):
        self.assertIn("## رضایت آگاهانه برای اعتماد و میزبانی همتا (T0186)", SECURITY_DOC)
        self.assertIn("prompt", SECURITY_DOC.lower())
        self.assertIn("context capsule", SECURITY_DOC.lower())
        self.assertIn("رمزگذاری انتقال", SECURITY_DOC)
        self.assertIn("T0185 — تکمیل‌شده", SECURITY_DOC)


if __name__ == "__main__":
    unittest.main()
