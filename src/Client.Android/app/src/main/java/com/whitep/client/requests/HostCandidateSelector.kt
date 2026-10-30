package com.whitep.client.requests

import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.ModelCapability

/**
 * One peer's advertised capabilities paired with its observed runtime status.
 *
 * These values are snapshots supplied by a discovery/status layer. This type does not
 * establish a connection or claim that the peer is reachable.
 */
data class HostCandidate(
    val announcement: HostAnnouncement,
    val status: HostStatus,
)

/**
 * Deterministically selects at most three distinct, ready peers that can accept one
 * already-built inference request within their advertised and observed limits.
 *
 * The selector consumes the canonical InferenceRequest produced by the payload builder.
 * It does not perform discovery, network I/O, retries, deadline scheduling, or dispatch.
 */
object HostCandidateSelector {
    const val MAX_HOSTS_PER_ROUND = 3

    fun select(
        request: InferenceRequest,
        candidates: Collection<HostCandidate>,
    ): List<HostCandidate> {
        if (!isLocallyValidRequest(request)) {
            return emptyList()
        }

        // A repeated ephemeral identity is ambiguous; fail closed instead of choosing
        // one of several snapshots for the same peer.
        val uniqueCandidates =
            candidates
                .groupBy { candidate -> candidate.announcement.ephemeralHostId }
                .values
                .filter { sameIdentityCandidates -> sameIdentityCandidates.size == 1 }
                .map { sameIdentityCandidates -> sameIdentityCandidates.single() }

        return uniqueCandidates
            .asSequence()
            .filter { candidate -> isCompatibleAndReady(request, candidate) }
            .sortedBy { candidate -> candidate.announcement.ephemeralHostId }
            .take(MAX_HOSTS_PER_ROUND)
            .toList()
    }

    private fun isLocallyValidRequest(request: InferenceRequest): Boolean {
        val promptBytes = request.prompt.toByteArray(Charsets.UTF_8).size
        return request.requestId.isNotBlank() &&
            request.attemptId.isNotBlank() &&
            request.modelId.isNotBlank() &&
            request.prompt.isNotBlank() &&
            request.protocolVersion.isNotBlank() &&
            request.deadlineUnixMs > 0L &&
            request.maxOutputTokens > 0 &&
            promptBytes <= InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES &&
            request.contextCapsule.size() <= InferenceRequestPayloadBuilder.MAX_CONTEXT_CAPSULE_BYTES &&
            request.toByteArray().size <= InferenceRequestPayloadBuilder.MAX_SERIALIZED_REQUEST_BYTES
    }

    private fun isCompatibleAndReady(
        request: InferenceRequest,
        candidate: HostCandidate,
    ): Boolean {
        val announcement = candidate.announcement
        val capabilities = announcement.capabilities
        val status = candidate.status
        val hostId = announcement.ephemeralHostId

        if (hostId.isBlank() ||
            capabilities.ephemeralHostId != hostId ||
            status.ephemeralHostId != hostId
        ) {
            return false
        }

        if (announcement.protocolVersion != request.protocolVersion ||
            capabilities.protocolVersion != request.protocolVersion
        ) {
            return false
        }

        if (announcement.announcedAtUnixMs <= 0L ||
            status.observedAtUnixMs <= 0L ||
            status.state != HostRuntimeState.HOST_RUNTIME_STATE_READY
        ) {
            return false
        }

        if (capabilities.maxConcurrentRequests <= 0 ||
            status.activeRequests < 0 ||
            status.activeRequests >= capabilities.maxConcurrentRequests
        ) {
            return false
        }

        val promptBytes = request.prompt.toByteArray(Charsets.UTF_8).size.toLong()
        val capsuleBytes = request.contextCapsule.size().toLong()
        if (capabilities.maxPromptBytes <= 0L ||
            promptBytes > capabilities.maxPromptBytes ||
            capabilities.maxContextCapsuleBytes <= 0L ||
            capsuleBytes > capabilities.maxContextCapsuleBytes
        ) {
            return false
        }

        return supportsModel(capabilities.modelsList, request) &&
            supportsModel(status.modelsList, request)
    }

    private fun supportsModel(
        models: List<ModelCapability>,
        request: InferenceRequest,
    ): Boolean {
        val matchingModels = models.filter { model -> model.modelId == request.modelId }
        if (matchingModels.size != 1) {
            return false
        }

        val matchingModel = matchingModels.single()
        val supportedMaxOutputTokens = matchingModel.maxOutputTokens
        val requestedMaxOutputTokens = request.maxOutputTokens.toLong()
        return supportedMaxOutputTokens >= requestedMaxOutputTokens
    }
}
