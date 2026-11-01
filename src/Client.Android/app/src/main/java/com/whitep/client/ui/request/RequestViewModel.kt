package com.whitep.client.ui.request

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.data.ClientRequestRepository
import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import com.whitep.client.protocol.InferenceRequestPayloadBuildResult
import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import com.whitep.client.requests.AndroidRequestExecutionGatewayFactory
import com.whitep.client.requests.DistributedRequest
import com.whitep.client.requests.FirstReleaseInferenceLimits
import com.whitep.client.requests.HostSelectionFeedback
import com.whitep.client.requests.InferenceRoundDispatchResult
import com.whitep.client.requests.InferenceRoundResponseFailure
import com.whitep.client.requests.InferenceRoundResponseFailureReason
import com.whitep.client.requests.RequestExecutionGateway
import com.whitep.client.requests.RequestExecutionGatewayFactory
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import java.net.URI
import java.util.UUID

enum class HostResponseOutcome {
    SUCCEEDED,
    FAILED,
    CANCELLED,
    TIMED_OUT,
    REJECTED,
    UNKNOWN,
}

/** One independently renderable, in-memory result for a single host attempt. */
data class HostResponseCardState(
    val requestId: String,
    val attemptId: String,
    val hostId: String,
    val modelId: String,
    val outcome: HostResponseOutcome,
    val outputText: String,
    val errorMessage: String,
    val completedAtUnixMs: Long,
    val receivedAtUnixMs: Long,
)

data class RequestPeerUiState(
    val peerId: String,
    val expiresAtUtc: String,
    val isTrusted: Boolean,
)

data class RequestUiState(
    val prompt: String = "",
    val status: RequestDraftStatus = RequestDraftStatus.NOT_SENT,
    val hostResponses: List<HostResponseCardState> = emptyList(),
    val hostAvailabilityMessage: String? = null,
    val discoveredPeers: List<RequestPeerUiState> = emptyList(),
    val isDiscoveringPeers: Boolean = false,
    val isSendingRequest: Boolean = false,
    val networkStatusMessage: String? = null,
) {
    val statusMessage: String
        get() =
            when (status) {
                RequestDraftStatus.NOT_SENT -> "Nothing has been sent."
                RequestDraftStatus.EMPTY_PROMPT -> "Enter a prompt first."
                RequestDraftStatus.TRANSPORT_UNAVAILABLE ->
                    "Request is not sent. Configure the HTTPS Rendezvous directory and explicitly trust a host."
            }
}

private data class RequestNetworkState(
    val discoveredPeers: List<RequestPeerUiState> = emptyList(),
    val isDiscoveringPeers: Boolean = false,
    val isSendingRequest: Boolean = false,
    val message: String? = null,
)

/**
 * Owns the live Android request flow. Peer metadata is used only for explicit trust decisions;
 * inference candidates require a fresh authenticated HostStatus probe from the concrete runtime.
 * Prompts and response bodies remain in ephemeral UI state and are never sent to Rendezvous.
 */
