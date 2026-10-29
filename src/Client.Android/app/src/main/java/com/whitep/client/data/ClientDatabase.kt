package com.whitep.client.data

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.TypeConverter
import androidx.room.TypeConverters
import androidx.room.migration.Migration
import androidx.sqlite.db.SupportSQLiteDatabase

class ClientDatabaseConverters {
    @TypeConverter
    fun persistedRequestStatusToString(value: PersistedRequestStatus): String = value.name

    @TypeConverter
    fun stringToPersistedRequestStatus(value: String): PersistedRequestStatus = PersistedRequestStatus.valueOf(value)

    @TypeConverter
    fun requestDraftStatusToString(value: RequestDraftStatus): String = value.name

    @TypeConverter
    fun stringToRequestDraftStatus(value: String): RequestDraftStatus = RequestDraftStatus.valueOf(value)
}

@Database(
    entities = [
        ConversationEntity::class,
        MessageEntity::class,
        RequestStateEntity::class,
        ClientRequestDraftEntity::class,
    ],
    version = 2,
    exportSchema = true,
)
@TypeConverters(ClientDatabaseConverters::class)
abstract class ClientDatabase : RoomDatabase() {
    abstract fun conversationDao(): ConversationDao

    abstract fun messageDao(): MessageDao

    abstract fun requestStateDao(): RequestStateDao

    abstract fun clientRequestDraftDao(): ClientRequestDraftDao

    companion object {
        internal const val DATABASE_NAME = "whitep-client.db"

        val MIGRATION_1_2: Migration =
            object : Migration(1, 2) {
                override fun migrate(db: SupportSQLiteDatabase) {
                    db.execSQL(
                        "CREATE INDEX IF NOT EXISTS index_request_states_conversationId_createdAtEpochMillis_requestId " +
                            "ON request_states (conversationId, createdAtEpochMillis, requestId)",
                    )
                }
            }

        @Volatile
        private var instance: ClientDatabase? = null

        fun getInstance(context: Context): ClientDatabase =
            instance ?: synchronized(this) {
                instance ?: Room
                    .databaseBuilder(
                        context.applicationContext,
                        ClientDatabase::class.java,
                        DATABASE_NAME,
                    ).addMigrations(MIGRATION_1_2)
                    .build()
                    .also { database -> instance = database }
            }
    }
}
