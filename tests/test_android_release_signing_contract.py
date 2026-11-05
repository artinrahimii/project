"""Contract checks for secure Android release APK signing and CI verification."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
GRADLE = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
PRODUCT_WORKFLOW = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
RELEASE_WORKFLOW = (ROOT / ".github/workflows/android-release.yml").read_text(encoding="utf-8")
GITIGNORE = (ROOT / ".gitignore").read_text(encoding="utf-8")
DOCUMENTATION = (ROOT / "docs/android-release-signing.md").read_text(encoding="utf-8")


class AndroidReleaseSigningContractTests(unittest.TestCase):
    def test_gradle_release_tasks_fail_closed_without_all_environment_inputs(self):
        for name in (
            "WHITEP_ANDROID_RELEASE_STORE_FILE",
            "WHITEP_ANDROID_RELEASE_STORE_PASSWORD",
            "WHITEP_ANDROID_RELEASE_KEY_ALIAS",
            "WHITEP_ANDROID_RELEASE_KEY_PASSWORD",
        ):
            with self.subTest(name=name):
                self.assertIn(name, GRADLE)
        self.assertIn("releaseBuildRequested", GRADLE)
        self.assertIn("missingReleaseSigningInputs", GRADLE)
        self.assertIn("GradleException", GRADLE)
        self.assertIn('getByName("release")', GRADLE)
        self.assertIn('signingConfig = signingConfigs.getByName("release")', GRADLE)

    def test_release_keystore_must_be_an_existing_file_outside_the_repository(self):
        self.assertIn("signingFile.isFile", GRADLE)
        repository_root_expression = GRADLE.split("val repositoryRoot", 1)[1].split(
            "if (signingFile.toPath()", 1
        )[0]
        self.assertIn('resolve("../..")', repository_root_expression)
        self.assertLess(
            repository_root_expression.index('resolve("../..")'),
            repository_root_expression.index(".canonicalFile"),
        )
        self.assertLess(
            repository_root_expression.index(".canonicalFile"),
            repository_root_expression.index(".toPath()"),
        )
        self.assertIn("signingFile.toPath().startsWith(repositoryRoot)", GRADLE)
        self.assertIn("must be stored outside the repository checkout", GRADLE)

    def test_product_ci_uses_a_disposable_key_and_verifies_then_cleans_it(self):
        for marker in (
            "openssl rand -hex 24",
            "keytool -genkeypair",
            "whitep-android-release-signing-smoke",
            "assembleRelease --no-build-cache",
            "apksigner" ,
            "if: always()",
            "Remove disposable release-signing key",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, PRODUCT_WORKFLOW)
        self.assertNotIn("ANDROID_RELEASE_KEYSTORE_BASE64", PRODUCT_WORKFLOW)

    def test_production_workflow_requires_protected_environment_and_pinned_certificate(self):
        for marker in (
            "workflow_dispatch:",
            "- 'android-v*'",
            "name: android-release",
            "secrets.ANDROID_RELEASE_KEYSTORE_BASE64",
            "secrets.ANDROID_RELEASE_STORE_PASSWORD",
            "secrets.ANDROID_RELEASE_KEY_ALIAS",
            "secrets.ANDROID_RELEASE_KEY_PASSWORD",
            "vars.ANDROID_RELEASE_CERT_SHA256",
            "apksigner",
            "whitep-android-release-signed",
            "Remove production signing keystore from runner",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, RELEASE_WORKFLOW)
        self.assertNotIn("contents: write", RELEASE_WORKFLOW)

    def test_repository_ignores_common_local_keystore_formats(self):
        for pattern in ("*.jks", "*.keystore", "*.p12", "*.pfx"):
            with self.subTest(pattern=pattern):
                self.assertIn(pattern, GITIGNORE)

    def test_documentation_separates_smoke_signing_from_real_release_signing(self):
        for marker in (
            "android-release",
            "ANDROID_RELEASE_KEYSTORE_BASE64",
            "ANDROID_RELEASE_CERT_SHA256",
            "protected",
            "disposable",
            "outside the repository",
            "checksum",
            "GitHub Release",
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker.lower(), DOCUMENTATION.lower())


if __name__ == "__main__":
    unittest.main()
