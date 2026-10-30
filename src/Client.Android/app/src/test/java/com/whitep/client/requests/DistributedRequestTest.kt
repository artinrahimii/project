package com.whitep.client.requests

import java.util.UUID
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class DistributedRequestTest {
    @Test
    fun preservesEphemeralIdDeadlineRoundLimitAndStatus() {
        val request =
            DistributedRequest(
                requestId = "ephemeral-request-001",
                deadlineUnixMs = 1_798_000_000_000L,
                maxRounds = 3,
                status = DistributedRequestStatus.WAITING,
            )

        assertEquals("ephemeral-request-001", request.requestId)
        assertEquals(1_798_000_000_000L, request.deadlineUnixMs)
        assertEquals(3, request.maxRounds)
        assertEquals(DistributedRequestStatus.WAITING, request.status)
    }

    @Test
    fun createGeneratesDistinctUuidRequestIdsAndStartsQueued() {
        val first = DistributedRequest.create(deadlineUnixMs = 1_798_000_000_000L, maxRounds = 2)
        val second = DistributedRequest.create(deadlineUnixMs = 1_798_000_000_000L, maxRounds = 2)

        assertNotEquals(first.requestId, second.requestId)
        assertEquals(first.requestId, UUID.fromString(first.requestId).toString())
        assertEquals(second.requestId, UUID.fromString(second.requestId).toString())
        assertEquals(DistributedRequestStatus.QUEUED, first.status)
        assertEquals(DistributedRequestStatus.QUEUED, second.status)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsBlankEphemeralRequestId() {
        DistributedRequest(requestId = "  ", deadlineUnixMs = 1_798_000_000_000L, maxRounds = 1)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNonPositiveDeadline() {
        DistributedRequest(requestId = "request-1", deadlineUnixMs = 0L, maxRounds = 1)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNonPositiveRoundLimit() {
        DistributedRequest(requestId = "request-1", deadlineUnixMs = 1_798_000_000_000L, maxRounds = 0)
    }

    @Test
    fun exposesTheSevenDefinedRequestLifecycleStates() {
        assertEquals(
            setOf("QUEUED", "CONTACTING", "WAITING", "PARTIAL", "COMPLETED", "FAILED", "CANCELLED"),
            DistributedRequestStatus.values().map { it.name }.toSet(),
        )
        assertTrue(DistributedRequestStatus.values().isNotEmpty())
    }
}
