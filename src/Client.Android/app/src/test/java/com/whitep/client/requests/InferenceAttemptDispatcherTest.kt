package com.whitep.client.requests

import com.google.protobuf.ByteString
import com.whitep.client.context.ConversationContextCapsule
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.hostAnnouncement
import org.whitep.shared.contracts.v1.hostCapabilities
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.modelCapability

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class InferenceAttemptDispatcherTest {
    @Test
    fun dispatchesCorrelatableAttemptPerSelectedHost() =
        runBlocking {
            val candidates =
                HostCandidateSelector.select(
                    validRequest(),
                    listOf(candidate("host-c"), candidate("host-a"), candidate("host-b")),
                )
            val transport = RecordingTransport()
            val ids = listOf("attempt-a", "attempt-b", "attempt-c").iterator()
            val dispatcher =
                InferenceAttemptDispatcher(
                    transport = transport,
                    attemptIdGenerator = { ids.next() },
                )
            val capsule =
                ConversationContextCapsule(
                    goal = "Run one local inference request",
                    keyFacts = listOf("Only the selected message is sent"),
                )
            val distributedRequest =
                DistributedRequest(
                    requestId = "ephemeral-request-42",
                    deadlineUnixMs = 1_798_000_000_000L,
                    maxRounds = 3,
                )
    
            val result =
                dispatcher.dispatchRound(
                    distributedRequest = distributedRequest,
                    selectedHosts = candidates,
                    modelId = MODEL_ID,
                    currentMessage = "Use the current message",
                    capsule = capsule,
                    maxOutputTokens = 128,
                    protocolVersion = PROTOCOL_VERSION,
                )
    
            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            val dispatched = (result as InferenceRoundDispatchResult.Dispatched).attempts
            assertEquals(listOf("host-a", "host-b", "host-c"), transport.hostIds)
            assertEquals(3, dispatched.size)
            assertEquals(
                listOf("attempt-a", "attempt-b", "attempt-c"),
                dispatched.map { it.attemptId },
            )
            assertEquals(3, dispatched.map { it.attemptId }.toSet().size)
            assertEquals(
                listOf("ephemeral-request-42", "ephemeral-request-42", "ephemeral-request-42"),
                transport.requests.map { it.requestId },
            )
            assertEquals(dispatched.map { it.attemptId }, transport.requests.map { it.attemptId })
            assertTrue(transport.requests.all { it.prompt == "Use the current message" })
            val encodedCapsule = requireNotNull(capsule.encode().bytes)
            assertTrue(
                transport.requests.all {
                    it.contextCapsule == ByteString.copyFrom(encodedCapsule)
                },
            )
            assertTrue(dispatched.all { it.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED })
        }

    @Test
    fun rejectsDuplicateHostIdentityBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("same-host"), candidate("same-host")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.DUPLICATE_HOST_IDENTITY,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsMoreThanThreeHostsBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(
                        candidate("host-a"),
                        candidate("host-b"),
                        candidate("host-c"),
                        candidate("host-d"),
                    ),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.TOO_MANY_HOSTS,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsInvalidPayloadBeforeSubmittingAnyHost() =
        runBlocking {
            val transport = RecordingTransport()
            val result =
                dispatcher(transport).dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b")),
                    MODEL_ID,
                    " ",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertTrue(result is InferenceRoundDispatchResult.Rejected)
            assertEquals(
                InferenceRequestPayloadRejection.EMPTY_PROMPT,
                (result as InferenceRoundDispatchResult.Rejected).payloadRejection,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun rejectsTerminalRequestBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val request = distributedRequest(status = DistributedRequestStatus.CANCELLED)
            val result =
                dispatcher(transport).dispatchRound(
                    request,
                    listOf(candidate("host-a")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.TERMINAL_REQUEST,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    @Test
    fun keepsSubmittingOtherHostsAfterTransportRejectsOrThrows() =
        runBlocking {
            val transport =
                RecordingTransport(
                    submission = { hostId, _ ->
                        when (hostId) {
                            "host-a" -> InferenceTransportSubmission.REJECTED
                            "host-b" -> error("simulated transport fault")
                            else -> InferenceTransportSubmission.QUEUED
                        }
                    },
                )
            val attemptIds = listOf("attempt-a", "attempt-b", "attempt-c").iterator()
            val attemptDispatcher = dispatcher(transport, attemptIds::next)
            val result =
                attemptDispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b"), candidate("host-c")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertTrue(result is InferenceRoundDispatchResult.Dispatched)
            assertEquals(
                listOf(
                    InferenceAttemptSubmissionStatus.REJECTED,
                    InferenceAttemptSubmissionStatus.FAILED,
                    InferenceAttemptSubmissionStatus.QUEUED,
                ),
                (result as InferenceRoundDispatchResult.Dispatched).attempts.map { it.submissionStatus },
            )
            assertEquals(3, transport.requests.size)
        }

    @Test
    fun rejectsDuplicateAttemptIdsBeforeSubmitting() =
        runBlocking {
            val transport = RecordingTransport()
            val dispatcher =
                InferenceAttemptDispatcher(
                    transport = transport,
                    attemptIdGenerator = { "same-attempt" },
                )
            val result =
                dispatcher.dispatchRound(
                    distributedRequest(),
                    listOf(candidate("host-a"), candidate("host-b")),
                    MODEL_ID,
                    "message",
                    capsule(),
                    64,
                    PROTOCOL_VERSION,
                )
    
            assertEquals(
                InferenceRoundDispatchResult.Rejected(
                    InferenceRoundDispatchRejection.DUPLICATE_ATTEMPT_ID,
                ),
                result,
            )
            assertTrue(transport.requests.isEmpty())
        }

    private fun dispatcher(
        transport: InferenceRequestTransport,
        attemptIdGenerator: () -> String = { "attempt-one" },
    ): InferenceAttemptDispatcher = InferenceAttemptDispatcher(transport, attemptIdGenerator)

    private fun distributedRequest(status: DistributedRequestStatus = DistributedRequestStatus.QUEUED) =
        DistributedRequest(
            requestId = "ephemeral-request-42",
            deadlineUnixMs = 1_798_000_000_000L,
            maxRounds = 3,
            status = status,
        )

    private fun capsule() = ConversationContextCapsule(goal = "Run one local inference request")

    private fun validRequest() =
        inferenceRequest {
            requestId = "selector-request"
            attemptId = "selector-attempt"
            modelId = MODEL_ID
            prompt = "select compatible hosts"
            deadlineUnixMs = 1_798_000_000_000L
            maxOutputTokens = 128
            contextCapsule = ByteString.copyFrom(ByteArray(32) { 1 })
            protocolVersion = PROTOCOL_VERSION
        }

    private fun candidate(hostId: String): HostCandidate {
        val model =
            modelCapability {
                modelId = MODEL_ID
                displayName = "Test model"
                maxContextTokens = 8192L
                maxOutputTokens = 1024L
            }
        val capabilities =
            hostCapabilities {
                ephemeralHostId = hostId
                protocolVersion = PROTOCOL_VERSION
                models += model
                maxConcurrentRequests = 2
                maxContextCapsuleBytes = 4096L
                maxPromptBytes = 8192L
            }
        return HostCandidate(
            announcement =
                hostAnnouncement {
                    ephemeralHostId = hostId
                    protocolVersion = PROTOCOL_VERSION
                    this.capabilities = capabilities
                    announcedAtUnixMs = 1_798_000_000_000L
                },
            status =
                hostStatus {
                    ephemeralHostId = hostId
                    state = HostRuntimeState.HOST_RUNTIME_STATE_READY
                    activeRequests = 0
                    observedAtUnixMs = 1_798_000_000_000L
                    models += model
                },
        )
    }

    private class RecordingTransport(
        private val submission: suspend (String, InferenceRequest) -> InferenceTransportSubmission =
            { _, _ -> InferenceTransportSubmission.QUEUED },
    ) : InferenceRequestTransport {
        val hostIds = mutableListOf<String>()
        val requests = mutableListOf<InferenceRequest>()

        override suspend fun submit(
            ephemeralHostId: String,
            request: InferenceRequest,
        ): InferenceTransportSubmission {
            hostIds += ephemeralHostId
            requests += request
            return submission(ephemeralHostId, request)
        }
    }

    private companion object {
        const val MODEL_ID = "local/model-1"
        const val PROTOCOL_VERSION = "1.0.0"
    }
}
