package com.whitep.client.data

import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/**
 * Room-backed draft repository. Request preparation remains local-only until a host transport is implemented.
 */
class RoomClientRequestRepository(
    private val dao: ClientRequestDraftDao,
) : ClientRequestRepository {
    override val draft: Flow<ClientRequestDraft> =
        dao.observeById(DEFAULT_DRAFT_ID).map { entity ->
            entity?.let { ClientRequestDraft(prompt = it.prompt, status = it.status) }
                ?: ClientRequestDraft()
        }

    override suspend fun updatePrompt(prompt: String) {
        dao.upsert(
            ClientRequestDraftEntity(
                draftId = DEFAULT_DRAFT_ID,
                prompt = prompt,
                status = RequestDraftStatus.NOT_SENT,
            ),
        )
    }

    override suspend fun prepareRequest(): RequestPreparationResult {
        val current =
            dao.findById(DEFAULT_DRAFT_ID)
                ?: ClientRequestDraftEntity(
                    draftId = DEFAULT_DRAFT_ID,
                    prompt = "",
                    status = RequestDraftStatus.NOT_SENT,
                )
        val status =
            if (current.prompt.isBlank()) {
                RequestDraftStatus.EMPTY_PROMPT
            } else {
                RequestDraftStatus.TRANSPORT_UNAVAILABLE
            }
        dao.upsert(current.copy(status = status))
        return if (status == RequestDraftStatus.EMPTY_PROMPT) {
            RequestPreparationResult.EMPTY_PROMPT
        } else {
            RequestPreparationResult.TRANSPORT_UNAVAILABLE
        }
    }

    override suspend fun clearDraft() {
        dao.deleteById(DEFAULT_DRAFT_ID)
    }

    private companion object {
        const val DEFAULT_DRAFT_ID = "current"
    }
}
