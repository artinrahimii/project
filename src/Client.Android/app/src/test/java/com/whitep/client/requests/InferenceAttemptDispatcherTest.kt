package com.whitep.client.requests

import com.google.protobuf.ByteString
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.protocol.InferenceRequestPayloadRejection
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.hostAnnouncement
import org.whitep.shared.contracts.v1.hostCapabilities
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.modelCapability
import java.io.IOException
import java.time.Clock
import java.time.Duration
import java.time.Instant
import java.time.ZoneId
import java.time.ZoneOffset
import java.util.UUID

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class InferenceAttemptDispatcherTest {
    @Test
    fun dispatchesCorrelatableAttemptPerSelectedHost() =
        runBlocking {
            val candidates =
                HostCandidateSelector.select(
                    validRequest(),
                    listOf(candidate("host-c"), candidate("host-a"), candidate("host-b")),
                )
            val transport = RecordingTransport()
            val ids = listOf("attempt-a", "attempt-b", "attempt-c").iterator()
            val dispatcher =
                InferenceAttemptDispatcher(
                    transport = transport,
                    attemptIdGenerator = { ids.next() },
                )
            val capsule =
                ConversationContextCapsule(
                    goal = "Run one local inference request",
                    keyFacts = listOf("Only the selected message is sent"),
                )
            val distributedRequest =
                DistributedRequest(
                    requestId = "ephemeral-request-42",
                    deadlineUnixMs = 1_798_000_000_000L,
                    maxRounds = 3,
                )

            val result =
                dispatcher.dispatchRound(
                    distributedRequest = distributedRequest,
                    selectedHosts = candidates,
                    modelId = MODEL_ID,
                    currentMessage = "Use the current message",
                    capsule = capsule,
                    maxOutputTokens = 128,
                    protocolVersion = PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            val dispatched = (result as InferenceRoundDispatchResult.Dispatched).attempts
            assertEquals(listOf("host-a", "host-b", "host-c"), transport.hostIds)
            assertEquals(3, dispatched.size)
            assertEquals(
                listOf("attempt-a", "attempt-b", "attempt-c"),
                dispatched.map { it.attemptId },
            )
            assertEquals(3, dispatched.map { it.attemptId }.toSet().size)
            assertEquals(
                listOf("ephemeral-request-42", "ephemeral-request-42", "ephemeral-request-42"),
                transport.requests.map { it.requestId },
            )
            assertEquals(dispatched.map { it.attemptId }, transport.requests.map { it.attemptId })
            assertTrue(transport.requests.all { it.prompt == "Use the current message" })
            val encodedCapsule = requireNotNull(capsule.encode().bytes)
            assertTrue(
                transport.requests.all {
                    it.contextCapsule == ByteString.copyFrom(encodedCapsule)
                },
            )
            assertTrue(dispatched.all { it.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED })
        }

    @Test
    fun continuesWithTwoAvailableDistinctHostsAndSubmitsBoth() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            assertEquals(listOf("host-a", "host-b"), transport.hostIds)
            assertEquals(2, (result as InferenceRoundDispatchResult.Dispatched).attempts.size)
        }

    @Test
    fun continuesWithOneAvailableHostAndSubmitsIt() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            assertEquals(listOf("host-a"), transport.hostIds)
            assertEquals(1, (result as InferenceRoundDispatchResult.Dispatched).attempts.size)
        }

    @Test
    fun rejectsZeroAvailableHostsWithoutSubmittingAnyRequest() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    emptyList(),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.NO_SELECTED_HOSTS),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe() =
        runBlocking {
            val selected =
                HostCandidateSelector.selectReachable(
                    request = validRequest(),
                    announcements = listOf(candidate("host-a").announcement, candidate("host-b").announcement),
                    statusProbe = { throw IOException("Simulated peer disconnect.") },
                )
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    selected,
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(selected.isEmpty())
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.NO_SELECTED_HOSTS),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsDuplicateHostIdentityBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("same-host"), candidate("same-host")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.DUPLICATE_HOST_IDENTITY,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsMoreThanThreeHostsBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(
                        candidate("host-a"),
                        candidate("host-b"),
                        candidate("host-c"),
                        candidate("host-d"),
                    ),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.TOO_MANY_HOSTS,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsRoundAfterTheConfiguredMaximumBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(maxRounds = 1, maxTotalHosts = 3),
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 1),
                )
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.ROUND_LIMIT_REACHED),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsBatchLargerThanRemainingTotalHostBudgetBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(maxRounds = 3, maxTotalHosts = 2),
                    listOf(candidate("host-a"), candidate("host-b"), candidate("host-c")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.TOTAL_HOST_LIMIT_REACHED),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsInconsistentRequestProgressBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(roundsDispatched = 2, hostsAttempted = 1),
                )
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.INVALID_BUDGET_STATE),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsBlankPromptWithRejectedResult() =
        runBlocking {
            val transport = RecordingTransport()
            val result = dispatchBlankPrompt(transport)

            assertTrue(result is InferenceRoundDispatchResult.Rejected)
        }

    @Test
    fun classifiesBlankPromptAsPayloadRejection() =
        runBlocking {
            val result = dispatchBlankPrompt(RecordingTransport())
            val rejected = result as InferenceRoundDispatchResult.Rejected

            assertEquals(InferenceRoundDispatchRejection.PAYLOAD_REJECTED, rejected.reason)
        }

    @Test
    fun identifiesEmptyPromptAsPayloadRejectionCause() =
        runBlocking {
            val result = dispatchBlankPrompt(RecordingTransport())
            val rejected = result as InferenceRoundDispatchResult.Rejected

            assertEquals(InferenceRequestPayloadRejection.EMPTY_PROMPT, rejected.payloadRejection)
        }

    @Test
    fun doesNotSubmitAnyHostWhenPromptIsBlank() =
        runBlocking {
            val transport = RecordingTransport()

            dispatchBlankPrompt(transport)

            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsTerminalRequestBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val request = distributedRequest(status = DistributedRequestStatus.CANCELLED)
            val result =
                dispatcher(transport).dispatchRound(
                    request,
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.TERMINAL_REQUEST,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun keepsSubmittingOtherHostsAfterTransportRejectsOrThrows() =
        runBlocking {
            val transport =
                RecordingTransport(
                    submission = { hostId, _ ->
                        when (hostId) {
                            "host-a" -> InferenceTransportSubmission.REJECTED
                            "host-b" -> error("simulated transport fault")
                            else -> InferenceTransportSubmission.QUEUED
                        }
                    },
                )
            val attemptIds = listOf("attempt-a", "attempt-b", "attempt-c").iterator()
            val attemptDispatcher = dispatcher(transport, attemptIds::next)
            val result =
                attemptDispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b"), candidate("host-c")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            assertEquals(
                listOf(
                    InferenceAttemptSubmissionStatus.REJECTED,
                    InferenceAttemptSubmissionStatus.FAILED,
                    InferenceAttemptSubmissionStatus.QUEUED,
                ),
                (result as InferenceRoundDispatchResult.Dispatched).attempts.map { it.submissionStatus },
            )
            assertEquals(3, transport.requests.size)
        }

    @Test
    fun stopsSubmittingFurtherHostsWhenTheRoundWindowExpires() =
        runBlocking {
            val clock = MutableClock(Instant.parse("2026-10-31T07:00:00Z"))
            val timer = InferenceRoundTimer(clock, Duration.ofSeconds(30))
            val roundWindow = timer.startRound(clock.instant().plusSeconds(90).toEpochMilli())
            val transport =
                RecordingTransport { hostId, _ ->
                    if (hostId == "host-a") {
                        clock.advance(Duration.ofSeconds(31))
                    }
                    InferenceTransportSubmission.QUEUED
                }
            val ids = listOf("attempt-a", "attempt-b", "attempt-c").iterator()
            val attemptDispatcher = InferenceAttemptDispatcher(transport, ids::next)
            val result =
                attemptDispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b"), candidate("host-c")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    roundWindow = roundWindow,
                    progress = DistributedRequestProgress(),
                )

            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            assertEquals(listOf("host-a"), transport.hostIds)
            assertEquals(
                listOf(
                    InferenceAttemptSubmissionStatus.QUEUED,
                    InferenceAttemptSubmissionStatus.TIMED_OUT,
                    InferenceAttemptSubmissionStatus.TIMED_OUT,
                ),
                (result as InferenceRoundDispatchResult.Dispatched).attempts.map { it.submissionStatus },
            )
        }

    @Test
    fun rejectsAnAlreadyExpiredRoundBeforeSubmitting() =
        runBlocking {
            val clock = MutableClock(Instant.parse("2026-10-31T07:00:00Z"))
            val timer = InferenceRoundTimer(clock, Duration.ofSeconds(30))
            val roundWindow = timer.startRound(clock.instant().minusSeconds(1).toEpochMilli())
            val transport = RecordingTransport()
            val attemptDispatcher = InferenceAttemptDispatcher(transport)
            val result =
                attemptDispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    roundWindow = roundWindow,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.ROUND_TIMEOUT),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsDuplicateAttemptIdsBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val dispatcher =
                InferenceAttemptDispatcher(
                    transport = transport,
                    attemptIdGenerator = { "same-attempt" },
                )
            val result =
                dispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )

            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.DUPLICATE_ATTEMPT_ID,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }


    @Test
    fun enforcesThreeDistinctHostsPerRoundAndNineAttemptsAcrossTheRequest() =
        runBlocking {
            val request = distributedRequest()
            val protocolRequest = validRequest(requestId = request.requestId)
            val candidates =
                (1..12).map { index ->
                    candidate("host-${index.toString().padStart(2, '0')}")
                }
            val transport = RecordingTransport()
            val requestDispatcher = dispatcher(transport)
            var progress = DistributedRequestProgress()
            var previousRoundAttempts = emptyList<InferenceHostAttempt>()
            val allAttemptedHosts = mutableListOf<String>()
            val roundHostIds = mutableListOf<List<String>>()

            repeat(request.maxRounds) { roundIndex ->
                val selected =
                    if (roundIndex == 0) {
                        HostCandidateSelector.selectFirstRound(
                            distributedRequest = request,
                            request = protocolRequest,
                            candidates = candidates,
                        )
                    } else {
                        HostCandidateSelector.selectNextRound(
                            distributedRequest = request,
                            request = protocolRequest,
                            candidates = candidates,
                            previousRoundAttempts = previousRoundAttempts,
                            previousRoundResponses =
                                InferenceRoundResponseCollection(emptyList(), emptyList()),
                            progress = progress,
                        )
                    }
                val selectedIds = selected.map { it.announcement.ephemeralHostId }

                assertTrue(selectedIds.size in 1..HostCandidateSelector.MAX_HOSTS_PER_ROUND)
                assertEquals(selectedIds.size, selectedIds.toSet().size)
                assertTrue(selectedIds.none { hostId -> hostId in allAttemptedHosts })

                val result =
                    requestDispatcher.dispatchRound(
                        distributedRequest = request,
                        selectedHosts = selected,
                        modelId = MODEL_ID,
                        currentMessage = "message",
                        capsule = capsule(),
                        maxOutputTokens = 64,
                        protocolVersion = PROTOCOL_VERSION,
                        progress = progress,
                    )
                assertTrue(result is InferenceRoundDispatchResult.Dispatched)

                val attempts = (result as InferenceRoundDispatchResult.Dispatched).attempts
                assertEquals(selectedIds, attempts.map { it.ephemeralHostId })
                assertTrue(attempts.size <= FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND)

                roundHostIds += selectedIds
                allAttemptedHosts += selectedIds
                progress = progress.afterDispatch(attempts.map { it.ephemeralHostId })
                previousRoundAttempts = attempts
            }

            assertEquals(FirstReleaseInferenceLimits.MAX_ROUNDS, progress.roundsDispatched)
            assertEquals(FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS, progress.hostsAttempted)
            assertEquals(9, transport.requests.size)
            assertEquals(9, allAttemptedHosts.size)
            assertEquals(9, allAttemptedHosts.toSet().size)
            assertTrue(roundHostIds.all { ids -> ids.size == ids.toSet().size })

            val fourthRound =
                HostCandidateSelector.selectNextRound(
                    distributedRequest = request,
                    request = protocolRequest,
                    candidates = candidates,
                    previousRoundAttempts = previousRoundAttempts,
                    previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                    progress = progress,
                )
            assertTrue(fourthRound.isEmpty())

            val rejected =
                requestDispatcher.dispatchRound(
                    distributedRequest = request,
                    selectedHosts = listOf(candidate("host-13")),
                    modelId = MODEL_ID,
                    currentMessage = "message",
                    capsule = capsule(),
                    maxOutputTokens = 64,
                    protocolVersion = PROTOCOL_VERSION,
                    progress = progress,
                )
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.ROUND_LIMIT_REACHED),
                rejected,
            )
            assertEquals(9, transport.requests.size)
        }

    @Test
    fun honorsTheRemainingAttemptBudgetWhenAPreviouslyUsedHostIsRetried() =
        runBlocking {
            val request = distributedRequest(maxRounds = 3, maxTotalHosts = 4)
            val protocolRequest = validRequest(requestId = request.requestId)
            val transport = RecordingTransport()
            val requestDispatcher = dispatcher(transport)
            val firstRoundCandidates = listOf(candidate("host-01"), candidate("host-02"), candidate("host-03"))
            val firstRound =
                HostCandidateSelector.selectFirstRound(
                    distributedRequest = request,
                    request = protocolRequest,
                    candidates = firstRoundCandidates,
                )
            assertEquals(3, firstRound.size)

            val firstResult =
                requestDispatcher.dispatchRound(
                    distributedRequest = request,
                    selectedHosts = firstRound,
                    modelId = MODEL_ID,
                    currentMessage = "message",
                    capsule = capsule(),
                    maxOutputTokens = 64,
                    protocolVersion = PROTOCOL_VERSION,
                    progress = DistributedRequestProgress(),
                )
            assertTrue(firstResult is InferenceRoundDispatchResult.Dispatched)
            val firstAttempts = (firstResult as InferenceRoundDispatchResult.Dispatched).attempts
            var progress = DistributedRequestProgress().afterDispatch(firstAttempts.map { it.ephemeralHostId })

            val retryBatch =
                HostCandidateSelector.selectNextRound(
                    distributedRequest = request,
                    request = protocolRequest,
                    candidates =
                        listOf(
                            candidate("host-01"),
                            candidate("host-04"),
                            candidate("host-05"),
                        ),
                    previousRoundAttempts = firstAttempts,
                    previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                    progress = progress,
                    retryableEphemeralHostIds = setOf("host-01"),
                )
            assertEquals(listOf("host-01"), retryBatch.map { it.announcement.ephemeralHostId })

            val retryResult =
                requestDispatcher.dispatchRound(
                    distributedRequest = request,
                    selectedHosts = retryBatch,
                    modelId = MODEL_ID,
                    currentMessage = "message",
                    capsule = capsule(),
                    maxOutputTokens = 64,
                    protocolVersion = PROTOCOL_VERSION,
                    progress = progress,
                )
            assertTrue(retryResult is InferenceRoundDispatchResult.Dispatched)
            val retryAttempts = (retryResult as InferenceRoundDispatchResult.Dispatched).attempts
            progress = progress.afterDispatch(retryAttempts.map { it.ephemeralHostId })

            assertEquals(2, progress.roundsDispatched)
            assertEquals(4, progress.hostsAttempted)
            assertEquals(listOf("host-01", "host-02", "host-03", "host-01"), progress.attemptedEphemeralHostIds)
            assertEquals(4, transport.requests.size)

            val exhaustedSelection =
                HostCandidateSelector.selectNextRound(
                    distributedRequest = request,
                    request = protocolRequest,
                    candidates = listOf(candidate("host-04"), candidate("host-05")),
                    previousRoundAttempts = retryAttempts,
                    previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                    progress = progress,
                )
            assertTrue(exhaustedSelection.isEmpty())

            val rejected =
                requestDispatcher.dispatchRound(
                    distributedRequest = request,
                    selectedHosts = listOf(candidate("host-04")),
                    modelId = MODEL_ID,
                    currentMessage = "message",
                    capsule = capsule(),
                    maxOutputTokens = 64,
                    protocolVersion = PROTOCOL_VERSION,
                    progress = progress,
                )
            assertEquals(
                InferenceRoundDispatchResult.Rejected(InferenceRoundDispatchRejection.TOTAL_HOST_LIMIT_REACHED),
                rejected,
            )
            assertEquals(4, transport.requests.size)
        }

    private suspend fun dispatchBlankPrompt(transport: RecordingTransport): InferenceRoundDispatchResult =
        dispatcher(transport).dispatchRound(
            distributedRequest(),
            listOf(candidate("host-a"), candidate("host-b")),
            MODEL_ID,
            " ",
            capsule(),
            64,
            PROTOCOL_VERSION,
            progress = DistributedRequestProgress(),
        )

    private fun dispatcher(
        transport: InferenceRequestTransport,
        attemptIdGenerator: () -> String = { UUID.randomUUID().toString() },
    ): InferenceAttemptDispatcher = InferenceAttemptDispatcher(transport, attemptIdGenerator)

    private fun distributedRequest(
        status: DistributedRequestStatus = DistributedRequestStatus.QUEUED,
        maxRounds: Int = 3,
        maxTotalHosts: Int = 9,
    ) = DistributedRequest(
        requestId = "ephemeral-request-42",
        deadlineUnixMs = 1_798_000_000_000L,
        maxRounds = maxRounds,
        maxTotalHosts = maxTotalHosts,
        status = status,
    )

    private fun capsule() = ConversationContextCapsule(goal = "Run one local inference request")

    private fun validRequest(requestId: String = "selector-request") =
        inferenceRequest {
            this.requestId = requestId
            attemptId = "selector-attempt"
            modelId = MODEL_ID
            prompt = "select compatible hosts"
            deadlineUnixMs = 1_798_000_000_000L
            maxOutputTokens = 128
            contextCapsule = ByteString.copyFrom(ByteArray(32) { 1 })
            protocolVersion = PROTOCOL_VERSION
        }

    private fun candidate(hostId: String): HostCandidate {
        val model =
            modelCapability {
                modelId = MODEL_ID
                displayName = "Test model"
                maxContextTokens = 8192L
                maxOutputTokens = 1024L
            }
        val capabilities =
            hostCapabilities {
                ephemeralHostId = hostId
                protocolVersion = PROTOCOL_VERSION
                models += model
                maxConcurrentRequests = 2
                maxContextCapsuleBytes = 4096L
                maxPromptBytes = 8192L
            }
        return HostCandidate(
            announcement =
                hostAnnouncement {
                    ephemeralHostId = hostId
                    protocolVersion = PROTOCOL_VERSION
                    this.capabilities = capabilities
                    announcedAtUnixMs = 1_798_000_000_000L
                },
            status =
                hostStatus {
                    ephemeralHostId = hostId
                    state = HostRuntimeState.HOST_RUNTIME_STATE_READY
                    activeRequests = 0
                    observedAtUnixMs = 1_798_000_000_000L
                    models += model
                },
        )
    }

    private class MutableClock(
        private var current: Instant,
    ) : Clock() {
        override fun getZone(): ZoneId = ZoneOffset.UTC

        override fun withZone(zone: ZoneId): Clock = this

        override fun instant(): Instant = current

        fun advance(duration: Duration) {
            current = current.plus(duration)
        }
    }

    private class RecordingTransport(
        private val submission: suspend (String, InferenceRequest) -> InferenceTransportSubmission =
            { _, _ -> InferenceTransportSubmission.QUEUED },
    ) : InferenceRequestTransport {
        val hostIds = mutableListOf<String>()
        val requests = mutableListOf<InferenceRequest>()

        override suspend fun submit(
            ephemeralHostId: String,
            request: InferenceRequest,
        ): InferenceTransportSubmission {
            hostIds += ephemeralHostId
            requests += request
            return submission(ephemeralHostId, request)
        }
    }

    private companion object {
        const val MODEL_ID = "local/model-1"
        const val PROTOCOL_VERSION = "1.0.0"
    }
}
