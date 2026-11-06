"""Contract checks for the selected direct-P2P architecture.

These tests verify the written decision and its traceability. They do not
implement or prove actual peer-to-peer transport.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DECISION = (ROOT / "docs/decisions/ADR-0002-direct-p2p-data-path.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
README = (ROOT / "README.md").read_text(encoding="utf-8")
ACCEPTANCE = (ROOT / "docs/acceptance.md").read_text(encoding="utf-8")


class DirectP2PDecisionTests(unittest.TestCase):
    def test_decision_matches_the_direct_data_path_requirement(self):
        self.assertIn("Direct peer-to-peer transfer is the default data path.", REQUIREMENTS)
        self.assertIn("Direct peer-to-peer transfer is the default data path.", DECISION)
        self.assertIn("REQ-010", DECISION)
        self.assertIn("REQ-011", DECISION)

    def test_rendezvous_is_control_plane_only_and_relay_fallback_is_disallowed(self):
        decision = DECISION.lower()
        self.assertIn("rendezvous", decision)
        self.assertIn("must not relay inference prompts or responses", decision)
        self.assertIn("must fail with an explicit connection-unavailable outcome", decision)
        self.assertIn("must not silently fall back to a server-relayed inference payload", decision)

    def test_documentation_links_the_decision_without_claiming_transport_exists(self):
        # The README's statement is Persian, so assert the actual source wording
        # rather than requiring English terms that are not present in that text.
        self.assertIn("docs/decisions/ADR-0002-direct-p2p-data-path.md", README)
        self.assertIn("انتقال p2p", README.lower())
        self.assertIn("هنوز موجود نیست", README)
        self.assertIn("این تصمیم هنوز پیاده‌سازی نیست", README)
        self.assertIn("AC-P2P-01", ACCEPTANCE)
        self.assertIn("not an implementation claim", ACCEPTANCE.lower())

    def test_decision_preserves_runtime_acceptance_as_not_run(self):
        self.assertIn("AC-P2P-01", DECISION)
        self.assertIn("NOT RUN", DECISION)
        self.assertIn("does not prove runtime routing", DECISION)


if __name__ == "__main__":
    unittest.main()
