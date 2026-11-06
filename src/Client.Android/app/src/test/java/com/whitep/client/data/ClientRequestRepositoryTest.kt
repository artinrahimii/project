package com.whitep.client.data

import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Test

class ClientRequestRepositoryTest {
    @Test
    fun blankPromptIsRejectedWithoutSending() =
        runBlocking {
            val repository = InMemoryClientRequestRepository()
            assertEquals(
                RequestPreparationResult.EMPTY_PROMPT,
                repository.prepareRequest(),
            )
            assertEquals(RequestDraftStatus.EMPTY_PROMPT, repository.draft.value.status)
        }

    @Test
    fun nonBlankPromptRemainsInMemoryAndIsNotSentWhenTransportIsUnavailable() =
        runBlocking {
            val repository = InMemoryClientRequestRepository()
            repository.updatePrompt("Check the local draft")
            assertEquals(
                RequestPreparationResult.TRANSPORT_UNAVAILABLE,
                repository.prepareRequest(),
            )
            assertEquals("Check the local draft", repository.draft.value.prompt)
            assertEquals(RequestDraftStatus.TRANSPORT_UNAVAILABLE, repository.draft.value.status)
        }

    @Test
    fun clearingTheDraftResetsPromptAndStatus() =
        runBlocking {
            val repository = InMemoryClientRequestRepository()
            repository.updatePrompt("temporary prompt")
            repository.prepareRequest()
            repository.clearDraft()
            assertEquals(ClientRequestDraft(), repository.draft.value)
        }
}
