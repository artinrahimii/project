package com.whitep.client.network

import com.whitep.client.requests.InferenceRequestTransport
import com.whitep.client.requests.InferenceTransportSubmission
import io.libp2p.core.Host
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.PrivKey
import io.libp2p.core.dsl.host
import io.libp2p.core.multiformats.Multiaddr
import io.libp2p.core.multistream.ProtocolBinding
import io.libp2p.core.mux.StreamMuxerProtocol
import io.libp2p.etc.SimpleClientHandler
import io.libp2p.etc.createSimpleBinding
import io.netty.buffer.ByteBuf
import io.netty.buffer.Unpooled
import io.netty.channel.ChannelHandlerContext
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.coroutines.withContext
import kotlinx.coroutines.withTimeout
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.InferenceResponse
import java.io.Closeable
import java.io.IOException
import java.time.Clock
import java.time.Duration
import java.util.Arrays
import java.util.concurrent.CompletableFuture
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicBoolean

/**
 * Android outbound libp2p adapter for the Windows /white-p/peer-message/1.0.0 protocol.
 * The Standard libp2p host builder configures direct TCP, NoiseXX and Mplex. The selected
 * PeerId must be explicitly trusted and must match the metadata-only HTTPS directory entry.
 *
 * QUEUED means the local stream write completed. Use awaitResponse to confirm that the
 * authenticated remote peer returned a correlated InferenceResponse.
 */
