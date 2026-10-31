from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt"
COLLECTOR = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceRoundResponseCollector.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundResponseCollectorTest.kt"


class AndroidInferenceRoundResponseCollectorContractTests(unittest.TestCase):
    def test_runtime_exposes_round_collection_using_each_attempts_shared_window(self):
        runtime = RUNTIME.read_text(encoding="utf-8")
        self.assertIn("suspend fun awaitRoundResponses(", runtime)
        self.assertIn("responseCollector.collect(", runtime)
        self.assertIn("attemptRoundWindows[AttemptKey(requestId, attempt.attemptId)]", runtime)
        self.assertIn("awaitResponse(requestId, attempt.attemptId, timeout)", runtime)

    def test_collector_waits_for_all_queued_attempts_and_isolates_failures(self):
        source = COLLECTOR.read_text(encoding="utf-8")
        self.assertIn("attempts.map { attempt ->", source)
        self.assertIn("async {", source)
        self.assertIn("}.awaitAll()", source)
        self.assertIn("roundWindow.remaining()", source)
        self.assertIn("RESPONSE_TIMED_OUT", source)
        self.assertIn("RESPONSE_FAILED", source)
        self.assertIn("currentCoroutineContext().ensureActive()", source)
        self.assertNotIn("firstOrNull", source)
        self.assertNotIn("return@coroutineScope", source)

    def test_jvm_tests_cover_first_success_timeout_sibling_and_caller_cancellation(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        self.assertIn("keepsWaitingForOtherHostsAfterTheFirstValidResponseArrives", source)
        self.assertIn("oneHostTimeoutDoesNotCancelASecondHostsValidResponse", source)
        self.assertIn("preservesSubmissionFailuresWithoutWaitingForUnqueuedHosts", source)
        self.assertIn("doesNotStartAnAttemptWhenItsSharedRoundWindowHasExpired", source)
        self.assertIn("callerCancellationIsNotMisreportedAsAnAttemptTimeout", source)


if __name__ == "__main__":
    unittest.main()
