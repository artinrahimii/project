package com.whitep.client.ui.conversations

import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity
import com.whitep.client.domain.ConversationMessageValidator
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationMessagesViewModelTest {
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
    fun persistsMessagesInChronologicalOrder() =
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-1",
                    title = "Test conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            var generatedId = 0
            val viewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-1",
                    messageDao = database.messageDao(),
                    clockMillis = { 100L },
                    messageIdGenerator = { "message-${++generatedId}" },
                )

            assertTrue(viewModel.sendMessage("  first message  "))
            assertTrue(viewModel.sendMessage("second message"))

            val messages = viewModel.messages.first()
            assertEquals(listOf("first message", "second message"), messages.map { it.content })
            assertEquals(listOf(100L, 101L), messages.map { it.createdAtEpochMillis })
            assertEquals(listOf("user", "user"), messages.map { it.authorRole })
            assertEquals(2, database.messageDao().countForConversation("conversation-1"))
        }

    @Test
    fun rejectsOverLimitMessagesWithoutPersistingThem() =
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-too-long",
                    title = "Test conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            val viewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-too-long",
                    messageDao = database.messageDao(),
                )

            val overLimit = "🙂".repeat(ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES / 4 + 1)
            assertFalse(viewModel.sendMessage(overLimit))
            assertTrue(viewModel.messages.first().isEmpty())
            assertEquals(0, database.messageDao().countForConversation("conversation-too-long"))
        }

    @Test
    fun rejectsBlankMessagesWithoutPersistingThem() =
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-blank",
                    title = "Test conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            val viewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-blank",
                    messageDao = database.messageDao(),
                )

            assertFalse(viewModel.sendMessage("  \n "))
            assertTrue(viewModel.messages.first().isEmpty())
        }
}