class Libp2pInferenceRequestTransport(
    private val localIdentity: PrivKey,
    private val resolveEndpoint: suspend (String) -> RendezvousPeerEndpoint?,
    private val trustVerifier: PeerTrustVerifier,
    private val clock: Clock = Clock.systemUTC(),
    private val connectionTimeout: Duration = DEFAULT_CONNECTION_TIMEOUT,
) : InferenceRequestTransport,
    Closeable {
    private data class AttemptKey(
        val requestId: String,
        val attemptId: String,
    )

    private class PendingAttempt(
        val peerId: String,
        val modelId: String,
    ) {
        val response = CompletableDeferred<InferenceResponse>()

        @Volatile
        var controller: InferencePeerMessageController? = null
    }

    private val closed = AtomicBoolean(false)
    private val startMutex = Mutex()
    private val pendingAttempts = ConcurrentHashMap<AttemptKey, PendingAttempt>()
    private var hostStarted = false

    @Volatile
    internal var lastSubmissionFailure: String? = null
        private set
    private val protocolBinding =
        createSimpleBinding(PROTOCOL_ID) {
            InferencePeerMessageController(::onIncomingFrame, ::onPeerClosed)
        }

    private val libp2pHost: Host =
        host {
            identity {
                factory = { localIdentity }
            }
            muxers {
                // The Windows .NET host offers Yamux; retain Mplex for existing JVM peers.
                +StreamMuxerProtocol.getYamux()
                +StreamMuxerProtocol.Mplex
            }
            protocols {
                add(protocolBindingForHost())
            }
        }

    init {
        require(!connectionTimeout.isNegative && !connectionTimeout.isZero) {
            "The libp2p connection timeout must be positive."
        }
    }

    /** Stable local PeerId to display during explicit pairing with the Windows host operator. */
    val localPeerId: String
        get() = libp2pHost.peerId.toBase58()

    override suspend fun submit(
        ephemeralHostId: String,
        request: InferenceRequest,
    ): InferenceTransportSubmission {
        lastSubmissionFailure = null
        if (closed.get() ||
            ephemeralHostId.isBlank() ||
            request.requestId.isBlank() ||
            request.attemptId.isBlank() ||
            request.modelId.isBlank()
        ) {
            lastSubmissionFailure = "invalid_request_or_closed_transport"
            return InferenceTransportSubmission.REJECTED
        }

        val explicitlyTrusted =
            try {
                trustVerifier.isTrusted(ephemeralHostId)
            } catch (_: Exception) {
                lastSubmissionFailure = "peer_trust_lookup_failed"
                false
            }
        if (!explicitlyTrusted) {
            if (lastSubmissionFailure == null) {
                lastSubmissionFailure = "peer_not_explicitly_trusted"
            }
            return InferenceTransportSubmission.REJECTED
        }

        val endpoint = resolveEndpoint(ephemeralHostId)
        if (endpoint == null) {
            lastSubmissionFailure = "directory_peer_not_found"
            return InferenceTransportSubmission.REJECTED
        }
        if (endpoint.peerId != ephemeralHostId || !endpoint.expiresAtUtc.isAfter(clock.instant())) {
            lastSubmissionFailure = "directory_peer_identity_or_expiry_mismatch"
            return InferenceTransportSubmission.REJECTED
        }

        val remotePeerId =
            try {
                PeerId.fromBase58(endpoint.peerId)
            } catch (_: IllegalArgumentException) {
                lastSubmissionFailure = "directory_peer_id_is_not_canonical"
                return InferenceTransportSubmission.REJECTED
            }
        if (remotePeerId.toBase58() != endpoint.peerId) {
            return InferenceTransportSubmission.REJECTED
        }

        val addresses =
            try {
                endpoint.tcpMultiaddresses.map { address ->
                    require(address.endsWith("/p2p/${endpoint.peerId}")) {
                        "The direct multiaddress must end with the announced PeerId."
                    }
                    require(address.contains("/tcp/") && !address.contains("/p2p-circuit")) {
                        "Only direct TCP multiaddresses are supported by this Android transport."
                    }
                    Multiaddr.fromString(address)
                }
            } catch (exception: Exception) {
                lastSubmissionFailure = "directory_address_rejected:${exception.javaClass.simpleName}"
                return InferenceTransportSubmission.REJECTED
            }
        if (addresses.isEmpty()) {
            lastSubmissionFailure = "directory_has_no_direct_tcp_address"
            return InferenceTransportSubmission.REJECTED
        }

        val requestWire = request.toByteArray()
        try {
            if (requestWire.size > PeerMessageFrameCodec.DEFAULT_MAXIMUM_MESSAGE_BYTES) {
                lastSubmissionFailure = "request_frame_exceeds_transport_limit"
                return InferenceTransportSubmission.REJECTED
            }

            val key = AttemptKey(request.requestId, request.attemptId)
            val pending = PendingAttempt(endpoint.peerId, request.modelId)
            if (pendingAttempts.putIfAbsent(key, pending) != null) {
                lastSubmissionFailure = "duplicate_pending_request_and_attempt_id"
                return InferenceTransportSubmission.REJECTED
            }

            var failureStage = "start_local_libp2p_host"
            try {
                ensureHostStarted()
                failureStage = "open_authenticated_peer_stream"
                val promise =
                    libp2pHost.newStream<InferencePeerMessageController>(
                        listOf(PROTOCOL_ID),
                        remotePeerId,
                        *addresses.toTypedArray(),
                    )
                val timeoutSeconds = connectionTimeout.toSeconds().coerceAtLeast(1L)
                failureStage = "await_peer_stream"
                val stream =
                    withContext(Dispatchers.IO) {
                        promise.stream.get(timeoutSeconds, TimeUnit.SECONDS)
                    }
                failureStage = "await_peer_protocol_controller"
                val controller =
                    withContext(Dispatchers.IO) {
                        promise.controller.get(timeoutSeconds, TimeUnit.SECONDS)
                    }

                failureStage = "verify_authenticated_remote_peer_id"
                val authenticatedPeerId = stream.remotePeerId().toBase58()
                if (authenticatedPeerId != endpoint.peerId) {
                    throw SecurityException(
                        "The authenticated libp2p PeerId did not match the trusted directory identity.",
                    )
                }

                pending.controller = controller
                failureStage = "write_request_frame"
                withContext(Dispatchers.IO) {
                    controller.sendRequest(requestWire).get(timeoutSeconds, TimeUnit.SECONDS)
                }
                return InferenceTransportSubmission.QUEUED
            } catch (cancellation: CancellationException) {
                pending.response.cancel(cancellation)
                pendingAttempts.remove(key, pending)
                pending.controller?.closeStream()
                throw cancellation
            } catch (exception: Exception) {
                lastSubmissionFailure = "$failureStage:${exception.javaClass.simpleName}"
                pending.response.completeExceptionally(
                    IOException("The authenticated peer request could not be submitted."),
                )
                pendingAttempts.remove(key, pending)
                pending.controller?.closeStream()
                return InferenceTransportSubmission.REJECTED
            }
        } finally {
            Arrays.fill(requestWire, 0)
        }
    }

    /**
     * Waits for one response. A response must arrive on the same authenticated PeerId and
     * match requestId, attemptId, modelId and the remote host identity in the payload.
     */
    suspend fun awaitResponse(
        requestId: String,
        attemptId: String,
        timeout: Duration = DEFAULT_RESPONSE_TIMEOUT,
    ): InferenceResponse {
        require(requestId.isNotBlank() && attemptId.isNotBlank()) {
            "Both requestId and attemptId are required."
        }
        require(!timeout.isNegative && !timeout.isZero) { "The response timeout must be positive." }
        val key = AttemptKey(requestId, attemptId)
        val pending =
            pendingAttempts[key]
                ?: throw IllegalStateException(
                    "No submitted attempt matches these identifiers.",
                )
        try {
            return withTimeout(timeout.toMillis().coerceAtLeast(1L)) {
                pending.response.await()
            }
        } finally {
            pendingAttempts.remove(key, pending)
            if (!pending.response.isCompleted) {
                pending.response.cancel()
            }
            pending.controller?.closeStream()
        }
    }

    private suspend fun ensureHostStarted() {
        startMutex.withLock {
            if (closed.get()) {
                throw IllegalStateException("The Android libp2p host is closed.")
            }
            if (!hostStarted) {
                withContext(Dispatchers.IO) {
                    libp2pHost.start().get(connectionTimeout.toSeconds().coerceAtLeast(1L), TimeUnit.SECONDS)
                }
                hostStarted = true
            }
        }
    }

    private fun onIncomingFrame(
        authenticatedPeerId: String,
        payload: ByteArray,
    ) {
        val response =
            try {
                InferenceResponse.parseFrom(payload)
            } catch (_: Exception) {
                failPendingAttemptsForPeer(authenticatedPeerId)
                return
            }

        val key = AttemptKey(response.requestId, response.attemptId)
        val pending = pendingAttempts[key] ?: return
        if (pending.peerId != authenticatedPeerId ||
            response.ephemeralHostId != authenticatedPeerId ||
            response.modelId != pending.modelId ||
            response.completedAtUnixMs <= 0L ||
            response.outcomeValue !in 1..5
        ) {
            pending.response.completeExceptionally(
                SecurityException("The response did not match its authenticated peer or request correlation."),
            )
            return
        }
        pending.response.complete(response)
    }

    private fun onPeerClosed(authenticatedPeerId: String) {
        failPendingAttemptsForPeer(authenticatedPeerId)
    }

    private fun failPendingAttemptsForPeer(authenticatedPeerId: String) {
        pendingAttempts.values
            .filter { attempt -> attempt.peerId == authenticatedPeerId }
            .forEach { attempt ->
                attempt.response.completeExceptionally(
                    IOException("The authenticated peer stream closed before a correlated response arrived."),
                )
            }
    }

    @Suppress("UNCHECKED_CAST")
    private fun protocolBindingForHost(): ProtocolBinding<Any> = protocolBinding as ProtocolBinding<Any>

    override fun close() {
        if (!closed.compareAndSet(false, true)) {
            return
        }
        pendingAttempts.values.forEach { pending ->
            pending.response.completeExceptionally(IOException("The Android libp2p transport was closed."))
            pending.controller?.closeStream()
        }
        pendingAttempts.clear()
        if (hostStarted) {
            try {
                libp2pHost.stop().get(SHUTDOWN_TIMEOUT_SECONDS, TimeUnit.SECONDS)
            } catch (_: Exception) {
                // Shutdown is best effort; no request or response payload is logged.
            }
        }
    }

    private companion object {
        const val PROTOCOL_ID = "/white-p/peer-message/1.0.0"
        const val SHUTDOWN_TIMEOUT_SECONDS = 5L
        val DEFAULT_CONNECTION_TIMEOUT: Duration = Duration.ofSeconds(15)
        val DEFAULT_RESPONSE_TIMEOUT: Duration = Duration.ofSeconds(60)
    }
}

