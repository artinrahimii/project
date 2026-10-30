package com.whitep.client.network

import io.libp2p.core.Host
import io.libp2p.core.PeerId
import io.libp2p.core.crypto.KeyType
import io.libp2p.core.crypto.PrivKey
import io.libp2p.core.crypto.generateKeyPair
import io.libp2p.core.dsl.host
import io.libp2p.core.multistream.ProtocolBinding
import io.libp2p.etc.SimpleClientHandler
import io.libp2p.etc.createSimpleBinding
import io.netty.buffer.ByteBuf
import io.netty.buffer.Unpooled
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.withTimeout
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Test
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.InferenceResponse
import java.time.Duration
import java.time.Instant
import java.util.Arrays
import java.util.concurrent.TimeUnit

class Libp2pInferenceRequestTransportTest {
    @Test
    fun sendsAndReceivesCorrelatedResponseOverAuthenticatedNoiseTcpStream() =
        runBlocking {
            val serverIdentity = generateKeyPair(KeyType.ED25519).first
            val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
            val responder = LoopbackInferenceResponder(expectedServerPeerId)
            val serverBinding =
                createSimpleBinding(PROTOCOL_ID) {
                    responder
                }
            val server = createHost(serverIdentity, serverBinding)
            server.start().get(15, TimeUnit.SECONDS)

            val serverPeerId = server.peerId.toBase58()
            assertEquals("The server PeerId must match its supplied identity.", expectedServerPeerId, serverPeerId)
            val listenAddress = server.listenAddresses().first { it.toString().contains("/tcp/") }.toString()
            val peerIdentitySuffix = "/p2p/$serverPeerId"
            val address =
                when {
                    listenAddress.endsWith(peerIdentitySuffix) -> listenAddress
                    !listenAddress.contains("/p2p/") -> "$listenAddress$peerIdentitySuffix"
                    else -> error("The server listen address contains a different PeerId.")
                }
            val endpoint =
                RendezvousPeerEndpoint(
                    peerId = serverPeerId,
                    tcpMultiaddresses = listOf(address),
                    expiresAtUtc = Instant.now().plusSeconds(120),
                )

            val clientIdentity = generateKeyPair(KeyType.ED25519).first
            val client =
                Libp2pInferenceRequestTransport(
                    localIdentity = clientIdentity,
                    resolveEndpoint = { requestedPeerId -> endpoint.takeIf { it.peerId == requestedPeerId } },
                    trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId == serverPeerId },
                    connectionTimeout = Duration.ofSeconds(15),
                )
            try {
                val request = createRequest()
                val submission = client.submit(serverPeerId, request)
                assertEquals(
                    "Direct submission should be accepted; diagnostic=${client.lastSubmissionFailure}",
                    com.whitep.client.requests.InferenceTransportSubmission.QUEUED,
                    submission,
                )
                val receivedRequest =
                    withTimeout(5_000L) {
                        responder.requestReceived.await()
                    }
                assertEquals("Responder must receive the request ID.", request.requestId, receivedRequest.requestId)
                assertEquals("Responder must receive the attempt ID.", request.attemptId, receivedRequest.attemptId)
                val response =
                    try {
                        client.awaitResponse(
                            requestId = request.requestId,
                            attemptId = request.attemptId,
                            timeout = Duration.ofSeconds(20),
                        )
                    } catch (exception: Exception) {
                        throw AssertionError("The authenticated peer did not return a correlated response.", exception)
                    }
                assertNotNull(response)
                assertEquals(request.requestId, response.requestId)
                assertEquals(request.attemptId, response.attemptId)
                assertEquals(request.modelId, response.modelId)
                assertEquals(serverPeerId, response.ephemeralHostId)
                assertEquals("loopback-response", response.outputText)
                assertEquals(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED, response.outcome)
            } finally {
                client.close()
                server.stop().get(10, TimeUnit.SECONDS)
            }
        }

    private fun createHost(
        identity: PrivKey,
        binding: ProtocolBinding<LoopbackInferenceResponder>,
    ): Host =
        host {
            identity {
                factory = { identity }
            }
            protocols {
                @Suppress("UNCHECKED_CAST")
                add(binding as ProtocolBinding<Any>)
            }
            network {
                listen("/ip4/127.0.0.1/tcp/0")
            }
        }

    private fun createRequest(): InferenceRequest =
        InferenceRequest
            .newBuilder()
            .setProtocolVersion("1.0.0")
            .setRequestId("req_00112233445566778899aabbccddeeff")
            .setAttemptId("attempt_android_libp2p_test")
            .setModelId("loopback-model")
            .setPrompt("protocol acceptance")
            .setDeadlineUnixMs(System.currentTimeMillis() + 30_000L)
            .setMaxOutputTokens(8)
            .build()

    private class LoopbackInferenceResponder(
        private val localPeerId: String,
    ) : SimpleClientHandler() {
        private val decoder = PeerMessageFrameDecoder()
        val requestReceived = CompletableDeferred<InferenceRequest>()

        override fun messageReceived(
            ctx: io.netty.channel.ChannelHandlerContext,
            msg: ByteBuf,
        ) {
            val chunk = ByteArray(msg.readableBytes())
            msg.readBytes(chunk)
            try {
                decoder.feed(chunk).forEach { payload ->
                    try {
                        val request = InferenceRequest.parseFrom(payload)
                        requestReceived.complete(request)
                        val response =
                            InferenceResponse
                                .newBuilder()
                                .setRequestId(request.requestId)
                                .setAttemptId(request.attemptId)
                                .setEphemeralHostId(localPeerId)
                                .setOutcome(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED)
                                .setOutputText("loopback-response")
                                .setCompletedAtUnixMs(System.currentTimeMillis())
                                .setModelId(request.modelId)
                                .setErrorMessage("")
                                .build()
                        writeAndFlush(Unpooled.wrappedBuffer(PeerMessageFrameCodec.encode(response.toByteArray())))
                    } finally {
                        Arrays.fill(payload, 0)
                    }
                }
            } finally {
                Arrays.fill(chunk, 0)
            }
        }
    }

    private companion object {
        const val PROTOCOL_ID = "/white-p/peer-message/1.0.0"
    }
}
