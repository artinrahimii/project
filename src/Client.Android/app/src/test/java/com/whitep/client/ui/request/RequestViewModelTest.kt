package com.whitep.client.ui.request

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import com.whitep.client.network.RendezvousPeerEndpoint
import com.whitep.client.requests.DistributedRequest
import com.whitep.client.requests.HostCandidate
import com.whitep.client.requests.HostSelectionFeedback
import com.whitep.client.requests.HostSelectionResult
import com.whitep.client.requests.InferenceAttemptCancellationResult
import com.whitep.client.requests.InferenceAttemptSubmissionStatus
import com.whitep.client.requests.InferenceHostAttempt
import com.whitep.client.requests.InferenceRoundDispatchResult
import com.whitep.client.requests.InferenceRoundResponseCollection
import com.whitep.client.requests.RequestExecutionGateway
import com.whitep.client.requests.RequestExecutionGatewayFactory
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.HostCapabilities
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.ModelCapability
import org.whitep.shared.contracts.v1.inferenceResponse
import java.time.Instant

@RunWith(RobolectricTestRunner::class)
class RequestViewModelTest {
    private val commandScope = CoroutineScope(SupervisorJob() + Dispatchers.Unconfined)
    private val viewModel =
        RequestViewModel(
            repository = InMemoryClientRequestRepository(),
            operationScope = commandScope,
            clockMillis = { 20_000L },
        )

    @After
    fun tearDown() {
        commandScope.cancel()
    }

    @Test
    fun promptUpdatesFlowToUiStateAndPrepareHonorsMissingTransport() {
        viewModel.updatePrompt("keep draft only")
        assertEquals("keep draft only", viewModel.uiState.value.prompt)
        assertEquals(RequestDraftStatus.NOT_SENT, viewModel.uiState.value.status)
        viewModel.prepareRequest()
        assertEquals(RequestDraftStatus.TRANSPORT_UNAVAILABLE, viewModel.uiState.value.status)
        assertEquals(
            "Request is not sent. Configure the HTTPS Rendezvous directory and explicitly trust a host.",
            viewModel.uiState.value.statusMessage,
        )
    }

    @Test
    fun reportsWhenTwoOfThreeReachableHostsAreAvailableWithoutStoringPeerMetadata() {
        viewModel.recordHostSelectionFeedback(HostSelectionFeedback(availableHostCount = 2))

        assertEquals(
            "Only 2 of 3 compatible, reachable hosts are currently available. Continuing with 2 hosts.",
            viewModel.uiState.value.hostAvailabilityMessage,
        )
        assertEquals("", viewModel.uiState.value.prompt)
    }

    @Test
    fun reportsOneAvailableHostAndAllowsTheRequestToContinue() {
        val feedback = HostSelectionFeedback(availableHostCount = 1)

        assertTrue(feedback.canContinue)
        assertEquals(
            "Only 1 of 3 compatible, reachable hosts are currently available. Continuing with 1 host.",
            feedback.message,
        )
    }

    @Test
    fun reportsNoAvailableHostAndExplainsThatNothingWasSent() {
        val feedback = HostSelectionFeedback(availableHostCount = 0)

        assertFalse(feedback.canContinue)
        assertEquals(
            "No compatible, reachable hosts are currently available. The request was not sent.",
            feedback.message,
        )
    }

    @Test
    fun clearingTheDraftRestoresInitialUiState() {
        viewModel.updatePrompt("temporary")
        viewModel.clearDraft()
        assertEquals("", viewModel.uiState.value.prompt)
        assertEquals(RequestDraftStatus.NOT_SENT, viewModel.uiState.value.status)
    }

