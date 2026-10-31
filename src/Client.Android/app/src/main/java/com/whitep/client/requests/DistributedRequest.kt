package com.whitep.client.requests

import java.util.UUID

/**
 * Conservative first-release limits for one Android distributed inference request.
 * The task does not prescribe numeric ceilings; these values cap retry amplification while
 * retaining the existing three-host-per-round contract.
 */
object FirstReleaseInferenceLimits {
    const val MAX_ROUNDS = 3
    const val MAX_HOSTS_PER_ROUND = 3
    const val MAX_TOTAL_HOSTS = MAX_ROUNDS * MAX_HOSTS_PER_ROUND
}

/** Immutable process-local progress for one request's dispatch budget. */
data class DistributedRequestProgress(
    val roundsDispatched: Int = 0,
    val hostsAttempted: Int = 0,
) {
    init {
        require(roundsDispatched >= 0) { "roundsDispatched must not be negative" }
        require(hostsAttempted >= 0) { "hostsAttempted must not be negative" }
    }

    fun afterDispatch(hostCount: Int): DistributedRequestProgress {
        require(hostCount in 1..FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND) {
            "A dispatched round must contain one to three host attempts"
        }
        return copy(
            roundsDispatched = Math.addExact(roundsDispatched, 1),
            hostsAttempted = Math.addExact(hostsAttempted, hostCount),
        )
    }
}

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
    val maxTotalHosts: Int =
        minOf(maxRounds, FirstReleaseInferenceLimits.MAX_ROUNDS) * FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND,
    val status: DistributedRequestStatus = DistributedRequestStatus.QUEUED,
) {
    init {
        require(requestId.isNotBlank()) { "requestId must not be blank" }
        require(deadlineUnixMs > 0L) { "deadlineUnixMs must be a positive Unix timestamp" }
        require(maxRounds in 1..FirstReleaseInferenceLimits.MAX_ROUNDS) {
            "maxRounds must be between one and the first-release maximum"
        }
        require(maxTotalHosts in 1..FirstReleaseInferenceLimits.MAX_TOTAL_HOSTS) {
            "maxTotalHosts must be within the first-release maximum"
        }
        require(maxTotalHosts <= maxRounds * FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND) {
            "maxTotalHosts cannot exceed the configured round capacity"
        }
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
            maxTotalHosts: Int =
                minOf(
                    maxRounds,
                    FirstReleaseInferenceLimits.MAX_ROUNDS,
                ) * FirstReleaseInferenceLimits.MAX_HOSTS_PER_ROUND,
        ): DistributedRequest =
            DistributedRequest(
                requestId = UUID.randomUUID().toString(),
                deadlineUnixMs = deadlineUnixMs,
                maxRounds = maxRounds,
                maxTotalHosts = maxTotalHosts,
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
