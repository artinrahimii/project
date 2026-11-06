package com.whitep.client.ui.conversations

import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.PersistedRequestStatus
import com.whitep.client.data.RequestStateEntity
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationWorkflowAcceptanceTest {
    @Test
    fun createsConversationSendsMessagesAndRestoresHistoryAcrossDatabaseReopen() =
        runBlocking {
            val application = RuntimeEnvironment.getApplication()
            val databaseName = "t0150-conversation-workflow.db"
            val conversationId = "conversation-workflow"
            application.deleteDatabase(databaseName)

            try {
                val firstDatabase =
                    Room
                        .databaseBuilder(
                            application,
                            ClientDatabase::class.java,
                            databaseName,
                        ).allowMainThreadQueries()
                        .build()
                try {
                    val conversationListViewModel =
                        ConversationListViewModel(
                            conversationDao = firstDatabase.conversationDao(),
                            clockMillis = { 100L },
                            conversationIdGenerator = { conversationId },
                        )
                    val createdConversation =
                        conversationListViewModel.createConversation("  Integration conversation  ")

                    assertEquals("Integration conversation", createdConversation.title)
                    assertEquals(
                        listOf(conversationId),
                        conversationListViewModel.conversations.first().map { it.conversationId },
                    )
                    assertEquals(
                        "Integration conversation",
                        conversationListViewModel.getConversation(conversationId)?.title,
                    )

                    var messageSequence = 0
                    var clockMillis = 200L
                    val messagesViewModel =
                        ConversationMessagesViewModel(
                            conversationId = conversationId,
                            messageDao = firstDatabase.messageDao(),
                            clockMillis = { clockMillis++ },
                            messageIdGenerator = { "message-${++messageSequence}" },
                        )
                    assertTrue(messagesViewModel.sendMessage("First integrated message"))
                    assertTrue(messagesViewModel.sendMessage("Second integrated message"))
                    assertEquals(
                        listOf("First integrated message", "Second integrated message"),
                        messagesViewModel.messages.first().map { it.content },
                    )

                    firstDatabase.requestStateDao().upsert(
                        RequestStateEntity(
                            requestId = "request-workflow",
                            conversationId = conversationId,
                            status = PersistedRequestStatus.WAITING,
                            createdAtEpochMillis = 210L,
                            updatedAtEpochMillis = 211L,
                            attemptNumber = 1,
                        ),
                    )
                } finally {
                    firstDatabase.close()
                }

                val reopenedDatabase =
                    Room
                        .databaseBuilder(
                            application,
                            ClientDatabase::class.java,
                            databaseName,
                        ).allowMainThreadQueries()
                        .build()
                try {
                    val restoredConversationListViewModel =
                        ConversationListViewModel(reopenedDatabase.conversationDao())
                    assertEquals(
                        listOf(conversationId),
                        restoredConversationListViewModel.conversations.first().map { it.conversationId },
                    )
                    val restoredConversation = restoredConversationListViewModel.getConversation(conversationId)
                    assertNotNull(restoredConversation)
                    assertEquals("Integration conversation", restoredConversation?.title)

                    val restoredMessagesViewModel =
                        ConversationMessagesViewModel(
                            conversationId = conversationId,
                            messageDao = reopenedDatabase.messageDao(),
                        )
                    val restoredMessages = restoredMessagesViewModel.messages.first()
                    assertEquals(
                        listOf("message-1", "message-2"),
                        restoredMessages.map { it.messageId },
                    )
                    assertEquals(
                        listOf("First integrated message", "Second integrated message"),
                        restoredMessages.map { it.content },
                    )

                    val restoredRequestStates =
                        reopenedDatabase
                            .requestStateDao()
                            .observeForConversation(conversationId)
                            .first()
                    assertEquals(PersistedRequestStatus.WAITING, restoredRequestStates.single().status)
                    assertEquals(1, restoredRequestStates.single().attemptNumber)
                } finally {
                    reopenedDatabase.close()
                }
            } finally {
                application.deleteDatabase(databaseName)
            }
        }
}
