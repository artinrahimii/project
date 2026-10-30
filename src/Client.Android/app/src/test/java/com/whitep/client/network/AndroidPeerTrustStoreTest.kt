package com.whitep.client.network

import android.content.Context
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.KeyType
import io.libp2p.core.crypto.generateKeyPair
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Assert.assertThrows
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.RuntimeEnvironment
import org.robolectric.annotation.Config
import java.util.Arrays
import javax.crypto.Cipher
import javax.crypto.spec.GCMParameterSpec
import javax.crypto.spec.SecretKeySpec

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class AndroidPeerTrustStoreTest {
    private lateinit var context: Context
    private val protector = TestProtector()

    @Before
    fun setUp() {
        context = RuntimeEnvironment.getApplication()
        context
            .getSharedPreferences(AndroidPeerTrustStore.TRUST_PREFERENCES, Context.MODE_PRIVATE)
            .edit()
            .clear()
            .commit()
    }

    @Test
    fun unknownPeerRemainsUntrustedUntilExactIdentityIsConfirmed() {
        val peerId = PeerId.fromPubKey(generateKeyPair(KeyType.ED25519).second).toBase58()
        val store = AndroidPeerTrustStore(context, protector)

        assertFalse(store.isTrusted(peerId))
        assertThrows(SecurityException::class.java) {
            store.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = false)
        }
        assertFalse(store.isTrusted(peerId))

        store.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = true)
        assertTrue(store.isTrusted(peerId))
        assertTrue(AndroidPeerTrustStore(context, protector).isTrusted(peerId))
    }

    @Test
    fun revokingTheLastPeerRemovesThePersistedRecord() {
        val peerId = PeerId.fromPubKey(generateKeyPair(KeyType.ED25519).second).toBase58()
        val store = AndroidPeerTrustStore(context, protector)

        store.trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed = true)
        assertTrue(store.isTrusted(peerId))

        store.revokeTrust(peerId)

        assertFalse(store.isTrusted(peerId))
        assertFalse(AndroidPeerTrustStore(context, protector).isTrusted(peerId))
        assertFalse(
            context
                .getSharedPreferences(AndroidPeerTrustStore.TRUST_PREFERENCES, Context.MODE_PRIVATE)
                .contains(AndroidPeerTrustStore.TRUST_ENVELOPE_KEY),
        )
    }

    private class TestProtector : ProtectedBlobProtector {
        private val key = SecretKeySpec(ByteArray(32) { index -> (index + 9).toByte() }, "AES")

        override fun protect(cleartext: ByteArray): ByteArray {
            val cipher = Cipher.getInstance("AES/GCM/NoPadding")
            cipher.init(Cipher.ENCRYPT_MODE, key)
            return byteArrayOf(1) + cipher.iv + cipher.doFinal(cleartext)
        }

        override fun unprotect(envelope: ByteArray): ByteArray {
            require(envelope.size > 29 && envelope[0] == 1.toByte())
            val nonce = envelope.copyOfRange(1, 13)
            val ciphertext = envelope.copyOfRange(13, envelope.size)
            try {
                val cipher = Cipher.getInstance("AES/GCM/NoPadding")
                cipher.init(Cipher.DECRYPT_MODE, key, GCMParameterSpec(128, nonce))
                return cipher.doFinal(ciphertext)
            } finally {
                Arrays.fill(nonce, 0)
                Arrays.fill(ciphertext, 0)
            }
        }
    }
}
