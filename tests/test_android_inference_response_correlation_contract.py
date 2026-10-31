import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROTO = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
BUILDER = (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt").read_text(encoding="utf-8")
DISPATCHER = (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt").read_text(encoding="utf-8")
TRANSPORT = (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt").read_text(encoding="utf-8")
VALIDATOR = (ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/InferenceResponseCorrelationValidator.kt").read_text(encoding="utf-8")
WINDOWS_VALIDATOR = (ROOT / "src/Host.Windows/Networking/PeerResponseCorrelationValidator.cs").read_text(encoding="utf-8")
VALIDATOR_TEST = (ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/InferenceResponseCorrelationValidatorTest.kt").read_text(encoding="utf-8")
VIEW_MODEL_TEST = (ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt").read_text(encoding="utf-8")
HOST = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
INTEROP = (ROOT / "tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs").read_text(encoding="utf-8")


class InferenceResponseRoundCorrelationContractTests(unittest.TestCase):
    def test_both_wire_messages_carry_an_additive_round_identifier(self):
        self.assertIn("string round_id = 9;", PROTO)
        self.assertEqual(2, PROTO.count("string round_id = 9;"))

    def test_dispatch_creates_one_round_id_and_reuses_it_for_each_host_attempt(self):
        self.assertIn("val roundId = UUID.randomUUID().toString()", DISPATCHER)
        self.assertIn("roundId = roundId", DISPATCHER)
        self.assertIn("this.roundId = roundId", BUILDER)
        self.assertIn("EMPTY_ROUND_ID", BUILDER)

    def test_android_pending_attempt_binds_round_and_absolute_deadline(self):
        self.assertIn("val roundId: String", TRANSPORT)
        self.assertIn("val deadlineUnixMs: Long", TRANSPORT)
        self.assertIn("request.roundId.isBlank()", TRANSPORT)
        self.assertIn("request.deadlineUnixMs <= clock.millis()", TRANSPORT)
        self.assertIn("InferenceResponseCorrelationValidator.matches(", TRANSPORT)

    def test_android_rejects_wrong_round_late_completion_and_late_arrival(self):
        self.assertIn("response.roundId == expectedRoundId", VALIDATOR)
        self.assertIn("response.completedAtUnixMs <= requestDeadlineUnixMs", VALIDATOR)
        self.assertIn("receivedAtUnixMs <= requestDeadlineUnixMs", VALIDATOR)

    def test_android_does_not_correlate_a_previous_request_response_to_a_new_request(self):
        self.assertIn("response.requestId == expectedRequestId", VALIDATOR)
        self.assertIn("response.attemptId == expectedAttemptId", VALIDATOR)
        self.assertIn("val key = AttemptKey(response.requestId, response.attemptId)", TRANSPORT)
        self.assertIn("val pending = pendingAttempts[key] ?: return", TRANSPORT)
        self.assertIn("pendingAttempts.remove(key, pending)", TRANSPORT)
        self.assertIn("receivedAtUnixMs = clock.millis()", TRANSPORT)
        self.assertIn(
            "fun rejectsAResponseFromPreviousRequestWithinTheNewRequestWindow()",
            VALIDATOR_TEST,
        )
        self.assertIn(
            "fun rejectsAResponseFromPreviousRequestWhenANewRequestIsExpected()",
            VIEW_MODEL_TEST,
        )

    def test_windows_echoes_and_validates_round_and_request_deadline(self):
        self.assertIn("RoundId = request.RoundId", HOST)
        self.assertIn("PeerResponseCorrelationValidator.Validate(request, response, localPeerIdentity)", HOST)
        self.assertIn("request.RoundId", WINDOWS_VALIDATOR)
        self.assertIn("response.RoundId", WINDOWS_VALIDATOR)
        self.assertIn("response.CompletedAtUnixMs > request.DeadlineUnixMs", WINDOWS_VALIDATOR)
        self.assertIn("RoundId = request.RoundId", INTEROP)


if __name__ == "__main__":
    unittest.main()
