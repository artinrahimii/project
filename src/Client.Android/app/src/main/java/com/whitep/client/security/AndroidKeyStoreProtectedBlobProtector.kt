package com.whitep.client.security

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import java.security.GeneralSecurityException
import java.security.KeyStore
import java.util.Arrays
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

/** Purpose-separated boundary for small app-private records protected by Android Keystore. */
internal interface ProtectedBlobProtector {
    fun protect(cleartext: ByteArray): ByteArray

    fun unprotect(envelope: ByteArray): ByteArray
}

/**
 * Versioned AES-256-GCM envelope backed by a non-exportable Android Keystore AES key.
 * Envelope: version byte || 96-bit random nonce || ciphertext || 128-bit authentication tag.
 */
internal class AndroidKeyStoreProtectedBlobProtector(
    private val keyAlias: String,
) : ProtectedBlobProtector {
    init {
        require(KEY_ALIAS_PATTERN.matches(keyAlias)) { "A bounded Android Keystore alias is required." }
    }

    override fun protect(cleartext: ByteArray): ByteArray {
        require(cleartext.isNotEmpty() && cleartext.size <= MAXIMUM_CLEAR_TEXT_BYTES) {
            "Protected data is empty or exceeds its configured size limit."
        }
        val aad = additionalData()
        try {
            val cipher = Cipher.getInstance(TRANSFORMATION)
            cipher.init(Cipher.ENCRYPT_MODE, getOrCreateKey())
            cipher.updateAAD(aad)
            val ciphertext = cipher.doFinal(cleartext)
            try {
                val nonce = cipher.iv
                try {
                    if (nonce.size != NONCE_BYTES) {
                        throw GeneralSecurityException("Android Keystore returned an unexpected GCM nonce size.")
                    }
                    return ByteArray(1 + nonce.size + ciphertext.size).also { envelope ->
                        envelope[0] = ENVELOPE_VERSION
                        nonce.copyInto(envelope, 1)
                        ciphertext.copyInto(envelope, 1 + nonce.size)
                    }
                } finally {
                    Arrays.fill(nonce, 0)
                }
            } finally {
                Arrays.fill(ciphertext, 0)
            }
        } finally {
            Arrays.fill(aad, 0)
        }
    }

    override fun unprotect(envelope: ByteArray): ByteArray {
        if (envelope.size !in MIN_ENVELOPE_BYTES..MAX_ENVELOPE_BYTES || envelope[0] != ENVELOPE_VERSION) {
            throw GeneralSecurityException("The protected record envelope is invalid or unsupported.")
        }
        val nonce = envelope.copyOfRange(1, 1 + NONCE_BYTES)
        val ciphertext = envelope.copyOfRange(1 + NONCE_BYTES, envelope.size)
        val aad = additionalData()
        try {
            val cipher = Cipher.getInstance(TRANSFORMATION)
            cipher.init(Cipher.DECRYPT_MODE, existingKey(), GCMParameterSpec(TAG_BITS, nonce))
            cipher.updateAAD(aad)
            val cleartext = cipher.doFinal(ciphertext)
            if (cleartext.size > MAXIMUM_CLEAR_TEXT_BYTES) {
                Arrays.fill(cleartext, 0)
                throw GeneralSecurityException("The unprotected record exceeds its configured size limit.")
            }
            return cleartext
        } finally {
            Arrays.fill(nonce, 0)
            Arrays.fill(ciphertext, 0)
            Arrays.fill(aad, 0)
        }
    }

    private fun additionalData(): ByteArray = byteArrayOf(ENVELOPE_VERSION) + keyAlias.toByteArray(Charsets.UTF_8)

    private fun getOrCreateKey(): SecretKey =
        synchronized(KEYSTORE_LOCK) {
            val store = loadKeyStore()
            if (store.containsAlias(keyAlias)) {
                val entry =
                    store.getEntry(keyAlias, null) as? KeyStore.SecretKeyEntry
                        ?: throw GeneralSecurityException("The Android Keystore alias has an unexpected entry type.")
                return@synchronized validateKey(entry.secretKey)
            }
            val generator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, ANDROID_KEY_STORE)
            val spec =
                KeyGenParameterSpec
                    .Builder(
                        keyAlias,
                        KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT,
                    ).setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                    .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                    .setRandomizedEncryptionRequired(true)
                    .setKeySize(AES_KEY_SIZE_BITS)
                    .build()
            generator.init(spec)
            validateKey(generator.generateKey())
        }

    private fun existingKey(): SecretKey {
        val store = loadKeyStore()
        if (!store.containsAlias(keyAlias)) {
            throw GeneralSecurityException("The Android Keystore key for an existing protected record is missing.")
        }
        val entry =
            store.getEntry(keyAlias, null) as? KeyStore.SecretKeyEntry
                ?: throw GeneralSecurityException("The Android Keystore key for an existing record is invalid.")
        return validateKey(entry.secretKey)
    }

    private fun validateKey(key: SecretKey): SecretKey {
        if (!key.algorithm.equals(KeyProperties.KEY_ALGORITHM_AES, ignoreCase = true)) {
            throw GeneralSecurityException("The Android Keystore entry is not an AES key.")
        }
        val encoded = key.encoded
        if (encoded != null) {
            Arrays.fill(encoded, 0)
            throw GeneralSecurityException("The Android Keystore returned an exportable key.")
        }
        return key
    }

    private fun loadKeyStore(): KeyStore = KeyStore.getInstance(ANDROID_KEY_STORE).apply { load(null) }

    private companion object {
        val KEYSTORE_LOCK = Any()
        val KEY_ALIAS_PATTERN = Regex("[A-Za-z0-9._-]{1,128}")
        const val ANDROID_KEY_STORE = "AndroidKeyStore"
        const val TRANSFORMATION = "AES/GCM/NoPadding"
        const val ENVELOPE_VERSION: Byte = 1
        const val AES_KEY_SIZE_BITS = 256
        const val NONCE_BYTES = 12
        const val TAG_BITS = 128
        const val TAG_BYTES = 16
        const val MAXIMUM_CLEAR_TEXT_BYTES = 64 * 1024
        const val MIN_ENVELOPE_BYTES = 1 + NONCE_BYTES + TAG_BYTES
        const val MAX_ENVELOPE_BYTES = 1 + NONCE_BYTES + MAXIMUM_CLEAR_TEXT_BYTES + TAG_BYTES
    }
}
