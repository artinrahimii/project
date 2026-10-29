package com.whitep.client.data

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import androidx.room.TypeConverter
import androidx.room.TypeConverters

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
    version = 1,
    exportSchema = true,
)
@TypeConverters(ClientDatabaseConverters::class)
abstract class ClientDatabase : RoomDatabase() {
    abstract fun conversationDao(): ConversationDao

    abstract fun messageDao(): MessageDao

    abstract fun requestStateDao(): RequestStateDao

    abstract fun clientRequestDraftDao(): ClientRequestDraftDao

    companion object {
        private const val DATABASE_NAME = "whitep-client.db"

        @Volatile
        private var instance: ClientDatabase? = null

        fun getInstance(context: Context): ClientDatabase =
            instance ?: synchronized(this) {
                instance ?: Room
                    .databaseBuilder(
                        context.applicationContext,
                        ClientDatabase::class.java,
                        DATABASE_NAME,
                    ).build()
                    .also { database -> instance = database }
            }
    }
}
