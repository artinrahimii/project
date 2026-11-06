package com.whitep.client.requests

import java.time.Clock
import java.time.Duration
import java.time.Instant

/**
 * Starts one independently testable deadline window for each distributed inference round.
 * The effective deadline is the earlier of the configured round timeout and the request's
 * absolute UTC deadline. Each response in the same round consumes the same remaining budget.
 */
class InferenceRoundTimer(
    private val clock: Clock = Clock.systemUTC(),
    val timeout: Duration = DEFAULT_TIMEOUT,
) {
    /** An immutable round window whose remaining time is evaluated against the injected clock. */
    class Window internal constructor(
        val startedAt: Instant,
        val deadlineAt: Instant,
        private val clock: Clock,
    ) {
        fun remaining(): Duration {
            val remaining = Duration.between(clock.instant(), deadlineAt)
            return if (remaining.isNegative) Duration.ZERO else remaining
        }

        fun isExpired(): Boolean = remaining().isZero

        /** Rounds a positive fractional millisecond up so the timeout is not shortened early. */
        fun remainingTimeoutMillis(): Long {
            val duration = remaining()
            if (duration.isZero) {
                return 0L
            }
            return try {
                val wholeMillis = duration.toMillis()
                if (duration.minusMillis(wholeMillis).isZero) wholeMillis else Math.addExact(wholeMillis, 1L)
            } catch (_: ArithmeticException) {
                Long.MAX_VALUE
            }
        }
    }

    init {
        require(timeout > Duration.ZERO) { "The round timeout must be positive." }
    }

    fun startRound(requestDeadlineUnixMs: Long): Window {
        require(requestDeadlineUnixMs > 0L) { "The request deadline must be a positive Unix timestamp." }
        val startedAt = clock.instant()
        val configuredDeadline =
            try {
                startedAt.plus(timeout)
            } catch (exception: Exception) {
                throw IllegalArgumentException(
                    "The configured round timeout is outside the supported clock range.",
                    exception,
                )
            }
        val requestDeadline = Instant.ofEpochMilli(requestDeadlineUnixMs)
        return Window(startedAt, minOf(configuredDeadline, requestDeadline), clock)
    }

    fun boundedTimeout(
        window: Window,
        requestedTimeout: Duration,
    ): Duration {
        require(requestedTimeout > Duration.ZERO) { "The response timeout must be positive." }
        val remaining = window.remaining()
        return if (remaining < requestedTimeout) remaining else requestedTimeout
    }

    companion object {
        val DEFAULT_TIMEOUT: Duration = Duration.ofSeconds(60)
    }
}