internal class InferencePeerMessageController(
    private val onFrame: (String, ByteArray) -> Unit,
    private val onClosed: (String) -> Unit,
) : SimpleClientHandler() {
    private val decoder = PeerMessageFrameDecoder()
    private var authenticatedPeerId: String? = null

    override fun handlerCreated() {
        authenticatedPeerId = stream.remotePeerId().toBase58()
    }

    override fun streamActive(ctx: ChannelHandlerContext) {
        this.ctx = ctx
    }

    override fun messageReceived(
        ctx: ChannelHandlerContext,
        msg: ByteBuf,
    ) {
        val peerId = authenticatedPeerId ?: return
        val bytes = ByteArray(msg.readableBytes())
        msg.readBytes(bytes)
        try {
            decoder.feed(bytes).forEach { frame ->
                try {
                    onFrame(peerId, frame)
                } finally {
                    Arrays.fill(frame, 0)
                }
            }
        } catch (_: Exception) {
            onClosed(peerId)
            stream.reset()
        } finally {
            Arrays.fill(bytes, 0)
        }
    }

    override fun streamClosed(ctx: ChannelHandlerContext) {
        authenticatedPeerId?.let(onClosed)
    }

    fun sendRequest(payload: ByteArray): CompletableFuture<Unit> {
        val frame = PeerMessageFrameCodec.encode(payload)
        try {
            return writeAndFlush(Unpooled.wrappedBuffer(frame)).whenComplete { _, _ ->
                Arrays.fill(frame, 0)
            }
        } catch (exception: Exception) {
            Arrays.fill(frame, 0)
            throw exception
        }
    }

    fun closeStream() {
        if (authenticatedPeerId != null) {
            stream.close()
        }
    }
}
