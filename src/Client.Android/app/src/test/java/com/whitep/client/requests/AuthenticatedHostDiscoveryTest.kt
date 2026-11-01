package com.whitep.client.requests

import com.whitep.client.network.RendezvousPeerEndpoint
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.runBlocking
import org.junit.Assert.assertEquals
import org.junit.Assert.assertThrows
import org.junit.Assert.assertTrue
import org.junit.Test
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.ModelCapability
import org.whitep.shared.contracts.v1.hostStatus
import org.whitep.shared.contracts.v1.inferenceRequest
import org.whitep.shared.contracts.v1.modelCapability
import java.time.Clock
import java.time.Instant
import java.time.ZoneOffset

class AuthenticatedHostDiscoveryTest {
    private val now = Instant.parse("2026-11-01T12:00:00Z")
    private val clock = Clock.fixed(now, ZoneOffset.UTC)
    private val peerIds =
        listOf(
            "QmYwAPJzv5CZsnAzt8auVTLz",
            "QmYwAPJzv5CZsnAzt8auVTLa",
            "QmYwAPJzv5CZsnAzt8auVTLb",
            "QmYwAPJzv5CZsnAzt8auVTLc",
        )

    @Test
    fun selectsThreeFreshAuthenticatedCompatibleHostsInStablePeerIdOrder() =
        runBlocking {
            val probed = mutableListOf<String>()
            val endpoints = peerIds.reversed().map(::endpoint)
            val discovery =
                AuthenticatedHostDiscovery(
                    discoverEndpoints = { endpoints },
                    probeHostStatus = { peerId ->
                        probed.add(peerId)
                        readyStatus(peerId)
                    },
                    clock = clock,
                )

            val result = discovery.selectReachableHosts(validRequest())

            assertEquals(peerIds.drop(1).take(3), result.candidates.map { it.announcement.ephemeralHostId })
            assertEquals(peerIds.drop(1).take(3), probed)
            assertEquals(3, result.feedback.availableHostCount)
            assertTrue(result.feedback.canContinue)
        }

    @Test
    fun returnsExactZeroOneTwoAndThreeHostCountsFromPostProbeResults() =
        runBlocking {
            for (availableCount in 0..3) {
                val endpoints = peerIds.take(availableCount).map(::endpoint)
                val statuses = peerIds.take(availableCount).associateWith(::readyStatus)
                val discovery =
                    AuthenticatedHostDiscovery(
                        discoverEndpoints = { endpoints },
                        probeHostStatus = { peerId -> statuses.getValue(peerId) },
                        clock = clock,
                    )

                val result = discovery.selectReachableHosts(validRequest())

                assertEquals(availableCount, result.candidates.size)
                assertEquals(availableCount, result.feedback.availableHostCount)
                assertEquals(3, result.feedback.requestedHostCount)
                assertEquals(availableCount > 0, result.feedback.canContinue)
            }
        }

    @Test
    fun skipsExpiredUntrustedBusyStaleAndModelIncompatiblePeers() =
        runBlocking {
            val expiredPeer = peerIds[0]
            val untrustedPeer = peerIds[1]
            val busyPeer = peerIds[2]
            val stalePeer = peerIds[3]
            val incompatiblePeer = "QmYwAPJzv5CZsnAzt8auVTLd"
            val endpoints =
                listOf(
                    endpoint(expiredPeer, now.minusSeconds(1)),
                    endpoint(untrustedPeer),
                    endpoint(busyPeer),
                    endpoint(stalePeer),
                    endpoint(incompatiblePeer),
                )
            val probed = mutableListOf<String>()
            val discovery =
                AuthenticatedHostDiscovery(
                    discoverEndpoints = { endpoints },
                    probeHostStatus = { peerId ->
                        probed.add(peerId)
                        when (peerId) {
                            untrustedPeer -> throw SecurityException("Peer is not trusted.")
                            busyPeer -> readyStatus(peerId, state = HostRuntimeState.HOST_RUNTIME_STATE_BUSY)
                            stalePeer -> readyStatus(peerId, observedAt = now.minusSeconds(30))
                            incompatiblePeer -> readyStatus(peerId, modelId = "another-model")
                            else -> readyStatus(peerId)
                        }
                    },
                    clock = clock,
                )

            val result = discovery.selectReachableHosts(validRequest())

            assertTrue(result.candidates.isEmpty())
            assertEquals(0, result.feedback.availableHostCount)
            assertTrue(result.feedback.message.contains("request was not sent"))
            assertEquals(listOf(untrustedPeer, busyPeer, stalePeer, incompatiblePeer), probed)
        }

    @Test
    fun rejectsDuplicatePeerIdentitiesReturnedByTheDirectory() {
        val duplicate = endpoint(peerIds.first())
        val discovery =
            AuthenticatedHostDiscovery(
                discoverEndpoints = { listOf(duplicate, duplicate) },
                probeHostStatus = { peerId -> readyStatus(peerId) },
                clock = clock,
            )

        assertThrows(IllegalArgumentException::class.java) {
            runBlocking { discovery.selectReachableHosts(validRequest()) }
        }
    }

    @Test
    fun propagatesCoroutineCancellationInsteadOfTreatingItAsAnUnavailablePeer() {
        val discovery =
            AuthenticatedHostDiscovery(
                discoverEndpoints = { listOf(endpoint(peerIds.first())) },
                probeHostStatus = { throw CancellationException("cancelled") },
                clock = clock,
            )

        assertThrows(CancellationException::class.java) {
            runBlocking { discovery.selectReachableHosts(validRequest()) }
        }
    }

    @Test
    fun rejectsAStatusSnapshotForADifferentAuthenticatedIdentity() =
        runBlocking {
            val expectedPeer = peerIds.first()
            val discovery =
                AuthenticatedHostDiscovery(
                    discoverEndpoints = { listOf(endpoint(expectedPeer)) },
                    probeHostStatus = { readyStatus(peerIds[1]) },
                    clock = clock,
                )

            val result = discovery.selectReachableHosts(validRequest())

            assertTrue(result.candidates.isEmpty())
            assertEquals(0, result.feedback.availableHostCount)
        }

    private fun endpoint(
        peerId: String,
        expiresAt: Instant = now.plusSeconds(300),
    ): RendezvousPeerEndpoint =
        RendezvousPeerEndpoint(
            peerId = peerId,
            tcpMultiaddresses = listOf("/ip4/127.0.0.1/tcp/41000/p2p/" + peerId),
            expiresAtUtc = expiresAt,
        )

    private fun readyStatus(
        peerId: String,
        state: HostRuntimeState = HostRuntimeState.HOST_RUNTIME_STATE_READY,
        observedAt: Instant = now,
        modelId: String = "model-a",
    ): HostStatus {
        val runtimeModel =
            modelCapability {
                this.modelId = modelId
                displayName = "Test model"
                maxContextTokens = 8_192L
                maxOutputTokens = 512L
                supportsStreaming = true
            }
        return hostStatus {
            ephemeralHostId = peerId
            this.state = state
            activeRequests = 0
            observedAtUnixMs = observedAt.toEpochMilli()
            protocolVersion = "1.0.0"
            models += runtimeModel
        }
    }

    private fun validRequest(): InferenceRequest =
        inferenceRequest {
            requestId = "request-1"
            attemptId = "attempt-1"
            roundId = "round-1"
            modelId = "model-a"
            prompt = "hello"
            deadlineUnixMs = now.plusSeconds(60).toEpochMilli()
            maxOutputTokens = 128
            protocolVersion = "1.0.0"
        }
}
