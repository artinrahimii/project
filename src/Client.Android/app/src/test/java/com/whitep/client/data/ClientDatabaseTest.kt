package com.whitep.client.data

import androidx.room.Room
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ClientDatabaseTest {
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
    fun conversationsMessagesAndRequestStatesArePersistedAndOrdered() =
        runBlocking {
            val conversations = database.conversationDao()
            val messages = database.messageDao()
            val requestStates = database.requestStateDao()
            conversations.upsert(
                ConversationEntity(
                    conversationId = "conversation-1",
                    title = "Local test",
                    createdAtEpochMillis = 10L,
                    updatedAtEpochMillis = 30L,
                ),
            )
            messages.upsert(
                MessageEntity(
                    messageId = "message-later",
                    conversationId = "conversation-1",
                    authorRole = "assistant",
                    content = "second",
                    createdAtEpochMillis = 20L,
                ),
            )
            messages.upsert(
                MessageEntity(
                    messageId = "message-first",
                    conversationId = "conversation-1",
                    authorRole = "user",
                    content = "first",
                    createdAtEpochMillis = 10L,
                ),
            )
            requestStates.upsert(
                RequestStateEntity(
                    requestId = "request-1",
                    conversationId = "conversation-1",
                    status = PersistedRequestStatus.WAITING,
                    createdAtEpochMillis = 15L,
                    updatedAtEpochMillis = 16L,
                    attemptNumber = 1,
                ),
            )

            assertEquals(
                listOf("message-first", "message-later"),
                messages
                    .observeForConversation("conversation-1")
                    .first()
                    .map { it.messageId },
            )
            assertEquals(
                PersistedRequestStatus.WAITING,
                requestStates
                    .observeForConversation("conversation-1")
                    .first()
                    .single()
                    .status,
            )
            assertEquals("Local test", conversations.findById("conversation-1")?.title)
        }

    @Test
    fun allPersistedRequestStatusesCanBeReadFromRoomInCreationOrder() =
        runBlocking {
            val conversationId = "conversation-statuses"
            database.conversationDao().upsert(
                ConversationEntity(
                    conversationId = conversationId,
                    title = "Request statuses",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )

            PersistedRequestStatus.values().forEachIndexed { index, status ->
                database.requestStateDao().upsert(
                    RequestStateEntity(
                        requestId = "request-status-$index",
                        conversationId = conversationId,
                        status = status,
                        createdAtEpochMillis = index.toLong(),
                        updatedAtEpochMillis = index.toLong(),
                    ),
                )
            }

            val persistedStatuses =
                database
                    .requestStateDao()
                    .observeForConversation(conversationId)
                    .first()
                    .map { it.status }
            assertEquals(PersistedRequestStatus.values().toList(), persistedStatuses)
        }

    @Test
    fun deletingConversationCascadesToMessagesAndRequestStates() =
        runBlocking {
            val conversations = database.conversationDao()
            conversations.upsert(
                ConversationEntity(
                    conversationId = "conversation-delete",
                    title = "Delete me",
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )
            database.messageDao().upsert(
                MessageEntity(
                    messageId = "message-delete",
                    conversationId = "conversation-delete",
                    authorRole = "user",
                    content = "locally stored",
                    createdAtEpochMillis = 1L,
                ),
            )
            database.requestStateDao().upsert(
                RequestStateEntity(
                    requestId = "request-delete",
                    conversationId = "conversation-delete",
                    status = PersistedRequestStatus.QUEUED,
                    createdAtEpochMillis = 1L,
                    updatedAtEpochMillis = 1L,
                ),
            )

            assertEquals(1, conversations.deleteById("conversation-delete"))
            assertEquals(
                0,
                database.messageDao().countForConversation("conversation-delete"),
            )
            assertEquals(
                0,
                database.requestStateDao().countForConversation("conversation-delete"),
            )
        }

    @Test
    fun roomRepositoryRetainsDraftAndDoesNotClaimTransportSuccess() =
        runBlocking {
            val firstRepository = RoomClientRequestRepository(database.clientRequestDraftDao())
            firstRepository.updatePrompt("draft retained locally")
            assertEquals(
                RequestPreparationResult.TRANSPORT_UNAVAILABLE,
                firstRepository.prepareRequest(),
            )

            val secondRepository = RoomClientRequestRepository(database.clientRequestDraftDao())
            val restored = secondRepository.draft.first()
            assertEquals("draft retained locally", restored.prompt)
            assertEquals(RequestDraftStatus.TRANSPORT_UNAVAILABLE, restored.status)
            assertNotNull(database.clientRequestDraftDao().findById("current"))
        }
}
