package com.whitep.client.network

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.requests.AndroidInferenceRequestRuntime
import com.whitep.client.requests.DistributedRequest
import com.whitep.client.requests.HostCandidate
import com.whitep.client.requests.InferenceAttemptSubmissionStatus
import com.whitep.client.requests.InferenceRoundDispatchResult
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
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.withContext
import kotlinx.coroutines.withTimeout
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Assume.assumeTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.InferenceOutcome
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.InferenceResponse
import org.whitep.shared.contracts.v1.hostAnnouncement
import org.whitep.shared.contracts.v1.hostCapabilities
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.modelCapability
import java.io.File
import java.net.ServerSocket
import java.time.Duration
import java.time.Instant
import java.util.Arrays
import java.util.UUID
import java.util.concurrent.ConcurrentLinkedQueue
import java.util.concurrent.TimeUnit

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
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

    @Test
    fun dispatchesOneSharedRequestWithDistinctAttemptsOverAuthenticatedPeerStreams() =
        runBlocking<Unit> {
            val servers = mutableListOf<Host>()
            val responders = mutableListOf<LoopbackInferenceResponder>()
            val endpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
            try {
                repeat(3) {
                    val serverIdentity = generateKeyPair(KeyType.ED25519).first
                    val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
                    val responder = LoopbackInferenceResponder(expectedServerPeerId)
                    val binding =
                        createSimpleBinding(PROTOCOL_ID) {
                            responder
                        }
                    val server = createHost(serverIdentity, binding)
                    servers += server
                    responders += responder
                    server.start().get(15, TimeUnit.SECONDS)

                    val serverPeerId = server.peerId.toBase58()
                    assertEquals(expectedServerPeerId, serverPeerId)
                    val listenAddress = server.listenAddresses().first { it.toString().contains("/tcp/") }.toString()
                    val suffix = "/p2p/$serverPeerId"
                    val address =
                        when {
                            listenAddress.endsWith(suffix) -> listenAddress
                            !listenAddress.contains("/p2p/") -> "$listenAddress$suffix"
                            else -> error("The server listen address contains a different PeerId.")
                        }
                    endpoints[serverPeerId] =
                        RendezvousPeerEndpoint(
                            peerId = serverPeerId,
                            tcpMultiaddresses = listOf(address),
                            expiresAtUtc = Instant.now().plusSeconds(120),
                        )
                }

                val client =
                    AndroidInferenceRequestRuntime(
                        localIdentity = generateKeyPair(KeyType.ED25519).first,
                        resolveEndpoint = { requestedPeerId -> endpoints[requestedPeerId] },
                        trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId in endpoints },
                        connectionTimeout = Duration.ofSeconds(15),
                    )
                try {
                    val attemptIds = listOf("attempt-host-a", "attempt-host-b", "attempt-host-c").iterator()
                    val requestId = "round-three-host-request"
                    val now = System.currentTimeMillis()
                    val result =
                        client.dispatchRound(
                            distributedRequest =
                                DistributedRequest(
                                    requestId = requestId,
                                    deadlineUnixMs = now + 60_000L,
                                    maxRounds = 1,
                                ),
                            selectedHosts = endpoints.keys.map(::candidate),
                            modelId = MODEL_ID,
                            currentMessage = "one request, three authenticated host attempts",
                            capsule = ConversationContextCapsule(goal = "Correlate every host attempt"),
                            maxOutputTokens = 8,
                            protocolVersion = PROTOCOL_VERSION,
                        )

                    assertTrue(result is InferenceRoundDispatchResult.Dispatched)
                    val attempts = (result as InferenceRoundDispatchResult.Dispatched).attempts
                    assertEquals(3, attempts.size)
                    assertEquals(3, attempts.map { it.ephemeralHostId }.toSet().size)
                    assertEquals(3, attempts.map { it.attemptId }.toSet().size)
                    assertEquals(
                        listOf(
                            InferenceAttemptSubmissionStatus.QUEUED,
                            InferenceAttemptSubmissionStatus.QUEUED,
                            InferenceAttemptSubmissionStatus.QUEUED,
                        ),
                        attempts.map { it.submissionStatus },
                    )

                    val receivedRequests =
                        responders.map { responder ->
                            withTimeout(10_000L) {
                                responder.requestReceived.await()
                            }
                        }
                    assertEquals(
                        listOf(requestId, requestId, requestId),
                        receivedRequests.map { it.requestId },
                    )
                    assertEquals(
                        attempts.map { it.attemptId }.toSet(),
                        receivedRequests.map { it.attemptId }.toSet(),
                    )
                    assertTrue(
                        receivedRequests.all {
                            it.prompt == "one request, three authenticated host attempts"
                        },
                    )

                    val responses =
                        attempts.map { attempt ->
                            client.awaitResponse(
                                requestId = requestId,
                                attemptId = attempt.attemptId,
                                timeout = Duration.ofSeconds(20),
                            )
                        }
                    assertEquals(
                        attempts.map { it.ephemeralHostId }.toSet(),
                        responses.map { it.ephemeralHostId }.toSet(),
                    )
                    assertEquals(
                        attempts.map { it.attemptId }.toSet(),
                        responses.map { it.attemptId }.toSet(),
                    )
                    assertTrue(responses.all { it.outcome == InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED })
                } finally {
                    client.close()
                }
            } finally {
                servers.asReversed().forEach { server ->
                    runCatching {
                        server.stop().get(10, TimeUnit.SECONDS)
                    }
                }
            }
        }

    @Test
    fun dispatchesRoundToThreeNativeDotNetLibp2pHosts() =
        runBlocking<Unit> {
            val serverDll = System.getenv("WHITEP_INTEROP_SERVER_DLL")
            assumeTrue(
                "The cross-runtime .NET peer fixture is enabled only by the CI interop job.",
                !serverDll.isNullOrBlank() && File(serverDll).isFile,
            )

            val clientIdentity = generateKeyPair(KeyType.ED25519).first
            val clientPeerId = PeerId.fromPubKey(clientIdentity.publicKey()).toBase58()
            val processes = mutableListOf<Process>()
            val serverOutput = ConcurrentLinkedQueue<String>()
            val outputReaders = mutableListOf<Thread>()
            val endpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
            var runtime: AndroidInferenceRequestRuntime? = null

            try {
                repeat(3) {
                    val serverNumber = processes.size + 1
                    val port = ServerSocket(0).use { socket -> socket.localPort }
                    val process =
                        ProcessBuilder(
                            "dotnet",
                            serverDll!!,
                            "--interop-peer-server",
                            clientPeerId,
                            port.toString(),
                        ).redirectErrorStream(true).start()
                    processes += process
                    val reader = process.inputStream.bufferedReader()

                    val readyLine =
                        withContext(Dispatchers.IO) {
                            withTimeout(30_000L) {
                                var ready: String? = null
                                while (ready == null) {
                                    val line = reader.readLine() ?: break
                                    if (line.startsWith("READY\t")) {
                                        ready = line
                                    }
                                }
                                ready
                            }
                        } ?: throw AssertionError(
                            "The .NET peer fixture exited before publishing its ready address.",
                        )

                    val outputReader =
                        Thread(
                            {
                                var lineCount = 0
                                while (true) {
                                    val line = runCatching { reader.readLine() }.getOrNull() ?: break
                                    if (lineCount < 40) {
                                        serverOutput.add("peer-" + serverNumber + ":" + line)
                                    }
                                    lineCount += 1
                                }
                            },
                            "whitep-dotnet-peer-" + serverNumber + "-output",
                        ).apply { isDaemon = true }
                    outputReaders += outputReader
                    outputReader.start()

                    val readyFields = readyLine.split('\t')
                    assertEquals(
                        "The .NET fixture must announce a ready endpoint.",
                        "READY",
                        readyFields.firstOrNull(),
                    )
                    assertEquals(
                        "The .NET fixture ready record must contain the PeerId and TCP address.",
                        3,
                        readyFields.size,
                    )

                    val peerId = readyFields[1]
                    endpoints[peerId] =
                        RendezvousPeerEndpoint(
                            peerId = peerId,
                            tcpMultiaddresses = listOf(readyFields[2]),
                            expiresAtUtc = Instant.now().plusSeconds(120),
                        )
                }

                val runtimeClient =
                    AndroidInferenceRequestRuntime(
                        localIdentity = clientIdentity,
                        resolveEndpoint = { requestedPeerId -> endpoints[requestedPeerId] },
                        trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId in endpoints },
                        connectionTimeout = Duration.ofSeconds(20),
                    )
                runtime = runtimeClient

                val requestId = "interop-${UUID.randomUUID()}"
                val now = System.currentTimeMillis()
                val dispatch =
                    runtimeClient.dispatchRound(
                        distributedRequest =
                            DistributedRequest(
                                requestId = requestId,
                                deadlineUnixMs = now + 60_000L,
                                maxRounds = 1,
                            ),
                        selectedHosts = endpoints.keys.map(::candidate),
                        modelId = MODEL_ID,
                        currentMessage = "Cross-runtime request to three .NET peers",
                        capsule =
                            ConversationContextCapsule(
                                goal = "Verify Android and Windows .NET libp2p interoperability",
                            ),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )

                assertTrue(dispatch is InferenceRoundDispatchResult.Dispatched)
                val attempts = (dispatch as InferenceRoundDispatchResult.Dispatched).attempts
                assertEquals(3, attempts.size)
                assertEquals(3, attempts.map { attempt -> attempt.ephemeralHostId }.toSet().size)
                assertEquals(3, attempts.map { attempt -> attempt.attemptId }.toSet().size)
                val attemptStatusSummary =
                    attempts.joinToString(separator = ", ") { attempt ->
                        "${attempt.ephemeralHostId}:${attempt.submissionStatus}"
                    }
                assertTrue(
                    "Expected three QUEUED .NET submissions; attempts=$attemptStatusSummary; " +
                        "lastSubmissionFailure=${runtimeClient.lastSubmissionFailure ?: "none"}",
                    attempts.all { attempt ->
                        attempt.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED
                    },
                )

                val responses =
                    attempts.map { attempt ->
                        try {
                            runtimeClient.awaitResponse(
                                requestId = requestId,
                                attemptId = attempt.attemptId,
                                timeout = Duration.ofSeconds(30),
                            )
                        } catch (exception: Exception) {
                            val phases =
                                serverOutput
                                    .toList()
                                    .takeLast(60)
                                    .joinToString(separator = " | ")
                            val processStates =
                                processes
                                    .mapIndexed { index, process ->
                                        "peer-" + (index + 1) + ":alive=" + process.isAlive
                                    }
                                    .joinToString(separator = ", ")
                            throw AssertionError(
                                "The .NET peer did not return a correlated response; phases=$phases; $processStates",
                                exception,
                            )
                        }
                    }

                assertEquals(3, responses.size)
                assertEquals(endpoints.keys, responses.map { response -> response.ephemeralHostId }.toSet())
                assertEquals(attempts.map { it.attemptId }.toSet(), responses.map { it.attemptId }.toSet())
                assertTrue(
                    responses.all { response ->
                        response.requestId == requestId &&
                            response.modelId == MODEL_ID &&
                            response.outputText == "dotnet-host-response" &&
                            response.outcome == InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
                    },
                )

                processes.forEach { process ->
                    val processExited = process.waitFor(15, TimeUnit.SECONDS)
                    assertTrue(
                        "The .NET peer fixture must finish after one correlated response.",
                        processExited,
                    )
                    assertEquals(0, process.exitValue())
                }
            } finally {
                runtime?.close()
                processes.forEach { process ->
                    if (process.isAlive) {
                        process.destroyForcibly()
                        process.waitFor(5, TimeUnit.SECONDS)
                    }
                    process.inputStream.close()
                }
            }
        }

    private fun candidate(peerId: String): HostCandidate {
        val model =
            modelCapability {
                modelId = MODEL_ID
                displayName = "Loopback model"
                maxContextTokens = 8192L
                maxOutputTokens = 128L
            }
        val capabilities =
            hostCapabilities {
                ephemeralHostId = peerId
                protocolVersion = PROTOCOL_VERSION
                models += model
                maxConcurrentRequests = 1
                maxContextCapsuleBytes = 4096L
                maxPromptBytes = 8192L
            }
        val now = System.currentTimeMillis()
        return HostCandidate(
            announcement =
                hostAnnouncement {
                    ephemeralHostId = peerId
                    protocolVersion = PROTOCOL_VERSION
                    this.capabilities = capabilities
                    announcedAtUnixMs = now
                },
            status =
                hostStatus {
                    ephemeralHostId = peerId
                    state = HostRuntimeState.HOST_RUNTIME_STATE_READY
                    activeRequests = 0
                    observedAtUnixMs = now
                    models += model
                },
        )
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

        override fun streamActive(ctx: io.netty.channel.ChannelHandlerContext) {
            this.ctx = ctx
        }

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
        const val MODEL_ID = "loopback-model"
        const val PROTOCOL_VERSION = "1.0.0"
        const val PROTOCOL_ID = "/white-p/peer-message/1.0.0"
    }
}
