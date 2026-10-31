package com.whitep.client.ui.request

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.whitep.client.data.ClientRequestRepository
import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.combine
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse

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

data class RequestUiState(
    val prompt: String = "",
    val status: RequestDraftStatus = RequestDraftStatus.NOT_SENT,
    val hostResponses: List<HostResponseCardState> = emptyList(),
) {
    val statusMessage: String
        get() =
            when (status) {
                RequestDraftStatus.NOT_SENT -> "Nothing has been sent."
                RequestDraftStatus.EMPTY_PROMPT -> "Enter a prompt first."
                RequestDraftStatus.TRANSPORT_UNAVAILABLE ->
                    "Request not sent: no host transport yet; Android-to-host transport is not implemented."
            }
}

/**
 * Draft ViewModel plus the ephemeral result state consumed by the independent host-response cards.
 * Call recordValidatedResponse only with a response returned by
 * AndroidInferenceRequestRuntime.awaitResponse; that transport validates the round, authenticated
 * peer, request/attempt identifiers, model and both completion/receive deadlines before returning.
 */
class RequestViewModel(
    private val repository: ClientRequestRepository = InMemoryClientRequestRepository(),
    private val operationScope: CoroutineScope? = null,
    private val clockMillis: () -> Long = System::currentTimeMillis,
) : ViewModel() {
    private val commandsScope: CoroutineScope
        get() = operationScope ?: viewModelScope

    private val mutableHostResponses = MutableStateFlow<List<HostResponseCardState>>(emptyList())
    val hostResponses: StateFlow<List<HostResponseCardState>> = mutableHostResponses.asStateFlow()
    private val responseLock = Any()

    val uiState: StateFlow<RequestUiState> =
        combine(repository.draft, mutableHostResponses) { draft, responses ->
            RequestUiState(
                prompt = draft.prompt,
                status = draft.status,
                hostResponses = responses,
            )
        }.stateIn(
            scope = commandsScope,
            started = SharingStarted.Eagerly,
            initialValue = RequestUiState(),
        )

    fun updatePrompt(prompt: String) {
        commandsScope.launch { repository.updatePrompt(prompt) }
    }

    fun prepareRequest() {
        commandsScope.launch { repository.prepareRequest() }
    }

    fun clearDraft() {
        commandsScope.launch { repository.clearDraft() }
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

    fun clearHostResponses() {
        synchronized(responseLock) {
            mutableHostResponses.value = emptyList()
        }
    }
}
