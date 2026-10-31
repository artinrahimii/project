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
import kotlinx.coroutines.withTimeoutOrNull
import org.whitep.shared.contracts.v1.InferenceResponse
import java.io.Closeable
import java.net.URI
import java.time.Clock
import java.time.Duration
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.TimeoutException

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
    private val roundTimer: InferenceRoundTimer,
) : Closeable {
    private data class AttemptKey(
        val requestId: String,
        val attemptId: String,
    )

    private val dispatcher = InferenceAttemptDispatcher(transport)
    private val responseCollector = InferenceRoundResponseCollector()
    private val attemptRoundWindows = ConcurrentHashMap<AttemptKey, InferenceRoundTimer.Window>()

    /** Test-safe, payload-free reason for the most recent failed transport submission. */
    internal val lastSubmissionFailure: String?
        get() = transport.lastSubmissionFailure

    internal constructor(
        localIdentity: PrivKey,
        resolveEndpoint: suspend (String) -> RendezvousPeerEndpoint?,
        trustVerifier: PeerTrustVerifier,
        connectionTimeout: Duration = DEFAULT_CONNECTION_TIMEOUT,
        clock: Clock = Clock.systemUTC(),
        roundTimeout: Duration = DEFAULT_ROUND_TIMEOUT,
    ) : this(
        Libp2pInferenceRequestTransport(
            localIdentity = localIdentity,
            resolveEndpoint = resolveEndpoint,
            trustVerifier = trustVerifier,
            clock = clock,
            connectionTimeout = connectionTimeout,
        ),
        null,
        InferenceRoundTimer(clock, roundTimeout),
    )

    /**
     * Creates the production runtime without connecting to a peer.
     * The caller must pass the HTTPS rendezvous service URL configured by the user/operator.
     */
    companion object {
        private val DEFAULT_CONNECTION_TIMEOUT: Duration = Duration.ofSeconds(15)
        private val DEFAULT_RESPONSE_TIMEOUT: Duration = Duration.ofSeconds(60)
        private val DEFAULT_ROUND_TIMEOUT: Duration = InferenceRoundTimer.DEFAULT_TIMEOUT

        fun create(
            context: Context,
            rendezvousServiceBaseUri: URI,
            connectionTimeout: Duration = DEFAULT_CONNECTION_TIMEOUT,
            clock: Clock = Clock.systemUTC(),
            roundTimeout: Duration = DEFAULT_ROUND_TIMEOUT,
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
                    clock = clock,
                    connectionTimeout = connectionTimeout,
                )
            return AndroidInferenceRequestRuntime(transport, trustStore, InferenceRoundTimer(clock, roundTimeout))
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
    ): InferenceRoundDispatchResult {
        val roundWindow = roundTimer.startRound(distributedRequest.deadlineUnixMs)
        val result =
            dispatcher.dispatchRound(
                distributedRequest = distributedRequest,
                selectedHosts = selectedHosts,
                modelId = modelId,
                currentMessage = currentMessage,
                capsule = capsule,
                maxOutputTokens = maxOutputTokens,
                protocolVersion = protocolVersion,
                roundWindow = roundWindow,
            )
        if (result is InferenceRoundDispatchResult.Dispatched) {
            result.attempts
                .filter { attempt -> attempt.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED }
                .forEach { attempt ->
                    attemptRoundWindows[AttemptKey(distributedRequest.requestId, attempt.attemptId)] = roundWindow
                }
        }
        return result
    }

    suspend fun awaitResponse(
        requestId: String,
        attemptId: String,
        timeout: Duration = DEFAULT_RESPONSE_TIMEOUT,
    ): InferenceResponse {
        val key = AttemptKey(requestId, attemptId)
        val roundWindow = attemptRoundWindows[key]
        if (roundWindow == null) {
            return transport.awaitResponse(
                requestId = requestId,
                attemptId = attemptId,
                timeout = timeout,
            )
        }
        val boundedTimeout = roundTimer.boundedTimeout(roundWindow, timeout)
        if (boundedTimeout.isZero) {
            attemptRoundWindows.remove(key, roundWindow)
            throw TimeoutException("The inference round expired before this host returned a response.")
        }
        return try {
            val response =
                withTimeoutOrNull(roundWindow.remainingTimeoutMillis()) {
                    transport.awaitResponse(
                        requestId = requestId,
                        attemptId = attemptId,
                        timeout = boundedTimeout,
                    )
                }
            response ?: throw TimeoutException("The inference round expired before this host returned a response.")
        } finally {
            attemptRoundWindows.remove(key, roundWindow)
        }
    }

    /**
     * Awaits every queued response from one dispatched round.
     *
     * All queued attempts share the round window created by dispatchRound. A response from one
     * host does not end collection for the remaining hosts; each is awaited concurrently until
     * it responds or the common round deadline expires.
     */
    suspend fun awaitRoundResponses(
        requestId: String,
        attempts: List<InferenceHostAttempt>,
    ): InferenceRoundResponseCollection {
        require(requestId.isNotBlank()) { "requestId must not be blank." }
        return responseCollector.collect(
            attempts = attempts,
            roundWindowFor = { attempt ->
                attemptRoundWindows[AttemptKey(requestId, attempt.attemptId)]
            },
            awaitResponse = { attempt, timeout ->
                awaitResponse(requestId, attempt.attemptId, timeout)
            },
        )
    }

    override fun close() {
        attemptRoundWindows.clear()
        transport.close()
    }

    private fun requirePeerTrustStore(): AndroidPeerTrustStore =
        peerTrustStore
            ?: throw IllegalStateException(
                "Peer-trust changes require the application-scoped AndroidInferenceRequestRuntime.",
            )
}
