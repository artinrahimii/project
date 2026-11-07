from __future__ import annotations

import json
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def graph_from_markdown(path: str) -> dict:
    markdown = read(path)
    marker = chr(96) * 3 + "json"
    start = markdown.find(marker)
    if start < 0:
        raise AssertionError(f"Missing JSON fence in {path}")
    start += len(marker)
    end = markdown.find(chr(96) * 3, start)
    if end < 0:
        raise AssertionError(f"Unclosed JSON fence in {path}")
    return json.loads(markdown[start:end].strip())


class UpdateBaselineAuditTests(unittest.TestCase):
    def test_update_graph_shape_and_task_ledger_remain_valid(self) -> None:
        graph = graph_from_markdown("update/Step two/project-graph.md")
        self.assertEqual(24, len(graph["nodes"]))
        self.assertEqual(80, len(graph["edges"]))
        self.assertEqual(13, len(graph["execution_waves"]))
        node = next(n for n in graph["nodes"] if n["id"] == "UPD-S1-001")
        self.assertIn(node["status"], {"in_progress", "done"})
        self.assertEqual(node["execution_summary"]["attempt_count"], len(node["execution_history"]))

    def test_audit_artifact_binds_to_the_real_baseline(self) -> None:
        evidence = read("update/Step three/UPD-S1-001/evidence/baseline-audit.md")
        report = read("update/Step three/UPD-S1-001/execution-report.md")
        self.assertIn("afa79587ee403942b5ba2f1e91b3e7ccc2988f08", evidence)
        self.assertIn("V 3.1.17", evidence)
        self.assertIn("4,014,945", evidence)
        self.assertIn("UPD-S1-001", report)
        self.assertIn("در حال اجرا", report)
        self.assertNotIn("6f70586b4b7ff69c463f7e5b02293090c63e78ec", evidence + report)

    def test_android_chat_send_is_local_and_network_request_is_separate(self) -> None:
        message_vm = read(
            "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/"
            "ConversationMessagesViewModel.kt"
        )
        conversation_ui = read(
            "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/"
            "ConversationDetailScreen.kt"
        )
        activity = read("src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt")
        self.assertIn("does not send content to a processing host", message_vm)
        self.assertIn("messageDao.upsert(", message_vm)
        self.assertIn("Sending to a processing host is not available yet", conversation_ui)
        self.assertIn('label = { Text("Exact model ID") }', activity)
        self.assertIn('label = { Text("Maximum output tokens") }', activity)
        self.assertIn("requestViewModel.sendRequest(", activity)

    def test_windows_admission_and_single_inference_are_real_code_paths(self) -> None:
        coordinator = read("src/Host.Windows/Requests/HostRequestCoordinator.cs")
        admission = read("src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs")
        lifecycle = read("src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs")
        self.assertIn("SemaphoreSlim(1, 1)", coordinator)
        self.assertIn("HasActiveParticipation", admission)
        self.assertIn("ModelNotReady", admission)
        self.assertIn("CapacityInsufficient", admission)
        self.assertIn("StartListeningAsync", lifecycle)
        self.assertIn("SendMessageAsync", lifecycle)

    def test_shared_contract_and_security_sources_exist(self) -> None:
        validator = read("src/Shared.Contracts/ProtocolMessageValidator.cs")
        trust = read("src/Shared.Security/PeerTrustStore.cs")
        replay = read("src/Shared.Security/AuthenticatedRequestReplayGuard.cs")
        self.assertIn("class ProtocolMessageValidator", validator)
        self.assertIn("class PeerTrustStore", trust)
        self.assertIn("class AuthenticatedRequestReplayGuard", replay)

    def test_ci_links_are_present_in_the_audit_record(self) -> None:
        evidence = read("update/Step three/UPD-S1-001/evidence/baseline-audit.md")
        for run_id in ("42401485764", "42401485830", "42401485784"):
            self.assertIn(run_id, evidence)


if __name__ == "__main__":
    unittest.main()
