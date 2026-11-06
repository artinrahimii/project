"""Contract checks for same-commit release bundling and draft publication."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
WORKFLOW = (ROOT / ".github/workflows/release-assets.yml").read_text(encoding="utf-8")
DOCUMENTATION = (ROOT / "docs/release-assets.md").read_text(encoding="utf-8")


class ReleaseAssetsContractTests(unittest.TestCase):
    def test_workflow_requires_actions_read_and_contents_write_only_for_release_job(self):
        self.assertIn("permissions:\n      actions: read\n      contents: write", WORKFLOW)

    def test_only_successful_same_commit_builds_are_accepted(self):
        self.assertIn("head_sha=${GITHUB_SHA}", WORKFLOW)
        self.assertIn('select(.name == "Android signed release" and .conclusion == "success")', WORKFLOW)
        self.assertIn('select(.name == "White-P product build" and .conclusion == "success")', WORKFLOW)
        self.assertIn("whitep-android-release-signed", WORKFLOW)
        self.assertIn("whitep-windows-installer", WORKFLOW)

    def test_workflow_placeholders_are_not_backslash_escaped(self):
        self.assertIn("${{ github.token }}", WORKFLOW)
        self.assertNotIn(r"\${", WORKFLOW)
        self.assertIn("${GITHUB_REPOSITORY}", WORKFLOW)

    def test_both_release_assets_and_license_inventory_are_required(self):
        for marker in ("WhiteP-Android-0.1.0.apk", "WhiteP-Setup-0.1.0-win-x64.exe",
                       "ThirdPartyNotices.md", "licenses.json", "ModelLicenses.md",
                       "model-licenses.json", "WhiteP-Sample-Model-Apache-2.0.txt", "WhiteP-Windows-Installer.zip"):
            with self.subTest(marker=marker):
                self.assertIn(marker, WORKFLOW)

    def test_sha256_manifest_is_generated_and_checked_before_upload(self):
        self.assertIn("sha256sum WhiteP-Android-0.1.0.apk WhiteP-Windows-Installer.zip BUILD-PROVENANCE.txt > SHA256SUMS.txt", WORKFLOW)
        self.assertIn("sha256sum --check SHA256SUMS.txt", WORKFLOW)
        self.assertIn("whitep-release-assets", WORKFLOW)

    def test_only_android_version_tags_create_draft_releases(self):
        self.assertIn("if: startsWith(github.ref, 'refs/tags/android-v')", WORKFLOW)
        self.assertIn("--draft", WORKFLOW)
        self.assertIn("--verify-tag", WORKFLOW)
        self.assertIn("is_draft", WORKFLOW)
        self.assertIn("Refusing to modify an already-published release", WORKFLOW)
        self.assertIn(".draft == true", WORKFLOW)

    def test_draft_release_lookup_uses_list_endpoint(self):
        # Draft releases are visible through the releases list to writers.
        # gh api emits each paginated response; jq -s combines those pages.
        self.assertIn("gh api --paginate", WORKFLOW)
        self.assertIn('jq -s --arg release_tag "$tag"', WORKFLOW)
        self.assertIn("releases?per_page=100", WORKFLOW)
        self.assertIn("select(.tag_name == $release_tag)", WORKFLOW)
        self.assertNotIn("--slurp", WORKFLOW)
        self.assertNotIn("releases/tags/${tag}", WORKFLOW)
        self.assertNotIn('gh release view "$tag"', WORKFLOW)

    def test_docs_explain_integrity_and_keep_release_draft_until_final_acceptance(self):
        for marker in ("sha256sum --check SHA256SUMS.txt", "BUILD-PROVENANCE.txt", "android-v0.1.0", "draft", "T0218"):
            with self.subTest(marker=marker):
                self.assertIn(marker.lower(), DOCUMENTATION.lower())


if __name__ == "__main__":
    unittest.main()
