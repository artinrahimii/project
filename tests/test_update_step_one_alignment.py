"""Contract checks that the V1.10-approved update is reconciled into Step One."""
import hashlib
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
V1 = (ROOT / "step one" / "V1.0.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs" / "requirements.md").read_text(encoding="utf-8")
ACCEPTANCE = (ROOT / "docs" / "acceptance.md").read_text(encoding="utf-8")
TASK_LINES = re.findall(r"(?m)^\s*-\s*\[[ xX]\]\s*(T\d{4})\s+—", V1)


class UpdateStepOneAlignmentTests(unittest.TestCase):
    def test_original_legacy_task_inventory_is_unchanged(self):
        self.assertEqual(229, len(TASK_LINES))
        self.assertEqual(229, len(set(TASK_LINES)))
        digest = hashlib.sha1("\n".join(TASK_LINES).encode("utf-8")).hexdigest()
        self.assertEqual("75c65d66c98bd5bd295617fde5098d7ccdff9059", digest)

    def test_v1_addendum_records_approved_decisions_without_claiming_delivery(self):
        self.assertIn("الحاقیهٔ نیازمندی‌های مصوب V1.10", V1)
        for phrase in ("ارتباط مستقیم و احرازشدهٔ P2P", "metadata کشف میزبان", "حداکثر سه تلاش ترتیبی", "retry فقط با اقدام صریح کاربر", "یک پاسخ کامل و terminal", "همان گفت‌وگو", "به‌صورت پیش‌فرض خاموش", "یک منبع برداری اصلی"):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, V1)
        self.assertIn("به معنی پیاده‌سازی یا پذیرش محصول نیست", V1)

    def test_new_requirements_are_unique_testable_and_traceable(self):
        ids = re.findall(r"(?m)^### (REQ-\d{3}) — ", REQUIREMENTS)
        self.assertEqual(24, len(ids))
        self.assertEqual(24, len(set(ids)))
        for req_id in ("REQ-021", "REQ-022", "REQ-023", "REQ-024"):
            block_match = re.search(rf"(?ms)^### {req_id} — .*?(?=^### REQ-\d{{3}} — |^## Implementation status\s*$)", REQUIREMENTS)
            self.assertIsNotNone(block_match, req_id)
            block = block_match.group(0)
            for field in ("Requirement:", "Acceptance:", "Verification:", "Trace:"):
                self.assertIn(f"**{field}**", block)
            self.assertRegex(block, r"(?m)^- \*\*Trace:\*\* T\d{4}")

    def test_retry_and_one_shot_requirements_match_accepted_decisions(self):
        retry = REQUIREMENTS.split("### REQ-022", 1)[1].split("### REQ-023", 1)[0]
        self.assertIn("at most one", retry)
        self.assertIn("three sequential host attempts", retry)
        self.assertIn("explicitly initiated by the user", retry)
        self.assertIn("No broadcast", retry)
        stream = REQUIREMENTS.split("### REQ-023", 1)[1].split("### REQ-024", 1)[0]
        self.assertIn("one complete terminal inference response", stream)
        self.assertIn("streaming is deferred", stream)

    def test_visual_identity_uses_exact_five_tokens_and_contrast_rules(self):
        visual = REQUIREMENTS.split("### REQ-024", 1)[1].split("## Implementation status", 1)[0]
        for token, color in (("midnight_ink", "#142537"), ("porcelain", "#F4F0E8"), ("pure_white", "#FFFFFF"), ("antique_gold", "#B89A62"), ("muted_teal", "#587C80")):
            with self.subTest(token=token):
                self.assertIn(f"{token}={color}", visual)
        self.assertIn("4.5:1", visual)
        self.assertIn("3:1", visual)
        self.assertIn("muted_teal", visual)
        self.assertIn("antique_gold", visual)

    def test_acceptance_matrix_maps_chat_retry_streaming_and_visual_gates(self):
        for gate in ("AC-CHAT-01", "AC-RETRY-01", "AC-STREAM-01", "AC-VISUAL-01"):
            with self.subTest(gate=gate):
                self.assertRegex(ACCEPTANCE, rf"(?m)^\| {gate} — .*\|$")
        for mapping in ("| REQ-021 | AC-CHAT-01, AC-P2P-01, AC-AND-01 |", "| REQ-022 | AC-RETRY-01, AC-HOST-01, AC-STATUS-01, AC-FAIL-01 |", "| REQ-023 | AC-STREAM-01, AC-PROTO-01, AC-FAIL-01 |", "| REQ-024 | AC-VISUAL-01, AC-WIN-01, AC-AND-01 |"):
            self.assertIn(mapping, ACCEPTANCE)

    def test_physical_e2e_gate_rejects_emulator_and_fixture_substitutes(self):
        row = next(line for line in ACCEPTANCE.splitlines() if line.startswith("| AC-P2P-01 "))
        for phrase in ("physical Android device", "installed Windows product host", "SHA-256", "Room conversation"):
            self.assertIn(phrase, row)
        for forbidden in ("Emulator", "mock", "fixture", "test peer", "CI green alone"):
            self.assertIn(forbidden, row)


if __name__ == "__main__":
    unittest.main()
