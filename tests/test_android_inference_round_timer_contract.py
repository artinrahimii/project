import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REQUESTS = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests"
TIMER = REQUESTS / "InferenceRoundTimer.kt"
DISPATCHER = REQUESTS / "InferenceAttemptDispatcher.kt"
RUNTIME = REQUESTS / "AndroidInferenceRequestRuntime.kt"
TIMER_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundTimerTest.kt"
DISPATCHER_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"


class AndroidInferenceRoundTimerContractTests(unittest.TestCase):
    def test_timer_uses_injected_clock_and_configured_timeout(self):
        source = TIMER.read_text(encoding="utf-8")
        self.assertIn("private val clock: Clock = Clock.systemUTC()", source)
        self.assertIn("val timeout: Duration = DEFAULT_TIMEOUT", source)
        self.assertIn("startedAt.plus(timeout)", source)
        self.assertIn("Instant.ofEpochMilli(requestDeadlineUnixMs)", source)
        self.assertIn("minOf(configuredDeadline, requestDeadline)", source)
        self.assertIn("fun remaining(): Duration", source)

    def test_dispatcher_bounds_each_submission_to_the_same_round_window(self):
        source = DISPATCHER.read_text(encoding="utf-8")
        self.assertIn("roundWindow: InferenceRoundTimer.Window? = null", source)
        self.assertIn("roundWindow?.isExpired() == true", source)
        self.assertIn("roundWindow?.remainingTimeoutMillis()", source)
        self.assertIn("withTimeoutOrNull(remainingMillis)", source)
        self.assertIn("InferenceAttemptSubmissionStatus.TIMED_OUT", source)
        self.assertIn("InferenceRoundDispatchRejection.ROUND_TIMEOUT", source)

    def test_runtime_creates_a_fresh_round_window_and_tracks_queued_attempts(self):
        source = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("roundTimer.startRound(distributedRequest.deadlineUnixMs)", source)
        self.assertIn("roundWindow = roundWindow", source)
        self.assertIn("attemptRoundWindows[AttemptKey(distributedRequest.requestId, attempt.attemptId)] = roundWindow", source)
        self.assertIn("InferenceAttemptSubmissionStatus.QUEUED", source)

    def test_response_wait_cannot_extend_beyond_the_round_deadline(self):
        source = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("roundTimer.boundedTimeout(roundWindow, timeout)", source)
        self.assertIn("roundWindow.remainingTimeoutMillis()", source)
        self.assertIn("attemptRoundWindows.remove(key, roundWindow)", source)
        self.assertIn("attemptRoundWindows.clear()", source)

    def test_round_timer_is_configurable_from_the_runtime_factory(self):
        source = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("roundTimeout: Duration = DEFAULT_ROUND_TIMEOUT", source)
        self.assertIn("InferenceRoundTimer(clock, roundTimeout)", source)
        self.assertIn("clock = clock", source)

    def test_jvm_tests_cover_fake_clock_expiry_and_mid_round_timeout(self):
        timer_test = TIMER_TEST.read_text(encoding="utf-8")
        dispatcher_test = DISPATCHER_TEST.read_text(encoding="utf-8")
        self.assertIn("class MutableClock", timer_test)
        self.assertIn("configuredTimeoutUsesTheInjectableClock", timer_test)
        self.assertIn("earlierAbsoluteRequestDeadlineCapsTheRoundTimeout", timer_test)
        self.assertIn("expiredRequestDeadlineCreatesAnImmediatelyExpiredWindow", timer_test)
        self.assertIn("stopsSubmittingFurtherHostsWhenTheRoundWindowExpires", dispatcher_test)
        self.assertIn("rejectsAnAlreadyExpiredRoundBeforeSubmitting", dispatcher_test)


if __name__ == "__main__":
    unittest.main()
