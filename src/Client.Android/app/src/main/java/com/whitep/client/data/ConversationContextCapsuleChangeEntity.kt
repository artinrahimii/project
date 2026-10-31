package com.whitep.client.data

import androidx.room.Entity
import androidx.room.ForeignKey
import androidx.room.Index
import androidx.room.PrimaryKey

enum class ConversationContextCapsuleChangeSource {
    USER_EDIT,
    LOCAL_HISTORY_SUMMARY,
    LOCAL_HISTORY_SUMMARY_REVIEWED,
}

@Entity(
    tableName = "conversation_context_capsule_changes",
    foreignKeys = [
        ForeignKey(
            entity = ConversationContextCapsuleEntity::class,
            parentColumns = ["conversationId"],
            childColumns = ["conversationId"],
            onDelete = ForeignKey.CASCADE,
        ),
    ],
    indices = [Index(value = ["conversationId", "changedAtEpochMillis"])],
)
data class ConversationContextCapsuleChangeEntity(
    @PrimaryKey(autoGenerate = true) val changeId: Long = 0,
    val conversationId: String,
    val source: String,
    val changedFieldsJson: String,
    val changedAtEpochMillis: Long,
    val schemaVersion: Int,
    val serializedByteCount: Int,
)
