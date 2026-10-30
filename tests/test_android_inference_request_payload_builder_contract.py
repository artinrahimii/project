from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
PROTO = ROOT / "protocol/whitep/v1/messages.proto"
BUILDER = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt"
KOTLIN_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/protocol/InferenceRequestPayloadBuilderTest.kt"


class AndroidInferenceRequestPayloadContractTests(unittest.TestCase):
    def test_inference_request_schema_contains_only_the_explicit_payload_fields(self):
        proto = PROTO.read_text(encoding="utf-8")
        match = re.search(r"\\bmessage\\s+InferenceRequest\\s*\\{([^}]*)\\}", proto, re.DOTALL)
        self.assertIsNotNone(match, "InferenceRequest message must exist in the canonical Protobuf schema")

        fields = dict(
            re.findall(
                r"^\\s*(?:optional\\s+|repeated\\s+)?([A-Za-z_][A-Za-z0-9_.]*)\\s+([A-Za-z_][A-Za-z0-9_]*)\\s*=\\s*\\d+\\s*;",
                match.group(1),
                re.MULTILINE,
            )
        )
        expected = {
            "request_id": "string",
            "attempt_id": "string",
            "model_id": "string",
            "prompt": "string",
            "deadline_unix_ms": "uint64",
            "max_output_tokens": "uint32",
            "context_capsule": "bytes",
            "protocol_version": "string",
        }
        self.assertEqual(expected, fields)
        forbidden = {"history", "message_history", "conversation_id", "permanent_conversation_id"}
        self.assertFalse(forbidden.intersection(name.lower() for name in fields))

    def test_payload_builder_accepts_one_selected_message_not_a_history_or_conversation_identity(self):
        source = BUILDER.read_text(encoding="utf-8")
        match = re.search(
            r"\\bfun\\s+build\\s*\\((.*?)\\)\\s*:\\s*InferenceRequestPayloadBuildResult",
            source,
            re.DOTALL,
        )
        self.assertIsNotNone(match, "Production payload builder API must be present")
        parameters = re.findall(r"^\\s*([A-Za-z_][A-Za-z0-9_]*)\\s*:", match.group(1), re.MULTILINE)
        self.assertEqual(
            [
                "requestId",
                "attemptId",
                "modelId",
                "currentMessage",
                "capsule",
                "deadlineUnixMs",
                "maxOutputTokens",
                "protocolVersion",
            ],
            parameters,
        )
        self.assertIn("this.prompt = currentMessage", source)
        self.assertIn("this.contextCapsule = ByteString.copyFrom(capsuleBytes)", source)
        self.assertIn("capsule.encode().bytes", source)

    def test_jvm_test_checks_the_selected_message_capsule_and_absence_of_old_history_content(self):
        source = KOTLIN_TEST.read_text(encoding="utf-8")
        for marker in (
            "payloadContainsOnlySelectedMessageAndEncodedCapsuleWithoutFullHistory",
            "assertEquals(selectedMessage, request.prompt)",
            "assertArrayEquals(encodedCapsule, request.contextCapsule.toByteArray())",
            "unrelatedHistory.none",
            "rejectsPromptThatExceedsTheUtf8ByteLimit",
            "rejectsInvalidCapsuleInsteadOfSendingAnEmptyFallback",
            "rejectsSerializedRequestThatExceedsSixteenKiB",
        ):
            self.assertIn(marker, source)
        self.assertNotIn("descriptorForType", source)


if __name__ == "__main__":
    unittest.main()
