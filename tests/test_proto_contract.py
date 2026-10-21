"""Contract checks for the shared versioned Protobuf schema and build-time generators."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "protocol/whitep/v1/messages.proto"
SCHEMA = SCHEMA_PATH.read_text(encoding="utf-8")
CSPROJ = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
ANDROID_ROOT = (ROOT / "src/Client.Android/build.gradle.kts").read_text(encoding="utf-8")
ANDROID_APP_PATH = ROOT / "src/Client.Android/app/build.gradle.kts"
ANDROID_APP = ANDROID_APP_PATH.read_text(encoding="utf-8")
README = (ROOT / "README.md").read_text(encoding="utf-8")


class ProtoContractTests(unittest.TestCase):
    def test_schema_is_versioned_and_contains_required_wire_messages(self):
        self.assertIn('syntax = "proto3";', SCHEMA)
        self.assertIn("package whitep.protocol.v1;", SCHEMA)
        for name in (
            "ModelCapability", "HostCapabilities", "InferenceRequest",
            "InferenceResponse", "CancelRequest", "HostStatusRequest",
            "HostStatus", "ProtocolError",
        ):
            with self.subTest(message=name):
                self.assertRegex(SCHEMA, rf"message\s+{name}\s*\{{")
        for enum_name in ("HostRuntimeState", "InferenceOutcome", "ProtocolErrorCode"):
            with self.subTest(enum=enum_name):
                self.assertRegex(SCHEMA, rf"enum\s+{enum_name}\s*\{{")

    def test_field_numbers_are_unique_inside_each_message_and_enum(self):
        blocks = re.findall(r"\b(?:message|enum)\s+\w+\s*\{([^{}]*)\}", SCHEMA, re.S)
        self.assertGreaterEqual(len(blocks), 11)
        for block in blocks:
            numbers = re.findall(r"=\s*(\d+)\s*;", block)
            with self.subTest(block=block[:60]):
                self.assertEqual(len(numbers), len(set(numbers)), "duplicate field/enum number")

    def test_windows_build_generates_csharp_from_the_shared_schema(self):
        self.assertIn('Include="../../protocol/whitep/v1/messages.proto"', CSPROJ)
        self.assertIn('ProtoRoot="../../protocol"', CSPROJ)
        self.assertIn('GrpcServices="None"', CSPROJ)
        self.assertIn('PackageReference Include="Google.Protobuf" Version="3.36.2"', CSPROJ)
        self.assertIn('PackageReference Include="Grpc.Tools" Version="2.84.0"', CSPROJ)

    def test_android_generates_java_lite_from_the_same_repository_schema(self):
        self.assertIn('id("com.google.protobuf") version "0.10.0" apply false', ANDROID_ROOT)
        self.assertIn('id("com.google.protobuf")', ANDROID_APP)
        self.assertIn('srcDir("../../../protocol")', ANDROID_APP)
        self.assertIn('com.google.protobuf:protoc:4.36.2', ANDROID_APP)
        self.assertIn('com.google.protobuf:protobuf-javalite:4.36.2', ANDROID_APP)
        self.assertIn('create("java") { option("lite") }', ANDROID_APP)
        self.assertEqual((ANDROID_APP_PATH.parent / "../../../protocol").resolve(), SCHEMA_PATH.parent.parent.parent.resolve())

    def test_readme_distinguishes_shared_schema_from_unimplemented_transport(self):
        self.assertIn("protocol/whitep/v1/messages.proto", README)
        self.assertIn("انتقال شبکه", README)
        self.assertIn("هنوز پیاده‌سازی نشده‌اند", README)


if __name__ == "__main__":
    unittest.main()
