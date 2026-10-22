package com.whitep.client.protocol

import com.google.protobuf.ByteString
import com.google.protobuf.MessageLite
import com.google.protobuf.Parser
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Test
import org.whitep.shared.contracts.v1.CancelRequest
import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.HostBusy
import org.whitep.shared.contracts.v1.HostCapabilities
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.HostStatusRequest
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.ModelCapability
import org.whitep.shared.contracts.v1.ProtocolError
import org.whitep.shared.contracts.v1.ProtocolErrorCode
import org.whitep.shared.contracts.v1.UnsupportedModel
import org.whitep.shared.contracts.v1.cancelRequest
import org.whitep.shared.contracts.v1.hostAnnouncement
import org.whitep.shared.contracts.v1.hostBusy
import org.whitep.shared.contracts.v1.hostCapabilities
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.hostStatusRequest
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.inferenceResponse
import org.whitep.shared.contracts.v1.modelCapability
import org.whitep.shared.contracts.v1.protocolError
import org.whitep.shared.contracts.v1.unsupportedModel

class ProtoRoundTripTest {
    private fun <T : MessageLite> assertRoundTrip(
        name: String,
        message: T,
        parser: Parser<T>,
    ) {
        val encoded = message.toByteArray()
        val decoded = parser.parseFrom(encoded)
        assertEquals("$name fields changed after Protobuf round-trip", message, decoded)
        assertArrayEquals("$name wire bytes changed after re-serialization", encoded, decoded.toByteArray())
    }

    @Test
    fun roundTripsEveryPrimaryProtocolMessage() {
        val model = modelCapability {
            modelId = "local/model-1"
            displayName = "Fixture model"
            maxContextTokens = 8192L
            maxOutputTokens = 1024L
            supportsStreaming = true
        }
        assertRoundTrip("ModelCapability", model, ModelCapability.parser())

        val capabilities = hostCapabilities {
            ephemeralHostId = "host-ephemeral-01"
            protocolVersion = "1.0.0"
            models += model
            maxConcurrentRequests = 1
            maxContextCapsuleBytes = 4096L
            maxPromptBytes = 8192L
        }
        assertRoundTrip("HostCapabilities", capabilities, HostCapabilities.parser())

        val announcement = hostAnnouncement {
            ephemeralHostId = "host-ephemeral-01"
            protocolVersion = "1.0.0"
            this.capabilities = capabilities
            announcedAtUnixMs = 1770000000123L
        }
        assertRoundTrip("HostAnnouncement", announcement, HostAnnouncement.parser())

        val request = inferenceRequest {
            requestId = "request-001"
            attemptId = "attempt-001"
            modelId = "local/model-1"
            prompt = "Hello, 世界"
            deadlineUnixMs = 1770000000999L
            maxOutputTokens = 128
            contextCapsule = ByteString.copyFrom(byteArrayOf(0, 1, 2, 255.toByte()))
        }
        assertRoundTrip("InferenceRequest", request, InferenceRequest.parser())

        val response = inferenceResponse {
            requestId = "request-001"
            attemptId = "attempt-001"
            ephemeralHostId = "host-ephemeral-01"
            outcome = InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
            outputText = "Generated output"
            completedAtUnixMs = 1770000000888L
            modelId = "local/model-1"
            errorMessage = ""
        }
        assertRoundTrip("InferenceResponse", response, InferenceResponse.parser())

        val cancel = cancelRequest {
            requestId = "request-001"
            attemptId = "attempt-001"
            reason = "caller cancelled"
        }
        assertRoundTrip("CancelRequest", cancel, CancelRequest.parser())

        val busy = hostBusy {
            requestId = "request-001"
            ephemeralHostId = "host-ephemeral-01"
            retryAfterUnixMs = 1770000001500L
            message = "Host is busy"
        }
        assertRoundTrip("HostBusy", busy, HostBusy.parser())

        val unsupported = unsupportedModel {
            requestId = "request-001"
            modelId = "missing/model"
            message = "Model is not installed"
        }
        assertRoundTrip("UnsupportedModel", unsupported, UnsupportedModel.parser())

        val statusRequest = hostStatusRequest {
            ephemeralHostId = "host-ephemeral-01"
        }
        assertRoundTrip("HostStatusRequest", statusRequest, HostStatusRequest.parser())

        val status = hostStatus {
            ephemeralHostId = "host-ephemeral-01"
            state = HostRuntimeState.HOST_RUNTIME_STATE_READY
            activeRequests = 1
            observedAtUnixMs = 1770000000222L
            models += model
        }
        assertRoundTrip("HostStatus", status, HostStatus.parser())

        val error = protocolError {
            code = ProtocolErrorCode.PROTOCOL_ERROR_CODE_UNSUPPORTED_VERSION
            message = "Unsupported protocol version"
            requestId = "request-001"
            retryable = false
        }
        assertRoundTrip("ProtocolError", error, ProtocolError.parser())
    }
}
