package com.whitep.client.data

import androidx.room.Dao
import androidx.room.Entity
import androidx.room.ForeignKey
import androidx.room.PrimaryKey
import androidx.room.Query
import androidx.room.Upsert
import kotlinx.coroutines.flow.Flow

@Entity(
    tableName = "conversation_context_capsules",
    foreignKeys = [
        ForeignKey(
            entity = ConversationEntity::class,
            parentColumns = ["conversationId"],
            childColumns = ["conversationId"],
            onDelete = ForeignKey.CASCADE,
        ),
    ],
)
data class ConversationContextCapsuleEntity(
    @PrimaryKey val conversationId: String,
    val serializedCapsule: ByteArray,
    val updatedAtEpochMillis: Long,
)

@Dao
interface ConversationContextCapsuleDao {
    @Query(
        "SELECT * FROM conversation_context_capsules WHERE conversationId = :conversationId LIMIT 1",
    )
    suspend fun findByConversationId(conversationId: String): ConversationContextCapsuleEntity?

    @Query(
        "SELECT * FROM conversation_context_capsules WHERE conversationId = :conversationId LIMIT 1",
    )
    fun observeForConversation(conversationId: String): Flow<ConversationContextCapsuleEntity?>

    @Upsert
    suspend fun upsert(capsule: ConversationContextCapsuleEntity)

    @Query("DELETE FROM conversation_context_capsules WHERE conversationId = :conversationId")
    suspend fun deleteForConversation(conversationId: String): Int

    @Query("SELECT COUNT(*) FROM conversation_context_capsules WHERE conversationId = :conversationId")
    suspend fun countForConversation(conversationId: String): Int
}
