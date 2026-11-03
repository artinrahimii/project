package com.whitep.client.data

import android.content.Context
import android.system.Os
import android.util.Base64
import androidx.room.Room
import com.whitep.client.security.AndroidKeyStoreProtectedBlobProtector
import net.zetetic.database.sqlcipher.SupportOpenHelperFactory
import java.io.File
import java.io.FileInputStream
import java.io.IOException
import java.security.GeneralSecurityException
import java.security.SecureRandom
import java.util.Arrays
import net.zetetic.database.sqlcipher.SQLiteDatabase as CipherSQLiteDatabase

/**
 * Opens the Room database through SQLCipher and stores its random passphrase only as
 * an Android-Keystore-protected envelope in app-private preferences.
 *
 * Existing plaintext SQLite files are exported to a separate encrypted file and replaced
 * only after the target has passed an integrity check.
 */
internal object EncryptedClientDatabaseFactory {
    internal const val KEY_PREFERENCES_NAME = "whitep-client-database-keys"
    internal const val DATABASE_KEYSTORE_ALIAS = "com.whitep.client.database_key.v1"
    private const val KEY_PREFERENCE_PREFIX = "wrapped."
    private const val DATABASE_KEY_BYTES = 32
    private const val SQLITE_HEADER = "SQLite format 3\u0000"
    private const val ENCRYPTED_DATABASE_ALIAS = "encrypted"
    private const val TEMPORARY_SUFFIX = ".sqlcipher-migrating"
    private val LOCK = Any()

    @Volatile
    private var nativeLibraryLoaded = false

    internal fun wrappedKeyPreference(databaseName: String): String = KEY_PREFERENCE_PREFIX + databaseName

    fun open(
        context: Context,
        databaseName: String,
    ): ClientDatabase {
        require(DATABASE_NAME_PATTERN.matches(databaseName)) {
            "A bounded application database file name is required."
        }
        val appContext = context.applicationContext
        loadNativeLibrary()
        val databaseFile = appContext.getDatabasePath(databaseName)
        val databaseKey =
            synchronized(LOCK) {
                val key = getOrCreateDatabaseKey(appContext, databaseFile, databaseName)
                try {
                    migratePlaintextDatabase(databaseFile, key)
                    key
                } catch (exception: Exception) {
                    Arrays.fill(key, 0)
                    throw exception
                }
            }

        return try {
            val openHelperFactory = SupportOpenHelperFactory(databaseKey.copyOf())
            val database =
                Room
                    .databaseBuilder(
                        appContext,
                        ClientDatabase::class.java,
                        databaseName,
                    ).openHelperFactory(openHelperFactory)
                    .addMigrations(
                        ClientDatabase.MIGRATION_1_2,
                        ClientDatabase.MIGRATION_2_3,
                        ClientDatabase.MIGRATION_3_4,
                    ).build()
            try {
                // Force key validation and database creation before returning the singleton.
                database.openHelper.writableDatabase
                database
            } catch (exception: Exception) {
                database.close()
                throw exception
            }
        } finally {
            Arrays.fill(databaseKey, 0)
        }
    }

    private fun loadNativeLibrary() {
        if (nativeLibraryLoaded) return
        synchronized(LOCK) {
            if (!nativeLibraryLoaded) {
                System.loadLibrary("sqlcipher")
                nativeLibraryLoaded = true
            }
        }
    }

    private fun getOrCreateDatabaseKey(
        context: Context,
        databaseFile: File,
        databaseName: String,
    ): ByteArray {
        val preferences = context.getSharedPreferences(KEY_PREFERENCES_NAME, Context.MODE_PRIVATE)
        val preferenceKey = wrappedKeyPreference(databaseName)
        val storedEnvelope = preferences.getString(preferenceKey, null)
        val protector = AndroidKeyStoreProtectedBlobProtector(DATABASE_KEYSTORE_ALIAS)

        if (storedEnvelope != null) {
            val envelope = Base64.decode(storedEnvelope, Base64.NO_WRAP)
            try {
                val key = protector.unprotect(envelope)
                if (key.size != DATABASE_KEY_BYTES) {
                    Arrays.fill(key, 0)
                    throw GeneralSecurityException("The protected database key has an invalid length.")
                }
                return key
            } finally {
                Arrays.fill(envelope, 0)
            }
        }

        // A missing key for a file that is not clearly plaintext must never create a new key.
        if (databaseFile.exists() && !hasPlaintextSQLiteHeader(databaseFile)) {
            throw GeneralSecurityException(
                "The encrypted database exists but its protected key is unavailable; refusing to replace it.",
            )
        }

        val key = ByteArray(DATABASE_KEY_BYTES)
        SecureRandom().nextBytes(key)
        val envelope = protector.protect(key)
        try {
            val encoded = Base64.encodeToString(envelope, Base64.NO_WRAP)
            if (!preferences.edit().putString(preferenceKey, encoded).commit()) {
                throw IOException("The protected database key could not be committed to app-private preferences.")
            }
        } catch (exception: Exception) {
            Arrays.fill(key, 0)
            throw exception
        } finally {
            Arrays.fill(envelope, 0)
        }
        return key
    }

