package com.whitep.client.ui.request

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.data.ClientRequestRepository
import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.InMemoryInferenceRequestLifecycleRepository
import com.whitep.client.data.InferenceRequestLifecycleRepository
import com.whitep.client.data.InferenceRequestLifecycleState
import com.whitep.client.data.PersistedRequestStatus
import com.whitep.client.data.RequestDraftStatus
import com.whitep.client.errors.RequestErrorCode
import com.whitep.client.errors.toRequestErrorCode
import com.whitep.client.protocol.InferenceRequestPayloadBuildResult
import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import com.whitep.client.requests.AndroidRequestExecutionGatewayFactory
import com.whitep.client.requests.DistributedRequest
import com.whitep.client.requests.DistributedRequestProgress
import com.whitep.client.requests.FirstReleaseInferenceLimits
import com.whitep.client.requests.HostSelectionFeedback
import com.whitep.client.requests.InferenceAttemptCancellationResult
import com.whitep.client.requests.InferenceAttemptSubmissionStatus
import com.whitep.client.requests.InferenceHostAttempt
import com.whitep.client.requests.InferenceRoundDispatchResult
import com.whitep.client.requests.InferenceRoundResponseFailure
import com.whitep.client.requests.InferenceRoundResponseFailureReason
import com.whitep.client.requests.RequestExecutionGateway
import com.whitep.client.requests.RequestExecutionGatewayFactory
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CompletableDeferred
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

enum class RequestRunStatus {
    IDLE,
    RUNNING,
    SUCCEEDED,
    PARTIAL,
    FAILED,
    CANCELLED,
    EXPIRED,
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
    val errorCode: RequestErrorCode? = null,
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
    val canCancelRequest: Boolean = false,
    val isCancellingRequest: Boolean = false,
    val runStatus: RequestRunStatus = RequestRunStatus.IDLE,
    val canRetryRequest: Boolean = false,
    val isRetryingRequest: Boolean = false,
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
    val canCancelRequest: Boolean = false,
    val isCancellingRequest: Boolean = false,
    val runStatus: RequestRunStatus = RequestRunStatus.IDLE,
    val canRetryRequest: Boolean = false,
    val isRetryingRequest: Boolean = false,
    val message: String? = null,
)

private data class ActiveRequestCancellation(
    val requestId: String,
    val attempts: List<InferenceHostAttempt>,
)

