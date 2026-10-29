package com.whitep.client.ui.conversations

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.createSavedStateHandle
import androidx.lifecycle.viewmodel.CreationExtras
import com.whitep.client.data.MessageDao
import com.whitep.client.data.MessageEntity
import com.whitep.client.domain.ConversationMessageValidator
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.first
import java.util.UUID

/** Local conversation messages only; this ViewModel does not send content to a processing host. */
class ConversationMessagesViewModel(
    private val conversationId: String,
    private val messageDao: MessageDao,
    private val clockMillis: () -> Long = System::currentTimeMillis,
    private val messageIdGenerator: () -> String = { UUID.randomUUID().toString() },
    private val savedStateHandle: SavedStateHandle = SavedStateHandle(),
) : ViewModel() {
    val messages: Flow<List<MessageEntity>> = messageDao.observeForConversation(conversationId)
    val draft: StateFlow<String> = savedStateHandle.getStateFlow(DRAFT_STATE_KEY, "")

    fun updateDraft(value: String) {
        savedStateHandle[DRAFT_STATE_KEY] = value
    }

    suspend fun sendMessage(content: String): Boolean {
        val validation = ConversationMessageValidator.validate(content)
        val normalizedContent = validation.normalizedText ?: return false

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
        override fun <T : ViewModel> create(modelClass: Class<T>): T =
            createViewModel(
                modelClass = modelClass,
                savedStateHandle = SavedStateHandle(),
            )

        override fun <T : ViewModel> create(
            modelClass: Class<T>,
            extras: CreationExtras,
        ): T =
            createViewModel(
                modelClass = modelClass,
                savedStateHandle = extras.createSavedStateHandle(),
            )

        @Suppress("UNCHECKED_CAST")
        private fun <T : ViewModel> createViewModel(
            modelClass: Class<T>,
            savedStateHandle: SavedStateHandle,
        ): T {
            if (!modelClass.isAssignableFrom(ConversationMessagesViewModel::class.java)) {
                throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
            }
            return ConversationMessagesViewModel(
                conversationId = conversationId,
                messageDao = messageDao,
                savedStateHandle = savedStateHandle,
            ) as T
        }
    }

    internal companion object {
        const val DRAFT_STATE_KEY = "conversation_draft"
    }
}
