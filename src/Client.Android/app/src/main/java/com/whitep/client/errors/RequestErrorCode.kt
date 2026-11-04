package com.whitep.client.errors

import com.google.protobuf.InvalidProtocolBufferException
import kotlinx.coroutines.CancellationException
import org.whitep.shared.contracts.v1.ProtocolErrorCode
import java.io.FileNotFoundException
import java.io.IOException
import java.net.ConnectException
import java.net.NoRouteToHostException
import java.net.SocketException
import java.net.SocketTimeoutException
import java.net.UnknownHostException
import java.util.Locale
import java.util.concurrent.TimeoutException

/**
 * Stable, payload-free error categories shared by request cards, persisted request state,
 * local transport failures and the protocol's typed error messages.
 *
 * Never place exception text, server-supplied message text, paths, URLs or credentials in a code.
 */
enum class RequestErrorCode(
    val wireCode: String,
    val userMessage: String,
) {
    NETWORK_UNAVAILABLE(
        "NETWORK_UNAVAILABLE",
        "Could not reach the selected host or peer directory. Check the connection and try again.",
    ),
    MODEL_UNAVAILABLE(
        "MODEL_UNAVAILABLE",
        "The selected model is unavailable on this host. Choose an advertised model or check its installation.",
    ),
    RESOURCE_LIMIT(
        "RESOURCE_LIMIT",
        "The host cannot accept this request because a resource limit was reached. Try a smaller request or free resources.",
    ),
    VALIDATION_FAILED(
        "VALIDATION_FAILED",
        "The request or protocol data is invalid. Check the model, request limits and protocol version.",
    ),
    REQUEST_EXPIRED(
        "REQUEST_EXPIRED",
        "The request deadline expired before a result arrived. Prepare a new request.",
    ),
    HOST_BUSY(
        "HOST_BUSY",
        "The selected host is busy or temporarily unavailable. Try again when it is ready.",
    ),
    CANCELLED(
        "CANCELLED",
        "The request was cancelled.",
    ),
    INTERNAL(
        "INTERNAL",
        "The request could not be processed. Internal details are hidden.",
    ),
    UNKNOWN(
        "UNKNOWN",
        "The request failed for an unknown reason. Internal details are hidden.",
    ),
    ;

    companion object {
        fun fromWireCode(value: String?): RequestErrorCode =
            when (value?.trim()?.uppercase(Locale.ROOT)) {
                "HOST_UNAVAILABLE" -> NETWORK_UNAVAILABLE
                "TIMEOUT", "TIMED_OUT" -> REQUEST_EXPIRED
                else -> values().firstOrNull { it.wireCode == value?.trim()?.uppercase(Locale.ROOT) } ?: UNKNOWN
            }

        fun fromProtocolCode(code: ProtocolErrorCode): RequestErrorCode =
            when (code) {
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_INVALID_MESSAGE,
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION,
                -> VALIDATION_FAILED
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSUPPORTED_MODEL -> MODEL_UNAVAILABLE
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_HOST_BUSY -> HOST_BUSY
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_REQUEST_EXPIRED -> REQUEST_EXPIRED
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_RESOURCE_LIMIT -> RESOURCE_LIMIT
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_INTERNAL -> INTERNAL
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSPECIFIED,
                ProtocolErrorCode.UNRECOGNIZED,
                -> UNKNOWN
            }
    }
}

/** An exception that carries only a stable code; server or exception detail is intentionally omitted. */
class RequestExecutionException(
    val errorCode: RequestErrorCode,
    cause: Throwable? = null,
) : IOException(errorCode.userMessage, cause)

fun Throwable.toRequestErrorCode(): RequestErrorCode =
    when (this) {
        is RequestExecutionException -> errorCode
        is CancellationException -> RequestErrorCode.CANCELLED
        is SocketTimeoutException, is TimeoutException -> RequestErrorCode.REQUEST_EXPIRED
        is UnknownHostException,
        is ConnectException,
        is NoRouteToHostException,
        is SocketException,
        -> RequestErrorCode.NETWORK_UNAVAILABLE
        is FileNotFoundException -> RequestErrorCode.MODEL_UNAVAILABLE
        is OutOfMemoryError -> RequestErrorCode.RESOURCE_LIMIT
        is InvalidProtocolBufferException,
        is IllegalArgumentException,
        is SecurityException,
        -> RequestErrorCode.VALIDATION_FAILED
        is IOException -> RequestErrorCode.NETWORK_UNAVAILABLE
        else -> RequestErrorCode.INTERNAL
    }
