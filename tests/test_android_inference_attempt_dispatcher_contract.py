from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
DISPATCHER = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"
FRAME_CODEC = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt"
FRAME_CODEC_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/PeerMessageFrameCodecTest.kt"


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

    def test_android_frame_codec_matches_the_windows_varint_message_boundary(self):
        source = FRAME_CODEC.read_text(encoding="utf-8")
        self.assertIn("DEFAULT_MAXIMUM_MESSAGE_BYTES = 4 * 1024 * 1024", source)
        self.assertIn("MAXIMUM_VARINT_BYTES = 5", source)
        self.assertIn("fun encode(", source)
        self.assertIn("fun feed(chunk: ByteArray)", source)
        self.assertIn("The peer message length varint is not canonical.", source)
        self.assertIn("fun finish()", source)

    def test_frame_codec_jvm_suite_covers_fragmentation_and_malformed_frames(self):
        source = FRAME_CODEC_TEST.read_text(encoding="utf-8")
        expected_tests = (
            "encodesUnsignedVarintLengthAtSingleAndMultiByteBoundaries",
            "decodesFramesSplitAcrossLengthPrefixAndPayloadChunks",
            "decodesMultipleFramesAndAnEmptyMessageFromOneChunk",
            "rejectsAnOversizedDeclaredLengthBeforeReceivingItsPayload",
            "rejectsNonCanonicalAndOverlongLengthPrefixes",
            "reportsTruncatedPayloadWhenThePeerStreamEnds",
            "rejectsInvalidLimitsAndEncoderOversize",
        )
        for test_name in expected_tests:
            with self.subTest(test_name=test_name):
                self.assertIn(f"fun {test_name}(", source)


if __name__ == "__main__":
    unittest.main()
