from __future__ import annotations

import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UPDATE_GRAPH_PATH = ROOT / "update" / "Step two" / "project-graph.md"
LEGACY_GRAPH_PATH = ROOT / "step three" / "project-graph.md"
RECONCILIATION_PATH = ROOT / "update" / "Step two" / "legacy-task-reconciliation.json"
QUALITY_PATH = ROOT / ".github" / "workflows" / "quality.yml"


def load_graph(path: Path) -> dict:
    markdown = path.read_text(encoding="utf-8")
    fence = chr(96) * 3
    opening = fence + "json"
    start = markdown.find(opening)
    if start < 0:
        raise AssertionError(f"No fenced JSON graph found in {path}.")
    start += len(opening)
    end = markdown.find(fence, start)
    if end < 0:
        raise AssertionError(f"JSON graph fence is not closed in {path}.")
    graph = json.loads(markdown[start:end].strip())
    if not isinstance(graph, dict) or not isinstance(graph.get("nodes"), list) or not isinstance(graph.get("edges"), list):
        raise AssertionError(f"Invalid graph structure in {path}.")
    return graph


class UpdateGraphContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.graph = load_graph(UPDATE_GRAPH_PATH)
        cls.legacy = load_graph(LEGACY_GRAPH_PATH)
        cls.reconciliation = json.loads(RECONCILIATION_PATH.read_text(encoding="utf-8"))
        cls.nodes = {node["id"]: node for node in cls.graph["nodes"]}
        cls.legacy_nodes = {node["id"]: node for node in cls.legacy["nodes"]}

    def test_graph_size_and_commit_references_are_consistent(self) -> None:
        self.assertEqual(24, len(self.graph["nodes"]))
        self.assertEqual(80, len(self.graph["edges"]))
        self.assertTrue(self.graph["commit_intent"].startswith("V 3.1."))
        self.assertTrue(self.graph["commit_intent"][6:].isdigit())
        base = self.graph["base_repository"]
        self.assertEqual("827d1586e367d8495ef052ac71a8cc83db0f4680", base["base_commit_sha"])
        self.assertEqual("8fa9aa812aeb49086a6d55e92ba17586534135a4", base["product_code_baseline_sha"])
        inputs = " ".join(self.nodes["UPD-S1-001"]["inputs"])
        self.assertNotIn("6f70586b4b7ff69c463f7e5b02293090c63e78ec", inputs)

    def test_update_graph_is_validated_in_ci_in_both_modes(self) -> None:
        workflow = QUALITY_PATH.read_text(encoding="utf-8")
        self.assertIn('python tools/graph_guard.py "update/Step two/project-graph.md"', workflow)
        self.assertIn('python tools/graph_guard.py "update/Step two/project-graph.md" --repo-root .', workflow)

    def test_all_legacy_impact_ids_are_mapped_and_review_ledger_is_consistent(self) -> None:
        sync = self.graph["legacy_task_sync"]
        self.assertIn(sync["state"], {"review_pending", "reviewed"})
        self.assertEqual("valid_parseable_graph", sync["source_graph_state_at_checkpoint"])
        self.assertEqual(168, sync["affected_task_id_count"])
        self.assertEqual(168, sync["matched_legacy_task_id_count"])
        self.assertEqual([], sync["missing_legacy_task_ids"])

        expected_update_ids: dict[str, list[str]] = {}
        for update_task in self.graph["nodes"]:
            for task_id in update_task.get("affected_task_ids", []):
                expected_update_ids.setdefault(task_id, []).append(update_task["id"])
        impacted = set(expected_update_ids)
        self.assertEqual(impacted, set(sync["affected_legacy_task_ids"]))
        self.assertEqual(168, len(impacted))

        records = self.reconciliation["records"]
        records_by_id = {record["legacy_task_id"]: record for record in records}
        self.assertEqual(168, len(records))
        self.assertEqual(168, len(records_by_id))
        self.assertEqual(impacted, set(records_by_id))
        reviewed = sum(record["review_status"] == "reviewed" for record in records)
        pending = sum(record["review_status"] == "pending" for record in records)
        self.assertEqual(168, reviewed + pending)
        self.assertEqual({"reviewed": reviewed, "pending": pending}, sync["review_status_summary"])
        self.assertEqual("pending" if sync["state"] == "review_pending" else "reviewed", self.reconciliation["review_state"])

        legacy_reconciliation = self.legacy["update_reconciliation"]
        legacy_state = legacy_reconciliation["state"]
        self.assertIn(legacy_state, {"pending_review", "reviewed"})
        for task_id in sorted(impacted):
            node = self.legacy_nodes[task_id]
            marker = node["update_impact"]
            record = records_by_id[task_id]
            self.assertEqual(node["status"], marker["legacy_status_at_review_start"])
            self.assertFalse(marker["legacy_status_changed"])
            self.assertEqual(node["status"], record["baseline_status"])
            self.assertEqual(record["baseline_status"], marker["legacy_status_at_review_start"])
            self.assertEqual(expected_update_ids[task_id], record["update_task_ids"])
            self.assertEqual(record["update_task_ids"], marker["update_task_ids"])
            self.assertTrue(record["evidence"], task_id)
            self.assertIn(record["decision"], {"re_review_required", "unchanged"})
            self.assertRegex(record.get("review_commit_sha", ""), r"^[0-9a-f]{40}$")
            self.assertEqual("reviewed", record["review_status"])
            review_docs = [
                evidence for evidence in record["evidence"]
                if evidence.startswith("update/Step three/UPD-S1-008/evidence/review-") and evidence.endswith(".md")
            ]
            self.assertTrue(review_docs, task_id)
            self.assertTrue(
                any(task_id in (ROOT / evidence).read_text(encoding="utf-8") for evidence in review_docs),
                f"no content-review document mentions {task_id}",
            )
            for evidence in record["evidence"]:
                if "://" in evidence:
                    self.assertTrue(evidence.startswith("https://"), evidence)
                else:
                    self.assertTrue((ROOT / evidence).is_file(), f"{task_id}: missing evidence {evidence}")

            if legacy_state == "reviewed":
                self.assertEqual(record["review_status"], marker["review_status"])
                self.assertEqual(record["decision"], marker["decision"])
            else:
                self.assertEqual("pending", marker["review_status"])
                self.assertEqual("re_review_required", marker["decision"])

        if legacy_state == "reviewed":
            self.assertEqual({"pending": 0, "reviewed": 168}, legacy_reconciliation["review_status_summary"])
        else:
            self.assertEqual({"pending": 168, "reviewed": 0}, legacy_reconciliation["review_status_summary"])
        if sync["state"] == "reviewed":
            self.assertEqual("reviewed", legacy_state)
        update_markdown = UPDATE_GRAPH_PATH.read_text(encoding="utf-8")
        self.assertNotIn("خالی بوده، همگام‌سازی وضعیت‌ها", update_markdown)

    def test_real_product_e2e_rejects_transport_and_model_fixtures(self) -> None:
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

    def test_in_place_upgrade_preserves_required_data_domains(self) -> None:
        contract = self.nodes["UPD-S1-021"]["upgrade_contract"]
        self.assertTrue(contract["required"])
        self.assertEqual(["Android", "Windows"], contract["platforms"])
        self.assertTrue(contract["pre_upgrade_post_upgrade_snapshot_required"])
        self.assertTrue(contract["uninstall_or_clear_data_prohibited"])
        domains = " ".join(contract["data_domains_must_survive_or_be_migrated"])
        for expected in ("Room", "Keystore", "host identity", "peer trust", "local model"):
            self.assertIn(expected, domains)


if __name__ == "__main__":
    unittest.main()
