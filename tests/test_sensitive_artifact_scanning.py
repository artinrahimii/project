"""Source-contract guards for prompt/capsule/secret canary scans in logs and temporary artifacts."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HOST_STORE = (ROOT / "src/Host.Windows/Chat/WindowsHostConversationStore.cs").read_text(encoding="utf-8")
HOST_RUNTIME_TEST = (ROOT / "tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs").read_text(encoding="utf-8")
HOST_LOG_TEST = (ROOT / "tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs").read_text(encoding="utf-8")
ANDROID_DB_FACTORY = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt"
).read_text(encoding="utf-8")
ANDROID_DB_TEST = (
    ROOT / "src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt"
).read_text(encoding="utf-8")
ANDROID_LOG = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt"
).read_text(encoding="utf-8")
ANDROID_LOG_TEST = (
    ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/SafeDiagnosticLogTest.kt"
).read_text(encoding="utf-8")
STATIC_LOG_TEST = (ROOT / "tests/test_network_payload_logging.py").read_text(encoding="utf-8")
SECURITY_DOC = (ROOT / "docs/security.md").read_text(encoding="utf-8")

MARKERS = (
    "T0187_PROMPT_CANARY_7F2A",
    "T0187_CONTEXT_CAPSULE_CANARY_91C4",
    "T0187_SECRET_CANARY_3BD8",
)


class SensitiveArtifactScanningTests(unittest.TestCase):
    def test_windows_temporary_file_scan_detects_and_then_rejects_all_canaries(self):
        for marker in MARKERS:
            with self.subTest(marker=marker):
                self.assertIn(marker, HOST_RUNTIME_TEST)
        self.assertIn("ContainsSensitiveCanariesInFiles(directory)", HOST_RUNTIME_TEST)
        self.assertIn("AssertNoSensitiveCanariesInFiles(directory", HOST_RUNTIME_TEST)
        self.assertIn("Directory.EnumerateFiles(directory", HOST_RUNTIME_TEST)
        self.assertIn("File.ReadAllBytes(candidate)", HOST_RUNTIME_TEST)
        self.assertIn("CryptographicOperations.ZeroMemory(contents)", HOST_RUNTIME_TEST)
        self.assertIn("A synthetic prompt, context capsule or secret marker remained", HOST_RUNTIME_TEST)

    def test_windows_store_protects_plaintext_before_writing_temporary_file(self):
        protect = HOST_STORE.index("_protector.Protect(plaintext)")
        temp_create = HOST_STORE.index("new FileStream(", HOST_STORE.index("temporaryPath = Path.Combine"))
        encrypted_write = HOST_STORE.index("stream.Write(protectedPayload)", temp_create)
        self.assertLess(protect, temp_create)
        self.assertLess(temp_create, encrypted_write)
        self.assertIn("CryptographicOperations.ZeroMemory(plaintext)", HOST_STORE)
        self.assertIn("CryptographicOperations.ZeroMemory(protectedPayload)", HOST_STORE)

    def test_windows_eventsource_runtime_check_covers_sensitive_canaries(self):
        for marker in MARKERS:
            with self.subTest(marker=marker):
                self.assertIn(marker, HOST_LOG_TEST)
        self.assertIn("record.Payload", HOST_LOG_TEST)
        self.assertIn("record.Message", HOST_LOG_TEST)
        self.assertIn("record.EventId != 1", HOST_LOG_TEST)
        self.assertIn("payload is not { Count: 3 }", HOST_LOG_TEST)

    def test_android_sqlcipher_migration_scans_database_and_temporary_sidecars(self):
        for marker in MARKERS:
            with self.subTest(marker=marker):
                self.assertIn(marker, ANDROID_DB_TEST)
        self.assertIn("assertNoSensitiveCanariesInDatabaseArtifacts(databaseFile)", ANDROID_DB_TEST)
        self.assertIn(".sqlcipher-migrating", ANDROID_DB_TEST)
        self.assertIn('File(databaseFile.absolutePath + "-wal")', ANDROID_DB_TEST)
        self.assertIn('File(databaseFile.absolutePath + "-shm")', ANDROID_DB_TEST)
        self.assertIn("sqlcipher_export", ANDROID_DB_FACTORY)
        self.assertIn("Os.rename(temporaryFile.absolutePath, databaseFile.absolutePath)", ANDROID_DB_FACTORY)
        self.assertIn("deleteIfPresent(temporaryFile)", ANDROID_DB_FACTORY)

    def test_android_diagnostic_formatter_runtime_test_covers_sensitive_canaries(self):
        for marker in MARKERS:
            with self.subTest(marker=marker):
                self.assertIn(marker, ANDROID_LOG_TEST)
        self.assertIn("Log.i(TAG, formatRecord(eventCode, outcomeCode, durationMilliseconds))", ANDROID_LOG)
        self.assertIn("durationMilliseconds: Long", ANDROID_LOG)

    def test_static_network_log_scan_remains_enabled_and_scope_is_documented(self):
        self.assertIn("LOG_SINK", STATIC_LOG_TEST)
        self.assertIn("SENSITIVE_REFERENCE", STATIC_LOG_TEST)
        self.assertIn("## آزمون اسکن log و فایل‌های موقت برای prompt، capsule و اسرار (T0187)", SECURITY_DOC)
        self.assertIn("pagefile/swap", SECURITY_DOC)
        self.assertIn("crash dump", SECURITY_DOC)


if __name__ == "__main__":
    unittest.main()
