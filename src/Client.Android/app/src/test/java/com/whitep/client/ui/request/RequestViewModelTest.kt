package com.whitep.client.ui.request

import com.whitep.client.data.InMemoryClientRequestRepository
import com.whitep.client.data.RequestDraftStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Test

class RequestViewModelTest {
    private val commandScope = CoroutineScope(SupervisorJob() + Dispatchers.Unconfined)
    private val viewModel =
        RequestViewModel(
            repository = InMemoryClientRequestRepository(),
            operationScope = commandScope,
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
}
