package com.whitep.client.data

import android.content.Context
import androidx.room.Room
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
class ClientDatabaseMigrationTest {
    private val context = RuntimeEnvironment.getApplication()
    private var database: ClientDatabase? = null

    @Before
    fun setUp() {
        context.deleteDatabase(ClientDatabase.DATABASE_NAME)
    }

    @After
    fun tearDown() {
        database?.close()
        context.deleteDatabase(ClientDatabase.DATABASE_NAME)
    }

    @Test
    fun migratesVersionOneDatabaseWithoutLosingLocalRecordsOrAddingContentToRequestState() =
        runBlocking {
            val legacyDatabase =
                context.openOrCreateDatabase(
                    ClientDatabase.DATABASE_NAME,
                    Context.MODE_PRIVATE,
                    null,
                )
            try {
                val schema =
                    checkNotNull(javaClass.getResourceAsStream("/db/v1-schema.sql"))
                        .bufferedReader()
                        .use { it.readText() }
                schema
                    .split(";")
                    .map(String::trim)
                    .filter(String::isNotEmpty)
                    .forEach(legacyDatabase::execSQL)

                legacyDatabase.execSQL(
                    "INSERT INTO conversations (conversationId, title, createdAtEpochMillis, updatedAtEpochMillis) VALUES (?, ?, ?, ?)",
                    arrayOf<Any>("conversation-legacy", "Legacy conversation", 10L, 20L),
                )
                legacyDatabase.execSQL(
                    "INSERT INTO messages (messageId, conversationId, authorRole, content, createdAtEpochMillis) VALUES (?, ?, ?, ?, ?)",
                    arrayOf<Any>("message-legacy", "conversation-legacy", "user", "old message", 11L),
                )
                legacyDatabase.execSQL(
                    "INSERT INTO request_states " +
                        "(requestId, conversationId, status, createdAtEpochMillis, updatedAtEpochMillis, " +
                        "attemptNumber, errorCode) VALUES (?, ?, ?, ?, ?, ?, ?)",
                    arrayOf<Any>(
                        "request-legacy",
                        "conversation-legacy",
                        "WAITING",
                        12L,
                        13L,
                        2,
                        "NETWORK_UNAVAILABLE",
                    ),
                )
                legacyDatabase.execSQL(
                    "INSERT INTO request_drafts (draftId, prompt, status) VALUES (?, ?, ?)",
                    arrayOf<Any>("current", "local draft", "NOT_SENT"),
                )
                legacyDatabase.version = 1
            } finally {
                legacyDatabase.close()
            }

            val migratedDatabase =
                Room
                    .databaseBuilder(
                        context,
                        ClientDatabase::class.java,
                        ClientDatabase.DATABASE_NAME,
                    ).addMigrations(ClientDatabase.MIGRATION_1_2)
                    .build()
                    .also { database = it }

            assertEquals(2, migratedDatabase.openHelper.readableDatabase.version)
            assertEquals(
                "Legacy conversation",
                migratedDatabase.conversationDao().findById("conversation-legacy")?.title,
            )
            assertEquals(
                listOf("message-legacy"),
                migratedDatabase
                    .messageDao()
                    .observeForConversation("conversation-legacy")
                    .first()
                    .map { it.messageId },
            )
            val request = migratedDatabase.requestStateDao().findById("request-legacy")
            assertEquals(PersistedRequestStatus.WAITING, request?.status)
            assertEquals("NETWORK_UNAVAILABLE", request?.errorCode)
            val draft = migratedDatabase.clientRequestDraftDao().findById("current")
            assertEquals("local draft", draft?.prompt)
            assertEquals(RequestDraftStatus.NOT_SENT, draft?.status)

            val indexNames = mutableSetOf<String>()
            migratedDatabase.openHelper.readableDatabase
                .query("PRAGMA index_list('request_states')")
                .use { cursor ->
                    val nameColumn = cursor.getColumnIndexOrThrow("name")
                    while (cursor.moveToNext()) {
                        indexNames += cursor.getString(nameColumn)
                    }
                }
            assertTrue(
                indexNames.contains(
                    "index_request_states_conversationId_createdAtEpochMillis_requestId",
                ),
            )
        }
}
