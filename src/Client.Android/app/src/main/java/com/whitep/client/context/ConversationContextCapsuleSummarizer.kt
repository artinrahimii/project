package com.whitep.client.context

import kotlinx.coroutines.CancellationException
import java.util.Locale

data class ConversationContextSummaryMessage(
    val messageId: String,
    val authorRole: String,
    val content: String,
    val createdAtEpochMillis: Long,
)

data class ConversationContextSummaryRequest(
    val messages: List<ConversationContextSummaryMessage>,
    val existingCapsule: ConversationContextCapsule? = null,
    val maxSerializedBytes: Int = ConversationContextCapsule.DEFAULT_MAX_SERIALIZED_BYTES,
)

sealed interface ConversationContextCapsuleSummaryResult {
    data class Accepted(
        val capsule: ConversationContextCapsule,
        val serializedByteCount: Int,
        val sourceMessageCount: Int,
    ) : ConversationContextCapsuleSummaryResult

    data class Rejected(
        val reason: ConversationContextSummaryRejectionReason,
        val capsuleError: ConversationContextCapsuleError? = null,
        val serializedByteCount: Int? = null,
    ) : ConversationContextCapsuleSummaryResult
}

enum class ConversationContextSummaryRejectionReason {
    NO_USABLE_MESSAGES,
    INVALID_SIZE_LIMIT,
    PROVIDER_FAILURE,
    INVALID_CAPSULE,
    SIZE_LIMIT_EXCEEDED,
}

/** Replaceable strategy boundary; implementations receive local message data. */
fun interface ConversationContextCapsuleSummaryProvider {
    suspend fun summarize(request: ConversationContextSummaryRequest): ConversationContextCapsule
}

/**
 * Validates provider results through the current v1 codec before exposing them to persistence.
 * Unsupported schemas, malformed fields, and payloads over the configured UTF-8 cap are rejected.
 */
class SchemaValidatedConversationContextCapsuleSummarizer(
    private val provider: ConversationContextCapsuleSummaryProvider =
        ExtractiveConversationContextCapsuleSummaryProvider(),
) {
    suspend fun summarize(request: ConversationContextSummaryRequest): ConversationContextCapsuleSummaryResult {
        if (!ConversationContextCapsule.isValidConfiguredSizeLimit(request.maxSerializedBytes)) {
            return ConversationContextCapsuleSummaryResult.Rejected(
                reason = ConversationContextSummaryRejectionReason.INVALID_SIZE_LIMIT,
                capsuleError = ConversationContextCapsuleError.INVALID_SIZE_LIMIT,
            )
        }

        val messages =
            request.messages
                .filter { it.content.isNotBlank() }
                .sortedWith(
                    compareBy<ConversationContextSummaryMessage> { it.createdAtEpochMillis }
                        .thenBy { it.messageId },
                )
        if (messages.isEmpty() && request.existingCapsule == null) {
            return ConversationContextCapsuleSummaryResult.Rejected(
                reason = ConversationContextSummaryRejectionReason.NO_USABLE_MESSAGES,
            )
        }

        val candidate =
            try {
                if (messages.isEmpty()) {
                    requireNotNull(request.existingCapsule)
                } else {
                    provider.summarize(request.copy(messages = messages))
                }
            } catch (exception: CancellationException) {
                throw exception
            } catch (_: Exception) {
                return ConversationContextCapsuleSummaryResult.Rejected(
                    reason = ConversationContextSummaryRejectionReason.PROVIDER_FAILURE,
                )
            }

        val encoded = candidate.encode(request.maxSerializedBytes)
        val bytes = encoded.bytes
        if (!encoded.isValid || bytes == null) {
            val reason =
                if (encoded.error == ConversationContextCapsuleError.TOO_LARGE) {
                    ConversationContextSummaryRejectionReason.SIZE_LIMIT_EXCEEDED
                } else {
                    ConversationContextSummaryRejectionReason.INVALID_CAPSULE
                }
            return ConversationContextCapsuleSummaryResult.Rejected(
                reason = reason,
                capsuleError = encoded.error,
                serializedByteCount = encoded.byteCount,
            )
        }

        val decoded = ConversationContextCapsule.decode(bytes)
        if (!decoded.isValid || decoded.capsule != candidate) {
            return ConversationContextCapsuleSummaryResult.Rejected(
                reason = ConversationContextSummaryRejectionReason.INVALID_CAPSULE,
                capsuleError = decoded.error,
                serializedByteCount = decoded.byteCount,
            )
        }
        return ConversationContextCapsuleSummaryResult.Accepted(
            capsule = decoded.capsule,
            serializedByteCount = bytes.size,
            sourceMessageCount = messages.size,
        )
    }
}

