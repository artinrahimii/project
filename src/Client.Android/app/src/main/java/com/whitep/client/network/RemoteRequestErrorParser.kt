package com.whitep.client.network

import com.whitep.client.errors.RequestErrorCode
import org.whitep.shared.contracts.v1.HostBusy
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.ProtocolError
import org.whitep.shared.contracts.v1.UnsupportedModel

/** Error envelope metadata correlated to the authenticated peer and request, not server text. */
internal data class RemoteRequestError(
    val requestId: String,
    val errorCode: RequestErrorCode,
    val ephemeralHostId: String? = null,
    val modelId: String? = null,
)

/**
 * Parses the existing Protobuf error message variants used by the Windows peer mapper.
 * The untrusted message fields are deliberately ignored; only typed codes and correlation data
 * cross this boundary.
 */
internal object RemoteRequestErrorParser {
    fun parse(payload: ByteArray): RemoteRequestError? {
        if (payload.isEmpty()) return null

        // ProtocolError starts with field 1 (enum code), encoded as protobuf tag 0x08.
        if ((payload[0].toInt() and 0xff) == 0x08) {
            val protocolError = runCatching { ProtocolError.parseFrom(payload) }.getOrNull()
            if (protocolError != null && protocolError.requestId.isNotBlank()) {
                return RemoteRequestError(
                    requestId = protocolError.requestId,
                    errorCode = RequestErrorCode.fromProtocolCode(protocolError.code),
                )
            }
        }

        val busy = runCatching { HostBusy.parseFrom(payload) }.getOrNull()
        if (busy != null &&
            busy.requestId.isNotBlank() &&
            busy.ephemeralHostId.isNotBlank() &&
            busy.retryAfterUnixMs > 0L
        ) {
            return RemoteRequestError(
                requestId = busy.requestId,
                errorCode = RequestErrorCode.HOST_BUSY,
                ephemeralHostId = busy.ephemeralHostId,
            )
        }

        // UnsupportedModel shares three string fields with an incomplete InferenceResponse.
        // It is accepted only when the response's required outcome discriminator is absent.
        val possibleResponse = runCatching { InferenceResponse.parseFrom(payload) }.getOrNull()
        if (possibleResponse == null ||
            possibleResponse.outcome == InferenceOutcome.INFERENCE_OUTCOME_UNSPECIFIED
        ) {
            val unsupported = runCatching { UnsupportedModel.parseFrom(payload) }.getOrNull()
            if (unsupported != null &&
                unsupported.requestId.isNotBlank() &&
                unsupported.modelId.isNotBlank()
            ) {
                return RemoteRequestError(
                    requestId = unsupported.requestId,
                    errorCode = RequestErrorCode.MODEL_UNAVAILABLE,
                    modelId = unsupported.modelId,
                )
            }
        }

        // A valid InferenceResponse is not an error envelope.
        if (possibleResponse?.outcome != null &&
            possibleResponse.outcome != InferenceOutcome.INFERENCE_OUTCOME_UNSPECIFIED &&
            possibleResponse.outcome != InferenceOutcome.UNRECOGNIZED
        ) {
            return null
        }
        return null
    }
}
