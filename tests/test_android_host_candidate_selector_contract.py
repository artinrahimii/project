from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
SELECTOR = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt"


class AndroidHostCandidateSelectorContractTests(unittest.TestCase):
    def test_selector_uses_the_canonical_inference_request_contract(self):
        source = SELECTOR.read_text(encoding="utf-8")

        self.assertIn("fun select(", source)
        self.assertIn("request: InferenceRequest", source)
        self.assertIn("request.modelId", source)
        self.assertIn("request.protocolVersion", source)
        self.assertIn("request.maxOutputTokens", source)
        self.assertIn("excludedEphemeralHostIds: Set<String>", source)
        self.assertIn("fun selectNextRound(", source)
        self.assertIn("previousRoundResponses.responses.isNotEmpty()", source)
        self.assertIn("previouslyAttemptedHostIds.toSet()", source)
        self.assertIn("val prompt = request.prompt", source)
        self.assertIn("prompt.toByteArray(Charsets.UTF_8)", source)
        self.assertIn("val contextCapsule = request.contextCapsule", source)
        self.assertIn("contextCapsule.size()", source)

    def test_selector_fails_closed_on_readiness_identity_and_capacity(self):
        source = SELECTOR.read_text(encoding="utf-8")

        self.assertIn("HostRuntimeState.HOST_RUNTIME_STATE_READY", source)
        self.assertIn("capabilities.ephemeralHostId != hostId", source)
        self.assertIn("status.ephemeralHostId != hostId", source)
        self.assertIn("status.activeRequests >= capabilities.maxConcurrentRequests", source)

    def test_selector_returns_at_most_three_unique_hosts_deterministically(self):
        source = SELECTOR.read_text(encoding="utf-8")

        self.assertIn("const val MAX_HOSTS_PER_ROUND = 3", source)
        self.assertIn(".groupBy { candidate -> candidate.announcement.ephemeralHostId }", source)
        self.assertIn(".sortedBy { candidate -> candidate.announcement.ephemeralHostId }", source)
        self.assertIn(".take(MAX_HOSTS_PER_ROUND)", source)

    def test_jvm_suite_covers_selection_rejection_and_bounds(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        expected_tests = (
            "selectsAtMostThreeDistinctCompatibleReadyHostsInStableOrder",
            "rejectsHostsThatAreNotReadyOrHaveNoAvailableConcurrency",
            "rejectsInconsistentPeerIdentityAndProtocolSnapshots",
            "requiresTheRequestedModelInBothAnnouncementAndLiveStatus",
            "rejectsCandidatesWhosePromptCapsuleOrOutputLimitsAreTooSmall",
            "rejectsLocallyInvalidOrOversizedInferenceRequests",
            "excludesAmbiguousDuplicatePeerIdentities",
            "doesNotSelectAHostWhenTheRequestedProtocolIsNotAdvertised",
            "selectsNextThreeDistinctHostsWhenThePreviousRoundHasNoValidResponse",
            "doesNotSelectAnotherRoundWhenThePreviousRoundContainsAValidResponse",
            "failsClosedWhenPreviousRoundAttemptsCannotDefineASafeExclusionSet",
        )
        for test_name in expected_tests:
            with self.subTest(test_name=test_name):
                self.assertIn(f"fun {test_name}(", source)


if __name__ == "__main__":
    unittest.main()
