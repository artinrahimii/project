"""Repeatable source/documentation review for claims not yet proven at runtime.

These checks detect misleading claims in the current preview. They do not
replace end-to-end network, cryptographic, or storage testing.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")
ACCEPTANCE = (ROOT / "docs/acceptance.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
ANDROID_UI = (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt").read_text(encoding="utf-8")
WINDOW_UI = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
HOST_LIFECYCLE = (ROOT / "src/Host.Windows/HostLifecycle.cs").read_text(encoding="utf-8")


class ClaimsAuditTests(unittest.TestCase):
    def test_android_ui_discloses_preview_and_does_not_claim_request_delivery(self):
        self.assertIn("P2P client preview (transport not implemented)", ANDROID_UI)
        self.assertIn("Requests are not sent yet; transport is not implemented.", ANDROID_UI)
        self.assertIn("Request not sent: no host transport yet", ANDROID_UI)

    def test_windows_ui_discloses_preview_and_unavailable_inference(self):
        self.assertIn('Title="White-P Host Preview"', WINDOW_UI)
        self.assertIn('Text="White-P — Host Preview"', WINDOW_UI)
        self.assertIn("Local inference, hardware discovery, and peer-to-peer transport are not implemented yet.", WINDOW_UI)
        self.assertIn("public sealed class UnconfiguredInferenceBackend", HOST_LIFECYCLE)
        self.assertIn("public bool IsAvailable=>false;", HOST_LIFECYCLE)
        self.assertIn("Local inference backend is not configured.", HOST_LIFECYCLE)

    def test_privacy_and_capability_statements_are_requirements_not_proven_claims(self):
        self.assertIn("الزامات طراحی‌اند، نه ادعاهای اثبات‌شده", README)
        self.assertIn("not claims about the current repository", REQUIREMENTS)
        self.assertIn("not marked implemented", REQUIREMENTS)

    def test_runtime_privacy_gates_remain_unpassed_without_runtime_evidence(self):
        for gate in ("AC-LOCAL-01", "AC-P2P-01", "AC-PRIV-01", "AC-CLAIMS-01"):
            with self.subTest(gate=gate):
                self.assertIn(gate, ACCEPTANCE)
        self.assertIn("does not prove", ACCEPTANCE)
        self.assertIn("encrypted transmission", ACCEPTANCE)
        self.assertIn("no persistent conversation storage", ACCEPTANCE)
        self.assertIn("All AC-* gates are **NOT RUN for the product**", ACCEPTANCE)

    def test_acceptance_document_matches_existing_platform_projects(self):
        self.assertIn("src/Host.Windows/", README)
        self.assertIn("src/Client.Android/", README)
        self.assertIn("does not establish real local inference", ACCEPTANCE)
        self.assertNotIn("there is no Windows host project", ACCEPTANCE)


if __name__ == "__main__":
    unittest.main()
