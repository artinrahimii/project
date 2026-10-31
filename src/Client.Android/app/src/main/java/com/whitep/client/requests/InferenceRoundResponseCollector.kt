package com.whitep.client.requests

import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.TimeoutCancellationException
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.currentCoroutineContext
import kotlinx.coroutines.ensureActive
import org.whitep.shared.contracts.v1.InferenceResponse
import java.time.Duration
import java.util.concurrent.TimeoutException

/** Describes why a queued or submitted host attempt did not yield a valid response. */
enum class InferenceRoundResponseFailureReason {
    SUBMISSION_REJECTED,
    SUBMISSION_FAILED,
    SUBMISSION_TIMED_OUT,
    ROUND_WINDOW_UNAVAILABLE,
    RESPONSE_TIMED_OUT,
    RESPONSE_FAILED,
}

/** A failure is kept per host attempt; it never discards responses from other attempts. */
data class InferenceRoundResponseFailure(
    val ephemeralHostId: String,
    val attemptId: String,
    val reason: InferenceRoundResponseFailureReason,
)

/** Responses and failures are returned separately in the original dispatch-attempt order. */
data class InferenceRoundResponseCollection(
    val responses: List<InferenceResponse>,
    val failures: List<InferenceRoundResponseFailure>,
)

/**
 * Waits for every queued host attempt against the same round window.
 *
 * A successful first response does not complete the collection. Other queued attempts keep
 * waiting until they respond or their shared round deadline expires. Each attempt is isolated:
 * an error or timeout from one peer is recorded without cancelling sibling waiters.
 *
 * The supplied awaitResponse callback must return only a response already checked against the
 * authenticated peer and request/attempt/round/model/deadline correlation contract.
 */
internal class InferenceRoundResponseCollector {
    private sealed interface AttemptOutcome {
        data class Received(
            val response: InferenceResponse,
        ) : AttemptOutcome

        data class Failed(
            val failure: InferenceRoundResponseFailure,
        ) : AttemptOutcome
    }

    suspend fun collect(
        attempts: List<InferenceHostAttempt>,
        roundWindowFor: (InferenceHostAttempt) -> InferenceRoundTimer.Window?,
        awaitResponse: suspend (InferenceHostAttempt, Duration) -> InferenceResponse,
    ): InferenceRoundResponseCollection =
        coroutineScope {
            require(
                attempts.all { it.ephemeralHostId.isNotBlank() && it.attemptId.isNotBlank() },
            ) {
                "Every host attempt must have a non-blank host and attempt ID."
            }
            require(
                attempts.map { it.attemptId }.toSet().size == attempts.size,
            ) {
                "Host attempt IDs must be unique within a response collection."
            }

            val outcomes =
                attempts
                    .map { attempt ->
                    async {
                        when (attempt.submissionStatus) {
                            InferenceAttemptSubmissionStatus.REJECTED ->
                                AttemptOutcome.Failed(
                                    attempt.failure(InferenceRoundResponseFailureReason.SUBMISSION_REJECTED),
                                )

                            InferenceAttemptSubmissionStatus.FAILED ->
                                AttemptOutcome.Failed(
                                    attempt.failure(InferenceRoundResponseFailureReason.SUBMISSION_FAILED),
                                )

                            InferenceAttemptSubmissionStatus.TIMED_OUT ->
                                AttemptOutcome.Failed(
                                    attempt.failure(InferenceRoundResponseFailureReason.SUBMISSION_TIMED_OUT),
                                )

                            InferenceAttemptSubmissionStatus.QUEUED -> {
                                val roundWindow = roundWindowFor(attempt)
                                if (roundWindow == null) {
                                    AttemptOutcome.Failed(
                                        attempt.failure(InferenceRoundResponseFailureReason.ROUND_WINDOW_UNAVAILABLE),
                                    )
                                } else {
                                    val remaining = roundWindow.remaining()
                                    if (remaining.isZero) {
                                        AttemptOutcome.Failed(
                                            attempt.failure(InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT),
                                        )
                                    } else {
                                        try {
                                            AttemptOutcome.Received(awaitResponse(attempt, remaining))
                                        } catch (_: TimeoutCancellationException) {
                                            currentCoroutineContext().ensureActive()
                                            AttemptOutcome.Failed(
                                                attempt.failure(InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT),
                                            )
                                        } catch (_: TimeoutException) {
                                            AttemptOutcome.Failed(
                                                attempt.failure(InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT),
                                            )
                                        } catch (cancellation: CancellationException) {
                                            throw cancellation
                                        } catch (_: Exception) {
                                            AttemptOutcome.Failed(
                                                attempt.failure(InferenceRoundResponseFailureReason.RESPONSE_FAILED),
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }
                }.awaitAll()

            InferenceRoundResponseCollection(
                responses = outcomes.mapNotNull { (it as? AttemptOutcome.Received)?.response },
                failures = outcomes.mapNotNull { (it as? AttemptOutcome.Failed)?.failure },
            )
        }

    private fun InferenceHostAttempt.failure(
        reason: InferenceRoundResponseFailureReason,
    ): InferenceRoundResponseFailure =
        InferenceRoundResponseFailure(
            ephemeralHostId = ephemeralHostId,
            attemptId = attemptId,
            reason = reason,
        )
}
