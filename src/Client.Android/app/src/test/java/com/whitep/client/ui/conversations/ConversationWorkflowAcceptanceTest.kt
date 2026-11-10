package com.whitep.client.ui.conversations

import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.InferenceRequestLifecycleState
import com.whitep.client.data.PersistedRequestStatus
import com.whitep.client.data.RequestStateEntity
import com.whitep.client.data.RoomConversationInferenceRequestLifecycleRepository
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
    fun requestLifecycleRecoveryIsScopedToItsOwnConversation() =
        runBlocking {
            val application = RuntimeEnvironment.getApplication()
            val databaseName = "conversation-request-lifecycle-isolation.db"
            application.deleteDatabase(databaseName)

            try {
                val database =
                    Room
                        .databaseBuilder(
                            application,
                            ClientDatabase::class.java,
                            databaseName,
                        ).allowMainThreadQueries()
                        .build()
                try {
                    database.conversationDao().upsert(
                        ConversationEntity(
                            conversationId = "conversation-a",
                            title = "Conversation A",
                            createdAtEpochMillis = 1L,
                            updatedAtEpochMillis = 1L,
                        ),
                    )
                    database.conversationDao().upsert(
                        ConversationEntity(
                            conversationId = "conversation-b",
                            title = "Conversation B",
                            createdAtEpochMillis = 2L,
                            updatedAtEpochMillis = 2L,
                        ),
                    )
                    val first =
                        RoomConversationInferenceRequestLifecycleRepository(
                            database.requestStateDao(),
                            "conversation-a",
                        )
                    val second =
                        RoomConversationInferenceRequestLifecycleRepository(
                            database.requestStateDao(),
                            "conversation-b",
                        )
                    first.save(
                        InferenceRequestLifecycleState(
                            requestId = "request-a",
                            status = PersistedRequestStatus.WAITING,
                            createdAtEpochMillis = 10L,
                            updatedAtEpochMillis = 11L,
                            attemptNumber = 1,
                        ),
                    )
                    second.save(
                        InferenceRequestLifecycleState(
                            requestId = "request-b",
                            status = PersistedRequestStatus.COMPLETED,
                            createdAtEpochMillis = 20L,
                            updatedAtEpochMillis = 21L,
                            attemptNumber = 2,
                        ),
                    )

                    val recoveredFirst = first.recoverLatest(30L)
                    val recoveredSecond = second.recoverLatest(31L)
                    val requestStateDao = database.requestStateDao()
                    val firstConversationRequestIds =
                        requestStateDao
                            .observeForConversation("conversation-a")
                            .first()
                            .map { it.requestId }
                    val secondConversationRequestIds =
                        requestStateDao
                            .observeForConversation("conversation-b")
                            .first()
                            .map { it.requestId }

                    assertEquals("request-a", recoveredFirst?.requestId)
                    assertEquals(PersistedRequestStatus.EXPIRED, recoveredFirst?.status)
                    assertEquals("request-b", recoveredSecond?.requestId)
                    assertEquals(PersistedRequestStatus.COMPLETED, recoveredSecond?.status)
                    assertEquals(listOf("request-a"), firstConversationRequestIds)
                    assertEquals(listOf("request-b"), secondConversationRequestIds)
                } finally {
                    database.close()
                }
            } finally {
                application.deleteDatabase(databaseName)
            }
        }

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
