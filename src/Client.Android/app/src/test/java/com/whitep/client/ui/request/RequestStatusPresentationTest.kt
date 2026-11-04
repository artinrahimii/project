package com.whitep.client.ui.request

import com.whitep.client.data.PersistedRequestStatus
import com.whitep.client.data.RequestDraftStatus
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class RequestStatusPresentationTest {
    @Test
    fun everyPersistedStatusHasAnExplicitUserFacingPresentation() {
        val expectedLabels =
            mapOf(
                PersistedRequestStatus.QUEUED to "Queued",
                PersistedRequestStatus.CONTACTING to "Contacting",
                PersistedRequestStatus.WAITING to "Waiting",
                PersistedRequestStatus.PARTIAL to "Partial",
                PersistedRequestStatus.COMPLETED to "Completed",
                PersistedRequestStatus.FAILED to "Failed",
                PersistedRequestStatus.CANCELLED to "Cancelled",
            )

        assertEquals(PersistedRequestStatus.values().toSet(), expectedLabels.keys)
        expectedLabels.forEach { (status, expectedLabel) ->
            val presentation = status.toPresentation()
            assertEquals(expectedLabel, presentation.label)
            assertFalse(presentation.detail.isBlank())
        }
        assertEquals(expectedLabels.size, expectedLabels.values.toSet().size)
        assertTrue(expectedLabels.values.none { it.isBlank() })
    }

    @Test
    fun liveNetworkAndInferenceStatusUsesStructuredStateWithoutFreeFormMessageContent() {
        val privateMessageContent = "PRIVATE_PROMPT_AND_INFERENCE_RESPONSE"
        val state =
            RequestUiState(
                prompt = privateMessageContent,
                status = RequestDraftStatus.TRANSPORT_UNAVAILABLE,
                networkStatusMessage = privateMessageContent,
                hostAvailabilityMessage = privateMessageContent,
                runStatus = RequestRunStatus.FAILED,
            )

        val presentation = state.toNetworkInferenceStatusPresentation()

        assertEquals("Transport unavailable.", presentation.networkStatus)
        assertEquals("Failed.", presentation.inferenceStatus)
        assertFalse(presentation.networkStatus.contains(privateMessageContent))
        assertFalse(presentation.inferenceStatus.contains(privateMessageContent))
    }

    @Test
    fun liveNetworkAndInferenceStatusReflectsPeerAndLifecycleState() {
        val state =
            RequestUiState(
                discoveredPeers =
                    listOf(
                        RequestPeerUiState("peer-one", "2030-01-01T00:00:00Z", isTrusted = true),
                        RequestPeerUiState("peer-two", "2030-01-01T00:00:00Z", isTrusted = false),
                    ),
                runStatus = RequestRunStatus.RUNNING,
            )

        val presentation = state.toNetworkInferenceStatusPresentation()

        assertEquals("Peers discovered: 2; trusted: 1.", presentation.networkStatus)
        assertEquals("Running.", presentation.inferenceStatus)
    }

}
