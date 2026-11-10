package com.whitep.client.ui.conversations

import androidx.lifecycle.SavedStateHandle
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
    fun persistsAssistantResponseAndTypedErrorInTheSameConversationExactlyOnce() =
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-response",
                    title = "Response conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            val viewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-response",
                    messageDao = database.messageDao(),
                    clockMillis = { 200L },
                    messageIdGenerator = { "user-message" },
                )

            assertTrue(viewModel.sendMessage("question"))
            assertTrue(viewModel.saveAssistantResponse("request-1", "attempt-1", "answer"))
            assertFalse(viewModel.saveAssistantResponse("request-1", "attempt-1", "answer"))
            assertTrue(viewModel.saveRequestNotice("request-2", "attempt-2", "The trusted host is unavailable."))
            assertFalse(viewModel.saveRequestNotice("request-2", "attempt-2", "The trusted host is unavailable."))

            val messages = viewModel.messages.first()
            assertEquals(listOf("user", "assistant", "system"), messages.map { it.authorRole })
            assertEquals(
                listOf("question", "answer", "The trusted host is unavailable."),
                messages.map { it.content },
            )
            assertEquals(listOf(200L, 201L, 202L), messages.map { it.createdAtEpochMillis })
            assertEquals(3, database.messageDao().countForConversation("conversation-response"))
        }

    @Test
    fun draftIsRestoredAfterViewModelRecreation() =
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-draft",
                    title = "Draft conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            val savedStateHandle = SavedStateHandle()
            val firstViewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-draft",
                    messageDao = database.messageDao(),
                    savedStateHandle = savedStateHandle,
                )
            firstViewModel.updateDraft("unfinished message")

            assertEquals("unfinished message", firstViewModel.draft.value)
            assertEquals(
                "unfinished message",
                savedStateHandle.get<String>(ConversationMessagesViewModel.DRAFT_STATE_KEY),
            )

            // Android restores SavedStateHandle values when it recreates the owner and ViewModel.
            val restoredStateHandle =
                SavedStateHandle(
                    mapOf(
                        ConversationMessagesViewModel.DRAFT_STATE_KEY to
                            savedStateHandle.get<String>(ConversationMessagesViewModel.DRAFT_STATE_KEY),
                    ),
                )
            val recreatedViewModel =
                ConversationMessagesViewModel(
                    conversationId = "conversation-draft",
                    messageDao = database.messageDao(),
                    savedStateHandle = restoredStateHandle,
                )

            assertEquals("unfinished message", recreatedViewModel.draft.value)
            assertTrue(recreatedViewModel.messages.first().isEmpty())
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
