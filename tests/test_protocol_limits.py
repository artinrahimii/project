"""Contract guards for protocol byte ceilings; executable runtime rejection lives in the C# harness."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LIMITS = (ROOT / "src/Shared.Contracts/ProtocolMessageLimits.cs").read_text(encoding="utf-8")
SCHEMA = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
HOST = (ROOT / "src/Host.Windows/HostLifecycle.cs").read_text(encoding="utf-8")
BACKEND = (ROOT / "src/Host.Windows/LlamaCppServerBackend.cs").read_text(encoding="utf-8")
CSHARP_TESTS = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")


class ProtocolLimitContractTests(unittest.TestCase):
    def test_shared_source_defines_explicit_initial_byte_ceilings(self):
        expected = {
            "MaximumRequestMessageBytes": "16 * 1024",
            "MaximumControlMessageBytes": "16 * 1024",
            "MaximumPromptUtf8Bytes": "8 * 1024",
            "MaximumContextCapsuleBytes": "4 * 1024",
            "MaximumResponseTextUtf8Bytes": "1024 * 1024",
            "MaximumResponseMessageBytes": "2 * 1024 * 1024",
            "MaximumLocalRequestBodyBytes": "64 * 1024",
            "MaximumLocalResponseBodyBytes": "2 * 1024 * 1024",
        }
        for name, value in expected.items():
            with self.subTest(limit=name):
                self.assertRegex(LIMITS, rf"public const int {name}\s*=\s*{re.escape(value)}\s*;")
        self.assertRegex(LIMITS, r"public const uint MaximumOutputTokens\s*=\s*131_072U\s*;")

    def test_wire_schema_documents_prompt_capsule_and_response_ceilings(self):
        self.assertIn("Maximum 8,192 UTF-8 bytes", SCHEMA)
        self.assertIn("Maximum 4,096 bytes", SCHEMA)
        self.assertIn("Maximum 1 MiB UTF-8", SCHEMA)
        self.assertIn("2 MiB", SCHEMA)

    def test_runtime_paths_validate_typed_requests_and_bound_local_http_bodies(self):
        self.assertIn("ProtocolMessageValidator.ValidateInferenceRequest", HOST)
        self.assertIn("ProtocolMessageLimits.ValidateInferenceRequest", (ROOT / "src/Shared.Contracts/ProtocolMessageValidator.cs").read_text(encoding="utf-8"))
        self.assertIn("ProtocolMessageLimits.ValidatePromptText", HOST)
        self.assertIn("ProtocolMessageLimits.ValidateResponseText", HOST)
        self.assertIn("ReadLimitedBodyAsync", BACKEND)
        self.assertIn("ProtocolMessageLimits.ValidateLocalRequestBodyBytes", BACKEND)
        self.assertIn("ProtocolMessageLimits.ValidateResponseText", BACKEND)

    def test_csharp_harness_executes_oversize_rejection_checks(self):
        for phrase in (
            "oversized capsule must be rejected before model execution",
            "oversized serialized request must be rejected before model execution",
            "local backend adapter must reject generated text beyond the UTF-8 response limit",
            "local backend adapter must reject an oversized HTTP response body",
            "The host session must reject oversized output from any inference backend",
        ):
            with self.subTest(assertion=phrase):
                self.assertIn(phrase, CSHARP_TESTS)

    def test_requirement_record_documents_the_same_limits_and_future_transport_boundary(self):
        for value in ("16,384 bytes", "8,192 UTF-8 bytes", "4,096 bytes", "1,048,576 UTF-8 bytes", "2,097,152 bytes", "65,536 bytes"):
            with self.subTest(limit=value):
                self.assertIn(value, REQUIREMENTS)
        self.assertIn("raw frame length before Protobuf parsing", REQUIREMENTS)
        self.assertIn("ValidateControlMessageBytes", LIMITS)


if __name__ == "__main__":
    unittest.main()
