package com.whitep.client.network

import android.content.Context
import android.content.SharedPreferences
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.KeyType
import io.libp2p.core.crypto.PrivKey
import io.libp2p.core.crypto.generateKeyPair
import io.libp2p.core.crypto.unmarshalPrivateKey
import java.security.GeneralSecurityException
import java.security.KeyStore
import java.util.Arrays
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

/** Authenticated-encryption boundary for small, app-private identity/trust records. */
internal interface ProtectedBlobProtector {
    fun protect(cleartext: ByteArray): ByteArray

    fun unprotect(envelope: ByteArray): ByteArray
}

/**
 * AES-256-GCM protection backed by a non-exportable Android Keystore key.
 * The format is version byte || 96-bit nonce || authenticated ciphertext/tag.
 */
internal class AndroidKeyStoreProtectedBlobProtector(
    private val keyAlias: String,
) : ProtectedBlobProtector {
    override fun protect(cleartext: ByteArray): ByteArray {
        require(cleartext.isNotEmpty()) { "Protected data must not be empty." }
        val cipher = Cipher.getInstance(TRANSFORMATION)
        cipher.init(Cipher.ENCRYPT_MODE, getOrCreateKey())
        cipher.updateAAD(byteArrayOf(ENVELOPE_VERSION))
        val ciphertext = cipher.doFinal(cleartext)
        val nonce = cipher.iv
        require(nonce.size == GCM_NONCE_BYTES) { "Android Keystore returned an unexpected GCM nonce size." }
        return ByteArray(1 + nonce.size + ciphertext.size).also { envelope ->
            envelope[0] = ENVELOPE_VERSION
            nonce.copyInto(envelope, destinationOffset = 1)
            ciphertext.copyInto(envelope, destinationOffset = 1 + nonce.size)
        }
    }

    override fun unprotect(envelope: ByteArray): ByteArray {
        if (envelope.size < MINIMUM_ENVELOPE_BYTES || envelope[0] != ENVELOPE_VERSION) {
            throw GeneralSecurityException("The protected record envelope is invalid or unsupported.")
        }
        val nonce = envelope.copyOfRange(1, 1 + GCM_NONCE_BYTES)
        val ciphertext = envelope.copyOfRange(1 + GCM_NONCE_BYTES, envelope.size)
        try {
            val cipher = Cipher.getInstance(TRANSFORMATION)
            cipher.init(Cipher.DECRYPT_MODE, getExistingKey(), GCMParameterSpec(GCM_TAG_BITS, nonce))
            cipher.updateAAD(byteArrayOf(ENVELOPE_VERSION))
            return cipher.doFinal(ciphertext)
        } finally {
            Arrays.fill(nonce, 0)
            Arrays.fill(ciphertext, 0)
        }
    }

    private fun getOrCreateKey(): SecretKey {
        val keyStore = loadKeyStore()
        val existing = keyStore.getKey(keyAlias, null) as? SecretKey
        if (existing != null) {
            return existing
        }
        val generator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, ANDROID_KEY_STORE)
        val keySpec =
            KeyGenParameterSpec
                .Builder(
                    keyAlias,
                    KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT,
                ).setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setRandomizedEncryptionRequired(true)
                .setKeySize(256)
                .build()
        generator.init(keySpec)
        return generator.generateKey()
    }

    private fun getExistingKey(): SecretKey =
        (loadKeyStore().getKey(keyAlias, null) as? SecretKey)
            ?: throw GeneralSecurityException("The Android Keystore key for an existing protected record is missing.")

    private fun loadKeyStore(): KeyStore =
        KeyStore.getInstance(ANDROID_KEY_STORE).apply {
            load(null)
        }

    private companion object {
        const val ANDROID_KEY_STORE = "AndroidKeyStore"
        const val TRANSFORMATION = "AES/GCM/NoPadding"
        const val ENVELOPE_VERSION: Byte = 1
        const val GCM_NONCE_BYTES = 12
        const val GCM_TAG_BITS = 128
        const val MINIMUM_ENVELOPE_BYTES = 1 + GCM_NONCE_BYTES + 16
    }
}

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
                throw IllegalStateException("The stored Android peer identity could not be authenticated.", exception)
            } finally {
                Arrays.fill(envelope, 0)
            }
            return try {
                unmarshalPrivateKey(cleartext)
            } catch (exception: Exception) {
                throw IllegalStateException("The authenticated Android peer identity is malformed.", exception)
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
