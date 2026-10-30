package com.whitep.client.network

import android.content.Context
import android.util.Base64
import io.libp2p.core.PeerId
import org.junit.Assert.assertEquals
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
class AndroidPeerIdentityStoreTest {
    private lateinit var context: Context
    private val protector = TestProtector()

    @Before
    fun setUp() {
        context = RuntimeEnvironment.getApplication()
        context
            .getSharedPreferences(AndroidPeerIdentityStore.IDENTITY_PREFERENCES, Context.MODE_PRIVATE)
            .edit()
            .clear()
            .commit()
    }

    @Test
    fun loadOrCreateKeepsTheSamePeerIdAcrossStoreInstances() {
        val first = AndroidPeerIdentityStore(context, protector).loadOrCreate()
        val firstPeerId = PeerId.fromPubKey(first.publicKey()).toBase58()
        val second = AndroidPeerIdentityStore(context, protector).loadOrCreate()
        val secondPeerId = PeerId.fromPubKey(second.publicKey()).toBase58()
        assertEquals(firstPeerId, secondPeerId)
    }

    @Test
    fun corruptExistingIdentityFailsClosedInsteadOfRotatingIt() {
        context
            .getSharedPreferences(AndroidPeerIdentityStore.IDENTITY_PREFERENCES, Context.MODE_PRIVATE)
            .edit()
            .putString(
                AndroidPeerIdentityStore.IDENTITY_ENVELOPE_KEY,
                Base64.encodeToString(byteArrayOf(0x01, 0x02, 0x03), Base64.NO_WRAP),
            ).commit()

        assertThrows(IllegalStateException::class.java) {
            AndroidPeerIdentityStore(context, protector).loadOrCreate()
        }
    }

    private class TestProtector : ProtectedBlobProtector {
        private val key = SecretKeySpec(ByteArray(32) { index -> (index + 1).toByte() }, "AES")

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
