package com.whitep.client.data

import androidx.room.Dao
import androidx.room.Entity
import androidx.room.PrimaryKey
import androidx.room.Query
import androidx.room.Upsert
import kotlinx.coroutines.flow.Flow

@Entity(tableName = "request_drafts")
data class ClientRequestDraftEntity(
    @PrimaryKey val draftId: String,
    val prompt: String,
    val status: RequestDraftStatus,
)

@Dao
interface ClientRequestDraftDao {
    @Query("SELECT * FROM request_drafts WHERE draftId = :draftId LIMIT 1")
    fun observeById(draftId: String): Flow<ClientRequestDraftEntity?>

    @Query("SELECT * FROM request_drafts WHERE draftId = :draftId LIMIT 1")
    suspend fun findById(draftId: String): ClientRequestDraftEntity?

    @Upsert
    suspend fun upsert(draft: ClientRequestDraftEntity)

    @Query("DELETE FROM request_drafts WHERE draftId = :draftId")
    suspend fun deleteById(draftId: String): Int
}
