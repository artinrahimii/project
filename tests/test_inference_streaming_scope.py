"""Regression contract for the first-release one-complete-response decision."""
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]


def source(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def message_body(proto: str, name: str) -> str:
    match = re.search(
        rf"message\\s+{re.escape(name)}\\s*\\{{([^{{}}]*)\\}}",
        proto,
        re.S,
    )
    if match is None:
        raise AssertionError(f"Protobuf message {name} was not found.")
    return match.group(1)


PROTO = source("protocol/whitep/v1/messages.proto")
BACKEND = source("src/Host.Infrastructure/LlamaCppServerBackend.cs")
ENGINE = source("src/Host.Windows/Inference/LlamaCppInferenceEngine.cs")
TRANSPORT = source(
    "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt"
)
ANDROID_RUNTIME = source(
    "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt"
)
REQUEST_VIEW_MODEL = source(
    "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt"
)
HOST_CAPABILITY_TEST = source(
    "tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs"
)


class InferenceStreamingScopeTests(unittest.TestCase):
    def test_wire_contract_is_one_terminal_response_not_a_chunk_protocol(self):
        response = message_body(PROTO, "InferenceResponse")
        for declaration in (
            r"string\\s+request_id\\s*=\\s*1\\s*;",
            r"string\\s+attempt_id\\s*=\\s*2\\s*;",
            r"InferenceOutcome\\s+outcome\\s*=\\s*4\\s*;",
            r"string\\s+output_text\\s*=\\s*5\\s*;",
            r"uint64\\s+completed_at_unix_ms\\s*=\\s*6\\s*;",
            r"string\\s+round_id\\s*=\\s*9\\s*;",
        ):
            with self.subTest(declaration=declaration):
                self.assertRegex(response, declaration)

        self.assertNotRegex(
            PROTO,
            r"(?m)^\\s*message\\s+\\w*(?:Chunk|Delta|Stream)\\w*\\s*\\{",
        )

    def test_active_windows_backend_does_not_advertise_or_request_streaming(self):
        self.assertIn("SupportsStreaming: false", BACKEND)
        self.assertIn(
            "Assert(!capability.SupportsStreaming",
            HOST_CAPABILITY_TEST,
        )
        request_record = re.search(
            r"private sealed record ChatCompletionRequest\\((.*?)\\);",
            BACKEND,
            re.S,
        )
        self.assertIsNotNone(request_record)
        self.assertIn("MaxTokens", request_record.group(1))
        self.assertNotRegex(request_record.group(1), r"(?i)\\bstream\\b")
        self.assertIn("public async Task<string> GenerateAsync(", BACKEND)
        self.assertIn("public async Task<string> GenerateAsync(", ENGINE)

    def test_android_waits_for_one_correlated_terminal_response_before_ui_update(self):
        await_body = TRANSPORT.split(
            "suspend fun awaitResponse(", 1
        )[1].split("suspend fun cancelAttempts(", 1)[0]
        self.assertIn("pending.response.await()", await_body)
        self.assertIn("pendingAttempts.remove(key, pending)", await_body)
        self.assertIn("InferenceResponseCorrelationValidator.matches(", TRANSPORT)
        self.assertIn("pending.roundId", TRANSPORT)
        self.assertIn("pending.deadlineUnixMs", TRANSPORT)
        self.assertIn(
            "awaitResponse(requestId, attempt.attemptId, timeout)",
            ANDROID_RUNTIME,
        )
        self.assertIn("gateway.awaitRoundResponses(", REQUEST_VIEW_MODEL)
        self.assertIn("outputText = response.outputText", REQUEST_VIEW_MODEL)

    def test_cancellation_remains_typed_and_bound_to_the_exact_attempt(self):
        cancel_body = TRANSPORT.split(
            "suspend fun cancelAttempts(", 1
        )[1].split(
            "private fun closeStreamAfterPendingWriteFailure", 1
        )[0]
        self.assertIn("CancelRequest.newBuilder()", cancel_body)
        self.assertIn(".setRequestId(requestId)", cancel_body)
        self.assertIn(".setAttemptId(attemptId)", cancel_body)
        self.assertIn("message CancelRequest", PROTO)
        self.assertIn("INFERENCE_OUTCOME_CANCELLED", PROTO)
        self.assertIn("INFERENCE_OUTCOME_TIMED_OUT", PROTO)
        self.assertIn("INFERENCE_OUTCOME_REJECTED", PROTO)


if __name__ == "__main__":
    unittest.main()
