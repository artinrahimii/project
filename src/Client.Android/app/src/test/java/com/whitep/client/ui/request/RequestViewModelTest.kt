package com.whitep.client.ui.request

import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.inferenceResponse

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
            "Request not sent: no host transport yet; Android-to-host transport is not implemented.",
            viewModel.uiState.value.statusMessage,
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
        assertTrue(viewModel.uiState.value.hostResponses.isEmpty())
    }

    @Test
    fun clearsOnlyTheEphemeralResponseCards() {
        val result = response(ATTEMPT_A, HOST_A, "temporary answer")
        assertTrue(viewModel.recordValidatedResponse(REQUEST_ID, ATTEMPT_A, HOST_A, MODEL_ID, result))

        viewModel.clearHostResponses()

        assertTrue(viewModel.hostResponses.value.isEmpty())
        assertEquals("", viewModel.uiState.value.prompt)
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
}
