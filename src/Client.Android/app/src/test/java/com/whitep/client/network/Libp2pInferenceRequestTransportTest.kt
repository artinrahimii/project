package com.whitep.client.network

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.requests.AndroidInferenceRequestRuntime
import com.whitep.client.requests.DistributedRequest
import com.whitep.client.requests.DistributedRequestProgress
import com.whitep.client.requests.HostCandidate
import com.whitep.client.requests.HostCandidateSelector
import com.whitep.client.requests.InferenceAttemptSubmissionStatus
import com.whitep.client.requests.InferenceRoundDispatchResult
import com.whitep.client.requests.InferenceRoundResponseFailureReason
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
import kotlinx.coroutines.delay
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
import org.whitep.shared.contracts.v1.CancelRequest
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.HostStatusRequest
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
import java.util.concurrent.CompletableFuture
import java.util.concurrent.ConcurrentLinkedQueue
import java.util.concurrent.TimeUnit

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class Libp2pInferenceRequestTransportTest {
    @Test
    fun rejectsNegativeUnsignedActiveRequestCountOverAuthenticatedPeerStream() =
        runBlocking<Unit> {
            val serverIdentity = generateKeyPair(KeyType.ED25519).first
            val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
            val status =
                HostStatus
                    .newBuilder()
                    .setEphemeralHostId(expectedServerPeerId)
                    .setState(HostRuntimeState.HOST_RUNTIME_STATE_READY)
                    .setActiveRequests(-1)
                    .setObservedAtUnixMs(System.currentTimeMillis())
                    .build()
            val responder = LoopbackHostStatusResponder(expectedServerPeerId) { status }
            val binding = createSimpleBinding(PROTOCOL_ID) { responder }
            val server = createHost(serverIdentity, binding)
            server.start().get(15, TimeUnit.SECONDS)

            val listenAddress = server.listenAddresses().first { it.toString().contains("/tcp/") }.toString()
            val suffix = "/p2p/$expectedServerPeerId"
            val address =
                when {
                    listenAddress.endsWith(suffix) -> listenAddress
                    !listenAddress.contains("/p2p/") -> "$listenAddress$suffix"
                    else -> error("The server listen address contains a different PeerId.")
                }
            val endpoint =
                RendezvousPeerEndpoint(
                    peerId = expectedServerPeerId,
                    tcpMultiaddresses = listOf(address),
                    expiresAtUtc = Instant.now().plusSeconds(120),
                )
            val client =
                Libp2pInferenceRequestTransport(
                    localIdentity = generateKeyPair(KeyType.ED25519).first,
                    resolveEndpoint = { requestedPeerId ->
                        if (requestedPeerId == expectedServerPeerId) endpoint else null
                    },
                    trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId == expectedServerPeerId },
                    connectionTimeout = Duration.ofSeconds(15),
                )

            try {
                var rejected = false
                try {
                    client.probeHostStatus(expectedServerPeerId)
                } catch (_: java.io.IOException) {
                    rejected = true
                }
                assertTrue(
                    "A uint32 active-request count represented as a negative Kotlin Int must be rejected.",
                    rejected,
                )
                val request = withTimeout(5_000L) { responder.requestReceived.await() }
                assertEquals(expectedServerPeerId, request.ephemeralHostId)
            } finally {
                client.close()
                server.stop().get(10, TimeUnit.SECONDS)
            }
        }

    @Test
    fun probesFreshHostStatusOverTheAuthenticatedPeerStream() =
        runBlocking<Unit> {
            val serverIdentity = generateKeyPair(KeyType.ED25519).first
            val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
            val observedAt = System.currentTimeMillis()
            val status =
                HostStatus
                    .newBuilder()
                    .setEphemeralHostId(expectedServerPeerId)
                    .setState(HostRuntimeState.HOST_RUNTIME_STATE_READY)
                    .setActiveRequests(0)
                    .setObservedAtUnixMs(observedAt)
                    .build()
            val responder = LoopbackHostStatusResponder(expectedServerPeerId) { status }
            val binding = createSimpleBinding(PROTOCOL_ID) { responder }
            val server = createHost(serverIdentity, binding)
            server.start().get(15, TimeUnit.SECONDS)

            val listenAddress = server.listenAddresses().first { it.toString().contains("/tcp/") }.toString()
            val suffix = "/p2p/$expectedServerPeerId"
            val address =
                when {
                    listenAddress.endsWith(suffix) -> listenAddress
                    !listenAddress.contains("/p2p/") -> "$listenAddress$suffix"
                    else -> error("The server listen address contains a different PeerId.")
                }
            val endpoint =
                RendezvousPeerEndpoint(
                    peerId = expectedServerPeerId,
                    tcpMultiaddresses = listOf(address),
                    expiresAtUtc = Instant.now().plusSeconds(120),
                )
            val client =
                Libp2pInferenceRequestTransport(
                    localIdentity = generateKeyPair(KeyType.ED25519).first,
                    resolveEndpoint = { requestedPeerId ->
                        if (requestedPeerId == expectedServerPeerId) endpoint else null
                    },
                    trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId == expectedServerPeerId },
                    connectionTimeout = Duration.ofSeconds(15),
                )

            try {
                val liveStatus = client.probeHostStatus(expectedServerPeerId)
                assertEquals(expectedServerPeerId, liveStatus.ephemeralHostId)
                assertEquals(HostRuntimeState.HOST_RUNTIME_STATE_READY, liveStatus.state)
                assertEquals(0, liveStatus.activeRequests)
                assertEquals(observedAt, liveStatus.observedAtUnixMs)
                val request = withTimeout(5_000L) { responder.requestReceived.await() }
                assertEquals(expectedServerPeerId, request.ephemeralHostId)
            } finally {
                client.close()
                server.stop().get(10, TimeUnit.SECONDS)
            }
        }

    @Test
    fun cancelAttemptsSendsCorrelatedCancelRequestsToEverySelectedAuthenticatedHost() =
        runBlocking<Unit> {
            val servers = mutableListOf<Host>()
            val responders = mutableListOf<LoopbackCancellationResponder>()
            val endpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
            val clientsByHost = linkedMapOf<String, String>()
            try {
                repeat(2) { index ->
                    val serverIdentity = generateKeyPair(KeyType.ED25519).first
                    val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
                    val responder = LoopbackCancellationResponder(expectedServerPeerId)
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
                    clientsByHost[serverPeerId] = "attempt-cancel-$index"
                }

                val client =
                    Libp2pInferenceRequestTransport(
                        localIdentity = generateKeyPair(KeyType.ED25519).first,
                        resolveEndpoint = { requestedPeerId -> endpoints[requestedPeerId] },
                        trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId in endpoints },
                        connectionTimeout = Duration.ofSeconds(15),
                    )
                try {
                    val baseRequest = createRequest()
                    val selectedHostIds = endpoints.keys.toList()
                    val attemptIds = clientsByHost.values.toList()
                    selectedHostIds.forEachIndexed { index, hostId ->
                        val request =
                            baseRequest
                                .toBuilder()
                                .setAttemptId(attemptIds[index])
                                .build()
                        assertEquals(
                            com.whitep.client.requests.InferenceTransportSubmission.QUEUED,
                            client.submit(hostId, request),
                        )
                    }

                    responders.forEachIndexed { index, responder ->
                        val received = withTimeout(5_000L) { responder.requestReceived.await() }
                        assertEquals(baseRequest.requestId, received.requestId)
                        assertEquals(attemptIds[index], received.attemptId)
                    }

                    val cancellationResults = client.cancelAttempts(baseRequest.requestId, attemptIds)
                    assertEquals(attemptIds.associateWith { true }, cancellationResults)

                    responders.forEachIndexed { index, responder ->
                        val cancel = withTimeout(5_000L) { responder.cancelReceived.await() }
                        assertEquals(baseRequest.requestId, cancel.requestId)
                        assertEquals(attemptIds[index], cancel.attemptId)
                        assertEquals("user_cancelled", cancel.reason)
                    }

                    selectedHostIds.forEachIndexed { index, hostId ->
                        val response =
                            client.awaitResponse(
                                requestId = baseRequest.requestId,
                                attemptId = attemptIds[index],
                                timeout = Duration.ofSeconds(10),
                            )
                        assertEquals(baseRequest.requestId, response.requestId)
                        assertEquals(attemptIds[index], response.attemptId)
                        assertEquals(hostId, response.ephemeralHostId)
                        assertEquals(InferenceOutcome.INFERENCE_OUTCOME_CANCELLED, response.outcome)
                        assertEquals("", response.outputText)
                    }
                } finally {
                    client.close()
                }
            } finally {
                servers.asReversed().forEach { server ->
                    server.stop().get(10, TimeUnit.SECONDS)
                }
            }
        }

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
                assertEquals(request.roundId, response.roundId)
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
                    assertEquals(
                        DistributedRequestProgress(
                            roundsDispatched = 1,
                            hostsAttempted = 3,
                            attemptedEphemeralHostIds = attempts.map { attempt -> attempt.ephemeralHostId },
                        ),
                        client.dispatchProgress(requestId),
                    )
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
                    assertEquals(1, receivedRequests.map { it.roundId }.toSet().size)
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
                    assertTrue(responses.all { it.roundId == receivedRequests.first().roundId })
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
    fun sendsCancellationToThreeNativeDotNetLibp2pHostsAndReceivesCancelledResponses() =
        runBlocking<Unit> {
            val serverDll = System.getenv("WHITEP_INTEROP_SERVER_DLL")
            assumeTrue(
                "The cross-runtime .NET peer fixture is enabled only by the CI interop job.",
                !serverDll.isNullOrBlank() && File(serverDll).isFile,
            )

            val clientIdentity = generateKeyPair(KeyType.ED25519).first
            val clientPeerId = PeerId.fromPubKey(clientIdentity.publicKey()).toBase58()
            val processes = mutableListOf<Process>()
            val phaseOutput = ConcurrentLinkedQueue<String>()
            val diagnosticOutput = ConcurrentLinkedQueue<String>()
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
                            "cancel",
                        ).redirectErrorStream(true).start()
                    processes += process
                    val readyLineFuture = CompletableFuture<String>()
                    val outputReader =
                        Thread(
                            {
                                val reader = process.inputStream.bufferedReader()
                                val allowedMarkers =
                                    setOf(
                                        "WAITING_FOR_REMOTE_SESSION",
                                        "SESSION_AUTHENTICATED",
                                        "APPLICATION_PROTOCOL_READY",
                                        "RECEIVED",
                                        "CANCEL_WAITING",
                                        "CANCEL_RECEIVED",
                                        "VALIDATED",
                                        "SENT",
                                        "PASS",
                                    )
                                while (true) {
                                    val line = runCatching { reader.readLine() }.getOrNull() ?: break
                                    when {
                                        line.startsWith("READY\t") -> readyLineFuture.complete(line)
                                        line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                        line.startsWith("SENT_META\t") -> {
                                            val fields = line.split('\t')
                                            if (fields.size == 3 &&
                                                fields[1].toIntOrNull() != null &&
                                                fields[2].toIntOrNull() != null &&
                                                diagnosticOutput.size < 240
                                            ) {
                                                diagnosticOutput.add(
                                                    "peer-" + serverNumber + ":sent-meta:bytes=" +
                                                        fields[1] + ":firstTag=" + fields[2],
                                                )
                                            }
                                        }
                                        line.startsWith("P2P_DIAG\t") -> {
                                            val fields = line.split('\t', limit = 6)
                                            if (fields.size == 6 && diagnosticOutput.size < 240) {
                                                diagnosticOutput.add(
                                                    "peer-" + serverNumber + ":libp2p:" +
                                                        fields.drop(1).joinToString(separator = ":"),
                                                )
                                            }
                                        }
                                    }
                                }
                            },
                            "whitep-dotnet-cancel-peer-" + serverNumber + "-output",
                        ).apply { isDaemon = true }
                    outputReaders += outputReader
                    outputReader.start()

                    val readyLine =
                        withContext(Dispatchers.IO) {
                            withTimeout(30_000L) {
                                readyLineFuture.get(30, TimeUnit.SECONDS)
                            }
                        }
                    val readyFields = readyLine.split('\t')
                    assertEquals("READY", readyFields.firstOrNull())
                    assertEquals(3, readyFields.size)
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
                val requestId = "interop-cancel-" + UUID.randomUUID()
                val dispatch =
                    runtimeClient.dispatchRound(
                        distributedRequest =
                            DistributedRequest(
                                requestId = requestId,
                                deadlineUnixMs = System.currentTimeMillis() + 60_000L,
                                maxRounds = 1,
                            ),
                        selectedHosts = endpoints.keys.map(::candidate),
                        modelId = MODEL_ID,
                        currentMessage = "Cancel the active request on all selected Windows hosts",
                        capsule = ConversationContextCapsule(goal = "Verify cross-runtime cancellation"),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(dispatch is InferenceRoundDispatchResult.Dispatched)
                val attempts = (dispatch as InferenceRoundDispatchResult.Dispatched).attempts
                assertEquals(3, attempts.size)
                assertTrue(attempts.all { it.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED })

                withTimeout(15_000L) {
                    while (phaseOutput.count { it.endsWith(":CANCEL_WAITING") } != 3) {
                        delay(25L)
                    }
                }

                val cancellationResults = runtimeClient.cancelSelectedHosts(requestId, attempts)
                assertEquals(3, cancellationResults.size)
                assertTrue(cancellationResults.all { it.cancellationSent })

                val responses =
                    attempts.map { attempt ->
                        try {
                            runtimeClient.awaitResponse(
                                requestId = requestId,
                                attemptId = attempt.attemptId,
                                timeout = Duration.ofSeconds(20),
                            )
                        } catch (exception: Exception) {
                            val phases = phaseOutput.toList().joinToString(separator = " | ")
                            val diagnostics =
                                diagnosticOutput
                                    .toList()
                                    .takeLast(60)
                                    .joinToString(separator = " | ")
                            throw AssertionError(
                                "The Windows host did not return its correlated CANCELLED response; " +
                                    "phases=$phases; diagnostics=$diagnostics; " +
                                    "androidTransport=${runtimeClient.transportDiagnosticSnapshot()}",
                                exception,
                            )
                        }
                    }

                assertEquals(3, responses.size)
                assertEquals(endpoints.keys, responses.map { it.ephemeralHostId }.toSet())
                assertEquals(attempts.map { it.attemptId }.toSet(), responses.map { it.attemptId }.toSet())
                assertTrue(
                    responses.all {
                        it.requestId == requestId &&
                            it.outcome == InferenceOutcome.INFERENCE_OUTCOME_CANCELLED &&
                            it.outputText.isEmpty() &&
                            it.errorMessage == "The request was cancelled by the remote client."
                    },
                )

                processes.forEach { process ->
                    assertTrue(
                        "The .NET cancellation fixture should exit after its response.",
                        process.waitFor(15, TimeUnit.SECONDS),
                    )
                    assertEquals(0, process.exitValue())
                }
                assertEquals(3, phaseOutput.count { it.endsWith(":CANCEL_RECEIVED") })
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
            val phaseOutput = ConcurrentLinkedQueue<String>()
            val diagnosticOutput = ConcurrentLinkedQueue<String>()
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
                    val readyLineFuture = CompletableFuture<String>()
                    val outputReader =
                        Thread(
                            {
                                val reader = process.inputStream.bufferedReader()
                                val allowedMarkers =
                                    setOf(
                                        "WAITING_FOR_REMOTE_SESSION",
                                        "SESSION_AUTHENTICATED",
                                        "APPLICATION_PROTOCOL_READY",
                                        "RECEIVED",
                                        "VALIDATED",
                                        "SENT",
                                    )
                                while (true) {
                                    val line = runCatching { reader.readLine() }.getOrNull() ?: break
                                    when {
                                        line.startsWith("READY\t") -> readyLineFuture.complete(line)
                                        line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                        line.startsWith("P2P_DIAG\t") -> {
                                            val fields = line.split('\t', limit = 6)
                                            if (fields.size == 6 && diagnosticOutput.size < 240) {
                                                diagnosticOutput.add(
                                                    "peer-" + serverNumber + ":libp2p:" +
                                                        fields.drop(1).joinToString(separator = ":"),
                                                )
                                            }
                                        }
                                    }
                                }
                            },
                            "whitep-dotnet-peer-" + serverNumber + "-output",
                        ).apply { isDaemon = true }
                    outputReaders += outputReader
                    outputReader.start()

                    val readyLine =
                        withContext(Dispatchers.IO) {
                            withTimeout(30_000L) {
                                readyLineFuture.get(30, TimeUnit.SECONDS)
                            }
                        }
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
                assertEquals(
                    DistributedRequestProgress(
                        roundsDispatched = 1,
                        hostsAttempted = 3,
                        attemptedEphemeralHostIds = attempts.map { attempt -> attempt.ephemeralHostId },
                    ),
                    runtimeClient.dispatchProgress(requestId),
                )
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
                                phaseOutput
                                    .toList()
                                    .joinToString(separator = " | ")
                            val diagnostics =
                                diagnosticOutput
                                    .toList()
                                    .takeLast(60)
                                    .joinToString(separator = " | ")
                            val processStates =
                                processes
                                    .mapIndexed { index, process ->
                                        "peer-" + (index + 1) + ":alive=" + process.isAlive
                                    }.joinToString(separator = ", ")
                            throw AssertionError(
                                "The .NET peer did not return a correlated response; phases=$phases; diagnostics=$diagnostics; $processStates",
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

    @Test
    fun dispatchesSecondRoundToThreeNewPeersAfterFirstThreeRemainSilent() =
        runBlocking<Unit> {
            val serverDll = System.getenv("WHITEP_INTEROP_SERVER_DLL")
            assumeTrue(
                "The cross-runtime .NET peer fixture is enabled only by the CI interop job.",
                !serverDll.isNullOrBlank() && File(serverDll).isFile,
            )

            val clientIdentity = generateKeyPair(KeyType.ED25519).first
            val clientPeerId = PeerId.fromPubKey(clientIdentity.publicKey()).toBase58()
            val processes = mutableListOf<Process>()
            val outputReaders = mutableListOf<Thread>()
            val phaseOutput = ConcurrentLinkedQueue<String>()
            val diagnosticOutput = ConcurrentLinkedQueue<String>()
            val activeEndpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
            var runtime: AndroidInferenceRequestRuntime? = null

            suspend fun startPeers(silentResponse: Boolean): LinkedHashMap<String, RendezvousPeerEndpoint> {
                val endpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
                repeat(3) {
                    val serverNumber = processes.size + 1
                    val port = ServerSocket(0).use { socket -> socket.localPort }
                    val command =
                        mutableListOf(
                            "dotnet",
                            serverDll!!,
                            "--interop-peer-server",
                            clientPeerId,
                            port.toString(),
                        )
                    if (silentResponse) command += "silent"
                    val process =
                        ProcessBuilder(command).redirectErrorStream(true).start()
                    processes += process

                    val readyLineFuture = CompletableFuture<String>()
                    val outputReader =
                        Thread(
                            {
                                val reader = process.inputStream.bufferedReader()
                                val allowedMarkers =
                                    setOf(
                                        "WAITING_FOR_REMOTE_SESSION",
                                        "SESSION_AUTHENTICATED",
                                        "APPLICATION_PROTOCOL_READY",
                                        "RECEIVED",
                                        "RECEIVED_WITHOUT_RESPONSE",
                                        "VALIDATED",
                                        "SENT",
                                        "PASS",
                                    )
                                while (true) {
                                    val line = runCatching { reader.readLine() }.getOrNull() ?: break
                                    when {
                                        line.startsWith("READY\t") -> readyLineFuture.complete(line)
                                        line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                        line.startsWith("P2P_DIAG\t") -> {
                                            val fields = line.split('\t', limit = 6)
                                            if (fields.size == 6 && diagnosticOutput.size < 240) {
                                                diagnosticOutput.add(
                                                    "peer-" + serverNumber + ":libp2p:" +
                                                        fields.drop(1).joinToString(separator = ":"),
                                                )
                                            }
                                        }
                                    }
                                }
                            },
                            "whitep-dotnet-peer-" + serverNumber + "-output",
                        ).apply { isDaemon = true }
                    outputReaders += outputReader
                    outputReader.start()

                    val readyLine =
                        withContext(Dispatchers.IO) {
                            withTimeout(30_000L) {
                                readyLineFuture.get(30, TimeUnit.SECONDS)
                            }
                        }
                    val readyFields = readyLine.split('\t')
                    assertEquals("READY", readyFields.firstOrNull())
                    assertEquals(3, readyFields.size)
                    val peerId = readyFields[1]
                    endpoints[peerId] =
                        RendezvousPeerEndpoint(
                            peerId = peerId,
                            tcpMultiaddresses = listOf(readyFields[2]),
                            expiresAtUtc = Instant.now().plusSeconds(180),
                        )
                }
                return endpoints
            }

            try {
                val firstProcessStart = processes.size
                val firstEndpoints = startPeers(silentResponse = true)
                val firstProcesses = processes.drop(firstProcessStart).toList()
                activeEndpoints.putAll(firstEndpoints)

                val clientRuntime =
                    AndroidInferenceRequestRuntime(
                        localIdentity = clientIdentity,
                        resolveEndpoint = { requestedPeerId -> activeEndpoints[requestedPeerId] },
                        trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId in activeEndpoints },
                        connectionTimeout = Duration.ofSeconds(20),
                        roundTimeout = Duration.ofSeconds(15),
                    )
                runtime = clientRuntime

                val requestId = "interop-round-two-${UUID.randomUUID()}"
                val deadline = System.currentTimeMillis() + 180_000L
                val distributedRequest =
                    DistributedRequest(
                        requestId = requestId,
                        deadlineUnixMs = deadline,
                        maxRounds = 2,
                        maxTotalHosts = 6,
                    )
                val selectorRequest =
                    createRequest()
                        .toBuilder()
                        .setRequestId(requestId)
                        .setAttemptId("round-two-selector-attempt")
                        .setRoundId("round-two-selector-round")
                        .setModelId(MODEL_ID)
                        .setProtocolVersion(PROTOCOL_VERSION)
                        .setDeadlineUnixMs(deadline)
                        .build()

                val firstSelection =
                    HostCandidateSelector.selectFirstRound(
                        distributedRequest = distributedRequest,
                        request = selectorRequest,
                        candidates = firstEndpoints.keys.map(::candidate),
                    )
                assertEquals(3, firstSelection.size)

                val firstDispatch =
                    clientRuntime.dispatchRound(
                        distributedRequest = distributedRequest,
                        selectedHosts = firstSelection,
                        modelId = MODEL_ID,
                        currentMessage = "First three peers deliberately withhold responses",
                        capsule =
                            ConversationContextCapsule(
                                goal = "Verify that a timed-out first round advances to three new peers",
                            ),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(
                    "The first round must dispatch: $firstDispatch",
                    firstDispatch is InferenceRoundDispatchResult.Dispatched,
                )
                val firstAttempts = (firstDispatch as InferenceRoundDispatchResult.Dispatched).attempts
                assertEquals(3, firstAttempts.size)
                assertTrue(firstAttempts.all { it.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED })
                val firstHostIds = firstAttempts.map { it.ephemeralHostId }.toSet()
                assertEquals(3, firstHostIds.size)

                withTimeout(15_000L) {
                    while (phaseOutput.count { it.endsWith(":RECEIVED_WITHOUT_RESPONSE") } < 3) {
                        delay(25L)
                    }
                }
                assertTrue(
                    "The first three silent peer processes must still be running.",
                    firstProcesses.all { process -> process.isAlive },
                )

                val firstCollection = clientRuntime.awaitRoundResponses(requestId, firstAttempts)
                assertTrue("No first-round peer may return a response.", firstCollection.responses.isEmpty())
                assertEquals(3, firstCollection.failures.size)
                assertTrue(
                    firstCollection.failures.all { failure ->
                        failure.reason == InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT &&
                            failure.ephemeralHostId in firstHostIds
                    },
                )

                firstProcesses.forEach { process ->
                    if (process.isAlive) {
                        process.destroyForcibly()
                        process.waitFor(5, TimeUnit.SECONDS)
                    }
                }
                activeEndpoints.clear()
                val secondProcessStart = processes.size
                val secondEndpoints = startPeers(silentResponse = false)
                val secondProcesses = processes.drop(secondProcessStart).toList()
                activeEndpoints.putAll(secondEndpoints)

                val secondHostIds = secondEndpoints.keys
                assertEquals(3, secondHostIds.size)
                assertTrue(firstHostIds.intersect(secondHostIds).isEmpty())

                val secondSelection =
                    HostCandidateSelector.selectNextRound(
                        distributedRequest = distributedRequest,
                        request = selectorRequest,
                        candidates = secondHostIds.map(::candidate),
                        previousRoundAttempts = firstAttempts,
                        previousRoundResponses = firstCollection,
                        progress = clientRuntime.dispatchProgress(requestId),
                    )
                assertEquals(3, secondSelection.size)
                assertEquals(secondHostIds, secondSelection.map { it.announcement.ephemeralHostId }.toSet())
                assertTrue(secondSelection.none { it.announcement.ephemeralHostId in firstHostIds })

                val secondDispatch =
                    clientRuntime.dispatchRound(
                        distributedRequest = distributedRequest,
                        selectedHosts = secondSelection,
                        modelId = MODEL_ID,
                        currentMessage = "Second round dispatched to three fresh .NET peers",
                        capsule =
                            ConversationContextCapsule(
                                goal = "Verify independent response correlation after first-round timeouts",
                            ),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(
                    "The second round must dispatch: $secondDispatch",
                    secondDispatch is InferenceRoundDispatchResult.Dispatched,
                )
                val secondAttempts = (secondDispatch as InferenceRoundDispatchResult.Dispatched).attempts
                assertEquals(3, secondAttempts.size)
                assertTrue(secondAttempts.all { it.submissionStatus == InferenceAttemptSubmissionStatus.QUEUED })

                val secondCollection = clientRuntime.awaitRoundResponses(requestId, secondAttempts)
                assertEquals(3, secondCollection.responses.size)
                assertTrue(secondCollection.failures.isEmpty())
                assertEquals(secondHostIds, secondCollection.responses.map { it.ephemeralHostId }.toSet())
                val dispatchedAttemptIds =
                    secondAttempts
                        .map { attempt -> attempt.attemptId }
                        .toSet()
                val responseAttemptIds =
                    secondCollection.responses
                        .map { response -> response.attemptId }
                        .toSet()
                assertEquals(dispatchedAttemptIds, responseAttemptIds)
                assertTrue(
                    secondCollection.responses.all { response ->
                        response.requestId == requestId &&
                            response.modelId == MODEL_ID &&
                            response.outputText == "dotnet-host-response" &&
                            response.outcome == InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED
                    },
                )

                val progress = clientRuntime.dispatchProgress(requestId)
                assertEquals(2, progress.roundsDispatched)
                assertEquals(6, progress.hostsAttempted)
                assertEquals(
                    firstAttempts.map { it.ephemeralHostId } + secondAttempts.map { it.ephemeralHostId },
                    progress.attemptedEphemeralHostIds,
                )

                secondProcesses.forEach { process ->
                    val processExited = process.waitFor(15, TimeUnit.SECONDS)
                    assertTrue(
                        "A responding .NET peer fixture must exit after its response is consumed.",
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
                    runCatching { process.inputStream.close() }
                }
                outputReaders.forEach { reader -> runCatching { reader.join(1_000L) } }
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

    private fun <T : SimpleClientHandler> createHost(
        identity: PrivKey,
        binding: ProtocolBinding<T>,
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
            .setRoundId("round-android-test")
            .setModelId("loopback-model")
            .setPrompt("protocol acceptance")
            .setDeadlineUnixMs(System.currentTimeMillis() + 30_000L)
            .setMaxOutputTokens(8)
            .build()

    private class LoopbackHostStatusResponder(
        private val localPeerId: String,
        private val statusProvider: () -> HostStatus,
    ) : SimpleClientHandler() {
        private val decoder = PeerMessageFrameDecoder()
        val requestReceived = CompletableDeferred<HostStatusRequest>()

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
                        val request = HostStatusRequest.parseFrom(payload)
                        check(request.ephemeralHostId == localPeerId)
                        requestReceived.complete(request)
                        val response = statusProvider()
                        check(response.ephemeralHostId == localPeerId)
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

    private class LoopbackCancellationResponder(
        private val localPeerId: String,
    ) : SimpleClientHandler() {
        private val decoder = PeerMessageFrameDecoder()
        private var activeRequest: InferenceRequest? = null
        val requestReceived = CompletableDeferred<InferenceRequest>()
        val cancelReceived = CompletableDeferred<CancelRequest>()

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
                        val parsedRequest = InferenceRequest.parseFrom(payload)
                        if (parsedRequest.deadlineUnixMs > 0L && parsedRequest.prompt.isNotBlank()) {
                            activeRequest = parsedRequest
                            requestReceived.complete(parsedRequest)
                        } else {
                            val cancel = CancelRequest.parseFrom(payload)
                            val request = activeRequest ?: error("CancelRequest arrived before an active request.")
                            check(cancel.requestId == request.requestId && cancel.attemptId == request.attemptId)
                            check(cancel.reason == "user_cancelled")
                            cancelReceived.complete(cancel)

                            val response =
                                InferenceResponse
                                    .newBuilder()
                                    .setRequestId(request.requestId)
                                    .setAttemptId(request.attemptId)
                                    .setRoundId(request.roundId)
                                    .setEphemeralHostId(localPeerId)
                                    .setOutcome(InferenceOutcome.INFERENCE_OUTCOME_CANCELLED)
                                    .setOutputText("")
                                    .setCompletedAtUnixMs(System.currentTimeMillis())
                                    .setModelId(request.modelId)
                                    .setErrorMessage("The request was cancelled by the remote client.")
                                    .build()
                            writeAndFlush(Unpooled.wrappedBuffer(PeerMessageFrameCodec.encode(response.toByteArray())))
                        }
                    } finally {
                        Arrays.fill(payload, 0)
                    }
                }
            } finally {
                Arrays.fill(chunk, 0)
            }
        }
    }

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
                                .setRoundId(request.roundId)
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
