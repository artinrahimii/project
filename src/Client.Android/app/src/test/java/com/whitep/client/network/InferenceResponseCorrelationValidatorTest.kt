package com.whitep.client.network

import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.inferenceResponse

class InferenceResponseCorrelationValidatorTest {
    @Test
    fun acceptsOnlyTheMatchingRequestAttemptRoundHostModelAndDeadline() {
        val response = validResponse()

        assertTrue(
            InferenceResponseCorrelationValidator.matches(
                response = response,
                expectedRequestId = REQUEST_ID,
                expectedAttemptId = ATTEMPT_ID,
                expectedRoundId = ROUND_ID,
                expectedHostId = HOST_ID,
                expectedModelId = MODEL_ID,
                requestDeadlineUnixMs = DEADLINE,
                receivedAtUnixMs = DEADLINE - 1L,
            ),
        )
    }

    @Test
    fun rejectsResponseFromAnotherRound() {
        val response = validResponse().toBuilder().setRoundId("round-old").build()

        assertFalse(matches(response, receivedAtUnixMs = DEADLINE - 1L))
    }

    @Test
    fun rejectsCompletionAfterRequestDeadline() {
        val response = validResponse().toBuilder().setCompletedAtUnixMs(DEADLINE + 1L).build()

        assertFalse(matches(response, receivedAtUnixMs = DEADLINE - 1L))
    }

    @Test
    fun rejectsResponseReceivedAfterRequestDeadlineEvenWhenCompletionTimestampIsEarlier() {
        assertFalse(matches(validResponse(), receivedAtUnixMs = DEADLINE + 1L))
    }

    private fun matches(
        response: InferenceResponse,
        receivedAtUnixMs: Long,
    ): Boolean =
        InferenceResponseCorrelationValidator.matches(
            response = response,
            expectedRequestId = REQUEST_ID,
            expectedAttemptId = ATTEMPT_ID,
            expectedRoundId = ROUND_ID,
            expectedHostId = HOST_ID,
            expectedModelId = MODEL_ID,
            requestDeadlineUnixMs = DEADLINE,
            receivedAtUnixMs = receivedAtUnixMs,
        )

    private fun validResponse(): InferenceResponse =
        inferenceResponse {
            requestId = REQUEST_ID
            attemptId = ATTEMPT_ID
            roundId = ROUND_ID
            ephemeralHostId = HOST_ID
            outcome = InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
            outputText = "verified output"
            completedAtUnixMs = DEADLINE - 2L
            modelId = MODEL_ID
            errorMessage = ""
        }

    private companion object {
        const val REQUEST_ID = "request-001"
        const val ATTEMPT_ID = "attempt-001"
        const val ROUND_ID = "round-001"
        const val HOST_ID = "host-001"
        const val MODEL_ID = "local/model-1"
        const val DEADLINE = 10_000L
    }
}
