"""Cross-layer regression guards for bounded retry ownership.

The Android distributed request flow owns explicit, budgeted retries. Lower
layers may time out, fail, or try alternate connection addresses, but must not
silently replay an inference request or ambiguously resend a response.
"""

from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
ANDROID_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"
HOST = ROOT / "src/Host.Windows"
SHARED = ROOT / "src/Shared.Contracts"

DISTRIBUTED_REQUEST = (ANDROID / "requests/DistributedRequest.kt").read_text(encoding="utf-8")
DISPATCHER = (ANDROID / "requests/InferenceAttemptDispatcher.kt").read_text(encoding="utf-8")
DISPATCHER_TESTS = (
    ANDROID_TESTS / "requests/InferenceAttemptDispatcherTest.kt"
).read_text(encoding="utf-8")
REQUEST_VIEW_MODEL = (ANDROID / "ui/request/RequestViewModel.kt").read_text(encoding="utf-8")
REQUEST_SCREEN = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
REMOTE_ERROR_PARSER = (ANDROID / "network/RemoteRequestErrorParser.kt").read_text(encoding="utf-8")
HOST_MAPPER = (HOST / "Networking/NetworkInferenceRequestResponseMapper.cs").read_text(encoding="utf-8")
DELIVERY_GATE = (HOST / "Requests/HostRequestResponseDeliveryGate.cs").read_text(encoding="utf-8")
DELIVERY_TESTS = (
    ROOT / "tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs"
).read_text(encoding="utf-8")
ANNOUNCEMENT = (SHARED / "Networking/PeerAnnouncement.cs").read_text(encoding="utf-8")
FALLBACK_CONNECTOR = (
    HOST / "Networking/PeerTransportFallbackConnector.cs"
).read_text(encoding="utf-8")


class RetryPolicyBoundaryTests(unittest.TestCase):
    def test_distributed_request_has_one_shared_and_enforced_attempt_budget(self):
        self.assertIn("const val MAX_ROUNDS = 3", DISTRIBUTED_REQUEST)
        self.assertIn("const val MAX_HOSTS_PER_ROUND = 3", DISTRIBUTED_REQUEST)
        self.assertIn("const val MAX_TOTAL_HOSTS = MAX_ROUNDS * MAX_HOSTS_PER_ROUND", DISTRIBUTED_REQUEST)
        self.assertIn("maxRounds in 1..FirstReleaseInferenceLimits.MAX_ROUNDS", DISTRIBUTED_REQUEST)
        self.assertIn("maxTotalHosts in 1..FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS", DISTRIBUTED_REQUEST)
        self.assertIn("maxTotalHosts <= maxRounds * FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND", DISTRIBUTED_REQUEST)
        self.assertIn("attemptedEphemeralHostIds = attemptedEphemeralHostIds + hostIds", DISTRIBUTED_REQUEST)

    def test_one_round_dispatch_times_out_instead_of_replaying_a_host_submission(self):
        # One direct call site is used in the bounded and unbounded branch of the same submission.
        self.assertEqual(2, DISPATCHER.count("transport.submit(hostId, payloads[index])"))
        self.assertIn("remainingMillis == null", DISPATCHER)
        self.assertIn("withTimeoutOrNull(remainingMillis)", DISPATCHER)
        self.assertIn("InferenceAttemptSubmissionStatus.TIMED_OUT", DISPATCHER)
        self.assertIn("progress.roundsDispatched >= distributedRequest.maxRounds", DISPATCHER)
        self.assertIn("progress.hostsAttempted >= distributedRequest.maxTotalHosts", DISPATCHER)
        self.assertIn("does not implement peer discovery", DISPATCHER)
        self.assertIn("timeout scheduling, retries", DISPATCHER)
        self.assertIn("keepsSubmittingOtherHostsAfterTransportRejectsOrThrows", DISPATCHER_TESTS)
        self.assertIn("assertEquals(3, transport.requests.size)", DISPATCHER_TESTS)

    def test_request_retries_are_explicit_and_share_the_original_distributed_budget(self):
        self.assertIn("fun retryFailedAttempts()", REQUEST_VIEW_MODEL)
        self.assertIn(
            "if (!network.canRetryRequest || network.isSendingRequest || network.isCancellingRequest) return",
            REQUEST_VIEW_MODEL,
        )
        self.assertIn("executeRequestRound(context, manualRetry = false)", REQUEST_VIEW_MODEL)
        self.assertIn("executeRequestRound(context, manualRetry = true)", REQUEST_VIEW_MODEL)
        self.assertIn("FirstReleaseInferenceLimits.MAX_ROUNDS", REQUEST_VIEW_MODEL)
        self.assertIn("FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS", REQUEST_VIEW_MODEL)
        self.assertIn("context.progress.afterDispatch(", REQUEST_VIEW_MODEL)
        self.assertIn("!hasCancellation", REQUEST_VIEW_MODEL)
        self.assertIn("attemptId = UUID.randomUUID().toString()", REQUEST_VIEW_MODEL)
        self.assertIn("onClick = requestViewModel::retryFailedAttempts", REQUEST_SCREEN)
        self.assertIn("Retry failed attempts manually", REQUEST_SCREEN)

    def test_busy_hint_and_host_response_delivery_do_not_create_hidden_request_retries(self):
        self.assertIn("private const long BusyRetryDelayMilliseconds = 1_000", HOST_MAPPER)
        self.assertIn("HostRequestStatus.Busy => CreateBusy", HOST_MAPPER)
        self.assertIn("retryable: false", HOST_MAPPER)
        self.assertIn("busy.retryAfterUnixMs > 0L", REMOTE_ERROR_PARSER)
        self.assertIn("errorCode = RequestErrorCode.HOST_BUSY", REMOTE_ERROR_PARSER)
        self.assertNotIn("retryAfterUnixMs", REQUEST_VIEW_MODEL)
        self.assertIn("Interlocked.CompareExchange(ref _deliveryClaimed, 1, 0)", DELIVERY_GATE)
        self.assertIn("retrying an ambiguous send could duplicate the response", DELIVERY_GATE)
        self.assertIn("AmbiguousFailureIsNotRetriedAsync", DELIVERY_TESTS)
        self.assertIn("ConcurrentDeliveryRaceInvokesCallbackOnceAsync", DELIVERY_TESTS)
        self.assertIn("SequentialDuplicateDeliveryIsSuppressedAsync", DELIVERY_TESTS)

    def test_connection_address_fallback_is_separate_and_bounded(self):
        self.assertIn("public const int MaximumAddresses = 8", ANNOUNCEMENT)
        self.assertIn("addressCopy.Length is < 1 or > MaximumAddresses", ANNOUNCEMENT)
        self.assertIn("seenAddresses.Add(candidate.Address)", FALLBACK_CONNECTOR)
        self.assertEqual(1, FALLBACK_CONNECTOR.count("ConnectSecureAsync("))
        self.assertIn("foreach (PeerTransportEndpoint candidate in unique)", FALLBACK_CONNECTOR)
        self.assertIn("Every fallback address must pin the same expected peer identity", FALLBACK_CONNECTOR)


if __name__ == "__main__":
    unittest.main()
