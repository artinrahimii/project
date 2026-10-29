package com.whitep.client.data

import android.database.sqlite.SQLiteConstraintException
import androidx.room.Room
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.context.ConversationContextCapsuleError
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
class ConversationContextCapsulePersistenceTest {
    private val context = RuntimeEnvironment.getApplication()
    private var activeDatabase: ClientDatabase? = null
    private val testDatabaseNames =
        listOf(
            "t0152-capsule-persistence.db",
            "t0152-capsule-invalid.db",
            "t0158-capsule-history.db",
            "t0159-capsule-corrupt.db",
            "t0159-capsule-boundary.db",
        )

    @Before
    fun setUp() {
        testDatabaseNames.forEach(context::deleteDatabase)
    }

    @After
    fun tearDown() {
        activeDatabase?.close()
        activeDatabase = null
        testDatabaseNames.forEach(context::deleteDatabase)
    }

    @Test
    fun capsulesRemainIndependentAcrossConversationsAndDatabaseReopen() =
        runBlocking {
            val databaseName = testDatabaseNames.first()
            val firstDatabase = openDatabase(databaseName)
            firstDatabase.conversationDao().upsert(conversation("conversation-one"))
            firstDatabase.conversationDao().upsert(conversation("conversation-two"))
            val firstRepository =
                RoomConversationContextCapsuleRepository(
                    firstDatabase.conversationContextCapsuleDao(),
                )
            val firstCapsule =
                ConversationContextCapsule(
                    goal = "Prepare the first release",
                    keyFacts = listOf("Runs locally"),
                    decisions = listOf("Keep conversations separate"),
                )
            val secondCapsule =
                ConversationContextCapsule(
                    goal = "Review the second release",
                    constraints = listOf("Offline-capable"),
                    unresolvedItems = listOf("Verify a second device"),
                )

            assertTrue(
                firstRepository.save("conversation-one", firstCapsule, 100L)
                    is ConversationContextCapsuleSaveResult.Saved,
            )
            assertTrue(
                firstRepository.save("conversation-two", secondCapsule, 200L)
                    is ConversationContextCapsuleSaveResult.Saved,
            )
            assertEquals(
                1,
                firstDatabase.conversationContextCapsuleDao().countForConversation("conversation-one"),
            )
            assertEquals(
                1,
                firstDatabase.conversationContextCapsuleDao().countForConversation("conversation-two"),
            )
            assertEquals(
                firstCapsule,
                (firstRepository.observe("conversation-one").first() as ConversationContextCapsuleLoadResult.Found)
                    .capsule,
            )

            firstDatabase.close()
            activeDatabase = null

            val reopenedDatabase = openDatabase(databaseName)
            val reopenedRepository =
                RoomConversationContextCapsuleRepository(
                    reopenedDatabase.conversationContextCapsuleDao(),
                )
            val restoredFirst = reopenedRepository.find("conversation-one")
            val restoredSecond = reopenedRepository.find("conversation-two")
            assertTrue(restoredFirst is ConversationContextCapsuleLoadResult.Found)
            assertTrue(restoredSecond is ConversationContextCapsuleLoadResult.Found)
            assertEquals(
                firstCapsule,
                (restoredFirst as ConversationContextCapsuleLoadResult.Found).capsule,
            )
            assertEquals(100L, restoredFirst.updatedAtEpochMillis)
            assertEquals(
                secondCapsule,
                (restoredSecond as ConversationContextCapsuleLoadResult.Found).capsule,
            )
            assertEquals(200L, restoredSecond.updatedAtEpochMillis)

            val updatedFirst =
                ConversationContextCapsule(
                    goal = "Updated first release goal",
                    keyFacts = listOf("Updated locally"),
                )
            assertTrue(
                reopenedRepository.save("conversation-one", updatedFirst, 300L)
                    is ConversationContextCapsuleSaveResult.Saved,
            )
            assertEquals(
                updatedFirst,
                (reopenedRepository.find("conversation-one") as ConversationContextCapsuleLoadResult.Found)
                    .capsule,
            )
            assertEquals(
                secondCapsule,
                (reopenedRepository.find("conversation-two") as ConversationContextCapsuleLoadResult.Found)
                    .capsule,
            )

            assertEquals(1, reopenedDatabase.conversationDao().deleteById("conversation-one"))
            assertEquals(
                ConversationContextCapsuleLoadResult.Missing,
                reopenedRepository.find("conversation-one"),
            )
            assertEquals(
                secondCapsule,
                (reopenedRepository.find("conversation-two") as ConversationContextCapsuleLoadResult.Found)
                    .capsule,
            )
        }

