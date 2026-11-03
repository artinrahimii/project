"""Source and workflow guards for Android Keystore-backed local peer secrets."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROTECTOR = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt"
IDENTITY = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt"
TRUST = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt"
RUNTIME = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt"
INSTRUMENTATION = ROOT / "src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidKeyStorePersistenceTest.kt"
MANIFEST = ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
GRADLE = ROOT / "src/Client.Android/app/build.gradle.kts"
WORKFLOW = ROOT / ".github/workflows/product-build.yml"
SECURITY_DOC = ROOT / "docs/security.md"


class AndroidKeystoreLocalSecretsTests(unittest.TestCase):
    def test_reusable_protector_uses_versioned_randomized_aes_gcm_and_non_exportable_keystore_key(self):
        source = PROTECTOR.read_text(encoding="utf-8")
        for marker in (
            '"AndroidKeyStore"',
            '"AES/GCM/NoPadding"',
            "KeyProperties.KEY_ALGORITHM_AES",
            "setKeySize(AES_KEY_SIZE_BITS)",
            "const val AES_KEY_SIZE_BITS = 256",
            "setRandomizedEncryptionRequired(true)",
            "KeyStore.SecretKeyEntry",
            "private fun validateKey",
            "key.encoded",
            "NONCE_BYTES = 12",
            "TAG_BITS = 128",
            "ENVELOPE_VERSION",
            "MAXIMUM_CLEAR_TEXT_BYTES",
            "additionalData()",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, source)

    def test_identity_and_trust_use_separate_keystore_aliases_and_fail_closed(self):
        identity = IDENTITY.read_text(encoding="utf-8")
        trust = TRUST.read_text(encoding="utf-8")
        self.assertIn("AndroidKeyStoreProtectedBlobProtector(IDENTITY_KEY_ALIAS)", identity)
        self.assertIn("AndroidKeyStoreProtectedBlobProtector(TRUST_KEY_ALIAS)", trust)
        self.assertIn('IDENTITY_KEY_ALIAS = "com.whitep.client.peer_identity.v1"', identity)
        self.assertIn('TRUST_KEY_ALIAS = "com.whitep.client.peer_trust.v1"', trust)
        self.assertIn("could not be authenticated", identity)
        self.assertIn("could not be authenticated", trust)
        self.assertIn("trustPeerAfterExplicitConfirmation", trust)
        self.assertIn("if (!explicitlyConfirmed)", trust)

    def test_production_runtime_uses_persistent_identity_and_trust_stores(self):
        runtime = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("AndroidPeerIdentityStore(applicationContext).loadOrCreate()", runtime)
        self.assertIn("AndroidPeerTrustStore(applicationContext)", runtime)
        manifest = MANIFEST.read_text(encoding="utf-8")
        self.assertIn('android:allowBackup="false"', manifest)

    def test_real_android_instrumentation_covers_nonexportability_missing_keys_and_tamper(self):
        source = INSTRUMENTATION.read_text(encoding="utf-8")
        self.assertIn("AndroidJUnit4", source)
        self.assertIn("AndroidKeyStore", source)
        self.assertIn("assertNonExportableAesKey", source)
        self.assertIn("missingIdentityWrappingKeyFailsClosedWithoutRotatingTheStoredPeerId", source)
        self.assertIn("tamperedIdentityEnvelopeFailsClosedAndIsNotSilentlyReplaced", source)
        self.assertIn("tamperedPeerTrustEnvelopeFailsClosedWithoutGrantingTrust", source)
        self.assertIn("identityAndPeerTrustUseDistinctNonExportableKeystoreKeys", source)

    def test_android_ci_executes_instrumentation_on_a_real_emulator_system_image(self):
        gradle = GRADLE.read_text(encoding="utf-8")
        for dependency in (
            "androidx.test:core:1.7.0",
            "androidx.test:runner:1.7.0",
            "androidx.test.ext:junit:1.3.0",
        ):
            with self.subTest(dependency=dependency):
                self.assertIn(dependency, gradle)
        workflow = WORKFLOW.read_text(encoding="utf-8")
        self.assertIn("reactivecircus/android-emulator-runner@a421e43855164a8197daf9d8d40fe71c6996bb0d", workflow)
        self.assertIn("gradle --no-daemon connectedDebugAndroidTest", workflow)
        self.assertIn("whitep-android-keystore-instrumentation-reports", workflow)

    def test_security_docs_do_not_claim_room_database_is_encrypted_by_this_task(self):
        docs = SECURITY_DOC.read_text(encoding="utf-8")
        self.assertIn("T0183", docs)
        self.assertIn("T0184", docs)
        self.assertIn("Android Keystore", docs)
        self.assertIn("StrongBox", docs)
        self.assertIn("Room", docs)


if __name__ == "__main__":
    unittest.main()
