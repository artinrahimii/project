from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ANDROID_APP = ROOT / "src" / "Client.Android" / "app"
DATABASE_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "ClientDatabase.kt"
ENCRYPTED_FACTORY_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "EncryptedClientDatabaseFactory.kt"
REQUEST_STATE_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "RequestStateEntity.kt"
LIFECYCLE_STATE_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "InferenceRequestStateEntity.kt"
MIGRATION_TEST = ANDROID_APP / "src" / "test" / "java" / "com" / "whitep" / "client" / "data" / "ClientDatabaseMigrationTest.kt"
LEGACY_SCHEMA = ANDROID_APP / "src" / "test" / "resources" / "db" / "v1-schema.sql"
INDEX_NAME = "index_request_states_conversationId_createdAtEpochMillis_requestId"


class AndroidDatabaseMigrationContractTests(unittest.TestCase):
    def test_room_version_and_migration_are_registered(self):
        database = DATABASE_SOURCE.read_text(encoding="utf-8")
        self.assertIn("version = 5", database)
        self.assertIn("Migration(1, 2)", database)
        self.assertIn("Migration(2, 3)", database)
        self.assertIn("Migration(3, 4)", database)
        self.assertIn("Migration(4, 5)", database)
        factory = ENCRYPTED_FACTORY_SOURCE.read_text(encoding="utf-8")
        self.assertIn("EncryptedClientDatabaseFactory", database)
        self.assertIn("addMigrations(", factory)
        self.assertIn("ClientDatabase.MIGRATION_1_2", factory)
        self.assertIn("ClientDatabase.MIGRATION_2_3", factory)
        self.assertIn("ClientDatabase.MIGRATION_3_4", factory)
        self.assertIn("ClientDatabase.MIGRATION_4_5", factory)
        self.assertIn("SupportOpenHelperFactory", factory)
        self.assertIn(INDEX_NAME, database)
        self.assertIn("conversation_context_capsule_changes", database)

    def test_app_request_lifecycle_table_is_metadata_only(self):
        entity = LIFECYCLE_STATE_SOURCE.read_text(encoding="utf-8")
        self.assertIn('tableName = "client_inference_request_states"', entity)
        self.assertIn("APP_CLOSED_BEFORE_RESULT", entity)
        self.assertIn("status IN ('QUEUED', 'CONTACTING', 'WAITING')", entity)
        for field in ("val prompt:", "contextCapsule", "responseText", "val payload:", "ephemeralHostId"):
            self.assertNotIn(field, entity)

    def test_migration_index_matches_the_entity_and_version_one_fixture(self):
        entity = REQUEST_STATE_SOURCE.read_text(encoding="utf-8")
        legacy = LEGACY_SCHEMA.read_text(encoding="utf-8")
        self.assertIn('Index(value = ["conversationId", "createdAtEpochMillis", "requestId"])', entity)
        for table in ("conversations", "messages", "request_states", "request_drafts"):
            self.assertIn(f"CREATE TABLE IF NOT EXISTS {table}", legacy)
        self.assertIn("index_request_states_conversationId_updatedAtEpochMillis", legacy)
        self.assertNotIn(INDEX_NAME, legacy)

    def test_runtime_upgrade_test_checks_version_data_and_index(self):
        test_source = MIGRATION_TEST.read_text(encoding="utf-8")
        self.assertIn("legacyDatabase.version = 1", test_source)
        self.assertIn("addMigrations(", test_source)
        self.assertIn("ClientDatabase.MIGRATION_1_2", test_source)
        self.assertIn("ClientDatabase.MIGRATION_2_3", test_source)
        self.assertIn("ClientDatabase.MIGRATION_3_4", test_source)
        self.assertIn("assertEquals(5, migratedDatabase.openHelper.readableDatabase.version)", test_source)
        self.assertIn("ClientDatabase.MIGRATION_4_5", test_source)
        self.assertIn("changeHistoryForConversation", test_source)
        self.assertIn("conversationContextCapsuleDao", test_source)
        self.assertIn("migratedDatabase.openHelper.readableDatabase.version", test_source)
        self.assertIn("Legacy conversation", test_source)
        self.assertIn(INDEX_NAME, test_source)
        self.assertIn("NETWORK_UNAVAILABLE", test_source)


if __name__ == "__main__":
    unittest.main()
