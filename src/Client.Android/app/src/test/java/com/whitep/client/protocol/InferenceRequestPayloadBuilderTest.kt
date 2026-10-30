package com.whitep.client.protocol

import com.whitep.client.context.ConversationContextCapsule
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.InferenceRequest

class InferenceRequestPayloadBuilderTest {
    @Test
    fun payloadContainsOnlySelectedMessageAndEncodedCapsuleWithoutFullHistory() {
        val selectedMessage = "Compare the latest test results and identify the next fix."
        val unrelatedHistory =
            listOf(
                "Old message: switch the product to cloud inference.",
                "Old message: permanently identify this conversation as conversation-123.",
            )
        val capsule =
            ConversationContextCapsule(
                goal = "Ship local inference safely",
                keyFacts = listOf("Inference runs on the host machine"),
                constraints = listOf("Do not send full conversation history"),
            )
        val encodedCapsule = requireNotNull(capsule.encode().bytes)

        val result = build(currentMessage = selectedMessage, capsule = capsule)

        assertTrue(result is InferenceRequestPayloadBuildResult.Built)
        val request = (result as InferenceRequestPayloadBuildResult.Built).request
        assertEquals(selectedMessage, request.prompt)
        assertArrayEquals(encodedCapsule, request.contextCapsule.toByteArray())
        assertEquals("request-001", request.requestId)
        assertEquals("attempt-001", request.attemptId)
        assertEquals("local/model-1", request.modelId)
        assertEquals("1.0.0", request.protocolVersion)
        assertTrue(
            unrelatedHistory.none { oldMessage ->
                request.prompt.contains(oldMessage) ||
                    String(request.toByteArray(), Charsets.UTF_8).contains(oldMessage)
            },
        )
        val decoded = InferenceRequest.parseFrom(request.toByteArray())
        assertEquals(request, decoded)
        assertTrue(
            request.toByteArray().size <= InferenceRequestPayloadBuilder.MAX_SERIALIZED_REQUEST_BYTES,
        )
    }

    @Test
    fun rejectsPromptThatExceedsTheUtf8ByteLimit() {
        val tooLargePrompt = "世".repeat(InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES / 3 + 1)

        val result = build(currentMessage = tooLargePrompt)

        assertEquals(
            InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.PROMPT_TOO_LARGE,
            ),
            result,
        )
    }

    @Test
    fun rejectsInvalidCapsuleInsteadOfSendingAnEmptyFallback() {
        val result = build(capsule = ConversationContextCapsule(goal = " "))

        assertEquals(
            InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.INVALID_CONTEXT_CAPSULE,
            ),
            result,
        )
    }

    @Test
    fun rejectsSerializedRequestThatExceedsSixteenKiB() {
        val result = build(requestId = "r".repeat(InferenceRequestPayloadBuilder.MAX_SERIALIZED_REQUEST_BYTES))

        assertEquals(
            InferenceRequestPayloadBuildResult.Rejected(
                InferenceRequestPayloadRejection.SERIALIZED_REQUEST_TOO_LARGE,
            ),
            result,
        )
    }

    private fun build(
        requestId: String = "request-001",
        attemptId: String = "attempt-001",
        modelId: String = "local/model-1",
        currentMessage: String = "Current message only",
        capsule: ConversationContextCapsule =
            ConversationContextCapsule(goal = "Ship local inference safely"),
        deadlineUnixMs: Long = 4_102_444_800_000L,
        maxOutputTokens: Int = 128,
        protocolVersion: String = "1.0.0",
    ): InferenceRequestPayloadBuildResult =
        InferenceRequestPayloadBuilder.build(
            requestId = requestId,
            attemptId = attemptId,
            modelId = modelId,
            currentMessage = currentMessage,
            capsule = capsule,
            deadlineUnixMs = deadlineUnixMs,
            maxOutputTokens = maxOutputTokens,
            protocolVersion = protocolVersion,
        )
}