/**
 * Conservative no-network default. It preserves existing user-entered fields and adds only fields
 * explicitly marked in local message text. The initial goal falls back to the latest user message.
 */
class ExtractiveConversationContextCapsuleSummaryProvider : ConversationContextCapsuleSummaryProvider {
    override suspend fun summarize(request: ConversationContextSummaryRequest): ConversationContextCapsule {
        val messages = request.messages.filter { it.content.isNotBlank() }
        require(messages.isNotEmpty()) { "At least one non-empty message is required" }

        val markedFields =
            messages.flatMap { message ->
                message.content
                    .lineSequence()
                    .mapNotNull(::parseMarkedField)
                    .toList()
            }
        val explicitGoal = markedFields.lastOrNull { it.category == FieldCategory.GOAL }?.value
        val existing = request.existingCapsule
        val latestUserMessage =
            messages.lastOrNull { it.authorRole.equals("user", ignoreCase = true) }
                ?: messages.last()
        val latestUnmarkedLine =
            latestUserMessage.content
                .lineSequence()
                .map { it.trim() }
                .firstOrNull { it.isNotEmpty() && parseMarkedField(it) == null }
        val goal =
            explicitGoal
                ?: existing?.goal?.takeIf { it.isNotBlank() }
                ?: latestUnmarkedLine
                ?: latestUserMessage.content
                    .trim()
                    .lineSequence()
                    .first { it.isNotEmpty() }

        return ConversationContextCapsule(
            schemaVersion = ConversationContextCapsule.CURRENT_SCHEMA_VERSION,
            goal = goal,
            keyFacts = mergeEntries(existing?.keyFacts.orEmpty(), markedFields.values(FieldCategory.KEY_FACT)),
            decisions = mergeEntries(existing?.decisions.orEmpty(), markedFields.values(FieldCategory.DECISION)),
            constraints = mergeEntries(existing?.constraints.orEmpty(), markedFields.values(FieldCategory.CONSTRAINT)),
            unresolvedItems =
                mergeEntries(
                    existing?.unresolvedItems.orEmpty(),
                    markedFields.values(FieldCategory.UNRESOLVED),
                ),
        )
    }

    private fun parseMarkedField(line: String): MarkedField? {
        val separator = line.indexOf(':')
        if (separator <= 0 || separator == line.lastIndex) return null
        val label = line.substring(0, separator).trim().lowercase(Locale.ROOT)
        val value = line.substring(separator + 1).trim().take(MAX_EXTRACTED_ITEM_CHARACTERS).trim()
        if (value.isBlank()) return null
        val category =
            when (label) {
                "goal" -> FieldCategory.GOAL
                "fact", "key fact", "key_fact" -> FieldCategory.KEY_FACT
                "decision" -> FieldCategory.DECISION
                "constraint" -> FieldCategory.CONSTRAINT
                "unresolved", "open item", "todo", "open question" -> FieldCategory.UNRESOLVED
                else -> return null
            }
        return MarkedField(category, value)
    }

    private fun List<MarkedField>.values(category: FieldCategory): List<String> =
        filter { it.category == category }.map { it.value }

    private fun mergeEntries(
        existing: List<String>,
        extracted: List<String>,
    ): List<String> {
        val previous = existing.filter { it.isNotBlank() }.distinctBy { it.lowercase(Locale.ROOT) }
        val previousKeys = previous.mapTo(mutableSetOf()) { it.lowercase(Locale.ROOT) }
        val remaining = (MAX_ITEMS_PER_FIELD - previous.size).coerceAtLeast(0)
        val additions =
            extracted
                .asSequence()
                .map { it.take(MAX_EXTRACTED_ITEM_CHARACTERS).trim() }
                .filter { it.isNotBlank() }
                .filter { it.lowercase(Locale.ROOT) !in previousKeys }
                .distinctBy { it.lowercase(Locale.ROOT) }
                .take(remaining)
                .toList()
        return previous + additions
    }

    private data class MarkedField(
        val category: FieldCategory,
        val value: String,
    )

    private enum class FieldCategory {
        GOAL,
        KEY_FACT,
        DECISION,
        CONSTRAINT,
        UNRESOLVED,
    }

    private companion object {
        const val MAX_EXTRACTED_ITEM_CHARACTERS = 240
        const val MAX_ITEMS_PER_FIELD = 8
    }
}
