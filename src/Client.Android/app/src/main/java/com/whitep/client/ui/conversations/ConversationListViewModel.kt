package com.whitep.client.ui.conversations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.whitep.client.data.ConversationDao
import com.whitep.client.data.ConversationEntity
import kotlinx.coroutines.flow.Flow

/** Exposes the persisted conversation list; mutations are implemented by the next task. */
class ConversationListViewModel(
    conversationDao: ConversationDao,
) : ViewModel() {
    val conversations: Flow<List<ConversationEntity>> = conversationDao.observeAll()

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
}
