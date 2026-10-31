package com.whitep.client.network

import android.content.Context
import android.content.SharedPreferences
import android.util.Base64
import io.libp2p.core.PeerId
import java.nio.charset.StandardCharsets
import java.util.Arrays

fun interface PeerTrustVerifier {
    fun isTrusted(peerId: String): Boolean
}

/**
 * Persists canonical remote PeerIds only after explicit user approval. The record is
 * authenticated-encrypted with a separate Android Keystore key and fails closed on
 * corruption, missing keys or invalid stored identities.
 */
class AndroidPeerTrustStore internal constructor(
    context: Context,
    private val protector: ProtectedBlobProtector,
) : PeerTrustVerifier {
    constructor(context: Context) : this(
        context.applicationContext,
        AndroidKeyStoreProtectedBlobProtector(TRUST_KEY_ALIAS),
    )

    private val preferences: SharedPreferences =
        context.applicationContext.getSharedPreferences(TRUST_PREFERENCES, Context.MODE_PRIVATE)

    override fun isTrusted(peerId: String): Boolean =
        synchronized(TRUST_LOCK) {
            val canonical = validatePeerId(peerId)
            loadTrustedPeers().contains(canonical)
        }

    fun trustPeerAfterExplicitConfirmation(
        peerId: String,
        explicitlyConfirmed: Boolean,
    ) {
        synchronized(TRUST_LOCK) {
            if (!explicitlyConfirmed) {
                throw SecurityException("Peer trust requires explicit user confirmation.")
            }
            val canonical = validatePeerId(peerId)
            val peers = loadTrustedPeers().toMutableSet()
            if (canonical in peers) {
                return
            }
            if (peers.size >= MAXIMUM_TRUSTED_PEERS) {
                throw IllegalStateException("The Android peer trust list has reached its configured limit.")
            }
            peers.add(canonical)
            saveTrustedPeers(peers)
        }
    }

    fun revokeTrust(peerId: String) =
        synchronized(TRUST_LOCK) {
            val canonical = validatePeerId(peerId)
            val peers = loadTrustedPeers().toMutableSet()
            if (peers.remove(canonical)) {
                saveTrustedPeers(peers)
            }
        }

    private fun loadTrustedPeers(): Set<String> {
        val stored = preferences.getString(TRUST_ENVELOPE_KEY, null) ?: return emptySet()
        val envelope =
            try {
                Base64.decode(stored, Base64.NO_WRAP)
            } catch (exception: IllegalArgumentException) {
                throw IllegalStateException("The Android peer trust envelope is malformed.", exception)
            }
        val cleartext =
            try {
                protector.unprotect(envelope)
            } catch (exception: Exception) {
                throw IllegalStateException("The Android peer trust record could not be authenticated.", exception)
            } finally {
                Arrays.fill(envelope, 0)
            }
        try {
            if (cleartext.size > MAXIMUM_TRUST_RECORD_BYTES) {
                throw IllegalStateException("The Android peer trust record exceeds its size limit.")
            }
            val text = String(cleartext, StandardCharsets.UTF_8)
            if (text.isEmpty()) {
                throw IllegalStateException("The Android peer trust record is empty.")
            }
            val peers = text.split('\n')
            if (peers.size > MAXIMUM_TRUSTED_PEERS || peers.any(String::isBlank) || peers.toSet().size != peers.size) {
                throw IllegalStateException("The Android peer trust record has an invalid structure.")
            }
            peers.forEach(::validatePeerId)
            return peers.toSet()
        } finally {
            Arrays.fill(cleartext, 0)
        }
    }

    private fun saveTrustedPeers(peers: Set<String>) {
        if (peers.isEmpty()) {
            if (!preferences.edit().remove(TRUST_ENVELOPE_KEY).commit()) {
                throw IllegalStateException("The Android peer trust decision could not be removed.")
            }
            return
        }
        val cleartext = peers.sorted().joinToString("\n").toByteArray(StandardCharsets.UTF_8)
        val envelope =
            try {
                protector.protect(cleartext)
            } finally {
                Arrays.fill(cleartext, 0)
            }
        val encoded = Base64.encodeToString(envelope, Base64.NO_WRAP)
        Arrays.fill(envelope, 0)
        if (!preferences.edit().putString(TRUST_ENVELOPE_KEY, encoded).commit()) {
            throw IllegalStateException("The Android peer trust decision could not be saved.")
        }
    }

    private fun validatePeerId(peerId: String): String {
        if (peerId.isBlank() || peerId != peerId.trim() || peerId.any(Char::isISOControl)) {
            throw IllegalArgumentException("A normalized libp2p PeerId is required.")
        }
        val canonical =
            try {
                PeerId.fromBase58(peerId).toBase58()
            } catch (exception: Exception) {
                throw IllegalArgumentException("The supplied value is not a valid libp2p PeerId.", exception)
            }
        if (canonical != peerId) {
            throw IllegalArgumentException("The supplied PeerId is not in canonical base58 form.")
        }
        return canonical
    }

    internal companion object {
        private val TRUST_LOCK = Any()
        const val TRUST_PREFERENCES = "whitep_peer_trust_v1"
        const val TRUST_ENVELOPE_KEY = "encrypted_trusted_peer_ids"
        const val TRUST_KEY_ALIAS = "com.whitep.client.peer_trust.v1"
        const val MAXIMUM_TRUSTED_PEERS = 256
        const val MAXIMUM_TRUST_RECORD_BYTES = 16 * 1024
    }
}
