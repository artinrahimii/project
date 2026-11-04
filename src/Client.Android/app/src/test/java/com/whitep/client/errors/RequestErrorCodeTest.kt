package com.whitep.client.errors

import java.io.FileNotFoundException
import java.net.SocketException
import java.net.UnknownHostException
import java.util.concurrent.TimeoutException
import kotlinx.coroutines.CancellationException
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Test
import org.whitep.shared.contracts.v1.ProtocolErrorCode

class RequestErrorCodeTest {
    @Test
    fun mapsEveryProtocolErrorCodeToAStableProductCategory() {
        assertEquals(
            RequestErrorCode.VALIDATION_FAILED,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_INVALID_MESSAGE,
            ),
        )
        assertEquals(
            RequestErrorCode.VALIDATION_FAILED,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION,
            ),
        )
        assertEquals(
            RequestErrorCode.MODEL_UNAVAILABLE,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSUPPORTED_MODEL,
            ),
        )
        assertEquals(
            RequestErrorCode.HOST_BUSY,
            RequestErrorCode.fromProtocolCode(ProtocolErrorCode.PROTOCOL_ERROR_CODE_HOST_BUSY),
        )
        assertEquals(
            RequestErrorCode.REQUEST_EXPIRED,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_REQUEST_EXPIRED,
            ),
        )
        assertEquals(
            RequestErrorCode.RESOURCE_LIMIT,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_RESOURCE_LIMIT,
            ),
        )
        assertEquals(
            RequestErrorCode.INTERNAL,
            RequestErrorCode.fromProtocolCode(ProtocolErrorCode.PROTOCOL_ERROR_CODE_INTERNAL),
        )
        assertEquals(
            RequestErrorCode.UNKNOWN,
            RequestErrorCode.fromProtocolCode(
                ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSPECIFIED,
            ),
        )
        assertEquals(
            RequestErrorCode.UNKNOWN,
            RequestErrorCode.fromProtocolCode(ProtocolErrorCode.UNRECOGNIZED),
        )
    }

    @Test
    fun keepsStableWireNamesAndReadsTheLegacyUnavailableAlias() {
        RequestErrorCode.values().forEach { code ->
            assertEquals(code, RequestErrorCode.fromWireCode(code.wireCode))
        }
        assertEquals(
            RequestErrorCode.NETWORK_UNAVAILABLE,
            RequestErrorCode.fromWireCode("HOST_UNAVAILABLE"),
        )
        assertEquals(
            RequestErrorCode.REQUEST_EXPIRED,
            RequestErrorCode.fromWireCode("TIMEOUT"),
        )
        assertEquals(
            RequestErrorCode.UNKNOWN,
            RequestErrorCode.fromWireCode("untrusted-server-code"),
        )
    }

    @Test
    fun classifiesLocalFailuresWithoutIncludingTheirPrivateDetails() {
        assertEquals(
            RequestErrorCode.NETWORK_UNAVAILABLE,
            UnknownHostException("secret.example").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.NETWORK_UNAVAILABLE,
            SocketException("secret socket detail").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.MODEL_UNAVAILABLE,
            FileNotFoundException("C:\\private\\model.gguf").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.REQUEST_EXPIRED,
            TimeoutException("secret deadline").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.VALIDATION_FAILED,
            IllegalArgumentException("secret payload").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.CANCELLED,
            CancellationException("secret cancellation").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.INTERNAL,
            Exception("C:\\private\\token.json").toRequestErrorCode(),
        )
        assertEquals(
            RequestErrorCode.RESOURCE_LIMIT,
            OutOfMemoryError("private memory detail").toRequestErrorCode(),
        )
        RequestErrorCode.values().forEach { code ->
            assertFalse(code.userMessage.contains("secret", ignoreCase = true))
            assertFalse(code.userMessage.contains("private", ignoreCase = true))
        }
    }

    @Test
    fun typedRemoteFailureRetainsItsCodeAndSafeMessage() {
        val exception =
            RequestExecutionException(
                RequestErrorCode.RESOURCE_LIMIT,
                IllegalStateException("private server detail"),
            )
        assertEquals(RequestErrorCode.RESOURCE_LIMIT, exception.toRequestErrorCode())
        assertEquals(RequestErrorCode.RESOURCE_LIMIT.userMessage, exception.message)
        assertFalse(exception.message.orEmpty().contains("private"))
    }
}
