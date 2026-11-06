package com.whitep.client.data

import android.content.Context
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
class ConversationContextCapsuleSizeLimitStoreTest {
    private val context = RuntimeEnvironment.getApplication()

    @Before
    fun setUp() {
        context
            .getSharedPreferences(
                SharedPreferencesConversationContextCapsuleSizeLimitStore.PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            ).edit()
            .clear()
            .commit()
    }

    @After
    fun tearDown() {
        context
            .getSharedPreferences(
                SharedPreferencesConversationContextCapsuleSizeLimitStore.PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            ).edit()
            .clear()
            .commit()
    }

    @Test
    fun defaultsAndPersistsDeviceWideLimitAcrossStoreInstances() {
        val store = SharedPreferencesConversationContextCapsuleSizeLimitStore(context)
        assertEquals(4_096, store.loadMaxSerializedBytes())
        assertTrue(store.saveMaxSerializedBytes(2_048))

        val reopenedStore = SharedPreferencesConversationContextCapsuleSizeLimitStore(context)
        assertEquals(2_048, reopenedStore.loadMaxSerializedBytes())
    }

    @Test
    fun rejectsOffStepAndOutOfRangeLimitsWithoutChangingSavedValue() {
        val store = SharedPreferencesConversationContextCapsuleSizeLimitStore(context)
        assertEquals(4_096, store.loadMaxSerializedBytes())
        assertFalse(store.saveMaxSerializedBytes(511))
        assertFalse(store.saveMaxSerializedBytes(513))
        assertFalse(store.saveMaxSerializedBytes(4_352))
        assertEquals(4_096, store.loadMaxSerializedBytes())
    }

    @Test
    fun normalizesCorruptedStoredValueToProtocolCeiling() {
        context
            .getSharedPreferences(
                SharedPreferencesConversationContextCapsuleSizeLimitStore.PREFERENCES_NAME,
                Context.MODE_PRIVATE,
            ).edit()
            .putInt(SharedPreferencesConversationContextCapsuleSizeLimitStore.KEY_MAX_SERIALIZED_BYTES, 9_999)
            .commit()

        assertEquals(
            4_096,
            SharedPreferencesConversationContextCapsuleSizeLimitStore(context).loadMaxSerializedBytes(),
        )
    }
}
