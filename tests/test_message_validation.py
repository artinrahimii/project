"""Static contract guards for the executable typed-message parser/validator and malformed-wire corpus."""
import base64
import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VALIDATOR_PATH = ROOT / "src/Shared.Contracts/ProtocolMessageValidator.cs"
PARSER_PATH = ROOT / "src/Host.Windows/ProtocolMessageParser.cs"
HOST_PATH = ROOT / "src/Host.Windows/HostLifecycle.cs"
LIMITS_PATH = ROOT / "src/Shared.Contracts/ProtocolMessageLimits.cs"
HARNESS_PATH = ROOT / "tests/HostLifecycle.Tests/Program.cs"
FIXTURES_PATH = ROOT / "tests/contracts/invalid-message-cases.json"

VALIDATOR = VALIDATOR_PATH.read_text(encoding="utf-8")
PARSER = PARSER_PATH.read_text(encoding="utf-8")
HOST = HOST_PATH.read_text(encoding="utf-8")
LIMITS = LIMITS_PATH.read_text(encoding="utf-8")
HARNESS = HARNESS_PATH.read_text(encoding="utf-8")
FIXTURES = json.loads(FIXTURES_PATH.read_text(encoding="utf-8"))


class InvalidMessageContractTests(unittest.TestCase):
    def test_malformed_wire_corpus_has_known_invalid_base64_payloads_and_error_codes(self):
        cases = FIXTURES["cases"]
        self.assertEqual(3, len(cases))
        self.assertEqual(
            {"zero-tag", "truncated-varint", "truncated-length-delimited-field"},
            {case["id"] for case in cases},
        )
        for case in cases:
            with self.subTest(case=case["id"]):
                payload = base64.b64decode(case["wireBase64"], validate=True)
                self.assertGreater(len(payload), 0)
                self.assertEqual("PROTOCOL_ERROR_CODE_INVALID_MESSAGE", case["expectedErrorCode"])

    def test_raw_bytes_are_bounded_before_protobuf_parse_and_parse_errors_are_rejected(self):
        self.assertIn("ValidateRequestMessageBytes(wireBytes.LongLength)", PARSER)
        self.assertIn("ValidateControlMessageBytes(wireBytes.LongLength)", PARSER)
        self.assertIn("InferenceRequest.Parser.ParseFrom(wireBytes)", PARSER)
        self.assertIn("HostAnnouncement.Parser.ParseFrom(wireBytes)", PARSER)
        self.assertIn("catch (InvalidProtocolBufferException)", PARSER)
        self.assertIn("MaximumControlMessageBytes", LIMITS)

    def test_semantic_validator_checks_required_values_versions_and_response_enum(self):
        for required in (
            "ValidateInferenceRequest(",
            "ValidateHostAnnouncement(",
            "ValidateInferenceResponse(",
            "ProtocolVersionPolicy.Evaluate",
            "deadlineUnixMs == 0",
            "maxOutputTokens == 0",
            "outcomeValue < 1 || outcomeValue > 5",
        ):
            with self.subTest(check=required):
                self.assertIn(required, VALIDATOR)

    def test_runtime_session_calls_shared_validator_before_generation(self):
        self.assertIn("ProtocolMessageValidator.ValidateInferenceRequest", HOST)
        self.assertIn("InvalidProtocolBufferException", PARSER)

    def test_csharp_harness_exercises_malformed_oversized_version_and_value_rejection(self):
        for assertion in (
            "A request missing attempt_id must be rejected",
            "An oversized raw request must be rejected before Protobuf parsing",
            "An announcement with an incompatible protocol version must be rejected",
            "A malformed version string must be rejected",
            "An unspecified InferenceResponse outcome must be rejected",
            "An oversized serialized InferenceResponse must be rejected",
        ):
            with self.subTest(assertion=assertion):
                self.assertIn(assertion, HARNESS)


if __name__ == "__main__":
    unittest.main()