private data class RetryableRequestContext(
    val request: DistributedRequest,
    val prompt: String,
    val modelId: String,
    val maxOutputTokens: Int,
    val capsule: ConversationContextCapsule,
    val createdAtEpochMillis: Long,
    val progress: DistributedRequestProgress = DistributedRequestProgress(),
    val attempts: List<InferenceHostAttempt> = emptyList(),
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
    private val lifecycleRepository: InferenceRequestLifecycleRepository =
        InMemoryInferenceRequestLifecycleRepository(),
) : ViewModel() {
    private val commandsScope: CoroutineScope
        get() = operationScope ?: viewModelScope

    private val mutableHostResponses = MutableStateFlow<List<HostResponseCardState>>(emptyList())
    val hostResponses: StateFlow<List<HostResponseCardState>> = mutableHostResponses.asStateFlow()
    private val mutableHostAvailabilityMessage = MutableStateFlow<String?>(null)
    private val mutableNetworkState = MutableStateFlow(RequestNetworkState())
    private val lifecycleRecovery = CompletableDeferred<Boolean>()
    private var lifecyclePersistenceAvailable = false
    private val responseLock = Any()
    private var requestRuntime: RequestExecutionGateway? = null
    private var requestRuntimeUri: URI? = null
    private var activeRequestCancellation: ActiveRequestCancellation? = null
    private var retryableRequest: RetryableRequestContext? = null

    init {
        commandsScope.launch {
            try {
                val restored = lifecycleRepository.recoverLatest(clockMillis())
                lifecyclePersistenceAvailable = true
                if (restored != null) {
                    val restoredStatus =
                        when (restored.status) {
                            PersistedRequestStatus.QUEUED,
                            PersistedRequestStatus.CONTACTING,
                            PersistedRequestStatus.WAITING,
                            PersistedRequestStatus.EXPIRED,
                            -> RequestRunStatus.EXPIRED
                            PersistedRequestStatus.PARTIAL -> RequestRunStatus.PARTIAL
                            PersistedRequestStatus.COMPLETED -> RequestRunStatus.SUCCEEDED
                            PersistedRequestStatus.FAILED -> RequestRunStatus.FAILED
                            PersistedRequestStatus.CANCELLED -> RequestRunStatus.CANCELLED
                        }
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            runStatus = restoredStatus,
                            message =
                                when (restoredStatus) {
                                    RequestRunStatus.IDLE -> "No request has been sent."
                                    RequestRunStatus.RUNNING -> "A request is running."
                                    RequestRunStatus.SUCCEEDED -> "The last local request completed."
                                    RequestRunStatus.PARTIAL -> "The last local request has partial results."
                                    RequestRunStatus.FAILED -> "The last local request failed."
                                    RequestRunStatus.CANCELLED -> "The last local request was cancelled."
                                    RequestRunStatus.EXPIRED ->
                                        "The app closed before a result was saved; the remote host may have completed."
                                },
                        )
                }
            } catch (cancellation: CancellationException) {
                throw cancellation
            } catch (_: Exception) {
                lifecyclePersistenceAvailable = false
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        runStatus = RequestRunStatus.FAILED,
                        message = "The local request state could not be recovered. No new request will be sent.",
                    )
            } finally {
                lifecycleRecovery.complete(lifecyclePersistenceAvailable)
            }
        }
    }

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
                canCancelRequest = network.canCancelRequest,
                isCancellingRequest = network.isCancellingRequest,
                runStatus = network.runStatus,
                canRetryRequest = network.canRetryRequest,
                isRetryingRequest = network.isRetryingRequest,
                networkStatusMessage = network.message,
            )
        }.stateIn(
            scope = commandsScope,
            started = SharingStarted.Eagerly,
            initialValue = RequestUiState(),
        )

    fun updatePrompt(prompt: String) {
        mutableHostAvailabilityMessage.value = null
        if (retryableRequest != null && retryableRequest?.prompt != prompt.trim()) {
            retryableRequest = null
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    canRetryRequest = false,
                    message = "The draft changed. Send a new request to use the edited prompt.",
                )
        }
        commandsScope.launch { repository.updatePrompt(prompt) }
    }

    fun prepareRequest() {
        mutableHostAvailabilityMessage.value = null
        commandsScope.launch { repository.prepareRequest() }
    }

    fun clearDraft() {
        mutableHostAvailabilityMessage.value = null
        retryableRequest = null
        mutableNetworkState.value =
            mutableNetworkState.value.copy(
                canRetryRequest = false,
                message = "The draft was cleared. Send a new request to start another execution.",
            )
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
        activeRequestCancellation = null
        retryableRequest = null
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
     * Starts one logical inference request. Manual retries keep the same request ID and bounded budget.
     * No retry is dispatched automatically.
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

        val context =
            RetryableRequestContext(
                request =
                    DistributedRequest.create(
                        deadlineUnixMs = Math.addExact(clockMillis(), REQUEST_DEADLINE_MILLIS),
                        maxRounds = FirstReleaseInferenceLimits.MAX_ROUNDS,
                        maxTotalHosts = FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS,
                    ),
                prompt = cleanPrompt,
                modelId = cleanModelId,
                maxOutputTokens = maxOutputTokens,
                capsule =
                    ConversationContextCapsule(
                        goal = "Single-turn request without additional conversation context.",
                    ),
                createdAtEpochMillis = clockMillis(),
            )
        retryableRequest = context
        mutableHostResponses.value = emptyList()
        mutableHostAvailabilityMessage.value = null
        activeRequestCancellation = null
        executeRequestRound(context, manualRetry = false)
    }

    /** Dispatches another bounded round only after the user explicitly presses Retry. */
    fun retryFailedAttempts() {
        val context = retryableRequest ?: return
        val network = mutableNetworkState.value
        if (!network.canRetryRequest || network.isSendingRequest || network.isCancellingRequest) return
        executeRequestRound(context, manualRetry = true)
    }

    private fun executeRequestRound(
        requestedContext: RetryableRequestContext,
        manualRetry: Boolean,
    ) {
        if (mutableNetworkState.value.isSendingRequest) return
        val currentContext =
            retryableRequest?.takeIf { current ->
                current.request.requestId == requestedContext.request.requestId
            } ?: return
        if (manualRetry && failedPeerIdsForRetry(currentContext).isEmpty()) return

        mutableNetworkState.value =
            mutableNetworkState.value.copy(
                isSendingRequest = true,
                canCancelRequest = false,
                isCancellingRequest = false,
                runStatus = RequestRunStatus.RUNNING,
                canRetryRequest = false,
                isRetryingRequest = manualRetry,
                message =
                    if (manualRetry) {
                        "Preparing a manual retry for failed host attempts…"
                    } else {
                        "Preparing a request for explicitly trusted, compatible peers…"
                    },
            )

        commandsScope.launch {
            var context = currentContext
            try {
                if (!lifecycleRecovery.await()) {
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            runStatus = RequestRunStatus.FAILED,
                            canRetryRequest = false,
                            message = "The local request state could not be recovered. No request was sent.",
                        )
                    return@launch
                }
                val gateway = requireRuntime()
                if (manualRetry) {
                    context =
                        context.copy(
                            request =
                                context.request.copy(
                                    deadlineUnixMs = Math.addExact(clockMillis(), REQUEST_DEADLINE_MILLIS),
                                ),
                        )
                    retryableRequest = context
                }

                val remainingHosts = context.request.maxTotalHosts - context.progress.hostsAttempted
                val failedPeerCount = failedPeerIdsForRetry(context).size
                val requestedHosts =
                    if (manualRetry) failedPeerCount else FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND
                val maxHosts =
                    minOf(
                        FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND,
                        remainingHosts,
                        requestedHosts,
                    )
                if (context.progress.roundsDispatched >= context.request.maxRounds || maxHosts <= 0) {
                    updateAggregatedRequestStatus(
                        context,
                        "The manual retry budget for this request has been exhausted.",
                    )
                    return@launch
                }

                val selectionRequest =
                    when (
                        val built =
                            InferenceRequestPayloadBuilder.build(
                                requestId = context.request.requestId,
                                attemptId = UUID.randomUUID().toString(),
                                roundId = UUID.randomUUID().toString(),
                                modelId = context.modelId,
                                currentMessage = context.prompt,
                                capsule = context.capsule,
                                deadlineUnixMs = context.request.deadlineUnixMs,
                                maxOutputTokens = context.maxOutputTokens,
                                protocolVersion = CURRENT_PROTOCOL_VERSION,
                            )
                    ) {
                        is InferenceRequestPayloadBuildResult.Built -> built.request
                        is InferenceRequestPayloadBuildResult.Rejected -> {
                            val message =
                                "Request validation failed: " +
                                    "${built.reason.name.lowercase().replace('_', ' ')}."
                            if (context.attempts.isEmpty()) {
                                mutableNetworkState.value =
                                    mutableNetworkState.value.copy(
                                        runStatus = RequestRunStatus.FAILED,
                                        canRetryRequest = false,
                                        message = message,
                                    )
                            } else {
                                updateAggregatedRequestStatus(context, message)
                            }
                            return@launch
                        }
                    }

                val selection =
                    gateway.discoverAndSelectReachableHosts(
                        request = selectionRequest,
                        excludedEphemeralHostIds = successfulPeerIds(context),
                        maxHosts = maxHosts,
                    )
                mutableHostAvailabilityMessage.value = selection.feedback.message
                if (selection.candidates.isEmpty()) {
                    if (context.attempts.isEmpty()) {
                        mutableNetworkState.value =
                            mutableNetworkState.value.copy(
                                runStatus = RequestRunStatus.IDLE,
                                canRetryRequest = false,
                                message =
                                    "No inference payload was sent. " +
                                        "No compatible, reachable hosts are currently available.",
                            )
                    } else {
                        updateAggregatedRequestStatus(
                            context,
                            "No eligible failed or untried host is currently reachable. Retry remains manual.",
                        )
                    }
                    return@launch
                }

                if (!persistLifecycleState(context, PersistedRequestStatus.QUEUED)) {
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            runStatus = RequestRunStatus.FAILED,
                            canRetryRequest = false,
                            message = "The request could not be saved locally, so no host request was sent.",
                        )
                    return@launch
                }
                if (!persistLifecycleState(context, PersistedRequestStatus.CONTACTING)) {
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            runStatus = RequestRunStatus.FAILED,
                            canRetryRequest = false,
                            message = "The request status could not be saved locally, so no host request was sent.",
                        )
                    return@launch
                }

                val dispatch =
                    gateway.dispatchRound(
                        distributedRequest = context.request,
                        selectedHosts = selection.candidates,
                        modelId = context.modelId,
                        currentMessage = context.prompt,
                        capsule = context.capsule,
                        maxOutputTokens = context.maxOutputTokens,
                        protocolVersion = CURRENT_PROTOCOL_VERSION,
                    )
                when (dispatch) {
                    is InferenceRoundDispatchResult.Rejected -> {
                        val message =
                            "The round was not dispatched: " +
                                "${dispatch.reason.name.lowercase().replace('_', ' ')}."
                        if (context.attempts.isEmpty()) {
                            mutableNetworkState.value =
                                mutableNetworkState.value.copy(
                                    runStatus = RequestRunStatus.FAILED,
                                    canRetryRequest = false,
                                    message = message,
                                )
                            persistLifecycleState(context, PersistedRequestStatus.FAILED, "DISPATCH_REJECTED")
                        } else {
                            updateAggregatedRequestStatus(context, message)
                        }
                        return@launch
                    }
                    is InferenceRoundDispatchResult.Dispatched -> {
                        val nextContext =
                            context.copy(
                                progress =
                                    context.progress.afterDispatch(
                                        dispatch.attempts.map { attempt -> attempt.ephemeralHostId },
                                    ),
                                attempts = context.attempts + dispatch.attempts,
                            )
                        if (retryableRequest?.request?.requestId == context.request.requestId) {
                            retryableRequest = nextContext
                        }
                        val hasQueuedAttempts =
                            dispatch.attempts.any { attempt ->
                                attempt.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED
                            }
                        activeRequestCancellation =
                            if (hasQueuedAttempts) {
                                ActiveRequestCancellation(context.request.requestId, dispatch.attempts.toList())
                            } else {
                                null
                            }
                        mutableNetworkState.value =
                            mutableNetworkState.value.copy(canCancelRequest = hasQueuedAttempts)
                        persistLifecycleState(nextContext, PersistedRequestStatus.WAITING)
                        val collection =
                            gateway.awaitRoundResponses(
                                requestId = context.request.requestId,
                                attempts = dispatch.attempts,
                            )
                        val attemptsById = dispatch.attempts.associateBy { attempt -> attempt.attemptId }
                        collection.responses.forEach { response ->
                            val attempt = attemptsById[response.attemptId] ?: return@forEach
                            recordValidatedResponse(
                                expectedRequestId = context.request.requestId,
                                expectedAttemptId = attempt.attemptId,
                                expectedHostId = attempt.ephemeralHostId,
                                expectedModelId = context.modelId,
                                response = response,
                            )
                        }
                        collection.failures.forEach { failure ->
                            recordFailedAttempt(context.request.requestId, failure)
                        }
                        updateAggregatedRequestStatus(
                            nextContext,
                            "Round finished: ${collection.responses.size} response(s), " +
                                "${collection.failures.size} failed or unanswered attempt(s).",
                        )
                    }
                }
            } catch (cancellation: CancellationException) {
                throw cancellation
            } catch (exception: Exception) {
                val message = "Request execution failed: ${safeMessage(exception)}"
                val latestContext =
                    retryableRequest?.takeIf { current ->
                        current.request.requestId == context.request.requestId
                    }
                if (latestContext != null && latestContext.attempts.isNotEmpty()) {
                    updateAggregatedRequestStatus(latestContext, message)
                } else {
                    mutableNetworkState.value =
                        mutableNetworkState.value.copy(
                            runStatus = RequestRunStatus.FAILED,
                            canRetryRequest = false,
                            message = message,
                        )
                    persistLifecycleState(context, PersistedRequestStatus.FAILED, "REQUEST_EXECUTION_FAILED")
                }
            } finally {
                activeRequestCancellation = null
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        isSendingRequest = false,
                        canCancelRequest = false,
                        isCancellingRequest = false,
                        isRetryingRequest = false,
                    )
            }
        }
    }

    private fun latestOutcomeByHost(context: RetryableRequestContext): Map<String, HostResponseOutcome?> {
        val responsesByAttempt =
            mutableHostResponses.value
                .filter { response -> response.requestId == context.request.requestId }
                .associateBy { response -> response.attemptId }
        return context.attempts
            .groupBy { attempt -> attempt.ephemeralHostId }
            .mapValues { (_, attempts) ->
                responsesByAttempt[attempts.last().attemptId]?.outcome
            }
    }

    private fun failedPeerIdsForRetry(context: RetryableRequestContext): Set<String> =
        latestOutcomeByHost(context)
            .filterValues { outcome ->
                outcome != HostResponseOutcome.SUCCEEDED && outcome != HostResponseOutcome.CANCELLED
            }.keys

    private fun successfulPeerIds(context: RetryableRequestContext): Set<String> =
        latestOutcomeByHost(context)
            .filterValues { outcome ->
                outcome == HostResponseOutcome.SUCCEEDED ||
                    outcome == HostResponseOutcome.CANCELLED
            }.keys

    private suspend fun updateAggregatedRequestStatus(
        context: RetryableRequestContext,
        message: String? = null,
    ) {
        val latestOutcomes = latestOutcomeByHost(context).values
        val successCount = latestOutcomes.count { outcome -> outcome == HostResponseOutcome.SUCCEEDED }
        val failedCount =
            latestOutcomes.count { outcome ->
                outcome != HostResponseOutcome.SUCCEEDED && outcome != HostResponseOutcome.CANCELLED
            }
        val hasCancellation = latestOutcomes.any { outcome -> outcome == HostResponseOutcome.CANCELLED }
        val status =
            when {
                latestOutcomes.isEmpty() -> RequestRunStatus.IDLE
                hasCancellation -> RequestRunStatus.CANCELLED
                failedCount == 0 -> RequestRunStatus.SUCCEEDED
                successCount > 0 -> RequestRunStatus.PARTIAL
                else -> RequestRunStatus.FAILED
            }
        val budgetRemaining =
            context.progress.roundsDispatched < context.request.maxRounds &&
                context.progress.hostsAttempted < context.request.maxTotalHosts
        val isCurrentRetryContext =
            retryableRequest?.request?.requestId == context.request.requestId
        val canRetry =
            isCurrentRetryContext &&
                failedCount > 0 &&
                !hasCancellation &&
                budgetRemaining
        val statusMessage =
            when (status) {
                RequestRunStatus.IDLE -> "No request has been sent."
                RequestRunStatus.RUNNING -> "The request is still running."
                RequestRunStatus.SUCCEEDED -> "All latest host attempts completed successfully."
                RequestRunStatus.PARTIAL ->
                    if (canRetry) {
                        "The request has partial results. Failed host attempts can be retried manually."
                    } else {
                        "The request has partial results; no further retry budget is available."
                    }
                RequestRunStatus.FAILED ->
                    if (canRetry) {
                        "All latest host attempts failed or timed out. A manual retry is available."
                    } else {
                        "All latest host attempts failed or timed out. The manual retry budget is exhausted."
                    }
                RequestRunStatus.CANCELLED ->
                    "The request was cancelled. Cancelled attempts cannot be retried."
                RequestRunStatus.EXPIRED ->
                    "The app closed before a result was saved; the remote host may have completed."
            }
        mutableNetworkState.value =
            mutableNetworkState.value.copy(
                runStatus = status,
                canRetryRequest = canRetry,
                isRetryingRequest = false,
                message = message?.let { "$it $statusMessage" } ?: statusMessage,
            )
        val persistedStatus =
            when (status) {
                RequestRunStatus.IDLE, RequestRunStatus.FAILED -> PersistedRequestStatus.FAILED
                RequestRunStatus.RUNNING -> PersistedRequestStatus.WAITING
                RequestRunStatus.SUCCEEDED -> PersistedRequestStatus.COMPLETED
                RequestRunStatus.PARTIAL -> PersistedRequestStatus.PARTIAL
                RequestRunStatus.CANCELLED -> PersistedRequestStatus.CANCELLED
                RequestRunStatus.EXPIRED -> PersistedRequestStatus.EXPIRED
            }
        val errorCode =
            when (status) {
                RequestRunStatus.FAILED -> "HOST_ATTEMPTS_FAILED"
                RequestRunStatus.CANCELLED -> "USER_CANCELLED"
                RequestRunStatus.EXPIRED -> "APP_CLOSED_BEFORE_RESULT"
                else -> null
            }
        persistLifecycleState(context, persistedStatus, errorCode)
    }

    private suspend fun persistLifecycleState(
        context: RetryableRequestContext,
        status: PersistedRequestStatus,
        errorCode: String? = null,
    ): Boolean {
        if (!lifecyclePersistenceAvailable) return false
        return try {
            lifecycleRepository.save(
                InferenceRequestLifecycleState(
                    requestId = context.request.requestId,
                    status = status,
                    createdAtEpochMillis = context.createdAtEpochMillis,
                    updatedAtEpochMillis = clockMillis(),
                    attemptNumber = context.progress.hostsAttempted,
                    errorCode = errorCode,
                ),
            )
            true
        } catch (cancellation: CancellationException) {
            throw cancellation
        } catch (_: Exception) {
            lifecyclePersistenceAvailable = false
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    message =
                        "The local request status could not be saved; it may not be recoverable after app closure.",
                )
            false
        }
    }

    /** Sends cancellation to every queued attempt from the exact active dispatch result. */
    fun cancelCurrentRequest() {
        val active = activeRequestCancellation ?: return
        if (!mutableNetworkState.value.canCancelRequest || mutableNetworkState.value.isCancellingRequest) return

        commandsScope.launch {
            if (activeRequestCancellation != active || mutableNetworkState.value.isCancellingRequest) return@launch
            mutableNetworkState.value =
                mutableNetworkState.value.copy(
                    canCancelRequest = false,
                    isCancellingRequest = true,
                    message = "Sending cancellation requests to the selected hosts…",
                )
            try {
                val results = requireRuntime().cancelSelectedHosts(active.requestId, active.attempts)
                val queuedCount =
                    active.attempts.count { attempt ->
                        attempt.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED
                    }
                val sentCount = results.count(InferenceAttemptCancellationResult::cancellationSent)
                val message =
                    if (sentCount == queuedCount) {
                        "Cancellation requests sent to all $queuedCount queued host attempts. " +
                            "Waiting for host responses."
                    } else {
                        "Cancellation requests sent to $sentCount of $queuedCount queued host attempts; " +
                            "the remaining attempts may have completed or could not accept cancellation."
                    }
                mutableNetworkState.value = mutableNetworkState.value.copy(message = message)
            } catch (cancellation: CancellationException) {
                throw cancellation
            } catch (exception: Exception) {
                mutableNetworkState.value =
                    mutableNetworkState.value.copy(
                        message = "Cancellation request failed: ${safeMessage(exception)}",
                    )
            } finally {
                mutableNetworkState.value = mutableNetworkState.value.copy(isCancellingRequest = false)
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

        val errorCode =
            when (response.outcome) {
                InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED -> null
                InferenceOutcome.INFERENCE_OUTCOME_FAILED -> RequestErrorCode.INTERNAL
                InferenceOutcome.INFERENCE_OUTCOME_CANCELLED -> RequestErrorCode.CANCELLED
                InferenceOutcome.INFERENCE_OUTCOME_TIMED_OUT -> RequestErrorCode.REQUEST_EXPIRED
                InferenceOutcome.INFERENCE_OUTCOME_REJECTED -> RequestErrorCode.VALIDATION_FAILED
                else -> RequestErrorCode.UNKNOWN
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
                errorMessage = errorCode?.userMessage.orEmpty(),
                completedAtUnixMs = response.completedAtUnixMs,
                receivedAtUnixMs = clockMillis(),
                errorCode = errorCode,
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
                errorMessage = failure.errorCode.userMessage,
                completedAtUnixMs = clockMillis(),
                receivedAtUnixMs = clockMillis(),
                errorCode = failure.errorCode,
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

    private fun safeMessage(exception: Exception): String = exception.toRequestErrorCode().userMessage

    override fun onCleared() {
        retryableRequest = null
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

/** Creates the production request ViewModel with the encrypted Room lifecycle store. */
class RequestViewModelFactory(
    private val lifecycleRepository: InferenceRequestLifecycleRepository,
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (!modelClass.isAssignableFrom(RequestViewModel::class.java)) {
            throw IllegalArgumentException("Unsupported ViewModel type: ${modelClass.name}")
        }
        @Suppress("UNCHECKED_CAST")
        return RequestViewModel(lifecycleRepository = lifecycleRepository) as T
    }
}
