package com.whitep.client.ui.conversations

import android.os.Looper
import androidx.lifecycle.ViewModelStore
import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationContextCapsuleLoadResult
import com.whitep.client.data.ConversationEntity
import com.whitep.client.data.RoomConversationContextCapsuleRepository
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

    @Before
    fun setUp() {
        database =
            Room
                .inMemoryDatabaseBuilder(
                    RuntimeEnvironment.getApplication(),
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

    private fun createViewModel(): ConversationContextCapsuleViewModel =
        ConversationContextCapsuleViewModel(
            conversationId = "conversation-ui",
            repository = repository,
            clockMillis = { 50L },
        ).also {
            viewModelStore.put("context-capsule", it)
            shadowOf(Looper.getMainLooper()).idle()
        }
}
