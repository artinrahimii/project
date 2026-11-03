package com.whitep.client.network

import android.content.Context
import android.content.SharedPreferences
import android.util.Base64
import com.whitep.client.security.AndroidKeyStoreProtectedBlobProtector
import com.whitep.client.security.ProtectedBlobProtector
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.KeyType
import io.libp2p.core.crypto.PrivKey
import io.libp2p.core.crypto.generateKeyPair
import io.libp2p.core.crypto.unmarshalPrivateKey
import java.util.Arrays

/**
 * Stable Android libp2p identity. Private-key bytes are encrypted with a non-exportable
 * Android Keystore AES key. An existing unreadable/corrupt record fails closed and is
 * never silently replaced with a new PeerId.
 */
class AndroidPeerIdentityStore internal constructor(
    context: Context,
    private val protector: ProtectedBlobProtector,
) {
    constructor(context: Context) : this(
        context.applicationContext,
        AndroidKeyStoreProtectedBlobProtector(IDENTITY_KEY_ALIAS),
    )

    private val preferences: SharedPreferences =
        context.applicationContext.getSharedPreferences(IDENTITY_PREFERENCES, Context.MODE_PRIVATE)

    fun loadOrCreate(): PrivKey =
        synchronized(IDENTITY_LOCK) {
            val stored = preferences.getString(IDENTITY_ENVELOPE_KEY, null)
            if (stored != null) {
                val envelope = decodeStoredEnvelope(stored)
                val cleartext =
                    try {
                        protector.unprotect(envelope)
                    } catch (exception: Exception) {
                        throw IllegalStateException(
                            "The stored Android peer identity could not be authenticated.",
                            exception,
                        )
                    } finally {
                        Arrays.fill(envelope, 0)
                    }
                return try {
                    unmarshalPrivateKey(cleartext)
                } catch (exception: Exception) {
                    throw IllegalStateException(
                        "The authenticated Android peer identity is malformed.",
                        exception,
                    )
                } finally {
                    Arrays.fill(cleartext, 0)
                }
            }

            val generated = generateKeyPair(KeyType.ED25519).first
            val cleartext = generated.bytes()
            val envelope =
                try {
                    protector.protect(cleartext)
                } finally {
                    Arrays.fill(cleartext, 0)
                }
            val encoded = Base64.encodeToString(envelope, Base64.NO_WRAP)
            Arrays.fill(envelope, 0)
            if (!preferences.edit().putString(IDENTITY_ENVELOPE_KEY, encoded).commit()) {
                throw IllegalStateException("The new Android peer identity could not be saved.")
            }
            return generated
        }

    fun loadOrCreatePeerId(): String = PeerId.fromPubKey(loadOrCreate().publicKey()).toBase58()

    private fun decodeStoredEnvelope(value: String): ByteArray =
        try {
            Base64.decode(value, Base64.NO_WRAP)
        } catch (exception: IllegalArgumentException) {
            throw IllegalStateException("The stored Android peer identity envelope is malformed.", exception)
        }

    internal companion object {
        private val IDENTITY_LOCK = Any()
        const val IDENTITY_PREFERENCES = "whitep_peer_identity_v1"
        const val IDENTITY_ENVELOPE_KEY = "encrypted_private_key"
        const val IDENTITY_KEY_ALIAS = "com.whitep.client.peer_identity.v1"
    }
}
