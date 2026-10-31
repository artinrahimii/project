package com.whitep.client.context

import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationContextCapsuleSummarizerTest {
    @Test
    fun defaultProviderExtractsExplicitMarkersWithoutCopyingUnmarkedHistory() =
        runBlocking {
            val result =
                SchemaValidatedConversationContextCapsuleSummarizer().summarize(
                    ConversationContextSummaryRequest(
                        messages =
                            listOf(
                                summaryMessage("m1", "user", "Plan the first version", 1L),
                                summaryMessage("m2", "assistant", "The model runs on device", 2L),
                                summaryMessage(
                                    "m3",
                                    "user",
                                    "Build a local-first client\nFact: model stays on device\n" +
                                        "Decision: keep history on device\nConstraint: 1,024 bytes max\n" +
                                        "Unresolved: verify tablet layout\nUnmarked transcript should not be copied",
                                    3L,
                                ),
                            ),
                        maxSerializedBytes = 2_048,
                    ),
                )

            assertTrue(result is ConversationContextCapsuleSummaryResult.Accepted)
            val accepted = result as ConversationContextCapsuleSummaryResult.Accepted
            assertEquals("Build a local-first client", accepted.capsule.goal)
            assertEquals(listOf("model stays on device"), accepted.capsule.keyFacts)
            assertEquals(listOf("keep history on device"), accepted.capsule.decisions)
            assertEquals(listOf("1,024 bytes max"), accepted.capsule.constraints)
            assertEquals(listOf("verify tablet layout"), accepted.capsule.unresolvedItems)
            assertFalse(accepted.capsule.keyFacts.contains("Unmarked transcript should not be copied"))
            assertEquals(3, accepted.sourceMessageCount)
            assertTrue(accepted.serializedByteCount <= 2_048)
        }

    @Test
    fun invalidProviderSchemaIsRejectedByTheCodecBoundary() =
        runBlocking {
            val summarizer =
                SchemaValidatedConversationContextCapsuleSummarizer(
                    ConversationContextCapsuleSummaryProvider {
                        ConversationContextCapsule(
                            schemaVersion = 2,
                            goal = "Unsupported schema",
                        )
                    },
                )

            val result =
                summarizer.summarize(
                    ConversationContextSummaryRequest(
                        messages = listOf(summaryMessage("m1", "user", "Create a client", 1L)),
                    ),
                )

            assertTrue(result is ConversationContextCapsuleSummaryResult.Rejected)
            val rejected = result as ConversationContextCapsuleSummaryResult.Rejected
            assertEquals(ConversationContextSummaryRejectionReason.INVALID_CAPSULE, rejected.reason)
            assertEquals(ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION, rejected.capsuleError)
        }

    @Test
    fun invalidConfiguredLimitIsRejectedWithoutCallingProvider() =
        runBlocking {
            var providerCalled = false
            val summarizer =
                SchemaValidatedConversationContextCapsuleSummarizer(
                    ConversationContextCapsuleSummaryProvider {
                        providerCalled = true
                        ConversationContextCapsule(goal = "Must not be accepted")
                    },
                )

            val result =
                summarizer.summarize(
                    ConversationContextSummaryRequest(
                        messages = listOf(summaryMessage("m1", "user", "Create a client", 1L)),
                        maxSerializedBytes = 513,
                    ),
                )

            assertFalse(providerCalled)
            assertTrue(result is ConversationContextCapsuleSummaryResult.Rejected)
            assertEquals(
                ConversationContextSummaryRejectionReason.INVALID_SIZE_LIMIT,
                (result as ConversationContextCapsuleSummaryResult.Rejected).reason,
            )
        }

    @Test
    fun oversizeProviderOutputIsRejectedAtTheSelectedLimit() =
        runBlocking {
            val summarizer =
                SchemaValidatedConversationContextCapsuleSummarizer(
                    ConversationContextCapsuleSummaryProvider {
                        ConversationContextCapsule(goal = "x".repeat(700))
                    },
                )

            val result =
                summarizer.summarize(
                    ConversationContextSummaryRequest(
                        messages = listOf(summaryMessage("m1", "user", "Create a client", 1L)),
                        maxSerializedBytes = 512,
                    ),
                )

            assertTrue(result is ConversationContextCapsuleSummaryResult.Rejected)
            val rejected = result as ConversationContextCapsuleSummaryResult.Rejected
            assertEquals(ConversationContextSummaryRejectionReason.SIZE_LIMIT_EXCEEDED, rejected.reason)
            assertEquals(ConversationContextCapsuleError.TOO_LARGE, rejected.capsuleError)
            assertTrue((rejected.serializedByteCount ?: 0) > 512)
        }

    @Test
    fun existingCapsuleIsPreservedWhenAllInputMessagesAreBlank() =
        runBlocking {
            val existing =
                ConversationContextCapsule(
                    goal = "Manually edited goal",
                    keyFacts = listOf("Keep this approved fact"),
                )
            var providerCalled = false
            val summarizer =
                SchemaValidatedConversationContextCapsuleSummarizer(
                    ConversationContextCapsuleSummaryProvider {
                        providerCalled = true
                        error("Provider should not run without usable history")
                    },
                )

            val result =
                summarizer.summarize(
                    ConversationContextSummaryRequest(
                        messages = listOf(summaryMessage("empty", "user", "   ", 1L)),
                        existingCapsule = existing,
                        maxSerializedBytes = 2_048,
                    ),
                )

            assertFalse(providerCalled)
            assertTrue(result is ConversationContextCapsuleSummaryResult.Accepted)
            assertEquals(existing, (result as ConversationContextCapsuleSummaryResult.Accepted).capsule)
        }

    private fun summaryMessage(
        id: String,
        role: String,
        content: String,
        timestamp: Long,
    ) = ConversationContextSummaryMessage(
        messageId = id,
        authorRole = role,
        content = content,
        createdAtEpochMillis = timestamp,
    )
}
