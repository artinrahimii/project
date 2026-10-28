package com.whitep.client.ui.request

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.whitep.client.data.ClientRequestRepository
import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

data class RequestUiState(
    val prompt: String = "",
    val status: RequestDraftStatus = RequestDraftStatus.NOT_SENT,
) {
    val statusMessage: String
        get() = when (status) {
            RequestDraftStatus.NOT_SENT -> "Nothing has been sent."
            RequestDraftStatus.EMPTY_PROMPT -> "Enter a prompt first."
            RequestDraftStatus.TRANSPORT_UNAVAILABLE ->
                "Request not sent: no host transport yet; Android-to-host transport is not implemented."
        }
}

/** ViewModel for draft preparation only; request delivery is not implied. */
class RequestViewModel(
    private val repository: ClientRequestRepository = InMemoryClientRequestRepository(),
    private val operationScope: CoroutineScope? = null,
) : ViewModel() {
    private val commandsScope: CoroutineScope
        get() = operationScope ?: viewModelScope

    val uiState: StateFlow<RequestUiState> = repository.draft
        .map { draft -> RequestUiState(prompt = draft.prompt, status = draft.status) }
        .stateIn(
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
}