class RequestViewModel(
    private val repository: ClientRequestRepository = InMemoryClientRequestRepository(),
    private val operationScope: CoroutineScope? = null,
    private val clockMillis: () -> Long = System::currentTimeMillis,
    private val runtimeFactory: RequestExecutionGatewayFactory = AndroidRequestExecutionGatewayFactory,
) : ViewModel() {
    private val commandsScope: CoroutineScope
        get() = operationScope ?: viewModelScope

    private val mutableHostResponses = MutableStateFlow<List<HostResponseCardState>>(emptyList())
    val hostResponses: StateFlow<List<HostResponseCardState>> = mutableHostResponses.asStateFlow()
    private val mutableHostAvailabilityMessage = MutableStateFlow<String?>(null)
    private val mutableNetworkState = MutableStateFlow(RequestNetworkState())
    private val responseLock = Any()
    private var requestRuntime: RequestExecutionGateway? = null
    private var requestRuntimeUri: URI? = null

    val uiState: StateFlow<RequestUiState> =
        combine(
            repository.draft,
            mutableHostResponses,
            mutableHostAvailabilityMessage,
            mutableNetworkState,
        ) { draft, responses, hostAvailabilityMessage, network ->
            RequestUiState(
                prompt = draft.prompt,
                status = draft.status,
                hostResponses = responses,
                hostAvailabilityMessage = hostAvailabilityMessage,
                discoveredPeers = network.discoveredPeers,
                isDiscoveringPeers = network.isDiscoveringPeers,
                isSendingRequest = network.isSendingRequest,
                networkStatusMessage = network.message,
            )
        }.stateIn(
            scope = commandsScope,
            started = SharingStarted.Eagerly,
            initialValue = RequestUiState(),
        )

    fun updatePrompt(prompt: String) {
        mutableHostAvailabilityMessage.value = null
        commandsScope.launch { repository.updatePrompt(prompt) }
    }

    fun prepareRequest() {
        mutableHostAvailabilityMessage.value = null
        commandsScope.launch { repository.prepareRequest() }
    }

    fun clearDraft() {
        mutableHostAvailabilityMessage.value = null
        commandsScope.launch { repository.clearDraft() }
    }

    /**
     * Connects to the explicitly configured HTTPS directory and lists its current TCP peers.
     * Listing a peer does not trust it; the UI must request a separate user confirmation.
     */
    fun discoverPeers(
        context: Context,
        rendezvousServiceUrl: String,
    ) {
        val uri =
            try {
                URI(rendezvousServiceUrl.trim())
            } catch (_: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        message = "Enter a valid absolute HTTPS Rendezvous URL.",
                    )
                return
            }
        if (!uri.isAbsolute ||
            uri.scheme?.lowercase() != "https" ||
            uri.host.isNullOrBlank() ||
            uri.userInfo != null ||
            uri.query != null ||
            uri.fragment != null
        ) {
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    message = "Use an absolute HTTPS URL without credentials, query parameters or a fragment.",
                )
            return
        }

        commandsScope.launch {
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    isDiscoveringPeers = true,
                    message = "Connecting to the configured peer directory…",
                )
            try {
                val gateway = getOrCreateRuntime(context, uri)
                val endpoints = gateway.discoverPeersForTrust()
                val peers =
                    endpoints.map { endpoint ->
                        RequestPeerUiState(
                            peerId = endpoint.peerId,
                            expiresAtUtc = endpoint.expiresAtUtc.toString(),
                            isTrusted = gateway.isPeerTrusted(endpoint.peerId),
                        )
                    }
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        discoveredPeers = peers,
                        message =
                            if (peers.isEmpty()) {
                                "The directory returned no unexpired direct TCP peers."
                            } else {
                                "Found ${peers.size} peer(s). None is trusted automatically."
                            },
                    )
            } catch (cancellation: CancellationException) {
                throw cancellation
            } catch (exception: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        message = "Peer discovery failed: ${safeMessage(exception)}",
                    )
            } finally {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(isDiscoveringPeers = false)
            }
        }
    }

    /** Drops a configured directory session when its URL is edited. */
    fun clearDirectorySession() {
        requestRuntime?.close()
        requestRuntime = null
        requestRuntimeUri = null
        mutableNetworkState.value = RequestNetworkState()
        mutableHostAvailabilityMessage.value = null
    }

    /** Called only by the explicit confirmation action in the UI. */
    fun trustPeerAfterExplicitConfirmation(peerId: String) {
        commandsScope.launch {
            try {
                val gateway = requireRuntime()
                gateway.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = true)
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        discoveredPeers =
                            mutableNetworkState.value.discoveredPeers.map { peer ->
                                if (peer.peerId == peerId) peer.copy(isTrusted = true) else peer
                            },
                        message = "Peer trust saved. This host may receive prompts and context you send to it.",
                    )
            } catch (exception: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(message = "Trust decision failed: ${safeMessage(exception)}")
            }
        }
    }

    fun revokePeerTrust(peerId: String) {
        commandsScope.launch {
            try {
                val gateway = requireRuntime()
                gateway.revokePeerTrust(peerId)
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        discoveredPeers =
                            mutableNetworkState.value.discoveredPeers.map { peer ->
                                if (peer.peerId == peerId) peer.copy(isTrusted = false) else peer
                            },
                        message = "Peer trust revoked.",
                    )
            } catch (exception: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(message = "Trust revocation failed: ${safeMessage(exception)}")
            }
        }
    }

    /**
     * Builds, selects and dispatches one real inference round. The directory receives only control
     * metadata; selected peers are authenticated and must have been explicitly trusted beforehand.
     */
    fun sendRequest(
        prompt: String,
        modelId: String,
        maxOutputTokens: Int,
    ) {
        val cleanPrompt = prompt.trim()
        val cleanModelId = modelId.trim()
        if (cleanPrompt.isEmpty() || cleanModelId.isEmpty() || maxOutputTokens <= 0) {
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    message = "A non-empty prompt, exact host model ID and positive output-token limit are required.",
                )
            return
        }
        if (mutableNetworkState.value.isSendingRequest) return

        commandsScope.launch {
            mutableHostResponses.value = emptyList()
            mutableHostAvailabilityMessage.value = null
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    isSendingRequest = true,
                    message = "Preparing a request for explicitly trusted, compatible peers…",
                )
            try {
                val gateway = requireRuntime()
                val distributedRequest =
                    DistributedRequest.create(
                        deadlineUnixMs = Math.addExact(clockMillis(), REQUEST_DEADLINE_MILLIS),
                        maxRounds = 1,
                        maxTotalHosts = FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND,
                    )
                val capsule =
                    ConversationContextCapsule(
                        goal = "Single-turn request without additional conversation context.",
                    )
                val selectionAttemptId = UUID.randomUUID().toString()
                val selectionRoundId = UUID.randomUUID().toString()
                val selectionRequest =
                    when (
                        val built =
                            InferenceRequestPayloadBuilder.build(
                                requestId = distributedRequest.requestId,
                                attemptId = selectionAttemptId,
                                roundId = selectionRoundId,
                                modelId = cleanModelId,
                                currentMessage = cleanPrompt,
                                capsule = capsule,
                                deadlineUnixMs = distributedRequest.deadlineUnixMs,
                                maxOutputTokens = maxOutputTokens,
                                protocolVersion = CURRENT_PROTOCOL_VERSION,
                            )
                    ) {
                        is InferenceRequestPayloadBuildResult.Built -> built.request
                        is InferenceRequestPayloadBuildResult.Rejected -> {
                            mutableNetworkState.value =
                                mutableNetworkState.value.copy(
                                    message =
                                        "Request validation failed: " +
                                            "${built.reason.name.lowercase().replace('_', ' ')}.",
                                )
                            return@launch
                        }
                    }

                val selection =
                    gateway.discoverAndSelectReachableHosts(
                        request = selectionRequest,
                        maxHosts = FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND,
                    )
                mutableHostAvailabilityMessage.value = selection.feedback.message
                if (selection.candidates.isEmpty()) {
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            message = "No inference payload was sent. Trust a compatible host and retry discovery.",
                        )
                    return@launch
                }

                val dispatch =
                    gateway.dispatchRound(
                        distributedRequest = distributedRequest,
                        selectedHosts = selection.candidates,
                        modelId = cleanModelId,
                        currentMessage = cleanPrompt,
                        capsule = capsule,
                        maxOutputTokens = maxOutputTokens,
                        protocolVersion = CURRENT_PROTOCOL_VERSION,
                    )
                when (dispatch) {
                    is InferenceRoundDispatchResult.Rejected -> {
                        mutableNetworkState.value =
                            mutableNetworkState.value.copy(
                                message =
                                    "The round was not dispatched: " +
                                        "${dispatch.reason.name.lowercase().replace('_', ' ')}.",
                            )
                        return@launch
                    }
                    is InferenceRoundDispatchResult.Dispatched -> {
                        val collection =
                            gateway.awaitRoundResponses(
                                requestId = distributedRequest.requestId,
                                attempts = dispatch.attempts,
                            )
                        val attemptsById = dispatch.attempts.associateBy { attempt -> attempt.attemptId }
                        collection.responses.forEach { response ->
                            val attempt = attemptsById[response.attemptId] ?: return@forEach
                            recordValidatedResponse(
                                expectedRequestId = distributedRequest.requestId,
                                expectedAttemptId = attempt.attemptId,
                                expectedHostId = attempt.ephemeralHostId,
                                expectedModelId = cleanModelId,
                                response = response,
                            )
                        }
                        collection.failures.forEach { failure ->
                            recordFailedAttempt(distributedRequest.requestId, failure)
                        }
                        mutableNetworkState.value =
                            mutableNetworkState.value.copy(
                                message =
                                    "Round finished: ${collection.responses.size} response(s), " +
                                        "${collection.failures.size} failed or unanswered attempt(s).",
                            )
                    }
                }
            } catch (cancellation: CancellationException) {
                throw cancellation
            } catch (exception: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        message = "Request execution failed: ${safeMessage(exception)}",
                    )
            } finally {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(isSendingRequest = false)
            }
        }
    }

    /** Updates the UI with the post-probe candidate count; peer metadata is never persisted. */
    fun recordHostSelectionFeedback(feedback: HostSelectionFeedback) {
        mutableHostAvailabilityMessage.value = feedback.message
    }

    fun clearHostSelectionFeedback() {
        mutableHostAvailabilityMessage.value = null
    }

    /**
     * Retains one response per request/attempt pair. The expected identifiers must come from the
     * dispatch result; the response itself must be returned by the correlation-validating transport.
     * A retry from the same host has a different attempt ID and therefore receives its own card.
     */
    fun recordValidatedResponse(
        expectedRequestId: String,
        expectedAttemptId: String,
        expectedHostId: String,
        expectedModelId: String,
        response: InferenceResponse,
    ): Boolean {
        if (expectedRequestId.isBlank() ||
            expectedAttemptId.isBlank() ||
            expectedHostId.isBlank() ||
            expectedModelId.isBlank() ||
            response.requestId != expectedRequestId ||
            response.attemptId != expectedAttemptId ||
            response.ephemeralHostId != expectedHostId ||
            response.modelId != expectedModelId ||
            response.roundId.isBlank() ||
            response.completedAtUnixMs <= 0L ||
            response.outcome == InferenceOutcome.INFERENCE_OUTCOME_UNSPECIFIED ||
            response.outcome == InferenceOutcome.UNRECOGNIZED
        ) {
            return false
        }

        val card =
            HostResponseCardState(
                requestId = response.requestId,
                attemptId = response.attemptId,
                hostId = response.ephemeralHostId,
                modelId = response.modelId,
                outcome =
                    when (response.outcome) {
                        InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED -> HostResponseOutcome.SUCCEEDED
                        InferenceOutcome.INFERENCE_OUTCOME_FAILED -> HostResponseOutcome.FAILED
                        InferenceOutcome.INFERENCE_OUTCOME_CANCELLED -> HostResponseOutcome.CANCELLED
                        InferenceOutcome.INFERENCE_OUTCOME_TIMED_OUT -> HostResponseOutcome.TIMED_OUT
                        InferenceOutcome.INFERENCE_OUTCOME_REJECTED -> HostResponseOutcome.REJECTED
                        else -> HostResponseOutcome.UNKNOWN
                    },
                outputText = response.outputText,
                errorMessage = response.errorMessage,
                completedAtUnixMs = response.completedAtUnixMs,
                receivedAtUnixMs = clockMillis(),
            )

        synchronized(responseLock) {
            val current = mutableHostResponses.value
            if (current.any { it.requestId == card.requestId && it.attemptId == card.attemptId }) {
                return false
            }
            mutableHostResponses.value = current + card
        }
        return true
    }

    private fun recordFailedAttempt(
        requestId: String,
        failure: InferenceRoundResponseFailure,
    ) {
        val outcome =
            when (failure.reason) {
                InferenceRoundResponseFailureReason.SUBMISSION_REJECTED -> HostResponseOutcome.REJECTED
                InferenceRoundResponseFailureReason.SUBMISSION_TIMED_OUT,
                InferenceRoundResponseFailureReason.ROUND_WINDOW_UNAVAILABLE,
                InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT,
                -> HostResponseOutcome.TIMED_OUT
                InferenceRoundResponseFailureReason.SUBMISSION_FAILED,
                InferenceRoundResponseFailureReason.RESPONSE_FAILED,
                -> HostResponseOutcome.FAILED
            }
        val card =
            HostResponseCardState(
                requestId = requestId,
                attemptId = failure.attemptId,
                hostId = failure.ephemeralHostId,
                modelId = "",
                outcome = outcome,
                outputText = "",
                errorMessage =
                    failure.reason.name
                        .lowercase()
                        .replace('_', ' '),
                completedAtUnixMs = clockMillis(),
                receivedAtUnixMs = clockMillis(),
            )
        synchronized(responseLock) {
            val attemptAlreadyRecorded =
                mutableHostResponses.value.any { recorded ->
                    recorded.requestId == requestId && recorded.attemptId == failure.attemptId
                }
            if (!attemptAlreadyRecorded) {
                mutableHostResponses.value = mutableHostResponses.value + card
            }
        }
    }

    fun clearHostResponses() {
        synchronized(responseLock) {
            mutableHostResponses.value = emptyList()
        }
    }

    private fun getOrCreateRuntime(
        context: Context,
        uri: URI,
    ): RequestExecutionGateway {
        if (requestRuntime != null && requestRuntimeUri == uri) return requireNotNull(requestRuntime)
        requestRuntime?.close()
        val created = runtimeFactory.create(context.applicationContext, uri)
        requestRuntime = created
        requestRuntimeUri = uri
        return created
    }

    private fun requireRuntime(): RequestExecutionGateway =
        requestRuntime
            ?: throw IllegalStateException("Discover peers using the configured HTTPS Rendezvous URL first.")

    private fun safeMessage(exception: Exception): String =
        exception.message
            ?.replace(Regex("[\\r\\n\\t]+"), " ")
            ?.take(180)
            ?.ifBlank { exception.javaClass.simpleName }
            ?: exception.javaClass.simpleName

    override fun onCleared() {
        requestRuntime?.close()
        requestRuntime = null
        requestRuntimeUri = null
        super.onCleared()
    }

    private companion object {
        const val CURRENT_PROTOCOL_VERSION = "1.0.0"
        const val REQUEST_DEADLINE_MILLIS = 90_000L
    }
}
