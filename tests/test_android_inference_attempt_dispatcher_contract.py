from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
DISPATCHER = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"


class AndroidInferenceAttemptDispatcherContractTests(unittest.TestCase):
    def test_dispatcher_uses_the_real_payload_contract_and_injected_transport_port(self):
        source = DISPATCHER.read_text(encoding="utf-8")

        self.assertIn("fun interface InferenceRequestTransport", source)
        self.assertIn("private val transport: InferenceRequestTransport", source)
        self.assertIn("InferenceRequestPayloadBuilder.build(", source)
        self.assertIn("requestId = distributedRequest.requestId", source)
        self.assertIn("attemptId = attemptId", source)
        self.assertIn("transport.submit(hostId, payloads[index])", source)

    def test_dispatcher_bounds_and_validates_correlated_host_attempts_before_submission(self):
        source = DISPATCHER.read_text(encoding="utf-8")

        self.assertIn("HostCandidateSelector.MAX_HOSTS_PER_ROUND", source)
        self.assertIn("DUPLICATE_HOST_IDENTITY", source)
        self.assertIn("DUPLICATE_ATTEMPT_ID", source)
        self.assertIn("INVALID_HOST_IDENTITY", source)
        self.assertIn("Prepare and validate every payload before sending any of them.", source)
        self.assertIn("catch (cancellation: CancellationException)", source)

    def test_jvm_suite_covers_correlations_rejections_and_transport_failures(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        expected_tests = (
            "dispatchesCorrelatableAttemptPerSelectedHost",
            "rejectsDuplicateHostIdentityBeforeSubmitting",
            "rejectsMoreThanThreeHostsBeforeSubmitting",
            "rejectsBlankPromptWithRejectedResult",
            "classifiesBlankPromptAsPayloadRejection",
            "identifiesEmptyPromptAsPayloadRejectionCause",
            "doesNotSubmitAnyHostWhenPromptIsBlank",
            "rejectsTerminalRequestBeforeSubmitting",
            "keepsSubmittingOtherHostsAfterTransportRejectsOrThrows",
            "rejectsDuplicateAttemptIdsBeforeSubmitting",
        )
        for test_name in expected_tests:
            with self.subTest(test_name=test_name):
                self.assertIn(f"fun {test_name}(", source)


if __name__ == "__main__":
    unittest.main()
