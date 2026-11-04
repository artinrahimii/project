package com.whitep.client.data

import androidx.room.Dao
import androidx.room.Entity
import androidx.room.Index
import androidx.room.PrimaryKey
import androidx.room.Query
import androidx.room.Transaction
import androidx.room.Upsert

/**
 * App-level request lifecycle metadata. This deliberately has no conversation foreign key:
 * the New request screen is independent from local conversation history.
 *
 * Prompts, context capsules, peer identifiers and response bodies must never be added here.
 */
@Entity(
    tableName = "client_inference_request_states",
    indices = [Index(value = ["updatedAtEpochMillis"])],
)
data class InferenceRequestStateEntity(
    @PrimaryKey val requestId: String,
    val status: PersistedRequestStatus,
    val createdAtEpochMillis: Long,
    val updatedAtEpochMillis: Long,
    val attemptNumber: Int,
    val errorCode: String? = null,
)

@Dao
interface InferenceRequestStateDao {
    @Query(
        "SELECT * FROM client_inference_request_states " +
            "ORDER BY updatedAtEpochMillis DESC, requestId DESC LIMIT 1",
    )
    suspend fun findLatest(): InferenceRequestStateEntity?

    @Upsert
    suspend fun upsert(state: InferenceRequestStateEntity)

    @Query(
        "UPDATE client_inference_request_states " +
            "SET status = 'EXPIRED', updatedAtEpochMillis = :nowEpochMillis, " +
            "errorCode = 'APP_CLOSED_BEFORE_RESULT' " +
            "WHERE status IN ('QUEUED', 'CONTACTING', 'WAITING')",
    )
    suspend fun expireInFlight(nowEpochMillis: Long): Int

    @Query(
        "DELETE FROM client_inference_request_states WHERE requestId NOT IN (" +
            "SELECT requestId FROM client_inference_request_states " +
            "ORDER BY updatedAtEpochMillis DESC, requestId DESC LIMIT :maximumRows)",
    )
    suspend fun retainMostRecent(maximumRows: Int)

    @Transaction
    suspend fun recoverLatestAfterProcessStart(nowEpochMillis: Long): InferenceRequestStateEntity? {
        expireInFlight(nowEpochMillis)
        return findLatest()
    }
}
