package com.whitep.client.ui.conversations

import androidx.room.Room
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertEquals
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
}
