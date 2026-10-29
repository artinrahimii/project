package com.whitep.client.ui.conversations

import android.content.Context
import android.os.Looper
import androidx.lifecycle.ViewModelStore
import androidx.room.Room
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationContextCapsuleLoadResult
import com.whitep.client.data.ConversationEntity
import com.whitep.client.data.MessageEntity
import com.whitep.client.data.RoomConversationContextCapsuleRepository
import com.whitep.client.data.SharedPreferencesConversationContextCapsuleSizeLimitStore
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.Shadows.shadowOf
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationContextCapsuleViewModelTest {
    private lateinit var database: ClientDatabase
    private lateinit var repository: RoomConversationContextCapsuleRepository
    private lateinit var viewModelStore: ViewModelStore
    private lateinit var sizeLimitStore: SharedPreferencesConversationContextCapsuleSizeLimitStore

    @Before
    fun setUp() {
        val application = RuntimeEnvironment.getApplication()
        application
            .getSharedPreferences(
                SharedPreferencesConversationContextCapsuleSizeLimitStore.PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            ).edit()
            .clear()
            .commit()
        sizeLimitStore = SharedPreferencesConversationContextCapsuleSizeLimitStore(application)
        database =
            Room
                .inMemoryDatabaseBuilder(
                    application,
                    ClientDatabase::class.java,
                ).addMigrations(ClientDatabase.MIGRATION_1_2, ClientDatabase.MIGRATION_2_3)
                .allowMainThreadQueries()
                .build()
        repository = RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
        viewModelStore = ViewModelStore()
        runBlocking {
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-ui",
                    title = "Capsule UI test",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
        }
    }

    @After
    fun tearDown() {
        viewModelStore.clear()
        database.close()
    }

    @Test
    fun createsEditsAndDeletesCapsuleUsingPersistentRepository() =
        runBlocking {
            val viewModel = createViewModel()
            viewModel.beginEditing()
            viewModel.updateGoal("Build a local-first client")
            viewModel.updateKeyFacts("Runs locally\nKeeps conversation scope")
            viewModel.updateDecisions("Store one capsule per conversation")
            viewModel.updateConstraints("At most four kilobytes")
            viewModel.updateUnresolvedItems("Verify tablet layout")

            assertTrue(viewModel.save())
            val first =
                repository.find("conversation-ui") as ConversationContextCapsuleLoadResult.Found
            assertEquals("Build a local-first client", first.capsule.goal)
            assertEquals(listOf("Runs locally", "Keeps conversation scope"), first.capsule.keyFacts)
            assertEquals(listOf("Store one capsule per conversation"), first.capsule.decisions)
            assertEquals(listOf("At most four kilobytes"), first.capsule.constraints)
            assertEquals(listOf("Verify tablet layout"), first.capsule.unresolvedItems)
            assertFalse(viewModel.uiState.value.isEditing)
            assertNotNull(viewModel.uiState.value.message)

            viewModel.beginEditing()
            viewModel.updateGoal("Updated local-first client goal")
            viewModel.updateKeyFacts("Still stays on device")
            assertTrue(viewModel.save())
            val updated =
                repository.find("conversation-ui") as ConversationContextCapsuleLoadResult.Found
            assertEquals("Updated local-first client goal", updated.capsule.goal)
            assertEquals(listOf("Still stays on device"), updated.capsule.keyFacts)

            assertTrue(viewModel.delete())
            assertEquals(ConversationContextCapsuleLoadResult.Missing, repository.find("conversation-ui"))
            assertEquals(null, viewModel.uiState.value.capsule)
            assertFalse(viewModel.uiState.value.isEditing)
        }

    @Test
    fun suggestsCapsuleFromOnlyThisConversationsLocalHistoryAndRequiresSave() =
        runBlocking {
            val original =
                ConversationContextCapsule(
                    goal = "Original goal",
                    keyFacts = listOf("Existing fact"),
                )
            repository.save("conversation-ui", original, 10L)
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = "conversation-other",
                    title = "Another conversation",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            database.messageDao().upsert(
                MessageEntity(
                    messageId = "local-one",
                    conversationId = "conversation-ui",
                    authorRole = "user",
                    content = "goal: Ship a local release\nfact: Uses on-device Room",
                    createdAtEpochMillis = 20L,
                ),
            )
            database.messageDao().upsert(
                MessageEntity(
                    messageId = "local-two",
                    conversationId = "conversation-ui",
                    authorRole = "assistant",
                    content = "decision: Keep data local\nconstraint: At most four kilobytes",
                    createdAtEpochMillis = 21L,
                ),
            )
            database.messageDao().upsert(
                MessageEntity(
                    messageId = "other-secret",
                    conversationId = "conversation-other",
                    authorRole = "user",
                    content = "goal: Unrelated private conversation",
                    createdAtEpochMillis = 22L,
                ),
            )
            shadowOf(Looper.getMainLooper()).idle()
            val viewModel = createViewModel()

            val updateSucceeded = viewModel.updateFromLocalHistory()
            assertTrue(
                "Local history update failed: ${viewModel.uiState.value.errorMessage}",
                updateSucceeded,
            )
            val draft = viewModel.uiState.value
            assertTrue(draft.isEditing)
            assertEquals("Ship a local release", draft.goal)
            assertTrue(draft.keyFactsText.contains("Existing fact"))
            assertTrue(draft.keyFactsText.contains("Uses on-device Room"))
            assertEquals("Keep data local", draft.decisionsText)
            assertEquals("At most four kilobytes", draft.constraintsText)
            assertEquals(2, draft.summarySourceMessageCount)
            assertFalse(draft.goal.contains("Unrelated private conversation"))
            assertEquals(
                original,
                (repository.find("conversation-ui") as ConversationContextCapsuleLoadResult.Found).capsule,
            )

            assertTrue(viewModel.save())
            val saved = repository.find("conversation-ui") as ConversationContextCapsuleLoadResult.Found
            assertEquals("Ship a local release", saved.capsule.goal)
            assertTrue(saved.capsule.keyFacts.contains("Existing fact"))
            assertTrue(saved.capsule.keyFacts.contains("Uses on-device Room"))
        }

    @Test
    fun blankGoalShowsValidationAndDoesNotWriteARecord() =
        runBlocking {
            val viewModel = createViewModel()
            viewModel.beginEditing()
            viewModel.updateGoal("   ")

            assertFalse(viewModel.save())
            assertNotNull(viewModel.uiState.value.errorMessage)
            assertTrue(viewModel.uiState.value.isEditing)
            assertEquals(ConversationContextCapsuleLoadResult.Missing, repository.find("conversation-ui"))
        }

    @Test
    fun selectedSizeLimitIsPersistedAndEnforcedBeforeWritingCapsule() =
        runBlocking {
            val viewModel = createViewModel()
            assertEquals(
                ConversationContextCapsule.DEFAULT_MAX_SERIALIZED_BYTES,
                viewModel.uiState.value.maxSerializedBytes,
            )
            viewModel.updateMaxSerializedBytes(512)
            assertEquals(512, viewModel.uiState.value.maxSerializedBytes)
            viewModel.beginEditing()
            viewModel.updateGoal("x".repeat(700))

            assertFalse(viewModel.save())
            val errorMessage = requireNotNull(viewModel.uiState.value.errorMessage)
            assertTrue(errorMessage.contains("selected size limit"))
            assertEquals(ConversationContextCapsuleLoadResult.Missing, repository.find("conversation-ui"))

            viewModel.updateMaxSerializedBytes(1_024)
            assertTrue(viewModel.save())
            val saved = repository.find("conversation-ui") as ConversationContextCapsuleLoadResult.Found
            assertTrue(saved.serializedByteCount <= 1_024)
            assertEquals(
                1_024,
                SharedPreferencesConversationContextCapsuleSizeLimitStore(
                    RuntimeEnvironment.getApplication(),
                ).loadMaxSerializedBytes(),
            )
        }

    private fun createViewModel(): ConversationContextCapsuleViewModel =
        ConversationContextCapsuleViewModel(
            conversationId = "conversation-ui",
            repository = repository,
            sizeLimitStore = sizeLimitStore,
            messageDao = database.messageDao(),
            clockMillis = { 50L },
        ).also {
            viewModelStore.put("context-capsule", it)
            shadowOf(Looper.getMainLooper()).idle()
        }
}
