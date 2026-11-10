package com.whitep.client.requests

import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.HostCapabilities
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.ModelCapability
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.modelCapability

class AutomaticModelSelectionPolicyTest {
    @Test
    fun rejectsNoReadyHostAndNoCatalogVerifiedModel() {
        val request = probeRequest()
        val noReadyHost =
            AutomaticModelSelectionPolicy.resolve(
                request,
                emptyList(),
            ) as AutomaticModelSelectionResult.Rejected
        assertEquals(AutomaticModelSelectionFailure.NO_READY_HOST, noReadyHost.failure)

        val noModel =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(candidate("peer-a", emptyList())),
            ) as AutomaticModelSelectionResult.Rejected
        assertEquals(AutomaticModelSelectionFailure.NO_VERIFIED_MODEL, noModel.failure)
    }

    @Test
    fun selectionAndOrdinalTieBreakAreDeterministicAcrossCandidateOrder() {
        val request = probeRequest()
        val candidates =
            listOf(
                candidate("peer-z", listOf(model("model-b", 16_384, 2_048))),
                candidate(
                    "peer-c",
                    listOf(
                        model("model-z", 32_768, 2_048),
                        model("model-a", 32_768, 2_048),
                    ),
                ),
                candidate("peer-a", listOf(model("model-a", 32_768, 2_048))),
            )
        val first =
            AutomaticModelSelectionPolicy.resolve(
                request,
                candidates,
            )
        val second =
            AutomaticModelSelectionPolicy.resolve(
                request,
                candidates.reversed(),
            )

        assertTrue(first is AutomaticModelSelectionResult.Selected)
        assertEquals(first, second)
        val result = first as AutomaticModelSelectionResult.Selected
        assertEquals("model-a", result.modelId)
        assertEquals("peer-a", result.hostId)
        assertEquals(1_024, result.maxOutputTokens)
    }

    @Test
    fun clampsOutputToThePositiveModelProductAndProtocolCeiling() {
        val request = probeRequest()
        val low =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(candidate("peer-low", listOf(model("model-low", 8_192, 128)))),
            ) as AutomaticModelSelectionResult.Selected
        assertEquals(128, low.maxOutputTokens)

        val high =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(candidate("peer-high", listOf(model("model-high", 8_192, 200_000)))),
            ) as AutomaticModelSelectionResult.Selected
        assertEquals(AutomaticModelSelectionPolicy.PREFERRED_OUTPUT_TOKENS, high.maxOutputTokens)
    }

    @Test
    fun rejectsBusyHostsAndInvalidOrDuplicateModelMetadata() {
        val request = probeRequest()
        val busy =
            candidate(
                peerId = "peer-busy",
                models = listOf(model("model-a", 8_192, 512)),
                state = HostRuntimeState.HOST_RUNTIME_STATE_BUSY,
            )
        val busyResult =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(busy),
            ) as AutomaticModelSelectionResult.Rejected
        assertEquals(AutomaticModelSelectionFailure.NO_READY_HOST, busyResult.failure)

        val invalid = candidate("peer-invalid", listOf(model(" ", 8_192, 512)))
        val invalidResult =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(invalid),
            ) as AutomaticModelSelectionResult.Rejected
        assertEquals(AutomaticModelSelectionFailure.INVALID_MODEL_METADATA, invalidResult.failure)

        val duplicate =
            candidate(
                "peer-duplicate",
                listOf(
                    model("model-a", 8_192, 512),
                    model("model-a", 16_384, 1_024),
                ),
            )
        val duplicateResult =
            AutomaticModelSelectionPolicy.resolve(
                request,
                listOf(duplicate),
            ) as AutomaticModelSelectionResult.Rejected
        assertEquals(AutomaticModelSelectionFailure.INVALID_MODEL_METADATA, duplicateResult.failure)
    }

    private fun probeRequest(): InferenceRequest =
        inferenceRequest {
            requestId = "selection-request"
            attemptId = "selection-attempt"
            roundId = "selection-round"
            modelId = "whitep-internal-model-selection-probe"
            prompt = "hello"
            deadlineUnixMs = 1_900_000_000_000L
            maxOutputTokens = 1
            protocolVersion = "1.0.0"
        }

    private fun model(
        id: String,
        context: Long,
        output: Long,
    ): ModelCapability =
        modelCapability {
            modelId = id
            displayName = id
            maxContextTokens = context
            maxOutputTokens = output
            supportsStreaming = false
        }

    private fun candidate(
        peerId: String,
        models: List<ModelCapability>,
        state: HostRuntimeState = HostRuntimeState.HOST_RUNTIME_STATE_READY,
    ): HostCandidate {
        val capabilities =
            HostCapabilities
                .newBuilder()
                .setEphemeralHostId(peerId)
                .setProtocolVersion("1.0.0")
                .addAllModels(models)
                .setMaxConcurrentRequests(1)
                .setMaxPromptBytes(8_192)
                .setMaxContextCapsuleBytes(4_096)
                .build()
        val announcement =
            HostAnnouncement
                .newBuilder()
                .setEphemeralHostId(peerId)
                .setProtocolVersion("1.0.0")
                .setCapabilities(capabilities)
                .setAnnouncedAtUnixMs(1_800_000_000_000L)
                .build()
        val status =
            hostStatus {
                ephemeralHostId = peerId
                this.state = state
                activeRequests = if (state == HostRuntimeState.HOST_RUNTIME_STATE_READY) 0 else 1
                observedAtUnixMs = 1_800_000_000_000L
                protocolVersion = "1.0.0"
                this.models += models
            }
        return HostCandidate(announcement, status)
    }
}
