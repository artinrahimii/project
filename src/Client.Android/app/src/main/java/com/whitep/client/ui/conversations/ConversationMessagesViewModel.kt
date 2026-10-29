package com.whitep.client.ui.conversations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.whitep.client.data.MessageDao
import com.whitep.client.data.MessageEntity
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import java.util.UUID

/** Local conversation messages only; this ViewModel does not send content to a processing host. */
class ConversationMessagesViewModel(
    private val conversationId: String,
    private val messageDao: MessageDao,
    private val clockMillis: () -> Long = System::currentTimeMillis,
    private val messageIdGenerator: () -> String = { UUID.randomUUID().toString() },
) : ViewModel() {
    val messages: Flow<List<MessageEntity>> = messageDao.observeForConversation(conversationId)

    suspend fun sendMessage(content: String): Boolean {
        val normalizedContent = content.trim()
        if (normalizedContent.isEmpty()) return false

        val previousTimestamp = messages.first().lastOrNull()?.createdAtEpochMillis
        val nextTimestamp =
            previousTimestamp?.let { previous ->
                if (previous == Long.MAX_VALUE) previous else previous + 1
            }
        val createdAt = nextTimestamp?.let { maxOf(clockMillis(), it) } ?: clockMillis()
        messageDao.upsert(
            MessageEntity(
                messageId = messageIdGenerator(),
                conversationId = conversationId,
                authorRole = "user",
                content = normalizedContent,
                createdAtEpochMillis = createdAt,
            ),
        )
        return true
    }

    class Factory(
        private val conversationId: String,
        private val messageDao: MessageDao,
    ) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (!modelClass.isAssignableFrom(ConversationMessagesViewModel::class.java)) {
                throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
            }
            return ConversationMessagesViewModel(conversationId, messageDao) as T
        }
    }
}