    private fun migratePlaintextDatabase(
        databaseFile: File,
        databaseKey: ByteArray,
    ) {
        if (!databaseFile.exists() || !hasPlaintextSQLiteHeader(databaseFile)) return

        val parent = databaseFile.parentFile ?: throw IOException("The database has no parent directory.")
        if (!parent.exists() && !parent.mkdirs()) {
            throw IOException("The database directory could not be created.")
        }
        val temporaryFile = File(parent, databaseFile.name + TEMPORARY_SUFFIX)

        try {
            deleteSidecars(temporaryFile)
            deleteIfPresent(temporaryFile)

            var sourceDatabase: CipherSQLiteDatabase? = null
            var attached = false
            var sourceVersion = 0
            try {
                // With no key, SQLCipher opens this legacy file as a normal SQLite database.
                val source =
                    CipherSQLiteDatabase.openDatabase(
                        databaseFile.absolutePath,
                        null,
                        CipherSQLiteDatabase.OPEN_READWRITE,
                    )
                sourceDatabase = source
                requireIntegrity(source, "PRAGMA quick_check", "legacy database")

                source.rawQuery("PRAGMA wal_checkpoint(TRUNCATE)", emptyArray<String>()).use { cursor ->
                    if (!cursor.moveToFirst() || cursor.getInt(0) != 0) {
                        throw GeneralSecurityException("The legacy database has an active or uncheckpointed WAL.")
                    }
                }
                sourceVersion = readPragmaInt(source, "PRAGMA user_version")
                val applicationId = readPragmaInt(source, "PRAGMA application_id")

                source.execSQL(
                    "ATTACH DATABASE ? AS $ENCRYPTED_DATABASE_ALIAS KEY ?",
                    arrayOf<Any>(temporaryFile.absolutePath, databaseKey),
                )
                attached = true
                source
                    .rawQuery(
                        "SELECT sqlcipher_export('$ENCRYPTED_DATABASE_ALIAS')",
                        emptyArray<String>(),
                    ).use { cursor ->
                        if (!cursor.moveToFirst()) {
                            throw GeneralSecurityException("SQLCipher returned no result for the database export.")
                        }
                    }
                source.execSQL("PRAGMA $ENCRYPTED_DATABASE_ALIAS.user_version = $sourceVersion")
                source.execSQL("PRAGMA $ENCRYPTED_DATABASE_ALIAS.application_id = $applicationId")
                requireIntegrity(
                    source,
                    "PRAGMA $ENCRYPTED_DATABASE_ALIAS.integrity_check",
                    "encrypted migration target",
                )
                source.execSQL("DETACH DATABASE $ENCRYPTED_DATABASE_ALIAS")
                attached = false
            } finally {
                if (attached) {
                    try {
                        sourceDatabase?.execSQL("DETACH DATABASE $ENCRYPTED_DATABASE_ALIAS")
                    } catch (_: Exception) {
                        // Closing the source connection also releases an attached migration target.
                    }
                }
                sourceDatabase?.close()
            }

            verifyEncryptedDatabase(temporaryFile, databaseKey, sourceVersion)
            // The source has been checkpointed and closed before its path is atomically replaced.
            deleteSidecars(databaseFile)
            Os.rename(temporaryFile.absolutePath, databaseFile.absolutePath)
        } catch (exception: Exception) {
            try {
                deleteSidecars(temporaryFile)
                deleteIfPresent(temporaryFile)
            } catch (cleanupException: Exception) {
                exception.addSuppressed(cleanupException)
            }
            throw exception
        }
    }

    private fun verifyEncryptedDatabase(
        file: File,
        databaseKey: ByteArray,
        expectedUserVersion: Int,
    ) {
        if (!file.isFile || file.length() <= 16L) {
            throw GeneralSecurityException("The encrypted migration output is missing or empty.")
        }
        if (hasPlaintextSQLiteHeader(file)) {
            throw GeneralSecurityException("The migration output still has a plaintext SQLite header.")
        }

        val encryptedDatabase =
            CipherSQLiteDatabase.openDatabase(
                file.absolutePath,
                databaseKey,
                null,
                CipherSQLiteDatabase.OPEN_READONLY,
            )
        try {
            requireIntegrity(encryptedDatabase, "PRAGMA integrity_check", "encrypted database")
            val actualVersion = readPragmaInt(encryptedDatabase, "PRAGMA user_version")
            if (actualVersion != expectedUserVersion) {
                throw GeneralSecurityException("The encrypted database version changed during migration.")
            }
        } finally {
            encryptedDatabase.close()
        }
    }

    private fun requireIntegrity(
        database: CipherSQLiteDatabase,
        pragma: String,
        label: String,
    ) {
        database.rawQuery(pragma, emptyArray<String>()).use { cursor ->
            if (!cursor.moveToFirst() || cursor.getString(0) != "ok") {
                throw GeneralSecurityException("The $label failed its integrity check.")
            }
        }
    }

    private fun readPragmaInt(
        database: CipherSQLiteDatabase,
        pragma: String,
    ): Int =
        database.rawQuery(pragma, emptyArray<String>()).use { cursor ->
            if (!cursor.moveToFirst()) {
                throw GeneralSecurityException("The database did not return the expected metadata.")
            }
            cursor.getInt(0)
        }

    private fun hasPlaintextSQLiteHeader(file: File): Boolean {
        val expectedHeader = SQLITE_HEADER.toByteArray(Charsets.US_ASCII)
        val header = ByteArray(expectedHeader.size)
        return FileInputStream(file).use { input ->
            var offset = 0
            while (offset < header.size) {
                val count = input.read(header, offset, header.size - offset)
                if (count < 0) return@use false
                offset += count
            }
            header.contentEquals(expectedHeader)
        }
    }

    private fun deleteSidecars(file: File) {
        deleteIfPresent(File(file.parentFile, file.name + "-wal"))
        deleteIfPresent(File(file.parentFile, file.name + "-shm"))
    }

    private fun deleteIfPresent(file: File) {
        if (file.exists() && !file.delete()) {
            throw IOException("A database migration temporary file could not be removed.")
        }
    }

    private val DATABASE_NAME_PATTERN = Regex("[A-Za-z0-9._-]{1,128}")
}
