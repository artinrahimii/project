package com.whitep.client.requests

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class HostSelectionFeedbackTest {
    @Test
    fun zeroHostsNeverAllowsDispatchAndStatesThatTheRequestWasNotSent() {
        val feedback = HostSelectionFeedback(availableHostCount = 0)

        assertFalse(feedback.canContinue)
        assertTrue(feedback.hasShortage)
        assertEquals(3, feedback.missingHostCount)
        assertEquals(
            "No compatible, reachable hosts are currently available. The request was not sent.",
            feedback.message,
        )
    }

    @Test
    fun oneHostAllowsDispatchAndReportsTheExactShortfall() {
        val feedback = HostSelectionFeedback(availableHostCount = 1)

        assertTrue(feedback.canContinue)
        assertEquals(2, feedback.missingHostCount)
        assertEquals(
            "Only 1 of 3 compatible, reachable hosts are currently available. Continuing with 1 host.",
            feedback.message,
        )
    }

    @Test
    fun twoHostsAllowDispatchAndReportTheExactShortfall() {
        val feedback = HostSelectionFeedback(availableHostCount = 2)

        assertTrue(feedback.canContinue)
        assertEquals(1, feedback.missingHostCount)
        assertEquals(
            "Only 2 of 3 compatible, reachable hosts are currently available. Continuing with 2 hosts.",
            feedback.message,
        )
    }

    @Test
    fun threeHostsReportThatTheRequestedCapacityIsAvailable() {
        val feedback = HostSelectionFeedback(availableHostCount = 3)

        assertTrue(feedback.canContinue)
        assertFalse(feedback.hasShortage)
        assertEquals(0, feedback.missingHostCount)
        assertEquals(
            "All 3 requested compatible, reachable hosts are available.",
            feedback.message,
        )
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsAvailabilityAboveTheRequestedHostCount() {
        HostSelectionFeedback(availableHostCount = 4)
    }
}
