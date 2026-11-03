from pathlib import Path
import unittest

from tools.verify_android_permissions import (
    DYNAMIC_RECEIVER_SUFFIX,
    EXPECTED_PERMISSION,
    PermissionPolicyError,
    parse_aapt_permissions,
    parse_declared_permissions,
    read_manifest_permissions,
    validate_permission_policy,
)

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
ANDROID_MAIN = ROOT / "src/Client.Android/app/src/main/java"
BUILD_FILE = ROOT / "src/Client.Android/app/build.gradle.kts"
APPLICATION_ID = "com.whitep.client"
DYNAMIC_PERMISSION = APPLICATION_ID + DYNAMIC_RECEIVER_SUFFIX

AAPT_PERMISSIONS = (
    "package: com.whitep.client\n"
    "uses-permission: name='android.permission.INTERNET'\n"
    f"uses-permission: name='{DYNAMIC_PERMISSION}'\n"
)
XMLTREE_SIGNATURE_PERMISSION = f'''N: android=http://schemas.android.com/apk/res/android
  E: manifest (line=2)
    E: permission (line=5)
      A: android:name(0x01010003)="{DYNAMIC_PERMISSION}" (Raw: "{DYNAMIC_PERMISSION}")
      A: android:protectionLevel(0x01010009)=(type 0x10)0x2
    E: uses-permission (line=9)
      A: android:name(0x01010003)="android.permission.INTERNET" (Raw: "android.permission.INTERNET")
'''
XMLTREE_SIGNATURE_PERMISSION_FULL_URI = (
    XMLTREE_SIGNATURE_PERMISSION
    .replace("android:name(", "http://schemas.android.com/apk/res/android:name(")
    .replace(
        "android:protectionLevel(",
        "http://schemas.android.com/apk/res/android:protectionLevel(",
    )
)


class AndroidMinimumPermissionTests(unittest.TestCase):
    def test_source_manifest_requests_only_internet(self):
        self.assertEqual([EXPECTED_PERMISSION], read_manifest_permissions(MANIFEST))

    def test_application_id_matches_gradle_configuration(self):
        build = BUILD_FILE.read_text(encoding="utf-8")
        self.assertIn(f'applicationId = "{APPLICATION_ID}"', build)

    def test_packaged_permission_output_is_parsed(self):
        self.assertEqual(
            [EXPECTED_PERMISSION, DYNAMIC_PERMISSION],
            parse_aapt_permissions(AAPT_PERMISSIONS),
        )

    def test_sdk_qualified_permission_is_detected(self):
        output = "uses-permission-sdk-23: name='android.permission.READ_CONTACTS'\n"
        self.assertEqual(["android.permission.READ_CONTACTS"], parse_aapt_permissions(output))

    def test_signature_only_androidx_declaration_is_parsed(self):
        self.assertEqual(
            [{"name": DYNAMIC_PERMISSION, "protection_level": 0x2}],
            parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION),
        )

    def test_aapt2_full_namespace_attributes_are_parsed(self):
        self.assertEqual(
            [{"name": DYNAMIC_PERMISSION, "protection_level": 0x2}],
            parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION_FULL_URI),
        )

    def test_exact_minimum_permission_set_with_androidx_signature_permission_passes(self):
        declarations = parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION)
        validate_permission_policy(
            [EXPECTED_PERMISSION],
            [EXPECTED_PERMISSION, DYNAMIC_PERMISSION],
            declarations,
            APPLICATION_ID,
        )

    def test_unneeded_source_permission_is_rejected(self):
        declarations = parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION)
        with self.assertRaises(PermissionPolicyError):
            validate_permission_policy(
                [EXPECTED_PERMISSION, "android.permission.READ_EXTERNAL_STORAGE"],
                [EXPECTED_PERMISSION, DYNAMIC_PERMISSION],
                declarations,
                APPLICATION_ID,
            )

    def test_dependency_added_sensitive_permission_is_rejected(self):
        declarations = parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION)
        with self.assertRaisesRegex(PermissionPolicyError, "ACCESS_NETWORK_STATE"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [EXPECTED_PERMISSION, DYNAMIC_PERMISSION, "android.permission.ACCESS_NETWORK_STATE"],
                declarations,
                APPLICATION_ID,
            )

    def test_unknown_custom_permission_is_rejected(self):
        declarations = parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION)
        with self.assertRaisesRegex(PermissionPolicyError, "unexpected packaged permissions"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [EXPECTED_PERMISSION, "com.whitep.client.UNKNOWN_PERMISSION"],
                declarations,
                APPLICATION_ID,
            )

    def test_dynamic_receiver_permission_without_signature_declaration_is_rejected(self):
        dangerous = XMLTREE_SIGNATURE_PERMISSION.replace("0x2", "0x1")
        declarations = parse_declared_permissions(dangerous)
        with self.assertRaisesRegex(PermissionPolicyError, "protectionLevel=signature"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [EXPECTED_PERMISSION, DYNAMIC_PERMISSION],
                declarations,
                APPLICATION_ID,
            )

    def test_dynamic_receiver_permission_must_be_declared(self):
        with self.assertRaisesRegex(PermissionPolicyError, "declared exactly once"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [EXPECTED_PERMISSION, DYNAMIC_PERMISSION],
                [],
                APPLICATION_ID,
            )

    def test_missing_internet_permission_is_rejected(self):
        declarations = parse_declared_permissions(XMLTREE_SIGNATURE_PERMISSION)
        with self.assertRaisesRegex(PermissionPolicyError, "missing"):
            validate_permission_policy(
                [EXPECTED_PERMISSION],
                [DYNAMIC_PERMISSION],
                declarations,
                APPLICATION_ID,
            )

    def test_client_does_not_request_runtime_permissions(self):
        source = "\n".join(
            path.read_text(encoding="utf-8") for path in ANDROID_MAIN.rglob("*.kt")
        )
        for marker in (
            "requestPermissions(",
            "checkSelfPermission(",
            "shouldShowRequestPermissionRationale(",
            "ActivityResultContracts.RequestPermission",
        ):
            self.assertNotIn(marker, source)


if __name__ == "__main__":
    unittest.main()
