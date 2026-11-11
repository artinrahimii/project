from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"
DISTRIBUTED = (ANDROID / "requests/DistributedRequest.kt").read_text(encoding="utf-8")
RUNTIME = (ANDROID / "requests/AndroidInferenceRequestRuntime.kt").read_text(encoding="utf-8")
VIEW_MODEL = (ANDROID / "ui/request/RequestViewModel.kt").read_text(encoding="utf-8")
ACTIVITY = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
VM_TESTS = (TESTS / "ui/request/RequestViewModelTest.kt").read_text(encoding="utf-8")
DISPATCHER_TESTS = (TESTS / "requests/InferenceAttemptDispatcherTest.kt").read_text(encoding="utf-8")


class UpdateS1015RetryPolicyTests(unittest.TestCase):
    def test_first_release_cardinality_is_one_host_and_three_sequential_attempts(self):
        self.assertIn("const val MAX_ROUNDS = 3", DISTRIBUTED)
        self.assertIn("const val MAX_ACTIVE_HOSTS_PER_REQUEST = 1", DISTRIBUTED)
        self.assertIn("const val MAX_TOTAL_HOSTS = MAX_ROUNDS * MAX_ACTIVE_HOSTS_PER_REQUEST", DISTRIBUTED)
        self.assertIn("maxHosts = FirstReleaseInferenceLimits.MAX_ACTIVE_HOSTS_PER_REQUEST", RUNTIME)
        self.assertIn("selectedHosts.size > FirstReleaseInferenceLimits.MAX_ACTIVE_HOSTS_PER_REQUEST", RUNTIME)
        self.assertIn("progress.roundsDispatched >= distributedRequest.maxRounds", RUNTIME)
        self.assertIn("progress.hostsAttempted >= distributedRequest.maxTotalHosts", RUNTIME)

    def test_retry_is_explicit_manual_bounded_and_targets_failed_hosts(self):
        self.assertIn("fun retryFailedAttempts()", VIEW_MODEL)
        self.assertIn("executeRequestRound(context, manualRetry = true)", VIEW_MODEL)
        self.assertIn("manualRetry && failedPeerIdsForRetry(currentContext).isEmpty()", VIEW_MODEL)
        self.assertIn("context.request.requestId", VIEW_MODEL)
        self.assertIn("attemptId = UUID.randomUUID().toString()", VIEW_MODEL)
        self.assertIn("failedPeerIdsForRetry(context)", VIEW_MODEL)
        self.assertIn("FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS", VIEW_MODEL)
        self.assertIn("onClick = requestViewModel::retryFailedAttempts", ACTIVITY)

    def test_kotlin_regressions_cover_manual_retry_and_no_duplicate_round(self):
        self.assertIn("showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds", VM_TESTS)
        self.assertIn("successfulSingleHostRequestDoesNotOfferOrDispatchAnotherAttempt", VM_TESTS)
        self.assertIn("continuesWithOneAvailableHostAndSubmitsIt", DISPATCHER_TESTS)
        self.assertIn("rejectsZeroAvailableHostsWithoutSubmittingAnyRequest", DISPATCHER_TESTS)


if __name__ == "__main__":
    unittest.main()
