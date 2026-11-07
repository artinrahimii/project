from __future__ import annotations

import json
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GRAPH_PATH = ROOT / "update" / "Step two" / "project-graph.md"
QUALITY_PATH = ROOT / ".github" / "workflows" / "quality.yml"


def load_update_graph() -> dict:
    markdown = GRAPH_PATH.read_text(encoding="utf-8")
    for match in re.finditer(r"```json\\s*([\\s\\S]*?)```", markdown, re.IGNORECASE):
        try:
            candidate = json.loads(match.group(1))
        except json.JSONDecodeError:
            continue
        if isinstance(candidate, dict) and isinstance(candidate.get("nodes"), list) and isinstance(candidate.get("edges"), list):
            return candidate
    raise AssertionError("No fenced Update graph JSON with nodes and edges was found.")


class UpdateGraphContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.graph = load_update_graph()
        cls.nodes = {node["id"]: node for node in cls.graph["nodes"]}

    def test_graph_size_and_commit_references_are_consistent(self) -> None:
        self.assertEqual(24, len(self.graph["nodes"]))
        self.assertEqual(80, len(self.graph["edges"]))
        self.assertEqual("V 3.1.13", self.graph["commit_intent"])
        base = self.graph["base_repository"]
        self.assertEqual("827d1586e367d8495ef052ac71a8cc83db0f4680", base["base_commit_sha"])
        self.assertEqual("8fa9aa812aeb49086a6d55e92ba17586534135a4", base["product_code_baseline_sha"])
        inputs = " ".join(self.nodes["UPD-S1-001"]["inputs"])
        self.assertIn(base["base_commit_sha"], inputs)
        self.assertIn(base["product_code_baseline_sha"], inputs)
        self.assertNotIn("6f70586b4b7ff69c463f7e5b02293090c63e78ec", inputs)

    def test_update_graph_is_validated_in_ci_in_both_modes(self) -> None:
        workflow = QUALITY_PATH.read_text(encoding="utf-8")
        self.assertIn('python tools/graph_guard.py "update/Step two/project-graph.md"', workflow)
        self.assertIn('python tools/graph_guard.py "update/Step two/project-graph.md" --repo-root .', workflow)

    def test_legacy_task_reconciliation_fails_closed(self) -> None:
        sync = self.graph["legacy_task_sync"]
        self.assertEqual("blocked", sync["state"])
        self.assertEqual("SOURCE_STEP_THREE_GRAPH_EMPTY", sync["reason_code"])
        expected = sorted({task_id for node in self.graph["nodes"] for task_id in node.get("affected_task_ids", [])})
        self.assertEqual(expected, sync["affected_legacy_task_ids"])
        contract = self.nodes["UPD-S1-008"]["legacy_task_reconciliation"]
        self.assertTrue(contract["required"])
        self.assertTrue(contract["require_one_record_per_id"])
        self.assertTrue(contract["completion_requires_valid_step_three_graph"])
        self.assertTrue(contract["forbid_automatic_status_transition_from_traceability_only"])

    def test_real_product_e2e_rejects_fixtures(self) -> None:
        contract = self.nodes["UPD-S1-020"]["verification_contract"]
        self.assertTrue(contract["must_start_from_android_chat_ui"])
        self.assertTrue(contract["must_use_physical_android_device"])
        self.assertTrue(contract["must_use_installed_windows_product_host"])
        self.assertTrue(contract["must_execute_loaded_local_model"])
        self.assertTrue(contract["must_persist_response_in_same_room_conversation"])
        exclusions = " ".join(contract["prohibited_as_primary_acceptance_evidence"])
        self.assertIn("dotnet-host-response", exclusions)
        self.assertIn("AndroidWindowsLanIntegrationTest.kt", exclusions)
        self.assertIn("real-inference.yml", exclusions)

    def test_upgrade_acceptance_covers_both_platforms_and_persistent_data(self) -> None:
        contract = self.nodes["UPD-S1-021"]["upgrade_contract"]
        self.assertTrue(contract["required"])
        self.assertEqual(["Android", "Windows"], contract["platforms"])
        self.assertTrue(contract["pre_upgrade_post_upgrade_snapshot_required"])
        self.assertTrue(contract["uninstall_or_clear_data_prohibited"])
        domains = " ".join(contract["data_domains_must_survive_or_be_migrated"])
        for expected in ("Room", "Keystore", "host identity", "peer trust", "local model"):
            self.assertIn(expected, domains)
        gates = {gate["id"] for gate in self.graph["product_acceptance_gates"]}
        self.assertIn("GATE-UPGRADE-01", gates)
        self.assertIn("GATE-LEGACY-RECONCILIATION-01", gates)


if __name__ == "__main__":
    unittest.main()
