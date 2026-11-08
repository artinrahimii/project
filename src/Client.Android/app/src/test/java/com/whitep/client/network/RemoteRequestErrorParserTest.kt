package com.whitep.client.network

import com.whitep.client.errors.RequestErrorCode
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test
import org.whitep.shared.contracts.v1.HostBusy
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.ProtocolError
import org.whitep.shared.contracts.v1.ProtocolErrorCode
import org.whitep.shared.contracts.v1.UnsupportedModel

class RemoteRequestErrorParserTest {
    @Test
    fun parsesProtocolErrorByStableCodeAndIgnoresUntrustedMessageText() {
        val payload =
            ProtocolError
                .newBuilder()
                .setCode(ProtocolErrorCode.PROTOCOL_ERROR_CODE_RESOURCE_LIMIT)
                .setRequestId("request-resource")
                .setAttemptId("attempt-resource")
                .setRoundId("round-resource")
                .setMessage("C:\\private\\token.json")
                .setRetryable(false)
                .build()
                .toByteArray()

        val parsed = requireNotNull(RemoteRequestErrorParser.parse(payload))
        assertEquals("request-resource", parsed.requestId)
        assertEquals("attempt-resource", parsed.attemptId)
        assertEquals("round-resource", parsed.roundId)
        assertEquals(RequestErrorCode.RESOURCE_LIMIT, parsed.errorCode)
        assertNull(parsed.ephemeralHostId)
        assertNull(parsed.modelId)
    }

    @Test
    fun parsesHostBusyAndCarriesTheExpectedPeerIdentity() {
        val payload =
            HostBusy
                .newBuilder()
                .setRequestId("request-busy")
                .setAttemptId("attempt-busy")
                .setRoundId("round-busy")
                .setEphemeralHostId("12D3KooWPeerIdentity")
                .setRetryAfterUnixMs(1_800_000_000_000L)
                .setMessage("untrusted busy detail")
                .build()
                .toByteArray()

        val parsed = requireNotNull(RemoteRequestErrorParser.parse(payload))
        assertEquals("request-busy", parsed.requestId)
        assertEquals("attempt-busy", parsed.attemptId)
        assertEquals("round-busy", parsed.roundId)
        assertEquals(RequestErrorCode.HOST_BUSY, parsed.errorCode)
        assertEquals("12D3KooWPeerIdentity", parsed.ephemeralHostId)
    }

    @Test
    fun parsesUnsupportedModelWithoutSurfacingTheRemoteMessage() {
        val payload =
            UnsupportedModel
                .newBuilder()
                .setRequestId("request-model")
                .setAttemptId("attempt-model")
                .setRoundId("round-model")
                .setModelId("model/missing")
                .setMessage("private model server detail")
                .build()
                .toByteArray()

        val parsed = requireNotNull(RemoteRequestErrorParser.parse(payload))
        assertEquals("request-model", parsed.requestId)
        assertEquals("attempt-model", parsed.attemptId)
        assertEquals("round-model", parsed.roundId)
        assertEquals("model/missing", parsed.modelId)
        assertEquals(RequestErrorCode.MODEL_UNAVAILABLE, parsed.errorCode)
    }

    @Test
    fun ignoresTypedErrorsMissingAttemptOrRoundCorrelation() {
        val protocolError =
            ProtocolError
                .newBuilder()
                .setCode(ProtocolErrorCode.PROTOCOL_ERROR_CODE_HOST_BUSY)
                .setRequestId("request-only")
                .build()
                .toByteArray()
        assertNull(RemoteRequestErrorParser.parse(protocolError))

        val busy =
            HostBusy
                .newBuilder()
                .setRequestId("request-busy")
                .setEphemeralHostId("12D3KooWPeerIdentity")
                .setRetryAfterUnixMs(1_800_000_000_000L)
                .build()
                .toByteArray()
        assertNull(RemoteRequestErrorParser.parse(busy))

        val unsupported =
            UnsupportedModel
                .newBuilder()
                .setRequestId("request-model")
                .setModelId("model/missing")
                .build()
                .toByteArray()
        assertNull(RemoteRequestErrorParser.parse(unsupported))
    }

    @Test
    fun doesNotClassifyAValidInferenceResponseAsAnError() {
        val payload =
            InferenceResponse
                .newBuilder()
                .setRequestId("request-success")
                .setAttemptId("attempt-success")
                .setEphemeralHostId("12D3KooWPeerIdentity")
                .setOutcome(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED)
                .setOutputText("valid response")
                .setCompletedAtUnixMs(1_800_000_000_000L)
                .setModelId("model/valid")
                .setRoundId("round-success")
                .build()
                .toByteArray()

        assertNull(RemoteRequestErrorParser.parse(payload))
    }

    @Test
    fun ignoresMalformedOrUncorrelatedErrorMessages() {
        assertNull(RemoteRequestErrorParser.parse(byteArrayOf()))
        assertNull(RemoteRequestErrorParser.parse(byteArrayOf(0x7f)))
        val malformed =
            ProtocolError
                .newBuilder()
                .setCode(ProtocolErrorCode.PROTOCOL_ERROR_CODE_INVALID_MESSAGE)
                .build()
                .toByteArray()
        assertNull(RemoteRequestErrorParser.parse(malformed))
    }
}
