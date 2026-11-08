package com.whitep.client.network

import com.whitep.client.errors.RequestErrorCode
import org.whitep.shared.contracts.v1.HostBusy
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.ProtocolError
import org.whitep.shared.contracts.v1.ProtocolErrorCode
import org.whitep.shared.contracts.v1.UnsupportedModel

/** Typed remote error bound to one exact logical request, attempt, dispatch round and authenticated peer. */
internal data class RemoteRequestError(
    val requestId: String,
    val attemptId: String,
    val roundId: String,
    val errorCode: RequestErrorCode,
    val ephemeralHostId: String? = null,
    val modelId: String? = null,
)

internal object RemoteRequestErrorParser {
    fun parse(payload: ByteArray): RemoteRequestError? {
        if (payload.isEmpty()) return null

        if ((payload[0].toInt() and 0xff) == 0x08) {
            val error = runCatching { ProtocolError.parseFrom(payload) }.getOrNull()
            if (error != null &&
                error.requestId.isNotBlank() &&
                error.attemptId.isNotBlank() &&
                error.roundId.isNotBlank() &&
                error.code != ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSPECIFIED &&
                error.code != ProtocolErrorCode.UNRECOGNIZED
            ) {
                return RemoteRequestError(
                    requestId = error.requestId,
                    attemptId = error.attemptId,
                    roundId = error.roundId,
                    errorCode = RequestErrorCode.fromProtocolCode(error.code),
                )
            }
        }

        val busy = runCatching { HostBusy.parseFrom(payload) }.getOrNull()
        if (busy != null &&
            busy.requestId.isNotBlank() &&
            busy.attemptId.isNotBlank() &&
            busy.roundId.isNotBlank() &&
            busy.ephemeralHostId.isNotBlank() &&
            busy.retryAfterUnixMs > 0L
        ) {
            return RemoteRequestError(
                requestId = busy.requestId,
                attemptId = busy.attemptId,
                roundId = busy.roundId,
                errorCode = RequestErrorCode.HOST_BUSY,
                ephemeralHostId = busy.ephemeralHostId,
            )
        }

        val possibleResponse = runCatching { InferenceResponse.parseFrom(payload) }.getOrNull()
        if (possibleResponse == null ||
            possibleResponse.outcome == InferenceOutcome.INFERENCE_OUTCOME_UNSPECIFIED
        ) {
            val unsupported = runCatching { UnsupportedModel.parseFrom(payload) }.getOrNull()
            if (unsupported != null &&
                unsupported.requestId.isNotBlank() &&
                unsupported.attemptId.isNotBlank() &&
                unsupported.roundId.isNotBlank() &&
                unsupported.modelId.isNotBlank()
            ) {
                return RemoteRequestError(
                    requestId = unsupported.requestId,
                    attemptId = unsupported.attemptId,
                    roundId = unsupported.roundId,
                    errorCode = RequestErrorCode.MODEL_UNAVAILABLE,
                    modelId = unsupported.modelId,
                )
            }
        }
        return null
    }
}
