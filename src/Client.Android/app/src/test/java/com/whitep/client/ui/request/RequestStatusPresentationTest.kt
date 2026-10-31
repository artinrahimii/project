package com.whitep.client.ui.request

import com.whitep.client.data.PersistedRequestStatus
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
}
