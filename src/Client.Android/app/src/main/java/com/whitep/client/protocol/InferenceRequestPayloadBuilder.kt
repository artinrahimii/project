package com.whitep.client.protocol

import com.google.protobuf.ByteString
import com.whitep.client.context.ConversationContextCapsule
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.inferenceRequest

enum class InferenceRequestPayloadRejection {
    EMPTY_REQUEST_ID,
    EMPTY_ATTEMPT_ID,
    EMPTY_MODEL_ID,
    EMPTY_PROMPT,
    PROMPT_TOO_LARGE,
    EMPTY_PROTOCOL_VERSION,
    INVALID_DEADLINE,
    INVALID_MAX_OUTPUT_TOKENS,
    INVALID_CONTEXT_CAPSULE,
    CONTEXT_CAPSULE_TOO_LARGE,
    SERIALIZED_REQUEST_TOO_LARGE,
}

sealed interface InferenceRequestPayloadBuildResult {
    data class Built(
        val request: InferenceRequest,
    ) : InferenceRequestPayloadBuildResult

    data class Rejected(
        val reason: InferenceRequestPayloadRejection,
    ) : InferenceRequestPayloadBuildResult
}

/**
 * Builds the exact Protobuf message intended for a future direct peer transport.
 *
 * Only the selected current message and a locally encoded context capsule are accepted as
 * content inputs. This does not open a socket or claim that Android-to-host delivery is wired.
 */
object InferenceRequestPayloadBuilder {
    const val MAX_PROMPT_BYTES = 8 * 1024
    const val MAX_CONTEXT_CAPSULE_BYTES = ConversationContextCapsule.MAX_SERIALIZED_BYTES
    const val MAX_SERIALIZED_REQUEST_BYTES = 16 * 1024

    fun build(
        requestId: String,
        attemptId: String,
        modelId: String,
        currentMessage: String,
        capsule: ConversationContextCapsule,
        deadlineUnixMs: Long,
        maxOutputTokens: Int,
        protocolVersion: String,
    ): InferenceRequestPayloadBuildResult {
        if (requestId.isBlank()) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.EMPTY_REQUEST_ID,
            )
        }
        if (attemptId.isBlank()) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.EMPTY_ATTEMPT_ID,
            )
        }
        if (modelId.isBlank()) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.EMPTY_MODEL_ID,
            )
        }
        if (currentMessage.isBlank()) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.EMPTY_PROMPT,
            )
        }
        val promptBytes = currentMessage.toByteArray(Charsets.UTF_8)
        if (promptBytes.size > MAX_PROMPT_BYTES) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.PROMPT_TOO_LARGE,
            )
        }
        if (protocolVersion.isBlank()) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.EMPTY_PROTOCOL_VERSION,
            )
        }
        if (deadlineUnixMs <= 0L) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.INVALID_DEADLINE,
            )
        }
        if (maxOutputTokens <= 0) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.INVALID_MAX_OUTPUT_TOKENS,
            )
        }

        val capsuleBytes =
            capsule.encode().bytes
                ?: return InferenceRequestPayloadBuildResult.Rejected(
                    InferenceRequestPayloadRejection.INVALID_CONTEXT_CAPSULE,
                )
        if (capsuleBytes.size > MAX_CONTEXT_CAPSULE_BYTES) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.CONTEXT_CAPSULE_TOO_LARGE,
            )
        }

        val request =
            inferenceRequest {
                this.requestId = requestId
                this.attemptId = attemptId
                this.modelId = modelId
                this.prompt = currentMessage
                this.deadlineUnixMs = deadlineUnixMs
                this.maxOutputTokens = maxOutputTokens
                this.contextCapsule = ByteString.copyFrom(capsuleBytes)
                this.protocolVersion = protocolVersion
            }
        if (request.toByteArray().size > MAX_SERIALIZED_REQUEST_BYTES) {
            return InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.SERIALIZED_REQUEST_TOO_LARGE,
            )
        }
        return InferenceRequestPayloadBuildResult.Built(request)
    }
}
