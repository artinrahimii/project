package com.whitep.client.requests

import com.google.protobuf.ByteString
import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.hostAnnouncement
import org.whitep.shared.contracts.v1.hostCapabilities
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.inferenceResponse
import org.whitep.shared.contracts.v1.modelCapability

class HostCandidateSelectorTest {
    @Test
    fun selectsAtMostThreeDistinctCompatibleReadyHostsInStableOrder() {
        val candidates =
            listOf(
                candidate("host-z"),
                candidate("host-c"),
                candidate("host-a"),
                candidate("host-d"),
                candidate("host-b"),
            )

        val selected = HostCandidateSelector.select(validRequest(), candidates)

        assertEquals(
            listOf("host-a", "host-b", "host-c"),
            selected.map { it.announcement.ephemeralHostId },
        )
        assertEquals(HostCandidateSelector.MAX_HOSTS_PER_ROUND, selected.size)
    }

    @Test
    fun rejectsHostsThatAreNotReadyOrHaveNoAvailableConcurrency() {
        val candidates =
            listOf(
                candidate("starting", state = HostRuntimeState.HOST_RUNTIME_STATE_STARTING),
                candidate("busy", state = HostRuntimeState.HOST_RUNTIME_STATE_BUSY),
                candidate("stopping", state = HostRuntimeState.HOST_RUNTIME_STATE_STOPPING),
                candidate("unavailable", state = HostRuntimeState.HOST_RUNTIME_STATE_UNAVAILABLE),
                candidate("full", activeRequests = 1, maxConcurrentRequests = 1),
            )

        assertTrue(HostCandidateSelector.select(validRequest(), candidates).isEmpty())
    }

    @Test
    fun rejectsInconsistentPeerIdentityAndProtocolSnapshots() {
        val candidates =
            listOf(
                candidate("nested-id", capabilitiesHostId = "different-peer"),
                candidate("status-id", statusHostId = "different-peer"),
                candidate(
                    "announcement-version",
                    announcementProtocolVersion = "2.0.0",
                    capabilitiesProtocolVersion = "2.0.0",
                ),
                candidate(
                    "capabilities-version",
                    capabilitiesProtocolVersion = "2.0.0",
                ),
            )

        assertTrue(HostCandidateSelector.select(validRequest(), candidates).isEmpty())
    }

    @Test
    fun requiresTheRequestedModelInBothAnnouncementAndLiveStatus() {
        val candidates =
            listOf(
                candidate("not-announced", announcementModelId = "another-model"),
                candidate("not-loaded", statusModelId = "another-model"),
                candidate("small-announcement-model", announcementMaxOutputTokens = 64L),
                candidate("small-runtime-model", statusMaxOutputTokens = 64L),
            )

        assertTrue(HostCandidateSelector.select(validRequest(), candidates).isEmpty())
    }

    @Test
    fun rejectsCandidatesWhosePromptCapsuleOrOutputLimitsAreTooSmall() {
        val candidates =
            listOf(
                candidate("small-prompt", maxPromptBytes = 2L),
                candidate("small-capsule", maxContextCapsuleBytes = 4L),
                candidate("small-output-announcement", announcementMaxOutputTokens = 64L),
                candidate("small-output-status", statusMaxOutputTokens = 64L),
            )

        assertTrue(HostCandidateSelector.select(validRequest(), candidates).isEmpty())
    }

    @Test
    fun rejectsLocallyInvalidOrOversizedInferenceRequests() {
        val candidate = candidate("host-a")
        val invalidRequests =
            listOf(
                validRequest(modelId = " "),
                validRequest(protocolVersion = " "),
                validRequest(prompt = " "),
                validRequest(prompt = "p".repeat(InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES + 1)),
                validRequest(
                    capsuleBytes = InferenceRequestPayloadBuilder.MAX_CONTEXT_CAPSULE_BYTES + 1,
                ),
                validRequest(requestId = "r".repeat(InferenceRequestPayloadBuilder.MAX_SERIALIZED_REQUEST_BYTES)),
            )

        invalidRequests.forEach { request ->
            assertTrue(HostCandidateSelector.select(request, listOf(candidate)).isEmpty())
        }
    }

    @Test
    fun excludesAmbiguousDuplicatePeerIdentities() {
        val selected =
            HostCandidateSelector.select(
                validRequest(),
                listOf(candidate("duplicate"), candidate("duplicate"), candidate("unique")),
            )

        assertEquals(listOf("unique"), selected.map { it.announcement.ephemeralHostId })
    }

