package com.whitep.client.requests

import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import java.util.UUID

class DistributedRequestTest {
    @Test
    fun preservesEphemeralIdDeadlineRoundLimitAndStatus() {
        val request =
            DistributedRequest(
                requestId = "ephemeral-request-001",
                deadlineUnixMs = 1_798_000_000_000L,
                maxRounds = 3,
                maxTotalHosts = 5,
                status = DistributedRequestStatus.WAITING,
            )

        assertEquals("ephemeral-request-001", request.requestId)
        assertEquals(1_798_000_000_000L, request.deadlineUnixMs)
        assertEquals(3, request.maxRounds)
        assertEquals(5, request.maxTotalHosts)
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
        assertEquals(6, first.maxTotalHosts)
        assertEquals(6, second.maxTotalHosts)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsBlankEphemeralRequestId() {
        DistributedRequest(requestId = "  ", deadlineUnixMs = 1_798_000_000_000L, maxRounds = 1)
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNonPositiveDeadline() {
        DistributedRequest(requestId = "request-1", deadlineUnixMs = 0L, maxRounds = 1)
    }

    @Test
    fun appliesTheThreeRoundAndNineHostFirstReleaseMaximums() {
        assertEquals(3, FirstReleaseInferenceLimits.MAX_ROUNDS)
        assertEquals(3, FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND)
        assertEquals(9, FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS)
        assertEquals(
            3,
            DistributedRequest.create(deadlineUnixMs = 1_798_000_000_000L, maxRounds = 1).maxTotalHosts,
        )
        assertEquals(
            9,
            DistributedRequest.create(deadlineUnixMs = 1_798_000_000_000L, maxRounds = 3).maxTotalHosts,
        )
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsMoreThanThreeRounds() {
        DistributedRequest(
            requestId = "request-1",
            deadlineUnixMs = 1_798_000_000_000L,
            maxRounds = 4,
        )
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsMoreThanNineTotalHosts() {
        DistributedRequest(
            requestId = "request-1",
            deadlineUnixMs = 1_798_000_000_000L,
            maxRounds = 3,
            maxTotalHosts = 10,
        )
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsTotalHostBudgetLargerThanRoundCapacity() {
        DistributedRequest(
            requestId = "request-1",
            deadlineUnixMs = 1_798_000_000_000L,
            maxRounds = 2,
            maxTotalHosts = 7,
        )
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
