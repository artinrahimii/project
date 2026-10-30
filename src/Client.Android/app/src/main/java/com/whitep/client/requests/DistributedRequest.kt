package com.whitep.client.requests

import java.util.UUID

/**
 * One short-lived distributed inference request owned by the Android client.
 *
 * [requestId] is an ephemeral identifier for this execution and maps directly to
 * InferenceRequest.request_id. It must not be reused as a conversation or user identifier.
 * [deadlineUnixMs] is an absolute UTC Unix timestamp in milliseconds.
 * [maxRounds] is the maximum number of host-selection rounds allowed for this request.
 */
data class DistributedRequest(
    val requestId: String,
    val deadlineUnixMs: Long,
    val maxRounds: Int,
    val status: DistributedRequestStatus = DistributedRequestStatus.QUEUED,
) {
    init {
        require(requestId.isNotBlank()) { "requestId must not be blank" }
        require(deadlineUnixMs > 0L) { "deadlineUnixMs must be a positive Unix timestamp" }
        require(maxRounds > 0) { "maxRounds must be positive" }
    }

    companion object {
        /**
         * Creates a request with a fresh UUID-backed request-scoped identifier.
         *
         * The caller supplies an absolute deadline and an explicit round limit so policy
         * and clock decisions remain outside this value object.
         */
        fun create(
            deadlineUnixMs: Long,
            maxRounds: Int,
        ): DistributedRequest =
            DistributedRequest(
                requestId = UUID.randomUUID().toString(),
                deadlineUnixMs = deadlineUnixMs,
                maxRounds = maxRounds,
            )
    }
}

/** Lifecycle values intentionally align with the existing persisted request-state vocabulary. */
enum class DistributedRequestStatus {
    QUEUED,
    CONTACTING,
    WAITING,
    PARTIAL,
    COMPLETED,
    FAILED,
    CANCELLED,
}
