package com.whitep.client.requests

import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.async
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.withTimeout
import kotlinx.coroutines.yield
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.inferenceResponse
import java.time.Clock
import java.time.Duration
import java.time.Instant
import java.time.ZoneId
import java.time.ZoneOffset

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class InferenceRoundResponseCollectorTest {
    private val collector = InferenceRoundResponseCollector()

    @Test
    fun keepsWaitingForOtherHostsAfterTheFirstValidResponseArrives() =
        runBlocking {
            val clock = MutableClock(Instant.parse("2026-10-31T07:00:00Z"))
            val roundWindow =
                InferenceRoundTimer(clock, Duration.ofSeconds(30))
                    .startRound(clock.instant().plusSeconds(60).toEpochMilli())
            val firstResponse = CompletableDeferred<InferenceResponse>()
            val secondResponse = CompletableDeferred<InferenceResponse>()
            val bothWaitersStarted = CompletableDeferred<Unit>()
            val startedAttemptIds = mutableSetOf<String>()

            val collection =
                async {
                    collector.collect(
                        attempts = listOf(attempt("attempt-a", "host-a"), attempt("attempt-b", "host-b")),
                        roundWindowFor = { roundWindow },
                        awaitResponse = { hostAttempt, timeout ->
                            assertTrue(timeout > Duration.ZERO)
                            startedAttemptIds += hostAttempt.attemptId
                            if (startedAttemptIds.size == 2) {
                                bothWaitersStarted.complete(Unit)
                            }
                            when (hostAttempt.attemptId) {
                                "attempt-a" -> firstResponse.await()
                                else -> secondResponse.await()
                            }
                        },
                    )
                }

            withTimeout(5_000L) { bothWaitersStarted.await() }
            firstResponse.complete(response("attempt-a", "host-a", "first response"))
            yield()
            assertTrue("The second host must still be awaited.", collection.isActive)

            clock.advance(Duration.ofSeconds(5))
            secondResponse.complete(response("attempt-b", "host-b", "second response"))
            val result = withTimeout(5_000L) { collection.await() }

            assertEquals(listOf("attempt-a", "attempt-b"), result.responses.map { it.attemptId })
            assertEquals(
                listOf("first response", "second response"),
                result.responses.map { it.outputText },
            )
            assertTrue(result.failures.isEmpty())
        }

    @Test
    fun oneHostTimeoutDoesNotCancelASecondHostsValidResponse() =
        runBlocking {
            val roundWindow = newRoundWindow()
            val neverResponds = CompletableDeferred<InferenceResponse>()
            val secondResponse = CompletableDeferred<InferenceResponse>()
            val secondWaiterStarted = CompletableDeferred<Unit>()

            val collection =
                async {
                    collector.collect(
                        attempts = listOf(attempt("attempt-a", "host-a"), attempt("attempt-b", "host-b")),
                        roundWindowFor = { roundWindow },
                        awaitResponse = { hostAttempt, timeout ->
                            when (hostAttempt.attemptId) {
                                "attempt-a" -> withTimeout(20L) { neverResponds.await() }
                                else -> {
                                    assertTrue(timeout > Duration.ZERO)
                                    secondWaiterStarted.complete(Unit)
                                    secondResponse.await()
                                }
                            }
                        },
                    )
                }

            withTimeout(5_000L) { secondWaiterStarted.await() }
            secondResponse.complete(response("attempt-b", "host-b", "surviving response"))
            val result = withTimeout(5_000L) { collection.await() }

            assertEquals(listOf("attempt-b"), result.responses.map { it.attemptId })
            assertEquals(1, result.failures.size)
            assertEquals(
                InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT,
                result.failures.single().reason,
            )
        }

    @Test
    fun preservesSubmissionFailuresWithoutWaitingForUnqueuedHosts() =
        runBlocking {
            val awaitCalls = mutableListOf<String>()
            val attempts =
                listOf(
                    attempt("attempt-a", "host-a"),
                    attempt(
                        "attempt-b",
                        "host-b",
                        InferenceAttemptSubmissionStatus.REJECTED,
                    ),
                    attempt(
                        "attempt-c",
                        "host-c",
                        InferenceAttemptSubmissionStatus.FAILED,
                    ),
                    attempt(
                        "attempt-d",
                        "host-d",
                        InferenceAttemptSubmissionStatus.TIMED_OUT,
                    ),
                )

            val result =
                collector.collect(
                    attempts = attempts,
                    roundWindowFor = { newRoundWindow() },
                    awaitResponse = { hostAttempt, _ ->
                        awaitCalls += hostAttempt.attemptId
                        response(hostAttempt.attemptId, hostAttempt.ephemeralHostId, "accepted")
                    },
                )

            assertEquals(listOf("attempt-a"), awaitCalls)
            assertEquals(listOf("attempt-a"), result.responses.map { it.attemptId })
            assertEquals(
                listOf(
                    InferenceRoundResponseFailureReason.SUBMISSION_REJECTED,
                    InferenceRoundResponseFailureReason.SUBMISSION_FAILED,
                    InferenceRoundResponseFailureReason.SUBMISSION_TIMED_OUT,
                ),
                result.failures.map { it.reason },
            )
        }

    @Test
    fun doesNotStartAnAttemptWhenItsSharedRoundWindowHasExpired() =
        runBlocking {
            val clock = MutableClock(Instant.parse("2026-10-31T07:00:00Z"))
            val roundWindow =
                InferenceRoundTimer(clock, Duration.ofSeconds(30))
                    .startRound(clock.instant().plusSeconds(30).toEpochMilli())
            clock.advance(Duration.ofSeconds(30))
            var awaitCalled = false

            val result =
                collector.collect(
                    attempts = listOf(attempt("attempt-a", "host-a")),
                    roundWindowFor = { roundWindow },
                    awaitResponse = { _, _ ->
                        awaitCalled = true
                        response("attempt-a", "host-a", "too late")
                    },
                )

            assertFalse(awaitCalled)
            assertEquals(
                InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT,
                result.failures.single().reason,
            )
        }

    @Test
    fun callerCancellationIsNotMisreportedAsAnAttemptTimeout() =
        runBlocking {
            val waitStarted = CompletableDeferred<Unit>()
            val collection =
                async {
                    collector.collect(
                        attempts = listOf(attempt("attempt-a", "host-a")),
                        roundWindowFor = { newRoundWindow() },
                        awaitResponse = { _, _ ->
                            waitStarted.complete(Unit)
                            CompletableDeferred<InferenceResponse>().await()
                        },
                    )
                }

            withTimeout(5_000L) { waitStarted.await() }
            collection.cancel()
            var callerCancelled = false
            try {
                collection.await()
            } catch (_: kotlinx.coroutines.CancellationException) {
                callerCancelled = true
            }
            assertTrue(callerCancelled)
        }

    private fun newRoundWindow(): InferenceRoundTimer.Window {
        val clock = MutableClock(Instant.parse("2026-10-31T07:00:00Z"))
        return InferenceRoundTimer(clock, Duration.ofSeconds(30))
            .startRound(clock.instant().plusSeconds(60).toEpochMilli())
    }

    private fun attempt(
        attemptId: String,
        hostId: String,
        status: InferenceAttemptSubmissionStatus = InferenceAttemptSubmissionStatus.QUEUED,
    ) = InferenceHostAttempt(hostId, attemptId, status)

    private fun response(
        attemptId: String,
        hostId: String,
        output: String,
    ): InferenceResponse =
        inferenceResponse {
            requestId = REQUEST_ID
            this.attemptId = attemptId
            roundId = ROUND_ID
            ephemeralHostId = hostId
            modelId = MODEL_ID
            outcome = InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
            outputText = output
            completedAtUnixMs = COMPLETED_AT
        }

    private class MutableClock(
        private var current: Instant,
    ) : Clock() {
        override fun getZone(): ZoneId = ZoneOffset.UTC

        override fun withZone(zone: ZoneId): Clock = this

        override fun instant(): Instant = current

        fun advance(duration: Duration) {
            current = current.plus(duration)
        }
    }

    private companion object {
        const val REQUEST_ID = "request-001"
        const val ROUND_ID = "round-001"
        const val MODEL_ID = "local/model-1"
        const val COMPLETED_AT = 1_798_000_000_000L
    }
}
