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
    fun dispatchesOnlyOneHostFromMultipleAuthenticatedPeerCandidates() =
        runBlocking<Unit> {
            val servers = mutableListOf<Host>()
            val responders = mutableListOf<LoopbackInferenceResponder>()
            val endpoints = linkedMapOf<String, RendezvousPeerEndpoint>()
            try {
                repeat(3) {
                    val serverIdentity = generateKeyPair(KeyType.ED25519).first
                    val expectedServerPeerId = PeerId.fromPubKey(serverIdentity.publicKey()).toBase58()
                    val responder = LoopbackInferenceResponder(expectedServerPeerId)
                    val binding = createSimpleBinding(PROTOCOL_ID) { responder }
                    val server = createHost(serverIdentity, binding)
                    servers += server
                    responders += responder
                    server.start().get(15, TimeUnit.SECONDS)

                    val serverPeerId = server.peerId.toBase58()
                    assertEquals(expectedServerPeerId, serverPeerId)
                    val listenAddress =
                        server.listenAddresses().first { it.toString().contains("/tcp/") }.toString()
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
                    val requestId = "single-host-from-three-candidates"
                    val selectedPeerId = endpoints.keys.first()
                    val result =
                        client.dispatchRound(
                            distributedRequest =
                                DistributedRequest(
                                    requestId = requestId,
                                    deadlineUnixMs = System.currentTimeMillis() + 60_000L,
                                    maxRounds = 3,
                                ),
                            selectedHosts = listOf(candidate(selectedPeerId)),
                            modelId = MODEL_ID,
                            currentMessage = "dispatch to one authenticated host only",
                            capsule = ConversationContextCapsule(goal = "Verify one-host production dispatch"),
                            maxOutputTokens = 8,
                            protocolVersion = PROTOCOL_VERSION,
                        )
                    assertTrue(
                        "The request must dispatch to its one selected peer: $result",
                        result is InferenceRoundDispatchResult.Dispatched,
                    )
                    val attempt = (result as InferenceRoundDispatchResult.Dispatched).attempts.single()
                    assertEquals(selectedPeerId, attempt.ephemeralHostId)
                    assertEquals(InferenceAttemptSubmissionStatus.QUEUED, attempt.submissionStatus)

                    val received = withTimeout(10_000L) { responders.first().requestReceived.await() }
                    assertEquals(requestId, received.requestId)
                    assertEquals(attempt.attemptId, received.attemptId)
                    assertEquals(attempt.roundId, received.roundId)
                    assertEquals("dispatch to one authenticated host only", received.prompt)
                    assertTrue(responders.drop(1).none { responder -> responder.requestReceived.isCompleted })

                    val response =
                        client.awaitResponse(
                            requestId = requestId,
                            attemptId = attempt.attemptId,
                            timeout = Duration.ofSeconds(20),
                        )
                    assertEquals(selectedPeerId, response.ephemeralHostId)
                    assertEquals(attempt.attemptId, response.attemptId)
                    assertEquals(received.roundId, response.roundId)
                    assertEquals(MODEL_ID, response.modelId)
                    assertEquals("loopback-response", response.outputText)
                    assertEquals(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED, response.outcome)
                    assertEquals(1, client.dispatchProgress(requestId).roundsDispatched)
                    assertEquals(1, client.dispatchProgress(requestId).hostsAttempted)
                } finally {
                    client.close()
                }
            } finally {
                servers.asReversed().forEach { server ->
                    runCatching { server.stop().get(10, TimeUnit.SECONDS) }
                }
            }
        }

    @Test
    fun sendsCancellationToOneNativeDotNetLibp2pHostAndReceivesCancelledResponse() =
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
                repeat(1) {
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
                                        line.startsWith("READY	") -> readyLineFuture.complete(line)
                                        line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                        line.startsWith("SENT_META	") -> {
                                            val fields = line.split('	')
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
                                        line.startsWith("P2P_DIAG	") -> {
                                            val fields = line.split('	', limit = 6)
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
                    val readyFields = readyLine.split('	')
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
                        selectedHosts = listOf(candidate(endpoints.keys.single())),
                        modelId = MODEL_ID,
                        currentMessage = "Cancel the active request on the selected Windows host",
                        capsule = ConversationContextCapsule(goal = "Verify cross-runtime cancellation"),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(dispatch is InferenceRoundDispatchResult.Dispatched)
                val attempt = (dispatch as InferenceRoundDispatchResult.Dispatched).attempts.single()
                assertEquals(InferenceAttemptSubmissionStatus.QUEUED, attempt.submissionStatus)

                withTimeout(15_000L) {
                    while (phaseOutput.none { it.endsWith(":CANCEL_WAITING") }) {
                        delay(25L)
                    }
                }
                val cancellationResults =
                    runtimeClient.cancelSelectedHosts(requestId, listOf(attempt))
                assertEquals(1, cancellationResults.size)
                assertTrue(cancellationResults.single().cancellationSent)

                val response =
                    try {
                        runtimeClient.awaitResponse(
                            requestId = requestId,
                            attemptId = attempt.attemptId,
                            timeout = Duration.ofSeconds(20),
                        )
                    } catch (exception: Exception) {
                        val phases = phaseOutput.toList().joinToString(separator = " | ")
                        val diagnostics = diagnosticOutput.toList().takeLast(60).joinToString(separator = " | ")
                        throw AssertionError(
                            "The Windows host did not return its correlated CANCELLED response; " +
                                "phases=$phases; diagnostics=$diagnostics",
                            exception,
                        )
                    }

                assertEquals(endpoints.keys.single(), response.ephemeralHostId)
                assertEquals(attempt.attemptId, response.attemptId)
                assertEquals(requestId, response.requestId)
                assertEquals(InferenceOutcome.INFERENCE_OUTCOME_CANCELLED, response.outcome)
                assertTrue(response.outputText.isEmpty())
                assertEquals("The request was cancelled by the remote client.", response.errorMessage)

                processes.forEach { process ->
                    assertTrue(
                        "The .NET cancellation fixture should exit after its response.",
                        process.waitFor(15, TimeUnit.SECONDS),
                    )
                    assertEquals(0, process.exitValue())
                }
                assertEquals(1, phaseOutput.count { it.endsWith(":CANCEL_RECEIVED") })
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
    fun dispatchesToOneSelectedNativeDotNetPeerAmongMultipleCandidates() =
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
                                        line.startsWith("READY	") -> readyLineFuture.complete(line)
                                        line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                        line.startsWith("P2P_DIAG	") -> {
                                            val fields = line.split('	', limit = 6)
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
                    val readyFields = readyLine.split('	')
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
                val requestId = "interop-single-selected-" + UUID.randomUUID()
                val selectedPeerId = endpoints.keys.first()
                val dispatch =
                    runtimeClient.dispatchRound(
                        distributedRequest =
                            DistributedRequest(
                                requestId = requestId,
                                deadlineUnixMs = System.currentTimeMillis() + 60_000L,
                                maxRounds = 3,
                            ),
                        selectedHosts = listOf(candidate(selectedPeerId)),
                        modelId = MODEL_ID,
                        currentMessage = "Cross-runtime request to one selected Windows peer",
                        capsule =
                            ConversationContextCapsule(
                                goal = "Verify one authenticated peer receives the product request",
                            ),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )

                assertTrue(dispatch is InferenceRoundDispatchResult.Dispatched)
                val attempt = (dispatch as InferenceRoundDispatchResult.Dispatched).attempts.single()
                assertEquals(selectedPeerId, attempt.ephemeralHostId)
                assertEquals(InferenceAttemptSubmissionStatus.QUEUED, attempt.submissionStatus)

                val response =
                    try {
                        runtimeClient.awaitResponse(
                            requestId = requestId,
                            attemptId = attempt.attemptId,
                            timeout = Duration.ofSeconds(30),
                        )
                    } catch (exception: Exception) {
                        val phases = phaseOutput.toList().joinToString(separator = " | ")
                        val diagnostics = diagnosticOutput.toList().takeLast(60).joinToString(separator = " | ")
                        throw AssertionError(
                            "The selected .NET peer did not return its correlated response; " +
                                "phases=$phases; diagnostics=$diagnostics",
                            exception,
                        )
                    }

                assertEquals(selectedPeerId, response.ephemeralHostId)
                assertEquals(attempt.attemptId, response.attemptId)
                assertEquals(requestId, response.requestId)
                assertEquals(MODEL_ID, response.modelId)
                assertEquals("dotnet-host-response", response.outputText)
                assertEquals(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED, response.outcome)
                assertTrue(
                    "Unselected Windows peers must not receive this request.",
                    phaseOutput.none {
                        it.startsWith("peer-2:RECEIVED") || it.startsWith("peer-3:RECEIVED")
                    },
                )

                val selectedProcess = processes.first()
                assertTrue(selectedProcess.waitFor(15, TimeUnit.SECONDS))
                assertEquals(0, selectedProcess.exitValue())
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
    fun selectsANewNativeDotNetPeerOnlyAfterThePreviousPeerTimesOut() =
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

            suspend fun startPeer(silentResponse: Boolean): RendezvousPeerEndpoint {
                val serverNumber = processes.size + 1
                val port = ServerSocket(0).use { socket -> socket.localPort }
                val command =
                    mutableListOf("dotnet", serverDll!!, "--interop-peer-server", clientPeerId, port.toString())
                if (silentResponse) command += "silent"
                val process = ProcessBuilder(command).redirectErrorStream(true).start()
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
                                    line.startsWith("READY	") -> readyLineFuture.complete(line)
                                    line in allowedMarkers -> phaseOutput.add("peer-" + serverNumber + ":" + line)
                                    line.startsWith("P2P_DIAG	") -> {
                                        val fields = line.split('	', limit = 6)
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
                        "whitep-dotnet-sequential-peer-" + serverNumber + "-output",
                    ).apply { isDaemon = true }
                outputReaders += outputReader
                outputReader.start()

                val readyLine =
                    withContext(Dispatchers.IO) {
                        withTimeout(30_000L) {
                            readyLineFuture.get(30, TimeUnit.SECONDS)
                        }
                    }
                val readyFields = readyLine.split('	')
                assertEquals("READY", readyFields.firstOrNull())
                assertEquals(3, readyFields.size)
                return RendezvousPeerEndpoint(
                    peerId = readyFields[1],
                    tcpMultiaddresses = listOf(readyFields[2]),
                    expiresAtUtc = Instant.now().plusSeconds(180),
                )
            }

            try {
                val firstEndpoint = startPeer(silentResponse = true)
                activeEndpoints[firstEndpoint.peerId] = firstEndpoint
                val firstProcess = processes.single()

                val clientRuntime =
                    AndroidInferenceRequestRuntime(
                        localIdentity = clientIdentity,
                        resolveEndpoint = { requestedPeerId -> activeEndpoints[requestedPeerId] },
                        trustVerifier = PeerTrustVerifier { trustedPeerId -> trustedPeerId in activeEndpoints },
                        connectionTimeout = Duration.ofSeconds(20),
                        roundTimeout = Duration.ofSeconds(5),
                    )
                runtime = clientRuntime

                val requestId = "interop-explicit-next-round-" + UUID.randomUUID()
                val deadline = System.currentTimeMillis() + 90_000L
                val distributedRequest =
                    DistributedRequest(
                        requestId = requestId,
                        deadlineUnixMs = deadline,
                        maxRounds = 2,
                        maxTotalHosts = 2,
                    )
                val selectorRequest =
                    createRequest()
                        .toBuilder()
                        .setRequestId(requestId)
                        .setAttemptId("sequential-timeout-selector")
                        .setRoundId("sequential-timeout-selector-round")
                        .setModelId(MODEL_ID)
                        .setProtocolVersion(PROTOCOL_VERSION)
                        .setDeadlineUnixMs(deadline)
                        .build()

                val firstSelection =
                    HostCandidateSelector.selectFirstRound(
                        distributedRequest = distributedRequest,
                        request = selectorRequest,
                        candidates = listOf(candidate(firstEndpoint.peerId)),
                    )
                assertEquals(1, firstSelection.size)

                val firstDispatch =
                    clientRuntime.dispatchRound(
                        distributedRequest = distributedRequest,
                        selectedHosts = firstSelection,
                        modelId = MODEL_ID,
                        currentMessage = "The first Windows peer deliberately withholds its response",
                        capsule = ConversationContextCapsule(goal = "Verify timeout before the next round"),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(firstDispatch is InferenceRoundDispatchResult.Dispatched)
                val firstAttempt = (firstDispatch as InferenceRoundDispatchResult.Dispatched).attempts.single()
                assertEquals(firstEndpoint.peerId, firstAttempt.ephemeralHostId)
                assertEquals(InferenceAttemptSubmissionStatus.QUEUED, firstAttempt.submissionStatus)

                withTimeout(10_000L) {
                    while (phaseOutput.none { it.endsWith(":RECEIVED_WITHOUT_RESPONSE") }) {
                        delay(25L)
                    }
                }
                assertTrue(firstProcess.isAlive)

                val firstCollection = clientRuntime.awaitRoundResponses(requestId, listOf(firstAttempt))
                assertTrue(firstCollection.responses.isEmpty())
                assertEquals(1, firstCollection.failures.size)
                assertEquals(
                    InferenceRoundResponseFailureReason.RESPONSE_TIMED_OUT,
                    firstCollection.failures.single().reason,
                )
                assertEquals(firstEndpoint.peerId, firstCollection.failures.single().ephemeralHostId)

                if (firstProcess.isAlive) {
                    firstProcess.destroyForcibly()
                    firstProcess.waitFor(5, TimeUnit.SECONDS)
                }
                activeEndpoints.clear()

                val secondEndpoint = startPeer(silentResponse = false)
                activeEndpoints[secondEndpoint.peerId] = secondEndpoint
                assertTrue(firstEndpoint.peerId != secondEndpoint.peerId)
                val secondSelection =
                    HostCandidateSelector.selectNextRound(
                        distributedRequest = distributedRequest,
                        request = selectorRequest,
                        candidates = listOf(candidate(secondEndpoint.peerId)),
                        previousRoundAttempts = listOf(firstAttempt),
                        previousRoundResponses = firstCollection,
                        progress = clientRuntime.dispatchProgress(requestId),
                    )
                assertEquals(1, secondSelection.size)
                assertEquals(secondEndpoint.peerId, secondSelection.single().announcement.ephemeralHostId)

                val secondDispatch =
                    clientRuntime.dispatchRound(
                        distributedRequest = distributedRequest,
                        selectedHosts = secondSelection,
                        modelId = MODEL_ID,
                        currentMessage = "The next Windows peer responds after the previous peer timed out",
                        capsule = ConversationContextCapsule(goal = "Verify a different peer is used in the next round"),
                        maxOutputTokens = 8,
                        protocolVersion = PROTOCOL_VERSION,
                    )
                assertTrue(secondDispatch is InferenceRoundDispatchResult.Dispatched)
                val secondAttempt = (secondDispatch as InferenceRoundDispatchResult.Dispatched).attempts.single()
                val secondCollection = clientRuntime.awaitRoundResponses(requestId, listOf(secondAttempt))
                assertEquals(1, secondCollection.responses.size)
                assertTrue(secondCollection.failures.isEmpty())
                val response = secondCollection.responses.single()
                assertEquals(secondEndpoint.peerId, response.ephemeralHostId)
                assertEquals(secondAttempt.attemptId, response.attemptId)
                assertEquals(secondAttempt.roundId, response.roundId)
                assertEquals(requestId, response.requestId)
                assertEquals(MODEL_ID, response.modelId)
                assertEquals("dotnet-host-response", response.outputText)
                assertEquals(InferenceOutcome.INFERENCE_OUTCOME_SUCCEEDED, response.outcome)
                assertEquals(
                    DistributedRequestProgress(
                        roundsDispatched = 2,
                        hostsAttempted = 2,
                        attemptedEphemeralHostIds = listOf(firstEndpoint.peerId, secondEndpoint.peerId),
                    ),
                    clientRuntime.dispatchProgress(requestId),
                )

                processes.filter { it.isAlive }.forEach { process ->
                    assertTrue(process.waitFor(15, TimeUnit.SECONDS))
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