    @Test
    fun doesNotSelectAHostWhenTheRequestedProtocolIsNotAdvertised() {
        val request = validRequest(protocolVersion = "2.0.0")

        assertTrue(HostCandidateSelector.select(request, listOf(candidate("host-a"))).isEmpty())
    }

    @Test
    fun selectFirstRoundHonorsTheTotalHostBudget() {
        val selected =
            HostCandidateSelector.selectFirstRound(
                distributedRequest = distributedRequest(maxRounds = 3, maxTotalHosts = 1),
                request = validRequest(),
                candidates = listOf(candidate("host-c"), candidate("host-a"), candidate("host-b")),
            )
        assertEquals(listOf("host-a"), selected.map { it.announcement.ephemeralHostId })
    }

    @Test
    fun rejectsFirstRoundWhenRequestIdentityDoesNotMatch() {
        val selected =
            HostCandidateSelector.selectFirstRound(
                distributedRequest = distributedRequest(requestId = "different-request"),
                request = validRequest(),
                candidates = listOf(candidate("host-a")),
            )
        assertTrue(selected.isEmpty())
    }

    @Test
    fun selectsNextThreeDistinctHostsWhenThePreviousRoundHasNoValidResponse() {
        val previousAttempts =
            listOf(
                InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED),
                InferenceHostAttempt("host-b", "attempt-b", InferenceAttemptSubmissionStatus.QUEUED),
                InferenceHostAttempt("host-c", "attempt-c", InferenceAttemptSubmissionStatus.FAILED),
            )
        val completedRound =
            InferenceRoundResponseCollection(
                responses = emptyList(),
                failures =
                    previousAttempts.map { attempt ->
                        InferenceRoundResponseFailure(
                            ephemeralHostId = attempt.ephemeralHostId,
                            attemptId = attempt.attemptId,
                            reason = InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT,
                        )
                    },
            )
        val candidates =
            listOf(
                candidate("host-f"),
                candidate("host-c"),
                candidate("host-e"),
                candidate("host-a"),
                candidate("host-d"),
                candidate("host-b"),
            )

        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = candidates,
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = completedRound,
                progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 3, attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c")),
            )

        assertEquals(listOf("host-d", "host-e", "host-f"), selected.map { it.announcement.ephemeralHostId })
        assertEquals(HostCandidateSelector.MAX_HOSTS_PER_ROUND, selected.size)
    }

    @Test
    fun doesNotSelectAnotherRoundAfterTheConfiguredRoundLimit() {
        val attempts =
            listOf(
                InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED),
                InferenceHostAttempt("host-b", "attempt-b", InferenceAttemptSubmissionStatus.FAILED),
                InferenceHostAttempt("host-c", "attempt-c", InferenceAttemptSubmissionStatus.TIMED_OUT),
            )
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(maxRounds = 1, maxTotalHosts = 3),
                request = validRequest(),
                candidates = listOf(candidate("host-d")),
                previousRoundAttempts = attempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 3, attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c")),
            )
        assertTrue(selected.isEmpty())
    }

    @Test
    fun capsNextBatchAtTheRemainingTotalHostBudget() {
        val attempts =
            listOf(
                InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED),
                InferenceHostAttempt("host-b", "attempt-b", InferenceAttemptSubmissionStatus.FAILED),
                InferenceHostAttempt("host-c", "attempt-c", InferenceAttemptSubmissionStatus.TIMED_OUT),
            )
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(maxRounds = 3, maxTotalHosts = 4),
                request = validRequest(),
                candidates = listOf(candidate("host-d"), candidate("host-e")),
                previousRoundAttempts = attempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 3, attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c")),
            )
        assertEquals(listOf("host-d"), selected.map { it.announcement.ephemeralHostId })
    }

    @Test
    fun doesNotSelectAnotherBatchWhenTheTotalHostBudgetIsExhausted() {
        val attempts =
            listOf(
                InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED),
                InferenceHostAttempt("host-b", "attempt-b", InferenceAttemptSubmissionStatus.FAILED),
                InferenceHostAttempt("host-c", "attempt-c", InferenceAttemptSubmissionStatus.TIMED_OUT),
            )
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(maxRounds = 3, maxTotalHosts = 3),
                request = validRequest(),
                candidates = listOf(candidate("host-d")),
                previousRoundAttempts = attempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 3, attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c")),
            )
        assertTrue(selected.isEmpty())
    }

    @Test
    fun doesNotSelectAnotherRoundWhenThePreviousRoundContainsAValidResponse() {
        val previousAttempts =
            listOf(InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED))
        val completedRound =
            InferenceRoundResponseCollection(
                responses =
                    listOf(
                        inferenceResponse {
                            requestId = "request-001"
                            attemptId = "attempt-a"
                            roundId = "round-001"
                            ephemeralHostId = "host-a"
                            modelId = MODEL_ID
                        },
                    ),
                failures = emptyList(),
            )

        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = listOf(candidate("host-b"), candidate("host-c")),
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = completedRound,
                progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 1, attemptedEphemeralHostIds = listOf("host-a")),
            )

        assertTrue(selected.isEmpty())
    }

    @Test
    fun failsClosedWhenPreviousRoundAttemptsCannotDefineASafeExclusionSet() {
        val candidates = listOf(candidate("host-a"), candidate("host-b"), candidate("host-c"))
        val completedRound = InferenceRoundResponseCollection(responses = emptyList(), failures = emptyList())

        assertTrue(
            HostCandidateSelector
                .selectNextRound(
                    distributedRequest = distributedRequest(),
                    request = validRequest(),
                    candidates = candidates,
                    previousRoundAttempts = emptyList(),
                    previousRoundResponses = completedRound,
                    progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 1),
                ).isEmpty(),
        )
        assertTrue(
            HostCandidateSelector
                .selectNextRound(
                    distributedRequest = distributedRequest(),
                    request = validRequest(),
                    candidates = candidates,
                    previousRoundAttempts =
                        listOf(
                            InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED),
                            InferenceHostAttempt("host-a", "attempt-b", InferenceAttemptSubmissionStatus.QUEUED),
                        ),
                    previousRoundResponses = completedRound,
                    progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 2),
                ).isEmpty(),
        )
        assertTrue(
            HostCandidateSelector
                .selectNextRound(
                    distributedRequest = distributedRequest(),
                    request = validRequest(),
                    candidates = candidates,
                    previousRoundAttempts =
                        listOf(InferenceHostAttempt(" ", "attempt-a", InferenceAttemptSubmissionStatus.QUEUED)),
                    previousRoundResponses = completedRound,
                    progress = DistributedRequestProgress(roundsDispatched = 1, hostsAttempted = 1),
                ).isEmpty(),
        )
    }

    @Test
    fun excludesHostsAttemptedInAnyEarlierRoundByDefault() {
        val previousAttempts =
            listOf(
                InferenceHostAttempt("host-d", "attempt-d", InferenceAttemptSubmissionStatus.TIMED_OUT),
                InferenceHostAttempt("host-e", "attempt-e", InferenceAttemptSubmissionStatus.FAILED),
                InferenceHostAttempt("host-f", "attempt-f", InferenceAttemptSubmissionStatus.QUEUED),
            )
        val progress =
            DistributedRequestProgress(
                roundsDispatched = 2,
                hostsAttempted = 6,
                attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c", "host-d", "host-e", "host-f"),
            )
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = listOf(
                    candidate("host-a"),
                    candidate("host-d"),
                    candidate("host-g"),
                    candidate("host-h"),
                    candidate("host-i"),
                ),
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = progress,
            )

        assertEquals(listOf("host-g", "host-h", "host-i"), selected.map { it.announcement.ephemeralHostId })
    }

    @Test
    fun retriesAPreviouslyAttemptedHostOnlyWhenExplicitlyAllowlisted() {
        val previousAttempts =
            listOf(
                InferenceHostAttempt("host-b", "attempt-b", InferenceAttemptSubmissionStatus.TIMED_OUT),
                InferenceHostAttempt("host-c", "attempt-c", InferenceAttemptSubmissionStatus.FAILED),
            )
        val progress =
            DistributedRequestProgress(
                roundsDispatched = 2,
                hostsAttempted = 3,
                attemptedEphemeralHostIds = listOf("host-a", "host-b", "host-c"),
            )
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = listOf(candidate("host-a"), candidate("host-d")),
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = progress,
                retryableEphemeralHostIds = setOf("host-a"),
            )

        assertEquals(listOf("host-a", "host-d"), selected.map { it.announcement.ephemeralHostId })
    }

    @Test
    fun rejectsRetryPolicyThatNamesAHostNeverAttemptedForThisRequest() {
        val previousAttempts =
            listOf(InferenceHostAttempt("host-a", "attempt-a", InferenceAttemptSubmissionStatus.TIMED_OUT))
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = listOf(candidate("host-a"), candidate("host-b")),
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = DistributedRequestProgress(
                    roundsDispatched = 1,
                    hostsAttempted = 1,
                    attemptedEphemeralHostIds = listOf("host-a"),
                ),
                retryableEphemeralHostIds = setOf("never-attempted"),
            )

        assertTrue(selected.isEmpty())
    }

    @Test
    fun failsClosedWhenMultiRoundProgressHasNoCompleteHostHistory() {
        val previousAttempts =
            listOf(InferenceHostAttempt("host-d", "attempt-d", InferenceAttemptSubmissionStatus.TIMED_OUT))
        val selected =
            HostCandidateSelector.selectNextRound(
                distributedRequest = distributedRequest(),
                request = validRequest(),
                candidates = listOf(candidate("host-a"), candidate("host-e")),
                previousRoundAttempts = previousAttempts,
                previousRoundResponses = InferenceRoundResponseCollection(emptyList(), emptyList()),
                progress = DistributedRequestProgress(roundsDispatched = 2, hostsAttempted = 4),
            )

        assertTrue(selected.isEmpty())
    }

    private fun distributedRequest(
        requestId: String = "request-001",
        maxRounds: Int = 3,
        maxTotalHosts: Int = 9,
    ): DistributedRequest =
        DistributedRequest(
            requestId = requestId,
            deadlineUnixMs = 4_102_444_800_000L,
            maxRounds = maxRounds,
            maxTotalHosts = maxTotalHosts,
        )

    private fun validRequest(
        requestId: String = "request-001",
        modelId: String = MODEL_ID,
        prompt: String = "A short prompt for a compatible host.",
        protocolVersion: String = PROTOCOL_VERSION,
        capsuleBytes: Int = 32,
    ): InferenceRequest =
        inferenceRequest {
            this.requestId = requestId
            attemptId = "attempt-001"
            this.modelId = modelId
            this.prompt = prompt
            deadlineUnixMs = 4_102_444_800_000L
            maxOutputTokens = 128
            contextCapsule = ByteString.copyFrom(ByteArray(capsuleBytes) { 1 })
            this.protocolVersion = protocolVersion
        }

    private fun candidate(
        hostId: String,
        state: HostRuntimeState = HostRuntimeState.HOST_RUNTIME_STATE_READY,
        activeRequests: Int = 0,
        maxConcurrentRequests: Int = 2,
        capabilitiesHostId: String = hostId,
        statusHostId: String = hostId,
        announcementProtocolVersion: String = PROTOCOL_VERSION,
        capabilitiesProtocolVersion: String = announcementProtocolVersion,
        announcementModelId: String = MODEL_ID,
        statusModelId: String = MODEL_ID,
        announcementMaxOutputTokens: Long = 1024L,
        statusMaxOutputTokens: Long = 1024L,
        maxPromptBytes: Long = 8192L,
        maxContextCapsuleBytes: Long = 4096L,
    ): HostCandidate {
        val advertisedModel =
            modelCapability {
                modelId = announcementModelId
                displayName = "Test model"
                maxContextTokens = 8192L
                maxOutputTokens = announcementMaxOutputTokens
            }
        val runtimeModel =
            modelCapability {
                modelId = statusModelId
                displayName = "Test model"
                maxContextTokens = 8192L
                maxOutputTokens = statusMaxOutputTokens
            }
        val capabilities =
            hostCapabilities {
                ephemeralHostId = capabilitiesHostId
                protocolVersion = capabilitiesProtocolVersion
                models += advertisedModel
                this.maxConcurrentRequests = maxConcurrentRequests
                this.maxContextCapsuleBytes = maxContextCapsuleBytes
                this.maxPromptBytes = maxPromptBytes
            }
        val announcement =
            hostAnnouncement {
                ephemeralHostId = hostId
                protocolVersion = announcementProtocolVersion
                this.capabilities = capabilities
                announcedAtUnixMs = 1_798_000_000_000L
            }
        val status =
            hostStatus {
                ephemeralHostId = statusHostId
                this.state = state
                this.activeRequests = activeRequests
                observedAtUnixMs = 1_798_000_000_000L
                models += runtimeModel
            }
        return HostCandidate(announcement = announcement, status = status)
    }

    private companion object {
        const val MODEL_ID = "local/model-1"
        const val PROTOCOL_VERSION = "1.0.0"
    }
}
