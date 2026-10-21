"""Contract checks for the final release acceptance matrix (not product runtime tests)."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ACCEPTANCE = (ROOT / "docs" / "acceptance.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs" / "requirements.md").read_text(encoding="utf-8")


class AcceptanceContractTests(unittest.TestCase):
    def test_matrix_defines_windows_android_and_direct_p2p_gates(self):
        for gate in ("AC-WIN-01", "AC-WIN-02", "AC-AND-01", "AC-P2P-01", "AC-CI-01"):
            with self.subTest(gate=gate):
                self.assertIn(gate, ACCEPTANCE)

    def test_every_requirement_has_an_explicit_acceptance_gate_mapping(self):
        requirement_ids = re.findall(r"(?m)^### (REQ-\d{3}) — ", REQUIREMENTS)
        self.assertEqual(len(requirement_ids), 20)
        self.assertEqual(len(requirement_ids), len(set(requirement_ids)))
        traceability = ACCEPTANCE.split("### Requirement-to-gate traceability", 1)[1]
        for requirement_id in requirement_ids:
            with self.subTest(requirement=requirement_id):
                self.assertRegex(traceability, rf"(?m)^\| {requirement_id} \| [A-Z0-9-]+(?:, [A-Z0-9-]+)* \|$")

    def test_release_gate_requires_real_evidence_and_fails_closed(self):
        for phrase in ("NOT RUN", "blocks release acceptance", "actual implementation",
                       "clean-checkout", "network trace", "mocked-only end-to-end path"):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, ACCEPTANCE)

    def test_document_does_not_claim_platform_or_p2p_implementation(self):
        self.assertIn("not yet passed by the product", ACCEPTANCE)
        self.assertIn("All AC-* gates are **NOT RUN for the product**", ACCEPTANCE)
        self.assertIn("cannot establish that the Windows host, Android client, local inference, or direct P2P transport works", ACCEPTANCE)

    def test_each_acceptance_gate_has_verification_and_failure_condition(self):
        rows = re.findall(r"(?m)^\| (AC-[A-Z0-9-]+) — .*? \| (.*?) \| (.*?) \| (.*?) \|$", ACCEPTANCE)
        self.assertGreaterEqual(len(rows), 15)
        for gate, evidence, verification, failure in rows:
            with self.subTest(gate=gate):
                self.assertTrue(evidence.strip())
                self.assertTrue(verification.strip())
                self.assertTrue(failure.strip())


if __name__ == "__main__":
    unittest.main()
