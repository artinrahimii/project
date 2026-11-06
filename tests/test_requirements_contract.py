import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REQUIREMENTS = (ROOT / "docs" / "requirements.md").read_text(encoding="utf-8")
REQUIREMENT_BLOCKS = re.findall(
    r"(?ms)^### (REQ-\d{3}) — .*?(?=^### REQ-\d{3} — |^## Implementation status\s*$)",
    REQUIREMENTS,
)
REQUIREMENT_IDS = re.findall(r"(?m)^### (REQ-\d{3}) — ", REQUIREMENTS)


class RequirementsContractTests(unittest.TestCase):
    def test_requirements_have_unique_ids_and_sufficient_coverage(self):
        self.assertGreaterEqual(len(REQUIREMENT_IDS), 15)
        self.assertEqual(len(REQUIREMENT_IDS), len(set(REQUIREMENT_IDS)))
        self.assertEqual(len(REQUIREMENT_BLOCKS), len(REQUIREMENT_IDS))

    def test_each_requirement_has_acceptance_verification_and_traceability(self):
        for match in re.finditer(
            r"(?ms)^### (REQ-\d{3}) — .*?(?=^### REQ-\d{3} — |^## Implementation status\s*$)",
            REQUIREMENTS,
        ):
            block = match.group(0)
            req_id = match.group(1)
            with self.subTest(requirement=req_id):
                self.assertRegex(block, r"(?m)^- \*\*Requirement:\*\* .+")
                self.assertRegex(block, r"(?m)^- \*\*Acceptance:\*\* .+")
                self.assertRegex(block, r"(?m)^- \*\*Verification:\*\* .+")
                self.assertRegex(block, r"(?m)^- \*\*Trace:\*\* T\d{4}")

    def test_core_product_constraints_are_explicit(self):
        for phrase in (
            "Windows host application",
            "Android client application",
            "Local inference only",
            "One active inference per host",
            "Direct P2P data path",
            "No central inference API or conversation history",
            "Temporary request identity",
            "Versioned cross-platform protocol",
            "Reproducible builds and quality gates",
        ):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, REQUIREMENTS)

    def test_specification_does_not_claim_implementation(self):
        self.assertIn("implementation is not implied", REQUIREMENTS)
        self.assertIn("not marked implemented", REQUIREMENTS)
        self.assertIn("Python graph checks alone do not satisfy this requirement", REQUIREMENTS)


if __name__ == "__main__":
    unittest.main()
