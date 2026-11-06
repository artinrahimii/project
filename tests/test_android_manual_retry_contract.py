"""Supplemental source contract for the Android manual-retry request flow.

Production behavior is exercised by RequestViewModelTest under the Android JVM
suite. These checks keep the live Compose/ViewModel wiring and retry contract
visible to the repository-level Step Three quality gate.
"""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
VIEW_MODEL = (ANDROID / "ui/request/RequestViewModel.kt").read_text(encoding="utf-8")
SCREEN = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
JVM_TEST = (
    ROOT
    / "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt"
).read_text(encoding="utf-8")


class AndroidManualRetryContractTests(unittest.TestCase):
    def test_aggregate_request_states_and_manual_retry_flags_are_exposed(self):
        self.assertIn("enum class RequestRunStatus", VIEW_MODEL)
        for status in ("IDLE", "RUNNING", "SUCCEEDED", "PARTIAL", "FAILED", "CANCELLED", "EXPIRED"):
            with self.subTest(status=status):
                self.assertIn(f"    {status},", VIEW_MODEL)
        self.assertIn("val canRetryRequest: Boolean = false", VIEW_MODEL)
        self.assertIn("val isRetryingRequest: Boolean = false", VIEW_MODEL)

    def test_manual_retry_preserves_request_identity_and_existing_budget(self):
        self.assertIn("fun retryFailedAttempts()", VIEW_MODEL)
        self.assertIn("executeRequestRound(context, manualRetry = true)", VIEW_MODEL)
        self.assertIn("requestId = context.request.requestId", VIEW_MODEL)
        self.assertIn("attempts = context.attempts + dispatch.attempts", VIEW_MODEL)
        self.assertIn("context.progress.afterDispatch(", VIEW_MODEL)
        self.assertIn("FirstReleaseInferenceLimits.MAX_ROUNDS", VIEW_MODEL)
        self.assertIn("FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS", VIEW_MODEL)
        self.assertIn("attemptId = UUID.randomUUID().toString()", VIEW_MODEL)

    def test_retry_eligibility_uses_latest_attempt_and_keeps_cancel_terminal(self):
        self.assertIn("private fun latestOutcomeByHost", VIEW_MODEL)
        self.assertIn("private fun failedPeerIdsForRetry", VIEW_MODEL)
        self.assertIn("private fun successfulPeerIds", VIEW_MODEL)
        self.assertIn("excludedEphemeralHostIds = successfulPeerIds(context)", VIEW_MODEL)
        self.assertIn("!hasCancellation", VIEW_MODEL)
        self.assertIn("RequestRunStatus.CANCELLED", VIEW_MODEL)
        self.assertIn("outcome == HostResponseOutcome.CANCELLED", VIEW_MODEL)

    def test_compose_screen_invokes_retry_only_from_the_explicit_user_action(self):
        self.assertIn("onClick = requestViewModel::retryFailedAttempts", SCREEN)
        self.assertIn("Retry failed attempts manually", SCREEN)
        self.assertIn("requestState.canRetryRequest", SCREEN)
        self.assertIn("!requestState.isCancellingRequest", SCREEN)

    def test_android_jvm_acceptance_covers_manual_retry_and_cancel_regression(self):
        expected = (
            "showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds",
            "manualRetryExcludesHostsWhoseLatestAttemptAlreadySucceeded",
            "cancelActionSendsCorrelatedCancellationForEveryQueuedAttemptFromTheLiveDispatch",
        )
        for name in expected:
            with self.subTest(test=name):
                self.assertIn(f"fun {name}(", JVM_TEST)
        self.assertIn("assertEquals(listOf(requestId, requestId), gateway.dispatchedRequestIds)", JVM_TEST)
        self.assertIn("firstAttemptIds.intersect(gateway.dispatchedAttemptIdsByRound[1].toSet()).isEmpty()", JVM_TEST)
        self.assertIn("assertFalse(activeViewModel.uiState.value.canRetryRequest)", JVM_TEST)


if __name__ == "__main__":
    unittest.main()
