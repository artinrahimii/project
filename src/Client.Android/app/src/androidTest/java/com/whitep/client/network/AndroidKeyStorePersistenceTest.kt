package com.whitep.client.network

import android.content.Context
import android.util.Base64
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.platform.app.InstrumentationRegistry
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.KeyType
import io.libp2p.core.crypto.generateKeyPair
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Assert.assertThrows
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import java.security.KeyStore
import java.util.Arrays

/**
 * Exercises the real AndroidKeyStore provider on an Android emulator/device.
 * This validates provider integration and non-exportability, not TEE/StrongBox backing.
 */
@RunWith(AndroidJUnit4::class)
class AndroidKeyStorePersistenceTest {
    private lateinit var context: Context

    @Before
    fun setUp() {
        context = InstrumentationRegistry.getInstrumentation().targetContext
        clearPreferences()
        deleteKeystoreAliases()
    }

    @After
    fun tearDown() {
        clearPreferences()
        deleteKeystoreAliases()
    }

    @Test
    fun peerIdentityUsesNonExportableKeystoreKeyAndSurvivesStoreRecreation() {
        val firstPeerId = AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        val storedEnvelope =
            context
                .getSharedPreferences(AndroidPeerIdentityStore.IDENTITY_PREFERENCES, Context.MODE_PRIVATE)
                .getString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, null)

        assertNotNull(storedEnvelope)
        assertTrue(storedEnvelope!!.isNotBlank())
        assertNonExportableAesKey(AndroidPeerIdentityStore.IDENTITY_KEY_ALIAS)
        assertEquals(firstPeerId, AndroidPeerIdentityStore(context).loadOrCreatePeerId())
    }

    @Test
    fun missingIdentityWrappingKeyFailsClosedWithoutRotatingTheStoredPeerId() {
        AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        val preferences =
            context.getSharedPreferences(AndroidPeerIdentityStore.IDENTITY_PREFERENCES, Context.MODE_PRIVATE)
        val originalEnvelope = preferences.getString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, null)
        assertNotNull(originalEnvelope)

        deleteKeystoreAlias(AndroidPeerIdentityStore.IDENTITY_KEY_ALIAS)

        assertThrows(IllegalStateException::class.java) {
            AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        }
        assertFalse(keystore().containsAlias(AndroidPeerIdentityStore.IDENTITY_KEY_ALIAS))
        assertEquals(originalEnvelope, preferences.getString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, null))
    }

    @Test
    fun identityAndPeerTrustUseDistinctNonExportableKeystoreKeys() {
        val peerIdentity = AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        val peerId = PeerId.fromPubKey(generateKeyPair(KeyType.ED25519).second).toBase58()
        val trustStore = AndroidPeerTrustStore(context)

        assertFalse(trustStore.isTrusted(peerId))
        assertThrows(SecurityException::class.java) {
            trustStore.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = false)
        }
        trustStore.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = true)

        val identityAlias = AndroidPeerIdentityStore.IDENTITY_KEY_ALIAS
        val trustAlias = AndroidPeerTrustStore.TRUST_KEY_ALIAS
        assertNotEquals(identityAlias, trustAlias)
        assertEquals(peerIdentity, AndroidPeerIdentityStore(context).loadOrCreatePeerId())
        assertTrue(AndroidPeerTrustStore(context).isTrusted(peerId))
        assertNonExportableAesKey(identityAlias)
        assertNonExportableAesKey(trustAlias)
    }

    @Test
    fun tamperedIdentityEnvelopeFailsClosedAndIsNotSilentlyReplaced() {
        AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        val preferences =
            context.getSharedPreferences(AndroidPeerIdentityStore.IDENTITY_PREFERENCES, Context.MODE_PRIVATE)
        val encoded =
            preferences.getString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, null)
                ?: throw AssertionError("The identity envelope was not persisted.")
        val envelope = Base64.decode(encoded, Base64.NO_WRAP)
        envelope[envelope.lastIndex] = (envelope.last().toInt() xor 1).toByte()
        val tampered = Base64.encodeToString(envelope, Base64.NO_WRAP)
        Arrays.fill(envelope, 0)
        assertTrue(preferences.edit().putString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, tampered).commit())

        assertThrows(IllegalStateException::class.java) {
            AndroidPeerIdentityStore(context).loadOrCreatePeerId()
        }
        assertEquals(tampered, preferences.getString(AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY, null))
    }

    @Test
    fun tamperedPeerTrustEnvelopeFailsClosedWithoutGrantingTrust() {
        val peerId = PeerId.fromPubKey(generateKeyPair(KeyType.ED25519).second).toBase58()
        AndroidPeerTrustStore(context).trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = true)
        val preferences =
            context.getSharedPreferences(AndroidPeerTrustStore.TRUST_PREFERENCES, Context.MODE_PRIVATE)
        val encoded =
            preferences.getString(AndroidPeerTrustStore.TRUST_ENVELOPE_KEY, null)
                ?: throw AssertionError("The peer-trust envelope was not persisted.")
        val envelope = Base64.decode(encoded, Base64.NO_WRAP)
        envelope[envelope.lastIndex] = (envelope.last().toInt() xor 1).toByte()
        val tampered = Base64.encodeToString(envelope, Base64.NO_WRAP)
        Arrays.fill(envelope, 0)
        assertTrue(preferences.edit().putString(AndroidPeerTrustStore.TRUST_ENVELOPE_KEY, tampered).commit())

        assertThrows(IllegalStateException::class.java) {
            AndroidPeerTrustStore(context).isTrusted(peerId)
        }
        assertEquals(tampered, preferences.getString(AndroidPeerTrustStore.TRUST_ENVELOPE_KEY, null))
    }

    private fun assertNonExportableAesKey(alias: String) {
        val key =
            (keystore().getEntry(alias, null) as? KeyStore.SecretKeyEntry)?.secretKey
                ?: throw AssertionError("The expected Android Keystore AES key was not created: $alias")
        assertEquals("AES", key.algorithm)
        assertNull("The Android Keystore key must not be exportable.", key.encoded)
    }

    private fun keystore(): KeyStore =
        KeyStore.getInstance("AndroidKeyStore").apply {
            load(null)
        }

    private fun clearPreferences() {
        listOf(
            AndroidPeerIdentityStore.IDENTITY_PREFERENCES,
            AndroidPeerTrustStore.TRUST_PREFERENCES,
        ).forEach { name ->
            assertTrue(context.getSharedPreferences(name, Context.MODE_PRIVATE).edit().clear().commit())
        }
    }

    private fun deleteKeystoreAliases() {
        deleteKeystoreAlias(AndroidPeerIdentityStore.IDENTITY_KEY_ALIAS)
        deleteKeystoreAlias(AndroidPeerTrustStore.TRUST_KEY_ALIAS)
    }

    private fun deleteKeystoreAlias(alias: String) {
        val keyStore = keystore()
        if (keyStore.containsAlias(alias)) {
            keyStore.deleteEntry(alias)
        }
    }
}
