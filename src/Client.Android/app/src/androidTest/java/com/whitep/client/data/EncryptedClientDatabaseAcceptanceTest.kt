package com.whitep.client.data

import android.content.Context
import android.util.Base64
import androidx.room.Room
import androidx.test.core.app.ApplicationProvider
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.security.AndroidKeyStoreProtectedBlobProtector
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import org.junit.After
import org.junit.Assert.assertArrayEquals
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertThrows
import org.junit.Test
import org.junit.runner.RunWith
import java.io.File
import java.io.FileInputStream
import java.security.GeneralSecurityException
import java.security.MessageDigest
import net.zetetic.database.sqlcipher.SQLiteDatabase as CipherSQLiteDatabase

@RunWith(AndroidJUnit4::class)
class EncryptedClientDatabaseAcceptanceTest {
    private val context = ApplicationProvider.getApplicationContext<Context>()
    private val sensitiveCanaryMarkers =
        listOf(
            "T0187_PROMPT_CANARY_7F2A",
            "T0187_CONTEXT_CAPSULE_CANARY_91C4",
            "T0187_SECRET_CANARY_3BD8",
        )
    private val databaseNames =
        listOf(
            "t0184-fresh-encrypted.db",
            "t0184-legacy-plaintext.db",
            "t0184-missing-key.db",
            "t0184-protected-key.db",
        )

    @After
    fun cleanUp() {
        databaseNames.forEach { name ->
            context.deleteDatabase(name)
            context
                .getSharedPreferences(
                    EncryptedClientDatabaseFactory.KEY_PREFERENCES_NAME,
                    Context.MODE_PRIVATE,
                ).edit()
                .remove(EncryptedClientDatabaseFactory.wrappedKeyPreference(name))
                .commit()
        }
    }

    @Test
    fun freshDatabaseIsEncryptedAndConversationDataSurvivesReopen() =
        runBlocking {
            val databaseName = databaseNames[0]
            val database = EncryptedClientDatabaseFactory.open(context, databaseName)
            try {
                database.conversationDao().upsert(
                    ConversationEntity(
                        conversationId = "encrypted-conversation",
                        title = "Encrypted local conversation",
                        createdAtEpochMillis = 10L,
                        updatedAtEpochMillis = 20L,
                    ),
                )
                database.messageDao().upsert(
                    MessageEntity(
                        messageId = "encrypted-message",
                        conversationId = "encrypted-conversation",
                        authorRole = "user",
                        content = "local message",
                        createdAtEpochMillis = 11L,
                    ),
                )
            } finally {
                database.close()
            }

            val databaseFile = context.getDatabasePath(databaseName)
            assertEncryptedHeader(databaseFile)
            assertCannotReadWithoutKey(databaseFile)

            val reopened = EncryptedClientDatabaseFactory.open(context, databaseName)
            try {
                assertEquals(
                    "Encrypted local conversation",
                    reopened.conversationDao().findById("encrypted-conversation")?.title,
                )
                assertEquals(
                    listOf("local message"),
                    reopened
                        .messageDao()
                        .observeForConversation("encrypted-conversation")
                        .first()
                        .map { it.content },
                )
            } finally {
                reopened.close()
            }
        }

    @Test
    fun legacyPlaintextRoomDatabaseIsExportedAndExistingRowsArePreserved() =
        runBlocking {
            val databaseName = databaseNames[1]
            val capsuleMarker = sensitiveCanaryMarkers[1]
            val capsule =
                ConversationContextCapsule(
                    goal = "T0187 context capsule persistence fixture",
                    keyFacts = listOf(capsuleMarker),
                )
            val capsuleBytes = checkNotNull(capsule.encode().bytes)
            val legacy =
                Room
                    .databaseBuilder(context, ClientDatabase::class.java, databaseName)
                    .addMigrations(
                        ClientDatabase.MIGRATION_1_2,
                        ClientDatabase.MIGRATION_2_3,
                        ClientDatabase.MIGRATION_3_4,
                    ).build()
            try {
                legacy.conversationDao().upsert(
                    ConversationEntity(
                        conversationId = "legacy-conversation",
                        title = "Preserve this record",
                        createdAtEpochMillis = 30L,
                        updatedAtEpochMillis = 40L,
                    ),
                )
                legacy.messageDao().upsert(
                    MessageEntity(
                        messageId = "legacy-message",
                        conversationId = "legacy-conversation",
                        authorRole = "assistant",
                        content = "T0187_SECRET_CANARY_3BD8",
                        createdAtEpochMillis = 35L,
                    ),
                )
                legacy.clientRequestDraftDao().upsert(
                    ClientRequestDraftEntity(
                        draftId = "current",
                        prompt = "T0187_PROMPT_CANARY_7F2A",
                        status = RequestDraftStatus.NOT_SENT,
                    ),
                )
                legacy.conversationContextCapsuleDao().upsert(
                    ConversationContextCapsuleEntity(
                        conversationId = "legacy-conversation",
                        serializedCapsule = capsuleBytes,
                        updatedAtEpochMillis = 36L,
                    ),
                )
                capsuleBytes.fill(0)
                legacy.openHelper.writableDatabase
            } finally {
                legacy.close()
            }

            val databaseFile = context.getDatabasePath(databaseName)
            assertPlaintextHeader(databaseFile)

            val encrypted = EncryptedClientDatabaseFactory.open(context, databaseName)
            try {
                assertEquals(
                    "Preserve this record",
                    encrypted.conversationDao().findById("legacy-conversation")?.title,
                )
                assertEquals(
                    "T0187_SECRET_CANARY_3BD8",
                    encrypted
                        .messageDao()
                        .observeForConversation("legacy-conversation")
                        .first()
                        .single()
                        .content,
                )
                assertEquals(
                    "T0187_PROMPT_CANARY_7F2A",
                    encrypted.clientRequestDraftDao().findById("current")?.prompt,
                )
                val migratedCapsule =
                    checkNotNull(
                        encrypted.conversationContextCapsuleDao().findByConversationId("legacy-conversation"),
                    )
                val decodedCapsule = ConversationContextCapsule.decode(migratedCapsule.serializedCapsule)
                assertEquals(null, decodedCapsule.error)
                assertEquals(listOf(capsuleMarker), decodedCapsule.capsule?.keyFacts)
            } finally {
                encrypted.close()
            }

            assertEncryptedHeader(databaseFile)
            assertCannotReadWithoutKey(databaseFile)
            assertNoSensitiveCanariesInDatabaseArtifacts(databaseFile)
            assertFalse(
                "A SQLCipher migration temporary file remained after the atomic rename.",
                File(databaseFile.absolutePath + ".sqlcipher-migrating").exists(),
            )
        }

