from pathlib import Path
import unittest

from tools.verify_android_permissions import (
    EXPECTED_PERMISSION,
    PermissionPolicyError,
    parse_aapt_permissions,
    read_manifest_permissions,
    validate_permission_policy,
)

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
ANDROID_MAIN = ROOT / "src/Client.Android/app/src/main/java"


class AndroidMinimumPermissionTests(unittest.TestCase):
    def test_source_manifest_requests_only_internet(self):
        self.assertEqual([EXPECTED_PERMISSION], read_manifest_permissions(MANIFEST))

    def test_packaged_permission_output_is_parsed(self):
        output = (
            "package: com.whitep.client\n"
            "uses-permission: name='android.permission.INTERNET'\n"
        )
        self.assertEqual([EXPECTED_PERMISSION], parse_aapt_permissions(output))

    def test_sdk_qualified_permission_is_detected(self):
        output = "uses-permission-sdk-23: name='android.permission.READ_CONTACTS'\n"
        self.assertEqual(
            ["android.permission.READ_CONTACTS"],
            parse_aapt_permissions(output),
        )

    def test_exact_minimum_permission_set_passes(self):
        validate_permission_policy([EXPECTED_PERMISSION], [EXPECTED_PERMISSION])

    def test_unneeded_source_permission_is_rejected(self):
        with self.assertRaises(PermissionPolicyError):
            validate_permission_policy(
                [EXPECTED_PERMISSION, "android.permission.READ_EXTERNAL_STORAGE"],
                [EXPECTED_PERMISSION],
            )

    def test_dependency_added_permission_in_packaged_apk_is_rejected(self):
        with self.assertRaisesRegex(PermissionPolicyError, "ACCESS_NETWORK_STATE"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [EXPECTED_PERMISSION, "android.permission.ACCESS_NETWORK_STATE"],
            )

    def test_missing_internet_permission_in_packaged_apk_is_rejected(self):
        with self.assertRaisesRegex(PermissionPolicyError, "missing"):
            validate_permission_policy([EXPECTED_PERMISSION], [])

    def test_client_does_not_request_runtime_permissions(self):
        source = "\n".join(path.read_text(encoding="utf-8") for path in ANDROID_MAIN.rglob("*.kt"))
        for marker in (
            "requestPermissions(",
            "checkSelfPermission(",
            "shouldShowRequestPermissionRationale(",
            "ActivityResultContracts.RequestPermission",
        ):
            self.assertNotIn(marker, source)


if __name__ == "__main__":
    unittest.main()
