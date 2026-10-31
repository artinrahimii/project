package com.whitep.client.data

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.context.ConversationContextCapsuleError
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import org.json.JSONArray

sealed interface ConversationContextCapsuleSaveResult {
    data class Saved(
        val serializedByteCount: Int,
        val updatedAtEpochMillis: Long,
    ) : ConversationContextCapsuleSaveResult

    data class Rejected(
        val error: ConversationContextCapsuleError,
        val byteCount: Int?,
    ) : ConversationContextCapsuleSaveResult
}

sealed interface ConversationContextCapsuleLoadResult {
    data class Found(
        val capsule: ConversationContextCapsule,
        val updatedAtEpochMillis: Long,
        val serializedByteCount: Int,
    ) : ConversationContextCapsuleLoadResult

    object Missing : ConversationContextCapsuleLoadResult

    data class InvalidStoredCapsule(
        val error: ConversationContextCapsuleError,
        val byteCount: Int,
    ) : ConversationContextCapsuleLoadResult
}

/** Persists validated v1 capsule bytes locally and scopes the row to one conversation. */
class RoomConversationContextCapsuleRepository(
    private val dao: ConversationContextCapsuleDao,
) {
    suspend fun save(
        conversationId: String,
        capsule: ConversationContextCapsule,
        updatedAtEpochMillis: Long,
        maxSerializedBytes: Int = ConversationContextCapsule.MAX_SERIALIZED_BYTES,
        source: ConversationContextCapsuleChangeSource = ConversationContextCapsuleChangeSource.USER_EDIT,
    ): ConversationContextCapsuleSaveResult {
        require(conversationId.isNotBlank()) { "conversationId must not be blank" }
        val encoded = capsule.encode(maxSerializedBytes)
        val bytes = encoded.bytes
        if (!encoded.isValid || bytes == null) {
            return ConversationContextCapsuleSaveResult.Rejected(
                error = encoded.error ?: ConversationContextCapsuleError.INVALID_FIELD_TYPE,
                byteCount = encoded.byteCount,
            )
        }

        val previousEntity = dao.findByConversationId(conversationId)
        val previousDecoded = previousEntity?.let { ConversationContextCapsule.decode(it.serializedCapsule) }
        val previousCapsule = previousDecoded?.takeIf { it.isValid }?.capsule
        val changedFields = changedFields(previousCapsule, capsule)
        val changedFieldsJson =
            JSONArray()
                .apply {
                    changedFields.forEach { put(it) }
                }.toString()
        val change =
            ConversationContextCapsuleChangeEntity(
                conversationId = conversationId,
                source = source.name,
                changedFieldsJson = changedFieldsJson,
                changedAtEpochMillis = updatedAtEpochMillis,
                schemaVersion = capsule.schemaVersion,
                serializedByteCount = bytes.size,
            )

        dao.saveWithChange(
            capsule =
                ConversationContextCapsuleEntity(
                    conversationId = conversationId,
                    serializedCapsule = bytes,
                    updatedAtEpochMillis = updatedAtEpochMillis,
                ),
            change = change,
        )
        return ConversationContextCapsuleSaveResult.Saved(
            serializedByteCount = bytes.size,
            updatedAtEpochMillis = updatedAtEpochMillis,
        )
    }

    suspend fun find(conversationId: String): ConversationContextCapsuleLoadResult =
        toLoadResult(dao.findByConversationId(conversationId))

    fun observe(conversationId: String): Flow<ConversationContextCapsuleLoadResult> =
        dao.observeForConversation(conversationId).map(::toLoadResult)

    suspend fun delete(conversationId: String): Int = dao.deleteForConversation(conversationId)

    suspend fun changeHistory(conversationId: String): List<ConversationContextCapsuleChangeEntity> =
        dao.changeHistoryForConversation(conversationId)

    private fun changedFields(
        previous: ConversationContextCapsule?,
        current: ConversationContextCapsule,
    ): List<String> =
        buildList {
            if (previous == null || previous.schemaVersion != current.schemaVersion) add("schemaVersion")
            if (previous == null || previous.goal != current.goal) add("goal")
            if (previous == null || previous.keyFacts != current.keyFacts) add("keyFacts")
            if (previous == null || previous.decisions != current.decisions) add("decisions")
            if (previous == null || previous.constraints != current.constraints) add("constraints")
            if (previous == null || previous.unresolvedItems != current.unresolvedItems) add("unresolvedItems")
        }

    private fun toLoadResult(entity: ConversationContextCapsuleEntity?): ConversationContextCapsuleLoadResult {
        if (entity == null) {
            return ConversationContextCapsuleLoadResult.Missing
        }
        val decoded = ConversationContextCapsule.decode(entity.serializedCapsule)
        val capsule = decoded.capsule
        if (!decoded.isValid || capsule == null) {
            return ConversationContextCapsuleLoadResult.InvalidStoredCapsule(
                error = decoded.error ?: ConversationContextCapsuleError.INVALID_FIELD_TYPE,
                byteCount = decoded.byteCount,
            )
        }
        return ConversationContextCapsuleLoadResult.Found(
            capsule = capsule,
            updatedAtEpochMillis = entity.updatedAtEpochMillis,
            serializedByteCount = decoded.byteCount,
        )
    }
}
