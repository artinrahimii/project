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
        excludedEphemeralHostIds: Set<String> = emptySet(),
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
            .filter { candidate -> candidate.announcement.ephemeralHostId !in excludedEphemeralHostIds }
            .filter { candidate -> isCompatibleAndReady(request, candidate) }
            .sortedBy { candidate -> candidate.announcement.ephemeralHostId }
            .take(MAX_HOSTS_PER_ROUND)
            .toList()
    }

    /**
     * Selects a new host batch only after the previous response window has completed.
     *
     * A valid response suppresses another round. When no valid response arrived, every host
     * attempted in that round is excluded, including rejected, failed, and timed-out submissions.
     * The completed collection must be the result of InferenceRoundResponseCollector.collect.
     */
    fun selectNextRound(
        request: InferenceRequest,
        candidates: Collection<HostCandidate>,
        previousRoundAttempts: List<InferenceHostAttempt>,
        previousRoundResponses: InferenceRoundResponseCollection,
    ): List<HostCandidate> {
        if (previousRoundResponses.responses.isNotEmpty() || previousRoundAttempts.isEmpty()) {
            return emptyList()
        }

        val previouslyAttemptedHostIds =
            previousRoundAttempts.map { attempt -> attempt.ephemeralHostId }
        if (previouslyAttemptedHostIds.any { hostId -> hostId.isBlank() } ||
            previouslyAttemptedHostIds.toSet().size != previouslyAttemptedHostIds.size
        ) {
            return emptyList()
        }

        return select(
            request = request,
            candidates = candidates,
            excludedEphemeralHostIds = previouslyAttemptedHostIds.toSet(),
        )
    }

    private fun isLocallyValidRequest(request: InferenceRequest): Boolean {
        val prompt = request.prompt
        val promptBytesArray = prompt.toByteArray(Charsets.UTF_8)
        val promptBytes = promptBytesArray.size
        val contextCapsule = request.contextCapsule
        val capsuleBytes = contextCapsule.size()
        val serializedRequest = request.toByteArray()
        val serializedRequestBytes = serializedRequest.size

        return request.requestId.isNotBlank() &&
            request.attemptId.isNotBlank() &&
            request.modelId.isNotBlank() &&
            prompt.isNotBlank() &&
            request.protocolVersion.isNotBlank() &&
            request.deadlineUnixMs > 0L &&
            request.maxOutputTokens > 0 &&
            promptBytes <= InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES &&
            capsuleBytes <= InferenceRequestPayloadBuilder.MAX_CONTEXT_CAPSULE_BYTES &&
            serializedRequestBytes <= InferenceRequestPayloadBuilder.MAX_SERIALIZED_REQUEST_BYTES
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

        val prompt = request.prompt
        val promptBytesArray = prompt.toByteArray(Charsets.UTF_8)
        val promptByteCount = promptBytesArray.size
        val promptBytes = promptByteCount.toLong()
        val contextCapsule = request.contextCapsule
        val capsuleByteCount = contextCapsule.size()
        val capsuleBytes = capsuleByteCount.toLong()
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
