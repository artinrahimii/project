package com.whitep.client.data

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/** Preparation state only; this repository does not deliver requests to a host. */
enum class RequestDraftStatus {
    NOT_SENT,
    EMPTY_PROMPT,
    TRANSPORT_UNAVAILABLE,
}

enum class RequestPreparationResult {
    EMPTY_PROMPT,
    TRANSPORT_UNAVAILABLE,
}

data class ClientRequestDraft(
    val prompt: String = "",
    val status: RequestDraftStatus = RequestDraftStatus.NOT_SENT,
)

/** Data boundary for the request UI; a Room-backed implementation can satisfy this contract later. */
interface ClientRequestRepository {
    val draft: Flow<ClientRequestDraft>

    suspend fun updatePrompt(prompt: String)

    suspend fun prepareRequest(): RequestPreparationResult

    suspend fun clearDraft()
}

/** Process-memory-only preview implementation; no database, file, socket, or network API is used. */
class InMemoryClientRequestRepository(
    initialDraft: ClientRequestDraft = ClientRequestDraft(),
) : ClientRequestRepository {
    private val mutableDraft = MutableStateFlow(initialDraft)
    override val draft: StateFlow<ClientRequestDraft> = mutableDraft.asStateFlow()

    override suspend fun updatePrompt(prompt: String) {
        mutableDraft.value = mutableDraft.value.copy(
            prompt = prompt,
            status = RequestDraftStatus.NOT_SENT,
        )
    }

    override suspend fun prepareRequest(): RequestPreparationResult {
        val current = mutableDraft.value
        val result =
            if (current.prompt.isBlank()) {
                mutableDraft.value = current.copy(status = RequestDraftStatus.EMPTY_PROMPT)
                RequestPreparationResult.EMPTY_PROMPT
            } else {
                mutableDraft.value = current.copy(status = RequestDraftStatus.TRANSPORT_UNAVAILABLE)
                RequestPreparationResult.TRANSPORT_UNAVAILABLE
            }
        return result
    }

    override suspend fun clearDraft() {
        mutableDraft.value = ClientRequestDraft()
    }
}
