package com.whitep.client.data

import android.content.Context
import com.whitep.client.context.ConversationContextCapsule

/** Persists the device-wide capsule limit. Invalid values never enter the saved preference. */
interface ConversationContextCapsuleSizeLimitStore {
    fun loadMaxSerializedBytes(): Int

    fun saveMaxSerializedBytes(value: Int): Boolean
}

class SharedPreferencesConversationContextCapsuleSizeLimitStore(
    context: Context,
) : ConversationContextCapsuleSizeLimitStore {
    private val preferences =
        context.applicationContext.getSharedPreferences(
            PREFERENCES_NAME,
            Context.MODE_PRIVATE,
        )

    override fun loadMaxSerializedBytes(): Int {
        val stored =
            preferences.getInt(
                KEY_MAX_SERIALIZED_BYTES,
                ConversationContextCapsule.DEFAULT_MAX_SERIALIZED_BYTES,
            )
        val normalized = ConversationContextCapsule.normalizeConfiguredSizeLimit(stored)
        if (stored != normalized) {
            preferences.edit().putInt(KEY_MAX_SERIALIZED_BYTES, normalized).apply()
        }
        return normalized
    }

    override fun saveMaxSerializedBytes(value: Int): Boolean {
        if (!ConversationContextCapsule.isValidConfiguredSizeLimit(value)) return false
        preferences.edit().putInt(KEY_MAX_SERIALIZED_BYTES, value).apply()
        return true
    }

    internal companion object {
        const val PREFERENCES_NAME = "whitep-context-capsule-settings"
        const val KEY_MAX_SERIALIZED_BYTES = "max-serialized-bytes"
    }
}