    @Test
    fun databaseKeyIsStoredOnlyAsKeystoreProtectedEnvelope() {
        val databaseName = databaseNames[3]
        EncryptedClientDatabaseFactory.open(context, databaseName).close()
        val preferences =
            context.getSharedPreferences(
                EncryptedClientDatabaseFactory.KEY_PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            )
        val encodedEnvelope =
            checkNotNull(
                preferences.getString(
                    EncryptedClientDatabaseFactory.wrappedKeyPreference(databaseName),
                    null,
                ),
            )
        val envelope = Base64.decode(encodedEnvelope, Base64.NO_WRAP)
        assertEquals(1 + 12 + 32 + 16, envelope.size)
        assertEquals(1.toByte(), envelope[0])

        val cleartext =
            AndroidKeyStoreProtectedBlobProtector(EncryptedClientDatabaseFactory.DATABASE_KEYSTORE_ALIAS)
                .unprotect(envelope)
        assertEquals(32, cleartext.size)
        cleartext.fill(0)
        envelope.fill(0)
    }

    @Test
    fun missingProtectedKeyFailsClosedAndLeavesEncryptedDatabaseUnchanged() {
        val databaseName = databaseNames[2]
        EncryptedClientDatabaseFactory.open(context, databaseName).close()
        val databaseFile = context.getDatabasePath(databaseName)
        val originalHash = sha256(databaseFile)

        context
            .getSharedPreferences(
                EncryptedClientDatabaseFactory.KEY_PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            ).edit()
            .remove(EncryptedClientDatabaseFactory.wrappedKeyPreference(databaseName))
            .commit()

        assertThrows(GeneralSecurityException::class.java) {
            EncryptedClientDatabaseFactory.open(context, databaseName)
        }
        assertArrayEquals(originalHash, sha256(databaseFile))
    }

    private fun assertPlaintextHeader(file: File) {
        assertArrayEquals(
            "SQLite format 3\u0000".toByteArray(Charsets.US_ASCII),
            readHeader(file),
        )
    }

    private fun assertEncryptedHeader(file: File) {
        assertFalse(
            "The database still has a plaintext SQLite header.",
            readHeader(file).contentEquals("SQLite format 3\u0000".toByteArray(Charsets.US_ASCII)),
        )
    }

    private fun assertNoSensitiveCanariesInDatabaseArtifacts(databaseFile: File) {
        val artifacts =
            listOf(
                databaseFile,
                File(databaseFile.absolutePath + "-wal"),
                File(databaseFile.absolutePath + "-shm"),
                File(databaseFile.absolutePath + ".sqlcipher-migrating"),
                File(databaseFile.absolutePath + ".sqlcipher-migrating-wal"),
                File(databaseFile.absolutePath + ".sqlcipher-migrating-shm"),
            )
        artifacts.filter(File::exists).forEach { artifact ->
            val contents = artifact.readBytes()
            try {
                val searchable = contents.toString(Charsets.ISO_8859_1)
                sensitiveCanaryMarkers.forEach { marker ->
                    assertFalse(
                        "A synthetic prompt/capsule/secret canary remained in an app-owned database artifact.",
                        searchable.contains(marker),
                    )
                }
            } finally {
                contents.fill(0)
            }
        }
    }

    private fun readHeader(file: File): ByteArray =
        FileInputStream(file).use { input ->
            val header = ByteArray(16)
            var offset = 0
            while (offset < header.size) {
                val count = input.read(header, offset, header.size - offset)
                if (count < 0) return@use header.copyOf(offset)
                offset += count
            }
            header
        }

    private fun assertCannotReadWithoutKey(file: File) {
        assertThrows(Exception::class.java) {
            val database =
                CipherSQLiteDatabase.openDatabase(
                    file.absolutePath,
                    null,
                    CipherSQLiteDatabase.OPEN_READONLY,
                )
            try {
                database.rawQuery("SELECT name FROM sqlite_master", emptyArray<String>()).use { cursor ->
                    cursor.moveToFirst()
                }
            } finally {
                database.close()
            }
        }
    }

    private fun sha256(file: File): ByteArray = MessageDigest.getInstance("SHA-256").digest(file.readBytes())
}
