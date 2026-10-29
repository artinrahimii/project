package com.whitep.client.data

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.context.ConversationContextCapsuleError
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

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

        dao.upsert(
            ConversationContextCapsuleEntity(
                conversationId = conversationId,
                serializedCapsule = bytes,
                updatedAtEpochMillis = updatedAtEpochMillis,
            ),
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
