from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ENTITY = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationContextCapsuleEntity.kt"
REPOSITORY = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data/RoomConversationContextCapsuleRepository.kt"
DATABASE = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt"
PERSISTENCE_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/data/ConversationContextCapsulePersistenceTest.kt"
MIGRATION_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseMigrationTest.kt"


class AndroidContextCapsulePersistenceContractTests(unittest.TestCase):
    def test_room_entity_is_one_to_one_and_cascades_with_conversation(self):
        source = ENTITY.read_text(encoding="utf-8")
        self.assertIn('tableName = "conversation_context_capsules"', source)
        self.assertIn("@PrimaryKey val conversationId: String", source)
        self.assertIn("serializedCapsule: ByteArray", source)
        self.assertIn("ForeignKey.CASCADE", source)

    def test_database_version_four_registers_sequential_migrations(self):
        source = DATABASE.read_text(encoding="utf-8")
        self.assertIn("version = 4", source)
        self.assertIn("object : Migration(1, 2)", source)
        self.assertIn("object : Migration(2, 3)", source)
        self.assertIn("object : Migration(3, 4)", source)
        self.assertIn("addMigrations(MIGRATION_1_2, MIGRATION_2_3, MIGRATION_3_4)", source)
        self.assertIn("conversationContextCapsuleDao", source)

    def test_repository_uses_capsule_codec_and_rejects_invalid_payloads(self):
        source = REPOSITORY.read_text(encoding="utf-8")
        self.assertIn("capsule.encode(maxSerializedBytes)", source)
        self.assertIn("maxSerializedBytes: Int = ConversationContextCapsule.MAX_SERIALIZED_BYTES", source)
        self.assertIn("ConversationContextCapsule.decode(entity.serializedCapsule)", source)
        self.assertIn("ConversationContextCapsuleSaveResult.Rejected", source)
        self.assertIn("dao.saveWithChange", source)
        self.assertIn("changedFields", source)
        self.assertIn("ConversationContextCapsuleChangeSource", source)

    def test_android_tests_cover_scope_reopen_cascade_and_migration(self):
        source = PERSISTENCE_TEST.read_text(encoding="utf-8")
        for marker in (
            "capsulesRemainIndependentAcrossConversationsAndDatabaseReopen",
            "invalidCapsuleIsRejectedAndForeignKeyPreventsOrphanRecords",
            "conversation-one",
            "conversation-two",
        ):
            self.assertIn(marker, source)
        migration = MIGRATION_TEST.read_text(encoding="utf-8")
        self.assertIn("assertEquals(4, migratedDatabase.openHelper.readableDatabase.version)", migration)
        self.assertIn("MIGRATION_2_3", migration)
        self.assertIn("MIGRATION_3_4", migration)
        self.assertIn("savesAppendLocalProvenanceWithFieldNamesAndTimestampWithoutCopyingValues", source)


if __name__ == "__main__":
    unittest.main()
