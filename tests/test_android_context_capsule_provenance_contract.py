from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ANDROID_DATA = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data"
VIEW_MODEL = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModel.kt"
PROTO = ROOT / "protocol/whitep/v1/messages.proto"


class AndroidCapsuleProvenanceContractTests(unittest.TestCase):
    def test_change_rows_record_source_time_and_field_names_without_field_values(self):
        entity = (ANDROID_DATA / "ConversationContextCapsuleChangeEntity.kt").read_text(encoding="utf-8")
        repository = (ANDROID_DATA / "RoomConversationContextCapsuleRepository.kt").read_text(encoding="utf-8")
        for marker in ("source: String", "changedFieldsJson: String", "changedAtEpochMillis: Long", "serializedByteCount: Int"):
            self.assertIn(marker, entity)
        self.assertIn("ForeignKey.CASCADE", entity)
        self.assertIn("entity = ConversationContextCapsuleEntity::class", entity)
        self.assertIn("ConversationContextCapsuleChangeSource", repository)
        self.assertIn("changedFields(previousCapsule, capsule)", repository)
        self.assertIn("dao.saveWithChange", repository)
        self.assertNotIn("val content:", entity)
        self.assertNotIn("val prompt:", entity)

    def test_view_model_logs_only_after_explicit_save_and_resolved_conflict_gate(self):
        source = VIEW_MODEL.read_text(encoding="utf-8")
        self.assertIn("current.conflictReviews.any { it.decision == null }", source)
        self.assertIn("ConversationContextCapsuleChangeSource.USER_EDIT", source)
        self.assertIn("ConversationContextCapsuleChangeSource.LOCAL_HISTORY_SUMMARY", source)
        self.assertIn("ConversationContextCapsuleChangeSource.LOCAL_HISTORY_SUMMARY_REVIEWED", source)
        self.assertIn("repository.save(conversationId, capsule, clockMillis(), current.maxSerializedBytes, changeSource)", source)

    def test_network_inference_request_has_no_permanent_conversation_id_or_history(self):
        proto = PROTO.read_text(encoding="utf-8")
        request = proto.split("message InferenceRequest {", 1)[1].split("}", 1)[0]
        self.assertIn("bytes context_capsule = 7;", request)
        self.assertIn("string request_id = 1;", request)
        self.assertIn("string attempt_id = 2;", request)
        self.assertNotIn("conversation_id", request)
        self.assertNotIn("conversationId", request)
        self.assertNotIn("history", request.lower())


if __name__ == "__main__":
    unittest.main()
