from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
LIMITS = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt"
SELECTOR = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt"
RUNTIME = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt"
DISPATCHER = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"


class AndroidInferenceAttemptBudgetAcceptanceTests(unittest.TestCase):
    def test_first_release_limits_are_centralized_and_bounded(self):
        source = LIMITS.read_text(encoding="utf-8")
        self.assertIn("const val MAX_ROUNDS = 3", source)
        self.assertIn("const val MAX_ACTIVE_HOSTS_PER_REQUEST = 1", source)
        self.assertIn("const val MAX_TOTAL_HOSTS = MAX_ROUNDS * MAX_ACTIVE_HOSTS_PER_REQUEST", source)
        self.assertIn("const val MAX_HOSTS_PER_ROUND = 3", source)
        self.assertIn("maxTotalHosts <= maxRounds * FirstReleaseInferenceLimits.MAX_ACTIVE_HOSTS_PER_REQUEST", source)

    def test_selector_caps_each_round_and_remaining_request_budget(self):
        source = SELECTOR.read_text(encoding="utf-8")
        self.assertIn("const val MAX_HOSTS_PER_ROUND = MultiHostCompatibilityLimits.MAX_HOSTS_PER_ROUND", source)
        self.assertIn(".take(maxHosts)", source)
        self.assertIn("distributedRequest.maxTotalHosts - progress.hostsAttempted", source)
        self.assertIn("previouslyAttemptedHostIds.toSet()", source)
        self.assertIn("retryableEphemeralHostIds", source)
        runtime_source = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("maxHosts == FirstReleaseInferenceLimits.MAX_ACTIVE_HOSTS_PER_REQUEST", runtime_source)
        self.assertIn("selectedHosts.size > FirstReleaseInferenceLimits.MAX_ACTIVE_HOSTS_PER_REQUEST", runtime_source)

    def test_dispatcher_rejects_oversized_or_exhausted_request_budgets_before_transport(self):
        source = DISPATCHER.read_text(encoding="utf-8")
        self.assertIn("progress.roundsDispatched >= distributedRequest.maxRounds", source)
        self.assertIn("progress.hostsAttempted >= distributedRequest.maxTotalHosts", source)
        self.assertIn("selectedHosts.size > distributedRequest.maxTotalHosts - progress.hostsAttempted", source)
        self.assertIn("selectedHosts.size > HostCandidateSelector.MAX_HOSTS_PER_ROUND", source)
        self.assertIn("DUPLICATE_HOST_IDENTITY", source)
        self.assertIn("transport.submit(hostId, payloads[index])", source)

    def test_jvm_acceptance_suite_exercises_full_and_custom_retry_budgets(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        for marker in (
            "enforcesThreeDistinctHostsPerRoundAndNineAttemptsAcrossTheRequest",
            "honorsTheRemainingAttemptBudgetWhenAPreviouslyUsedHostIsRetried",
            "assertEquals(FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS, progress.hostsAttempted)",
            "assertEquals(4, progress.hostsAttempted)",
            "assertEquals(9, transport.requests.size)",
            "assertEquals(4, transport.requests.size)",
            'retryableEphemeralHostIds = setOf("host-01")',
        ):
            with self.subTest(marker=marker):
                self.assertIn(marker, source)


if __name__ == "__main__":
    unittest.main()
