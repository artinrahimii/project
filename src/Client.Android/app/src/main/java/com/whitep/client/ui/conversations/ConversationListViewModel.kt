package com.whitep.client.ui.conversations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.whitep.client.data.ConversationDao
import com.whitep.client.data.ConversationEntity
import kotlinx.coroutines.flow.Flow
import java.util.UUID

/** Exposes persisted conversations and their local lifecycle; these operations do not send requests. */
class ConversationListViewModel(
    private val conversationDao: ConversationDao,
    private val clockMillis: () -> Long = System::currentTimeMillis,
    private val conversationIdGenerator: () -> String = { UUID.randomUUID().toString() },
) : ViewModel() {
    val conversations: Flow<List<ConversationEntity>> = conversationDao.observeAll()

    suspend fun createConversation(title: String = DEFAULT_TITLE): ConversationEntity {
        val normalizedTitle = title.trim().ifEmpty { DEFAULT_TITLE }
        val now = clockMillis()
        val conversation =
            ConversationEntity(
                conversationId = conversationIdGenerator(),
                title = normalizedTitle,
                createdAtEpochMillis = now,
                updatedAtEpochMillis = now,
            )
        conversationDao.upsert(conversation)
        return conversation
    }

    suspend fun renameConversation(
        conversationId: String,
        title: String,
    ): Boolean {
        val normalizedTitle = title.trim()
        if (normalizedTitle.isEmpty()) return false

        val current = conversationDao.findById(conversationId) ?: return false
        conversationDao.upsert(
            current.copy(
                title = normalizedTitle,
                updatedAtEpochMillis = nextUpdatedAt(current.updatedAtEpochMillis),
            ),
        )
        return true
    }

    suspend fun deleteConversation(conversationId: String): Boolean {
        return conversationDao.deleteById(conversationId) > 0
    }

    suspend fun getConversation(conversationId: String): ConversationEntity? {
        return conversationDao.findById(conversationId)
    }

    private fun nextUpdatedAt(previous: Long): Long {
        val next = if (previous == Long.MAX_VALUE) previous else previous + 1
        return maxOf(clockMillis(), next)
    }

    class Factory(
        private val conversationDao: ConversationDao,
    ) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (!modelClass.isAssignableFrom(ConversationListViewModel::class.java)) {
                throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
            }
            return ConversationListViewModel(conversationDao) as T
        }
    }

    private companion object {
        const val DEFAULT_TITLE = "New conversation"
    }
}