    @Test
    fun savesAppendLocalProvenanceWithFieldNamesAndTimestampWithoutCopyingValues() =
        runBlocking {
            val database = openDatabase(testDatabaseNames.last())
            database.conversationDao().upsert(conversation("conversation-history"))
            val repository =
                RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
            val original =
                ConversationContextCapsule(
                    goal = "Private original goal",
                    keyFacts = listOf("Private original fact"),
                )
            assertTrue(
                repository.save(
                    "conversation-history",
                    original,
                    100L,
                    source = ConversationContextCapsuleChangeSource.USER_EDIT,
                ) is ConversationContextCapsuleSaveResult.Saved,
            )
            val revised =
                ConversationContextCapsule(
                    goal = "Private revised goal",
                    keyFacts = listOf("Private revised fact"),
                )
            assertTrue(
                repository.save(
                    "conversation-history",
                    revised,
                    200L,
                    source = ConversationContextCapsuleChangeSource.LOCAL_HISTORY_SUMMARY_REVIEWED,
                ) is ConversationContextCapsuleSaveResult.Saved,
            )

            val history = repository.changeHistory("conversation-history")
            assertEquals(2, history.size)
            assertEquals("USER_EDIT", history.first().source)
            assertEquals(100L, history.first().changedAtEpochMillis)
            assertEquals(
                "[\"schemaVersion\",\"goal\",\"keyFacts\",\"decisions\",\"constraints\",\"unresolvedItems\"]",
                history.first().changedFieldsJson,
            )
            assertEquals("LOCAL_HISTORY_SUMMARY_REVIEWED", history.last().source)
            assertEquals(200L, history.last().changedAtEpochMillis)
            assertEquals("[\"goal\",\"keyFacts\"]", history.last().changedFieldsJson)
            assertEquals(1, history.last().schemaVersion)
            assertEquals(revised.encode().bytes?.size, history.last().serializedByteCount)
            assertTrue(
                history.none {
                    it.changedFieldsJson.contains("Private original goal") ||
                        it.changedFieldsJson.contains("Private revised fact")
                },
            )

            assertEquals(1, repository.delete("conversation-history"))
            assertTrue(repository.changeHistory("conversation-history").isEmpty())
            assertEquals(
                ConversationContextCapsuleLoadResult.Missing,
                repository.find("conversation-history"),
            )
        }

    @Test
    fun invalidCapsuleIsRejectedAndForeignKeyPreventsOrphanRecords() =
        runBlocking {
            val database = openDatabase(testDatabaseNames[1])
            database.conversationDao().upsert(conversation("valid-conversation"))
            val repository =
                RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
            val rejected =
                repository.save(
                    "valid-conversation",
                    ConversationContextCapsule(goal = "  "),
                    10L,
                )
            assertTrue(rejected is ConversationContextCapsuleSaveResult.Rejected)
            assertEquals(
                ConversationContextCapsuleError.EMPTY_GOAL,
                (rejected as ConversationContextCapsuleSaveResult.Rejected).error,
            )
            assertEquals(
                ConversationContextCapsuleLoadResult.Missing,
                repository.find("valid-conversation"),
            )

            var foreignKeyRejected = false
            try {
                repository.save(
                    "missing-conversation",
                    ConversationContextCapsule(goal = "A valid goal"),
                    20L,
                )
            } catch (_: SQLiteConstraintException) {
                foreignKeyRejected = true
            }
            assertTrue(foreignKeyRejected)
            assertEquals(
                ConversationContextCapsuleLoadResult.Missing,
                repository.find("missing-conversation"),
            )
        }


