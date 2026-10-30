package com.whitep.client.requests

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.protocol.InferenceRequestPayloadBuildResult
import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import org.whitep.shared.contracts.v1.InferenceRequest
import kotlinx.coroutines.CancellationException
import java.util.UUID

/**
 * Submission boundary for one selected-host round.
 *
 * An implementation must resolve the ephemeral host ID to an authenticated, usable peer and
 * submit the request through the real host transport. Returning QUEUED means only that the
 * injected transport accepted the submission locally; it does not prove remote receipt or
 * inference completion.
 */
fun interface InferenceRequestTransport {
    suspend fun submit(
        ephemeralHostId: String,
        request: InferenceRequest,
    ): InferenceTransportSubmission
}

enum class InferenceTransportSubmission {
    QUEUED,
    REJECTED,
}

enum class InferenceAttemptSubmissionStatus {
    QUEUED,
    REJECTED,
    FAILED,
}

data class InferenceHostAttempt(
    val ephemeralHostId: String,
    val attemptId: String,
    val submissionStatus: InferenceAttemptSubmissionStatus,
)

enum class InferenceRoundDispatchRejection {
    TERMINAL_REQUEST,
    NO_SELECTED_HOSTS,
    TOO_MANY_HOSTS,
    INVALID_HOST_IDENTITY,
    DUPLICATE_HOST_IDENTITY,
    EMPTY_ATTEMPT_ID,
    DUPLICATE_ATTEMPT_ID,
    PAYLOAD_REJECTED,
}

sealed interface InferenceRoundDispatchResult {
    data class Dispatched(
        val attempts: List<InferenceHostAttempt>,
    ) : InferenceRoundDispatchResult

    data class Rejected(
        val reason: InferenceRoundDispatchRejection,
        val payloadRejection: com.whitep.client.protocol.InferenceRequestPayloadRejection? = null,
    ) : InferenceRoundDispatchResult
}

/**
 * Builds one correlated InferenceRequest for each selected host and submits it via an injected
 * transport port. All payloads are validated before the first submit call, so a local payload
 * error cannot produce a partially submitted round.
 *
 * The request ID is shared by the round; each host submission receives a fresh attempt ID.
 * This class deliberately does not implement peer discovery, address resolution, authentication,
 * socket creation, timeout scheduling, retries, or remote-delivery confirmation.
 */
class InferenceAttemptDispatcher(
    private val transport: InferenceRequestTransport,
    private val attemptIdGenerator: () -> String = { UUID.randomUUID().toString() },
) {
    suspend fun dispatchRound(
        distributedRequest: DistributedRequest,
        selectedHosts: List<HostCandidate>,
        modelId: String,
        currentMessage: String,
        capsule: ConversationContextCapsule,
        maxOutputTokens: Int,
        protocolVersion: String,
    ): InferenceRoundDispatchResult {
        if (distributedRequest.status in TERMINAL_STATUSES) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.TERMINAL_REQUEST,
            )
        }
        if (selectedHosts.isEmpty()) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.NO_SELECTED_HOSTS,
            )
        }
        if (selectedHosts.size > HostCandidateSelector.MAX_HOSTS_PER_ROUND) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.TOO_MANY_HOSTS,
            )
        }

        val hostIds = selectedHosts.map { it.announcement.ephemeralHostId }
        if (selectedHosts.any { candidate ->
                val hostId = candidate.announcement.ephemeralHostId
                hostId.isBlank() ||
                    candidate.announcement.capabilities.ephemeralHostId != hostId ||
                    candidate.status.ephemeralHostId != hostId
            }
        ) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.INVALID_HOST_IDENTITY,
            )
        }
        if (hostIds.toSet().size != hostIds.size) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.DUPLICATE_HOST_IDENTITY,
            )
        }

        val attemptIds = selectedHosts.map { attemptIdGenerator() }
        if (attemptIds.any(String::isBlank)) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.EMPTY_ATTEMPT_ID,
            )
        }
        if (attemptIds.toSet().size != attemptIds.size) {
            return InferenceRoundDispatchResult.Rejected(
                InferenceRoundDispatchRejection.DUPLICATE_ATTEMPT_ID,
            )
        }

        // Prepare and validate every payload before sending any of them.
        val payloads = ArrayList<InferenceRequest>(selectedHosts.size)
        for (attemptId in attemptIds) {
            when (
                val result =
                    InferenceRequestPayloadBuilder.build(
                        requestId = distributedRequest.requestId,
                        attemptId = attemptId,
                        modelId = modelId,
                        currentMessage = currentMessage,
                        capsule = capsule,
                        deadlineUnixMs = distributedRequest.deadlineUnixMs,
                        maxOutputTokens = maxOutputTokens,
                        protocolVersion = protocolVersion,
                    )
            ) {
                is InferenceRequestPayloadBuildResult.Built -> payloads.add(result.request)
                is InferenceRequestPayloadBuildResult.Rejected ->
                    return InferenceRoundDispatchResult.Rejected(
                        reason = InferenceRoundDispatchRejection.PAYLOAD_REJECTED,
                        payloadRejection = result.reason,
                    )
            }
        }

        val attempts = ArrayList<InferenceHostAttempt>(selectedHosts.size)
        for (index in selectedHosts.indices) {
            val hostId = hostIds[index]
            val submissionStatus =
                try {
                    when (transport.submit(hostId, payloads[index])) {
                        InferenceTransportSubmission.QUEUED -> InferenceAttemptSubmissionStatus.QUEUED
                        InferenceTransportSubmission.REJECTED -> InferenceAttemptSubmissionStatus.REJECTED
                    }
                } catch (cancellation: CancellationException) {
                    throw cancellation
                } catch (_: Exception) {
                    InferenceAttemptSubmissionStatus.FAILED
                }
            attempts.add(
                InferenceHostAttempt(
                    ephemeralHostId = hostId,
                    attemptId = attemptIds[index],
                    submissionStatus = submissionStatus,
                ),
            )
        }
        return InferenceRoundDispatchResult.Dispatched(attempts)
    }

    private companion object {
        val TERMINAL_STATUSES =
            setOf(
                DistributedRequestStatus.COMPLETED,
                DistributedRequestStatus.FAILED,
                DistributedRequestStatus.CANCELLED,
            )
    }
}