    @Test
    fun keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard() {
        val first = response(ATTEMPT_A, HOST_A, "answer from host A")
        val second = response(ATTEMPT_B, HOST_B, "answer from host B")

        assertTrue(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_A, HOST_A, MODEL_ID, first))
        assertTrue(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_B, HOST_B, MODEL_ID, second))

        val cards = viewModel.uiState.value.hostResponses
        assertEquals(2, cards.size)
        assertEquals(listOf(HOST_A, HOST_B), cards.map { it.hostId })
        assertEquals(listOf("answer from host A", "answer from host B"), cards.map { it.outputText })
        assertEquals(listOf(ATTEMPT_A, ATTEMPT_B), cards.map { it.attemptId })
        assertEquals(HostResponseOutcome.SUCCEEDED, cards[0].outcome)

        assertFalse(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_A, HOST_A, MODEL_ID, first))
        assertEquals(2, viewModel.hostResponses.value.size)
    }

    @Test
    fun rejectsResponsesThatDoNotMatchTheExpectedRequestAttemptHostOrModel() {
        val mismatched = response(ATTEMPT_B, HOST_B, "must not be stored")

        assertFalse(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_A, HOST_A, MODEL_ID, mismatched))
        val currentState = viewModel.uiState.value
        assertTrue(currentState.hostResponses.isEmpty())
    }

    @Test
    fun rejectsAResponseFromPreviousRequestWhenANewRequestIsExpected() {
        val staleResponse =
            response(ATTEMPT_A, HOST_A, "late response from previous request")
                .toBuilder()
                .setRequestId("request-old")
                .build()

        assertFalse(
            viewModel.recordValidatedResponse(
                expectedRequestId = "request-new",
                expectedAttemptId = ATTEMPT_A,
                expectedHostId = HOST_A,
                expectedModelId = MODEL_ID,
                response = staleResponse,
            ),
        )
        assertTrue(viewModel.hostResponses.value.isEmpty())
    }

    @Test
    fun clearsOnlyTheEphemeralResponseCards() {
        val result = response(ATTEMPT_A, HOST_A, "temporary answer")
        assertTrue(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_A, HOST_A, MODEL_ID, result))

        viewModel.clearHostResponses()

        assertTrue(viewModel.hostResponses.value.isEmpty())
        assertEquals("", viewModel.uiState.value.prompt)
    }

    @Test
    fun discoveryDoesNotTrustPeersAndOnlyExplicitlyApprovedPeersReceiveRequests() {
        val gateway = FakeRequestExecutionGateway()
        val factory = RequestExecutionGatewayFactory { _, _ -> gateway }
        val activeViewModel =
            RequestViewModel(
                repository = InMemoryClientRequestRepository(),
                operationScope = commandScope,
                clockMillis = { 20_000L },
                runtimeFactory = factory,
            )

        activeViewModel.discoverPeers(
            context = RuntimeEnvironment.getApplication(),
            rendezvousServiceUrl = "https://directory.example.test",
        )
        assertEquals(1, activeViewModel.uiState.value.discoveredPeers.size)
        val discoveredPeer =
            activeViewModel.uiState.value.discoveredPeers
                .single()
        assertFalse(discoveredPeer.isTrusted)
        assertTrue(gateway.trustedPeers.isEmpty())

        activeViewModel.sendRequest("hello host", "local/test-model", 64)
        assertEquals(0, gateway.dispatchCount)
        assertTrue(gateway.sentHostIds.isEmpty())
        val statusMessage =
            activeViewModel.uiState.value.networkStatusMessage
                .orEmpty()
        assertTrue(statusMessage.contains("No inference payload was sent"))
        assertEquals(
            "No compatible, reachable hosts are currently available. The request was not sent.",
            activeViewModel.uiState.value.hostAvailabilityMessage,
        )

        activeViewModel.trustPeerAfterExplicitConfirmation(gateway.peerId)
        val trustedPeer =
            activeViewModel.uiState.value.discoveredPeers
                .single()
        assertTrue(trustedPeer.isTrusted)
        assertTrue(gateway.trustedPeers.contains(gateway.peerId))

        activeViewModel.sendRequest("hello host", "local/test-model", 64)
        assertEquals(1, gateway.dispatchCount)
        assertEquals(listOf(gateway.peerId), gateway.sentHostIds)
        assertEquals(
            "Only 1 of 3 compatible, reachable hosts are currently available. Continuing with 1 host.",
            activeViewModel.uiState.value.hostAvailabilityMessage,
        )
        assertEquals(1, activeViewModel.uiState.value.hostResponses.size)
        val hostResponse =
            activeViewModel.uiState.value.hostResponses
                .single()
        assertEquals("host response", hostResponse.outputText)

        activeViewModel.clearDirectorySession()
        assertTrue(gateway.closed)
    }

    @Test
    fun cancelActionSendsCorrelatedCancellationForEveryQueuedAttemptFromTheLiveDispatch() {
        val gateway =
            FakeRequestExecutionGateway(
                peerIds = listOf("peer-a", "peer-b", "peer-c"),
            )
        gateway.holdRoundResponses = true
        val activeViewModel =
            RequestViewModel(
                repository = InMemoryClientRequestRepository(),
                operationScope = commandScope,
                clockMillis = { 20_000L },
                runtimeFactory = RequestExecutionGatewayFactory { _, _ -> gateway },
            )

        activeViewModel.discoverPeers(
            context = RuntimeEnvironment.getApplication(),
            rendezvousServiceUrl = "https://directory.example.test",
        )
        gateway.peerIds.forEach(activeViewModel::trustPeerAfterExplicitConfirmation)
        activeViewModel.sendRequest("cancel this request", "local/test-model", 64)

        assertTrue(activeViewModel.uiState.value.isSendingRequest)
        assertTrue(activeViewModel.uiState.value.canCancelRequest)
        assertEquals(gateway.peerIds, gateway.sentHostIds)

        activeViewModel.cancelCurrentRequest()

        assertEquals(1, gateway.cancellationCallCount)
        assertTrue(gateway.lastCancellationRequestId.isNotBlank())
        assertEquals(
            listOf("attempt-1", "attempt-2", "attempt-3"),
            gateway.lastCancelledAttempts.map { attempt -> attempt.attemptId },
        )
        assertEquals(gateway.peerIds, gateway.lastCancelledAttempts.map { attempt -> attempt.ephemeralHostId })
        assertTrue(gateway.lastCancellationResults.all { result -> result.cancellationSent })
        assertFalse(activeViewModel.uiState.value.canCancelRequest)
        assertTrue(
            activeViewModel.uiState.value.networkStatusMessage
                .orEmpty()
                .contains("Cancellation requests sent to all 3 queued host attempts."),
        )

        gateway.releaseRoundResponses.complete(Unit)
        assertFalse(activeViewModel.uiState.value.isSendingRequest)
        assertEquals(3, activeViewModel.uiState.value.hostResponses.size)
        assertTrue(
            activeViewModel.uiState.value.hostResponses.all { response ->
                response.outcome == HostResponseOutcome.CANCELLED
            },
        )
    }

    @Test
    fun clearingDirectorySessionRemovesItsPeerListAndClosesTheGateway() {
        val gateway = FakeRequestExecutionGateway()
        val activeViewModel =
            RequestViewModel(
                repository = InMemoryClientRequestRepository(),
                operationScope = commandScope,
                clockMillis = { 20_000L },
                runtimeFactory = RequestExecutionGatewayFactory { _, _ -> gateway },
            )

        activeViewModel.discoverPeers(
            context = RuntimeEnvironment.getApplication(),
            rendezvousServiceUrl = "https://directory.example.test",
        )
        assertEquals(1, activeViewModel.uiState.value.discoveredPeers.size)

        activeViewModel.clearDirectorySession()

        assertTrue(gateway.closed)
        val discoveredPeers = activeViewModel.uiState.value.discoveredPeers
        assertTrue(discoveredPeers.isEmpty())
    }

    private fun response(
        attemptId: String,
        hostId: String,
        outputText: String,
    ): InferenceResponse =
        inferenceResponse {
            requestId = REQUEST_ID
            this.attemptId = attemptId
            ephemeralHostId = hostId
            outcome = InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
            this.outputText = outputText
            completedAtUnixMs = 19_999L
            modelId = MODEL_ID
            errorMessage = ""
            roundId = "round-001"
        }

    private companion object {
        const val REQUEST_ID = "request-001"
        const val ATTEMPT_A = "attempt-a"
        const val ATTEMPT_B = "attempt-b"
        const val HOST_A = "host-a"
        const val HOST_B = "host-b"
        const val MODEL_ID = "local/model-1"
    }

    private class FakeRequestExecutionGateway(
        val peerIds: List<String> = listOf("peer-test-01"),
    ) : RequestExecutionGateway {
        init {
            require(peerIds.isNotEmpty() && peerIds.distinct().size == peerIds.size)
        }

        val peerId = peerIds.first()
        val trustedPeers = mutableSetOf<String>()
        var dispatchCount = 0
        var cancellationCallCount = 0
        var lastCancellationRequestId = ""
        var lastCancelledAttempts: List<InferenceHostAttempt> = emptyList()
        var lastCancellationResults: List<InferenceAttemptCancellationResult> = emptyList()
        val cancelledAttemptIds = mutableSetOf<String>()
        var holdRoundResponses = false
        val releaseRoundResponses = CompletableDeferred<Unit>()
        var closed = false
        var sentHostIds: List<String> = emptyList()

        private val endpoints =
            peerIds.map { id ->
                RendezvousPeerEndpoint(
                    peerId = id,
                    tcpMultiaddresses = listOf("/ip4/127.0.0.1/tcp/4001/p2p/$id"),
                    expiresAtUtc = Instant.parse("2099-01-01T00:00:00Z"),
                )
            }

        override fun isPeerTrusted(peerId: String): Boolean = peerId in trustedPeers

        override fun trustPeerAfterExplicitConfirmation(
            peerId: String,
            explicitlyConfirmed: Boolean,
        ) {
            check(explicitlyConfirmed)
            trustedPeers.add(peerId)
        }

        override fun revokePeerTrust(peerId: String) {
            trustedPeers.remove(peerId)
        }

        override suspend fun discoverPeersForTrust(): List<RendezvousPeerEndpoint> = endpoints

        override suspend fun discoverAndSelectReachableHosts(
            request: org.whitep.shared.contracts.v1.InferenceRequest,
            excludedEphemeralHostIds: Set<String>,
            maxHosts: Int,
        ): HostSelectionResult {
            val candidates =
                peerIds
                    .filter { id -> id in trustedPeers && id !in excludedEphemeralHostIds }
                    .take(maxHosts)
                    .map(::candidate)
            return HostSelectionResult(
                candidates = candidates,
                feedback = HostSelectionFeedback(availableHostCount = candidates.size),
            )
        }

        override suspend fun dispatchRound(
            distributedRequest: DistributedRequest,
            selectedHosts: List<HostCandidate>,
            modelId: String,
            currentMessage: String,
            capsule: ConversationContextCapsule,
            maxOutputTokens: Int,
            protocolVersion: String,
        ): InferenceRoundDispatchResult {
            check(selectedHosts.all { it.announcement.ephemeralHostId in trustedPeers })
            dispatchCount++
            sentHostIds = selectedHosts.map { it.announcement.ephemeralHostId }
            return InferenceRoundDispatchResult.Dispatched(
                selectedHosts.mapIndexed { index, host ->
                    InferenceHostAttempt(
                        ephemeralHostId = host.announcement.ephemeralHostId,
                        attemptId = "attempt-${index + 1}",
                        submissionStatus = InferenceAttemptSubmissionStatus.QUEUED,
                    )
                },
            )
        }

        override suspend fun cancelSelectedHosts(
            requestId: String,
            selectedAttempts: List<InferenceHostAttempt>,
        ): List<InferenceAttemptCancellationResult> {
            cancellationCallCount++
            lastCancellationRequestId = requestId
            lastCancelledAttempts = selectedAttempts.toList()
            cancelledAttemptIds.addAll(
                selectedAttempts
                    .filter { attempt -> attempt.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED }
                    .map { attempt -> attempt.attemptId },
            )
            lastCancellationResults =
                selectedAttempts.map { attempt ->
                    InferenceAttemptCancellationResult(
                        requestId = requestId,
                        attemptId = attempt.attemptId,
                        ephemeralHostId = attempt.ephemeralHostId,
                        cancellationSent = attempt.attemptId in cancelledAttemptIds,
                    )
                }
            return lastCancellationResults
        }

        override suspend fun awaitRoundResponses(
            requestId: String,
            attempts: List<InferenceHostAttempt>,
        ): InferenceRoundResponseCollection {
            if (holdRoundResponses) releaseRoundResponses.await()
            return InferenceRoundResponseCollection(
                responses =
                    attempts.map { attempt ->
                        inferenceResponse {
                            this.requestId = requestId
                            attemptId = attempt.attemptId
                            ephemeralHostId = attempt.ephemeralHostId
                            outcome =
                                if (attempt.attemptId in cancelledAttemptIds) {
                                    InferenceOutcome.INFERENCE_OUTCOME_CANCELLED
                                } else {
                                    InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
                                }
                            outputText = if (attempt.attemptId in cancelledAttemptIds) "" else "host response"
                            completedAtUnixMs = 19_999L
                            modelId = MODEL_ID
                            roundId = "round-001"
                        }
                    },
                failures = emptyList(),
            )
        }

        override fun close() {
            closed = true
        }

        private fun candidate(peerId: String): HostCandidate {
            val model =
                ModelCapability
                    .newBuilder()
                    .setModelId(MODEL_ID)
                    .setMaxOutputTokens(128)
                    .build()
            val capabilities =
                HostCapabilities
                    .newBuilder()
                    .setEphemeralHostId(peerId)
                    .setProtocolVersion("1.0.0")
                    .addModels(model)
                    .setMaxConcurrentRequests(1)
                    .setMaxPromptBytes(8_192)
                    .setMaxContextCapsuleBytes(4_096)
                    .build()
            val announcement =
                HostAnnouncement
                    .newBuilder()
                    .setEphemeralHostId(peerId)
                    .setProtocolVersion("1.0.0")
                    .setCapabilities(capabilities)
                    .setAnnouncedAtUnixMs(19_000L)
                    .build()
            val status =
                HostStatus
                    .newBuilder()
                    .setEphemeralHostId(peerId)
                    .setState(HostRuntimeState.HOST_RUNTIME_STATE_READY)
                    .setActiveRequests(0)
                    .setObservedAtUnixMs(19_000L)
                    .setProtocolVersion("1.0.0")
                    .addModels(model)
                    .build()
            return HostCandidate(announcement, status)
        }

        private companion object {
            const val MODEL_ID = "local/test-model"
        }
    }
}
