package com.whitep.client.network

import org.whitep.shared.contracts.v1.InferenceResponse

/**
 * Validates one response against the original request/attempt/round and authenticated peer.
 * Both the host-reported completion time and local receive time must be within the request deadline.
 */
internal object InferenceResponseCorrelationValidator {
    fun matches(
        response: InferenceResponse,
        expectedRequestId: String,
        expectedAttemptId: String,
        expectedRoundId: String,
        expectedHostId: String,
        expectedModelId: String,
        requestDeadlineUnixMs: Long,
        receivedAtUnixMs: Long,
    ): Boolean =
        expectedRequestId.isNotBlank() &&
            expectedAttemptId.isNotBlank() &&
            expectedRoundId.isNotBlank() &&
            expectedHostId.isNotBlank() &&
            expectedModelId.isNotBlank() &&
            requestDeadlineUnixMs > 0L &&
            receivedAtUnixMs > 0L &&
            response.requestId == expectedRequestId &&
            response.attemptId == expectedAttemptId &&
            response.roundId == expectedRoundId &&
            response.ephemeralHostId == expectedHostId &&
            response.modelId == expectedModelId &&
            response.completedAtUnixMs > 0L &&
            response.completedAtUnixMs <= requestDeadlineUnixMs &&
            receivedAtUnixMs <= requestDeadlineUnixMs
}
