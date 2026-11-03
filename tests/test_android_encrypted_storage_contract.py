from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ANDROID_APP = ROOT / "src" / "Client.Android" / "app"
DATABASE_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "ClientDatabase.kt"
FACTORY_SOURCE = ANDROID_APP / "src" / "main" / "java" / "com" / "whitep" / "client" / "data" / "EncryptedClientDatabaseFactory.kt"
INSTRUMENTATION_TEST = ANDROID_APP / "src" / "androidTest" / "java" / "com" / "whitep" / "client" / "data" / "EncryptedClientDatabaseAcceptanceTest.kt"
BUILD_SOURCE = ANDROID_APP / "build.gradle.kts"


class AndroidEncryptedStorageContractTests(unittest.TestCase):
    def test_room_database_uses_sqlcipher_and_keystore_wrapped_key(self):
        database = DATABASE_SOURCE.read_text(encoding="utf-8")
        factory = FACTORY_SOURCE.read_text(encoding="utf-8")
        build = BUILD_SOURCE.read_text(encoding="utf-8")

        self.assertIn("EncryptedClientDatabaseFactory", database)
        self.assertIn("net.zetetic:sqlcipher-android:4.20.0", build)
        self.assertIn("androidx.sqlite:sqlite:2.7.0", build)
        self.assertIn("SupportOpenHelperFactory", factory)
        self.assertIn("AndroidKeyStoreProtectedBlobProtector", factory)
        self.assertIn("SecureRandom().nextBytes(key)", factory)
        self.assertIn("Base64.encodeToString(envelope", factory)
        self.assertNotIn("putString(preferenceKey, Base64.encodeToString(key", factory)

    def test_plaintext_migration_is_checked_and_atomically_replaced(self):
        factory = FACTORY_SOURCE.read_text(encoding="utf-8")

        self.assertIn("sqlcipher_export", factory)
        self.assertIn("PRAGMA wal_checkpoint(TRUNCATE)", factory)
        self.assertIn("integrity_check", factory)
        self.assertIn("Os.rename(temporaryFile.absolutePath, databaseFile.absolutePath)", factory)
        self.assertIn("hasPlaintextSQLiteHeader", factory)
        self.assertIn("refusing to replace it", factory)

    def test_instrumentation_covers_reopen_migration_and_missing_key(self):
        test_source = INSTRUMENTATION_TEST.read_text(encoding="utf-8")

        self.assertIn("freshDatabaseIsEncryptedAndConversationDataSurvivesReopen", test_source)
        self.assertIn("legacyPlaintextRoomDatabaseIsExportedAndExistingRowsArePreserved", test_source)
        self.assertIn("databaseKeyIsStoredOnlyAsKeystoreProtectedEnvelope", test_source)
        self.assertIn("missingProtectedKeyFailsClosedAndLeavesEncryptedDatabaseUnchanged", test_source)


if __name__ == "__main__":
    unittest.main()
