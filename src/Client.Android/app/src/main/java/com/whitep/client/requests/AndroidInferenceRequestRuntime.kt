package com.whitep.client.requests

import android.content.Context
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.network.AndroidPeerIdentityStore
import com.whitep.client.network.AndroidPeerTrustStore
import com.whitep.client.network.Libp2pInferenceRequestTransport
import com.whitep.client.network.PeerTrustVerifier
import com.whitep.client.network.RendezvousPeerDirectoryClient
import com.whitep.client.network.RendezvousPeerEndpoint
import io.libp2p.core.crypto.PrivKey
import org.whitep.shared.contracts.v1.InferenceResponse
import java.io.Closeable
import java.net.URI
import java.time.Duration

/**
 * Application composition root for authenticated Android inference dispatch.
 *
 * The production factory uses the persistent Android identity, explicit peer-trust store,
 * HTTPS rendezvous directory and concrete libp2p TCP/Noise transport. It does not trust peers
 * automatically and does not invent a rendezvous URL or host capability snapshot.
 *
 * Callers must supply HostCandidate values obtained from a separately validated discovery/status
 * source. A directory entry alone contains routing metadata, not proof of model readiness.
 */
class AndroidInferenceRequestRuntime private constructor(
    private val transport: Libp2pInferenceRequestTransport,
    private val peerTrustStore: AndroidPeerTrustStore?,
) : Closeable {
    private val dispatcher = InferenceAttemptDispatcher(transport)

    internal constructor(
        localIdentity: PrivKey,
        resolveEndpoint: suspend (String) -> RendezvousPeerEndpoint?,
        trustVerifier: PeerTrustVerifier,
        connectionTimeout: Duration = DEFAULT_CONNECTION_TIMEOUT,
    ) : this(
        Libp2pInferenceRequestTransport(
            localIdentity = localIdentity,
            resolveEndpoint = resolveEndpoint,
            trustVerifier = trustVerifier,
            connectionTimeout = connectionTimeout,
        ),
        null,
    )

    /**
     * Creates the production runtime without connecting to a peer.
     * The caller must pass the HTTPS rendezvous service URL configured by the user/operator.
     */
    companion object {
        private val DEFAULT_CONNECTION_TIMEOUT: Duration = Duration.ofSeconds(15)
        private val DEFAULT_RESPONSE_TIMEOUT: Duration = Duration.ofSeconds(60)

        fun create(
            context: Context,
            rendezvousServiceBaseUri: URI,
            connectionTimeout: Duration = DEFAULT_CONNECTION_TIMEOUT,
        ): AndroidInferenceRequestRuntime {
            val applicationContext = context.applicationContext
            val identity = AndroidPeerIdentityStore(applicationContext).loadOrCreate()
            val trustStore = AndroidPeerTrustStore(applicationContext)
            val directory = RendezvousPeerDirectoryClient(rendezvousServiceBaseUri)
            val transport =
                Libp2pInferenceRequestTransport(
                    localIdentity = identity,
                    resolveEndpoint = directory::resolve,
                    trustVerifier = trustStore,
                    connectionTimeout = connectionTimeout,
                )
            return AndroidInferenceRequestRuntime(transport, trustStore)
        }
    }

    fun isPeerTrusted(peerId: String): Boolean = requirePeerTrustStore().isTrusted(peerId)

    fun trustPeerAfterExplicitConfirmation(
        peerId: String,
        explicitlyConfirmed: Boolean,
    ) {
        requirePeerTrustStore().trustPeerAfterExplicitConfirmation(peerId, explicitlyConfirmed)
    }

    fun revokePeerTrust(peerId: String) {
        requirePeerTrustStore().revokeTrust(peerId)
    }

    suspend fun dispatchRound(
        distributedRequest: DistributedRequest,
        selectedHosts: List<HostCandidate>,
        modelId: String,
        currentMessage: String,
        capsule: ConversationContextCapsule,
        maxOutputTokens: Int,
        protocolVersion: String,
    ): InferenceRoundDispatchResult =
        dispatcher.dispatchRound(
            distributedRequest = distributedRequest,
            selectedHosts = selectedHosts,
            modelId = modelId,
            currentMessage = currentMessage,
            capsule = capsule,
            maxOutputTokens = maxOutputTokens,
            protocolVersion = protocolVersion,
        )

    suspend fun awaitResponse(
        requestId: String,
        attemptId: String,
        timeout: Duration = DEFAULT_RESPONSE_TIMEOUT,
    ): InferenceResponse =
        transport.awaitResponse(
            requestId = requestId,
            attemptId = attemptId,
            timeout = timeout,
        )

    override fun close() {
        transport.close()
    }

    private fun requirePeerTrustStore(): AndroidPeerTrustStore =
        peerTrustStore
            ?: throw IllegalStateException(
                "Peer-trust changes require the application-scoped AndroidInferenceRequestRuntime.",
            )
}