    @Test
    fun acceptsExactByteCeilingAndRejectsNextByteWithoutReplacingSavedCapsule() =
        runBlocking {
            val database = openDatabase("t0159-capsule-boundary.db")
            database.conversationDao().upsert(conversation("conversation-boundary"))
            val repository =
                RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
            val oneCharacterByteCount =
                requireNotNull(ConversationContextCapsule(goal = "x").encode().byteCount)
            val exactGoalLength =
                ConversationContextCapsule.MAX_SERIALIZED_BYTES - oneCharacterByteCount + 1
            val exactLimitCapsule = ConversationContextCapsule(goal = "x".repeat(exactGoalLength))
            val exactEncoding = exactLimitCapsule.encode()

            assertTrue(exactEncoding.isValid)
            assertEquals(
                ConversationContextCapsule.MAX_SERIALIZED_BYTES,
                requireNotNull(exactEncoding.byteCount),
            )
            val saved =
                repository.save(
                    "conversation-boundary",
                    exactLimitCapsule,
                    100L,
                    maxSerializedBytes = ConversationContextCapsule.MAX_SERIALIZED_BYTES,
                )
            assertTrue(saved is ConversationContextCapsuleSaveResult.Saved)
            assertEquals(
                ConversationContextCapsule.MAX_SERIALIZED_BYTES,
                (saved as ConversationContextCapsuleSaveResult.Saved).serializedByteCount,
            )

            val overLimitCapsule = ConversationContextCapsule(goal = "x".repeat(exactGoalLength + 1))
            val overLimitEncoding = overLimitCapsule.encode()
            assertFalse(overLimitEncoding.isValid)
            assertEquals(ConversationContextCapsuleError.TOO_LARGE, overLimitEncoding.error)
            assertEquals(
                ConversationContextCapsule.MAX_SERIALIZED_BYTES + 1,
                requireNotNull(overLimitEncoding.byteCount),
            )
            val rejected =
                repository.save(
                    "conversation-boundary",
                    overLimitCapsule,
                    200L,
                    maxSerializedBytes = ConversationContextCapsule.MAX_SERIALIZED_BYTES,
                )
            assertTrue(rejected is ConversationContextCapsuleSaveResult.Rejected)
            assertEquals(
                ConversationContextCapsuleError.TOO_LARGE,
                (rejected as ConversationContextCapsuleSaveResult.Rejected).error,
            )
            assertEquals(
                exactLimitCapsule,
                (repository.find("conversation-boundary") as ConversationContextCapsuleLoadResult.Found).capsule,
            )
            assertEquals(1, repository.changeHistory("conversation-boundary").size)
        }

    @Test
    fun malformedAndInvalidUtf8StoredCapsulesStayInvalidUntilReplacedByValidSave() =
        runBlocking {
            val database = openDatabase("t0159-capsule-corrupt.db")
            database.conversationDao().upsert(conversation("conversation-corrupt"))
            val repository =
                RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
            val dao = database.conversationContextCapsuleDao()

            dao.upsert(
                ConversationContextCapsuleEntity(
                    conversationId = "conversation-corrupt",
                    serializedCapsule = "{not-json".toByteArray(Charsets.UTF_8),
                    updatedAtEpochMillis = 10L,
                ),
            )
            val malformed = repository.find("conversation-corrupt")
            assertTrue(malformed is ConversationContextCapsuleLoadResult.InvalidStoredCapsule)
            assertEquals(
                ConversationContextCapsuleError.MALFORMED_JSON,
                (malformed as ConversationContextCapsuleLoadResult.InvalidStoredCapsule).error,
            )

            dao.upsert(
                ConversationContextCapsuleEntity(
                    conversationId = "conversation-corrupt",
                    serializedCapsule = byteArrayOf(0xC3.toByte(), 0x28.toByte()),
                    updatedAtEpochMillis = 20L,
                ),
            )
            val invalidUtf8 = repository.find("conversation-corrupt")
            assertTrue(invalidUtf8 is ConversationContextCapsuleLoadResult.InvalidStoredCapsule)
            assertEquals(
                ConversationContextCapsuleError.INVALID_UTF8,
                (invalidUtf8 as ConversationContextCapsuleLoadResult.InvalidStoredCapsule).error,
            )

            val recovered = ConversationContextCapsule(goal = "Recovered after corrupt local bytes")
            val replacement = repository.save("conversation-corrupt", recovered, 30L)
            assertTrue(replacement is ConversationContextCapsuleSaveResult.Saved)
            assertEquals(
                recovered,
                (repository.find("conversation-corrupt") as ConversationContextCapsuleLoadResult.Found).capsule,
            )
            val history = repository.changeHistory("conversation-corrupt")
            assertEquals(1, history.size)
            assertEquals("USER_EDIT", history.single().source)
            assertTrue(history.single().changedFieldsJson.contains("goal"))
        }

    private fun openDatabase(name: String): ClientDatabase =
        Room
            .databaseBuilder(context, ClientDatabase::class.java, name)
            .addMigrations(ClientDatabase.MIGRATION_1_2, ClientDatabase.MIGRATION_2_3, ClientDatabase.MIGRATION_3_4)
            .allowMainThreadQueries()
            .build()
            .also { activeDatabase = it }

    private fun conversation(id: String) =
        ConversationEntity(
            conversationId = id,
            title = id,
            createdAtEpochMillis = 1L,
            updatedAtEpochMillis = 1L,
        )
}
