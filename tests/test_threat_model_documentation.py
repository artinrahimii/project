"""Source/documentation contract for the current threat model and anonymity limits.

These tests verify that claims in docs/security.md stay tied to concrete
repository controls. They are not a network privacy or anonymity acceptance test.
"""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
SECURITY = (DOCS / "security.md").read_text(encoding="utf-8")
START = "## مدل تهدید و محدودیت ناشناس‌بودن (T0188)"
START_INDEX = SECURITY.index(START)
NEXT_INDEX = SECURITY.find("\n## ", START_INDEX + len(START))
SECTION = SECURITY[START_INDEX:] if NEXT_INDEX < 0 else SECURITY[START_INDEX:NEXT_INDEX]


class ThreatModelDocumentationTests(unittest.TestCase):
    def test_threat_model_names_the_actual_observers_and_assets(self):
        for term in (
            "PeerId",
            "prompt",
            "context capsule",
            "rendezvous",
            "Noise/TLS",
            "IP/port",
            "همتای پردازشگر",
            "سیستم‌عامل",
        ):
            with self.subTest(term=term):
                self.assertIn(term, SECTION)

    def test_anonymity_and_metadata_limits_are_not_overstated(self):
        for statement in (
            "ناشناس‌ساز شبکه نیست",
            "غیرقابل‌پیوندبودن",
            "traffic padding",
            "onion routing",
            "circuit-relay",
            "PeerId یک شناسهٔ پایدار",
            "نشانی‌های مستقیم",
        ):
            with self.subTest(statement=statement):
                self.assertIn(statement, SECTION)
        self.assertIn("AC-P2P-01", SECTION)
        self.assertIn("AC-PRIV-01", SECTION)
        self.assertIn("NOT RUN", SECTION)

    def test_documentation_is_linked_to_existing_implementation_and_decisions(self):
        links = re.findall(r"\[[^\]]+\]\(([^)]+)\)", SECTION)
        local_links = [
            target.split("#", 1)[0]
            for target in links
            if not target.startswith(("http://", "https://", "mailto:", "#"))
        ]
        self.assertGreaterEqual(len(local_links), 15)
        for target in local_links:
            with self.subTest(target=target):
                path = (DOCS / target).resolve()
                self.assertTrue(path.is_file(), f"Broken repository link: {target}")

    def test_directory_contract_and_limits_match_runtime_source(self):
        announcement = (
            ROOT / "src/Shared.Contracts/Networking/PeerAnnouncement.cs"
        ).read_text(encoding="utf-8")
        directory = (
            ROOT / "src/Rendezvous.Service/PeerAnnouncementDirectory.cs"
        ).read_text(encoding="utf-8")
        android_directory = (
            ROOT
            / "src/Client.Android/app/src/main/java/com/whitep/client/network/"
            "RendezvousPeerDirectoryClient.kt"
        ).read_text(encoding="utf-8")
        self.assertIn("MaximumAddresses = 8", announcement)
        self.assertIn("MaximumCapabilities = 32", announcement)
        self.assertIn("MaximumLifetime = TimeSpan.FromHours(24)", announcement)
        self.assertIn('"/p2p-circuit"', announcement)
        self.assertIn("MaximumActivePeers = 2048", directory)
        self.assertIn("Dictionary<string, PeerAnnouncement>", directory)
        self.assertIn("metadata-only", android_directory)
        self.assertIn("neither sent to nor downloaded", android_directory)

    def test_existing_controls_and_acceptance_caveats_are_linked(self):
        required = (
            "../src/Shared.Security/AuthenticatedRequestReplayGuard.cs",
            "../src/Shared.Security/SafeDiagnosticLog.cs",
            "../src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt",
            "decisions/ADR-0004-encrypted-relay-boundary.md",
            "decisions/ADR-0019-host-visible-inference-content.md",
            "decisions/ADR-0020-android-client-local-conversation-storage.md",
        )
        for target in required:
            with self.subTest(target=target):
                self.assertIn(f"]({target})", SECTION)


if __name__ == "__main__":
    unittest.main()
