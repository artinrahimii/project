package com.whitep.client.data

import androidx.room.Dao
import androidx.room.Entity
import androidx.room.ForeignKey
import androidx.room.Index
import androidx.room.PrimaryKey
import androidx.room.Query
import androidx.room.Upsert
import kotlinx.coroutines.flow.Flow

enum class PersistedRequestStatus {
    QUEUED,
    CONTACTING,
    WAITING,
    PARTIAL,
    COMPLETED,
    FAILED,
    CANCELLED,
}

@Entity(
    tableName = "request_states",
    foreignKeys = [
        ForeignKey(
            entity = ConversationEntity::class,
            parentColumns = ["conversationId"],
            childColumns = ["conversationId"],
            onDelete = ForeignKey.CASCADE,
        ),
    ],
    indices = [Index(value = ["conversationId", "updatedAtEpochMillis"])],
)
data class RequestStateEntity(
    @PrimaryKey val requestId: String,
    val conversationId: String,
    val status: PersistedRequestStatus,
    val createdAtEpochMillis: Long,
    val updatedAtEpochMillis: Long,
    val attemptNumber: Int = 0,
    val errorCode: String? = null,
)

@Dao
interface RequestStateDao {
    @Query(
        "SELECT * FROM request_states WHERE conversationId = :conversationId " +
            "ORDER BY createdAtEpochMillis ASC, requestId ASC",
    )
    fun observeForConversation(conversationId: String): Flow<List<RequestStateEntity>>

    @Query("SELECT * FROM request_states WHERE requestId = :requestId LIMIT 1")
    suspend fun findById(requestId: String): RequestStateEntity?

    @Upsert
    suspend fun upsert(state: RequestStateEntity)

    @Query("SELECT COUNT(*) FROM request_states WHERE conversationId = :conversationId")
    suspend fun countForConversation(conversationId: String): Int
}
