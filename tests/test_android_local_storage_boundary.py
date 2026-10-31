from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data"


class AndroidLocalStorageBoundaryTests(unittest.TestCase):
    def test_room_database_is_versioned_and_uses_app_private_context(self):
        source = (ANDROID / "ClientDatabase.kt").read_text(encoding="utf-8")
        self.assertIn("@Database(", source)
        self.assertIn("version = 4", source)
        self.assertIn("Migration(1, 2)", source)
        self.assertIn("Migration(2, 3)", source)
        self.assertIn("Migration(3, 4)", source)
        self.assertIn("addMigrations(MIGRATION_1_2, MIGRATION_2_3, MIGRATION_3_4)", source)
        self.assertIn("exportSchema = true", source)
        self.assertIn("context.applicationContext", source)
        self.assertIn('"whitep-client.db"', source)
        self.assertNotIn("fallbackToDestructiveMigration", source)

    def test_capsule_provenance_is_local_metadata_and_cascades_with_capsule(self):
        source = (ANDROID / "ConversationContextCapsuleChangeEntity.kt").read_text(encoding="utf-8")
        self.assertIn("conversation_context_capsule_changes", source)
        self.assertIn("changedAtEpochMillis", source)
        self.assertIn("changedFieldsJson", source)
        self.assertIn("ForeignKey.CASCADE", source)
        self.assertNotIn("val content:", source)
        self.assertNotIn("val prompt:", source)

    def test_message_and_request_status_rows_cascade_with_conversation_deletion(self):
        for filename in ("MessageEntity.kt", "RequestStateEntity.kt"):
            source = (ANDROID / filename).read_text(encoding="utf-8")
            self.assertIn("ForeignKey.CASCADE", source)
            self.assertIn('parentColumns = ["conversationId"]', source)
            self.assertIn('childColumns = ["conversationId"]', source)

    def test_request_state_contains_only_status_metadata_not_request_content(self):
        source = (ANDROID / "RequestStateEntity.kt").read_text(encoding="utf-8")
        self.assertIn("val status: PersistedRequestStatus", source)
        self.assertIn("val attemptNumber: Int", source)
        self.assertIn("val errorCode: String?", source)
        for field in ("val prompt:", "contextCapsule", "val payload:", "responseText"):
            self.assertNotIn(field, source)

    def test_room_repository_keeps_transport_unavailable_as_an_explicit_result(self):
        source = (ANDROID / "RoomClientRequestRepository.kt").read_text(encoding="utf-8")
        self.assertIn("RequestPreparationResult.TRANSPORT_UNAVAILABLE", source)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("Socket", source)

    def test_storage_decision_discloses_persistence_and_at_rest_limit(self):
        decision = (ROOT / "docs/decisions/ADR-0020-android-client-local-conversation-storage.md").read_text(encoding="utf-8")
        self.assertIn("app-private", decision)
        self.assertIn("not encrypted at rest", decision)
        self.assertIn("T0184", decision)
        self.assertIn("ADR-0019", decision)
        self.assertIn("does not synchronize", decision)


if __name__ == "__main__":
    unittest.main()
