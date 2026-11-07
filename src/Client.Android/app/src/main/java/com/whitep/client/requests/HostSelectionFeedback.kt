package com.whitep.client.requests

/**
 * User-facing summary of the fresh, authenticated hosts selected for one request round.
 * Availability is based on post-probe candidates, never raw directory entries or cached status.
 */
data class HostSelectionFeedback(
    val availableHostCount: Int,
    val requestedHostCount: Int = MultiHostCompatibilityLimits.MAX_HOSTS_PER_ROUND,
) {
    init {
        require(requestedHostCount in 1..MultiHostCompatibilityLimits.MAX_HOSTS_PER_ROUND) {
            "requestedHostCount must be within the generic multi-host compatibility ceiling."
        }
        require(availableHostCount in 0..requestedHostCount) {
            "availableHostCount must be between zero and the requested host count."
        }
    }

    val missingHostCount: Int
        get() = requestedHostCount - availableHostCount

    val canContinue: Boolean
        get() = availableHostCount > 0

    val hasShortage: Boolean
        get() = availableHostCount < requestedHostCount

    val message: String
        get() =
            when {
                availableHostCount == 0 ->
                    "No compatible, reachable hosts are currently available. The request was not sent."
                hasShortage ->
                    "Only $availableHostCount of $requestedHostCount compatible, reachable hosts " +
                        "are currently available. Continuing with $availableHostCount host" +
                        (if (availableHostCount == 1) "" else "s") + "."
                else -> "All $requestedHostCount requested compatible, reachable hosts are available."
            }
}

/** Couples the actual candidate list with feedback computed from that exact list. */
data class HostSelectionResult(
    val candidates: List<HostCandidate>,
    val feedback: HostSelectionFeedback,
) {
    init {
        require(feedback.availableHostCount == candidates.size) {
            "Host selection feedback must describe the exact selected candidate count."
        }
        val hostIds = candidates.map { candidate -> candidate.announcement.ephemeralHostId }
        require(hostIds.none(String::isBlank) && hostIds.toSet().size == hostIds.size) {
            "A host selection result must contain distinct, nonblank peer identities."
        }
    }
}
