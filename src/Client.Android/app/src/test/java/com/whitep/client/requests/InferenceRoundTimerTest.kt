package com.whitep.client.requests

import java.time.Clock
import java.time.Duration
import java.time.Instant
import java.time.ZoneId
import java.time.ZoneOffset
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class InferenceRoundTimerTest {
    private val start = Instant.parse("2026-10-31T07:00:00Z")

    @Test
    fun configuredTimeoutUsesTheInjectableClock() {
        val clock = MutableClock(start)
        val timer = InferenceRoundTimer(clock, Duration.ofSeconds(30))
        val window = timer.startRound(start.plusSeconds(120).toEpochMilli())

        assertEquals(start, window.startedAt)
        assertEquals(start.plusSeconds(30), window.deadlineAt)
        assertEquals(Duration.ofSeconds(30), window.remaining())
        clock.advance(Duration.ofSeconds(12))
        assertEquals(Duration.ofSeconds(18), window.remaining())
        assertFalse(window.isExpired())
    }

    @Test
    fun earlierAbsoluteRequestDeadlineCapsTheRoundTimeout() {
        val clock = MutableClock(start)
        val timer = InferenceRoundTimer(clock, Duration.ofMinutes(2))
        val window = timer.startRound(start.plusSeconds(9).toEpochMilli())

        assertEquals(start.plusSeconds(9), window.deadlineAt)
        assertEquals(Duration.ofSeconds(9), window.remaining())
    }

    @Test
    fun expiredRequestDeadlineCreatesAnImmediatelyExpiredWindow() {
        val clock = MutableClock(start)
        val timer = InferenceRoundTimer(clock, Duration.ofSeconds(45))
        val window = timer.startRound(start.minusMillis(1).toEpochMilli())

        assertTrue(window.isExpired())
        assertEquals(Duration.ZERO, window.remaining())
        assertEquals(0L, window.remainingTimeoutMillis())
    }

    @Test
    fun remainingTimeoutIsCappedByTheCallerResponseTimeout() {
        val clock = MutableClock(start)
        val timer = InferenceRoundTimer(clock, Duration.ofSeconds(20))
        val window = timer.startRound(start.plusSeconds(60).toEpochMilli())
        clock.advance(Duration.ofSeconds(5))

        assertEquals(Duration.ofSeconds(8), timer.boundedTimeout(window, Duration.ofSeconds(8)))
        assertEquals(Duration.ofSeconds(15), timer.boundedTimeout(window, Duration.ofSeconds(60)))
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNonPositiveConfiguredTimeout() {
        InferenceRoundTimer(MutableClock(start), Duration.ZERO)
    }

    private class MutableClock(private var current: Instant) : Clock() {
        override fun getZone(): ZoneId = ZoneOffset.UTC
        override fun withZone(zone: ZoneId): Clock = this
        override fun instant(): Instant = current
        fun advance(duration: Duration) { current = current.plus(duration) }
    }
}
