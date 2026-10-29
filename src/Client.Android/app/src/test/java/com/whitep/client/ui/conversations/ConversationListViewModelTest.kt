package com.whitep.client.ui.conversations

import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity
import com.whitep.client.data.MessageEntity
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationListViewModelTest {
    private lateinit var database: ClientDatabase

    @Before
    fun setUp() {
        database =
            Room
                .inMemoryDatabaseBuilder(
                    RuntimeEnvironment.getApplication(),
                    ClientDatabase::class.java,
                ).allowMainThreadQueries()
                .build()
    }

    @After
    fun tearDown() {
        database.close()
    }

    @Test
    fun exposesConversationsInsertedIntoRoomInDaoOrder() =
        runBlocking {
            val viewModel = ConversationListViewModel(database.conversationDao())
            assertTrue(viewModel.conversations.first().isEmpty())

            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "older",
                    title = "Older conversation",
                    createdAtEpochMillis = 10L,
                    updatedAtEpochMillis = 20L,
                ),
            )
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "newer",
                    title = "Newest conversation",
                    createdAtEpochMillis = 30L,
                    updatedAtEpochMillis = 40L,
                ),
            )

            assertEquals(
                listOf("newer", "older"),
                viewModel.conversations.first().map { it.conversationId },
            )
            assertEquals(
                listOf("Newest conversation", "Older conversation"),
                viewModel.conversations.first().map { it.title },
            )
        }

    @Test
    fun supportsConversationCreateRenameDeleteAndCascade() =
        runBlocking {
            var now = 100L
            val viewModel =
                ConversationListViewModel(
                    conversationDao = database.conversationDao(),
                    clockMillis = { now },
                    conversationIdGenerator = { "conversation-1" },
                )

            val created = viewModel.createConversation()
            assertEquals("conversation-1", created.conversationId)
            assertEquals("New conversation", created.title)
            assertEquals(100L, created.createdAtEpochMillis)
            assertEquals(created, viewModel.getConversation(created.conversationId))

            database.messageDao().upsert(
                MessageEntity(
                    messageId = "message-1",
                    conversationId = created.conversationId,
                    authorRole = "user",
                    content = "Local test content",
                    createdAtEpochMillis = now,
                ),
            )
            assertEquals(1, database.messageDao().countForConversation(created.conversationId))

            assertTrue(viewModel.renameConversation(created.conversationId, "  Updated title  "))
            val renamed = viewModel.getConversation(created.conversationId)
            assertEquals("Updated title", renamed?.title)
            assertEquals(101L, renamed?.updatedAtEpochMillis)
            assertFalse(viewModel.renameConversation(created.conversationId, "  "))
            assertFalse(viewModel.renameConversation("missing-conversation", "Other"))
            assertEquals("Updated title", viewModel.getConversation(created.conversationId)?.title)

            assertTrue(viewModel.deleteConversation(created.conversationId))
            assertFalse(viewModel.deleteConversation(created.conversationId))
            assertNull(viewModel.getConversation(created.conversationId))
            assertEquals(0, database.messageDao().countForConversation(created.conversationId))
        }
}
