package com.whitep.client.requests

import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.network.RendezvousPeerEndpoint
import com.whitep.client.protocol.InferenceRequestPayloadBuilder
import java.time.Clock
import kotlinx.coroutines.CancellationException
import org.whitep.shared.contracts.v1.HostAnnouncement
import org.whitep.shared.contracts.v1.HostCapabilities
import org.whitep.shared.contracts.v1.HostRuntimeState
import org.whitep.shared.contracts.v1.HostStatus
import org.whitep.shared.contracts.v1.InferenceRequest

/**
 * Joins metadata-only Rendezvous routes with fresh, authenticated HostStatus responses.
 *
 * The supplied probe must enforce explicit peer trust and authenticated libp2p identity.
 * The production runtime binds it to Libp2pInferenceRequestTransport.probeHostStatus.
 * No model capability is inferred from the public "inference.v1" directory tag.
 */
internal class AuthenticatedHostDiscovery(
    private val discoverEndpoints: suspend () -> List<RendezvousPeerEndpoint>,
    private val probeHostStatus: suspend (String) -> HostStatus,
    private val clock: Clock = Clock.systemUTC(),
) {
    /**
     * Returns metadata identities for a user-mediated trust decision only.
     * This does not probe, trust, or select a peer for inference.
     */
    suspend fun discoverPeersForTrust(): List<RendezvousPeerEndpoint> {
        val endpoints = discoverEndpoints()
        val peerIds = endpoints.map { endpoint -> endpoint.peerId }
        require(peerIds.none(String::isBlank) && peerIds.distinct().size == peerIds.size) {
            "The Rendezvous directory returned an empty or duplicate peer identity."
        }
        val now = clock.instant()
        return endpoints
            .filter { endpoint ->
                endpoint.tcpMultiaddresses.isNotEmpty() && endpoint.expiresAtUtc.isAfter(now)
            }.sortedBy { endpoint -> endpoint.peerId }
    }

    suspend fun selectReachableHosts(
        request: InferenceRequest,
        excludedEphemeralHostIds: Set<String> = emptySet(),
        maxHosts: Int = HostCandidateSelector.MAX_HOSTS_PER_ROUND,
    ): HostSelectionResult {
        require(maxHosts in 1..HostCandidateSelector.MAX_HOSTS_PER_ROUND) {
            "The host selection limit must be between one and the per-round ceiling."
        }

        val endpoints = discoverEndpoints()
        val peerIds = endpoints.map { endpoint -> endpoint.peerId }
        require(peerIds.none(String::isBlank) && peerIds.distinct().size == peerIds.size) {
            "The Rendezvous directory returned an empty or duplicate peer identity."
        }

        val candidates = ArrayList<HostCandidate>(maxHosts)
        for (endpoint in endpoints.sortedBy { peer -> peer.peerId }) {
            if (candidates.size >= maxHosts) break
            if (endpoint.tcpMultiaddresses.isEmpty() ||
                !endpoint.expiresAtUtc.isAfter(clock.instant()) ||
                endpoint.peerId in excludedEphemeralHostIds
            ) {
                continue
            }

            val status =
                try {
                    probeHostStatus(endpoint.peerId)
                } catch (cancellation: CancellationException) {
                    throw cancellation
                } catch (_: Exception) {
                    // Untrusted, unreachable, or unresponsive peers are not selectable.
                    continue
                }

            if (!endpoint.expiresAtUtc.isAfter(clock.instant())) continue
            val candidate = createCandidate(endpoint, status) ?: continue
            val validCandidate =
                HostCandidateSelector.select(
                    request = request,
                    candidates = listOf(candidate),
                    excludedEphemeralHostIds = excludedEphemeralHostIds,
                    maxHosts = maxHosts,
                )
            if (validCandidate.isNotEmpty()) candidates.add(candidate)
        }

        val selected =
            HostCandidateSelector.select(
                request = request,
                candidates = candidates,
                excludedEphemeralHostIds = excludedEphemeralHostIds,
                maxHosts = maxHosts,
            )
        return HostSelectionResult(
            candidates = selected,
            feedback =
                HostSelectionFeedback(
                    availableHostCount = selected.size,
                    requestedHostCount = maxHosts,
                ),
        )
    }

    private fun createCandidate(
        endpoint: RendezvousPeerEndpoint,
        status: HostStatus,
    ): HostCandidate? {
        val now = clock.millis()
        if (endpoint.peerId != status.ephemeralHostId ||
            status.state != HostRuntimeState.HOST_RUNTIME_STATE_READY ||
            status.protocolVersion.isBlank() ||
            status.observedAtUnixMs <= 0L ||
            status.observedAtUnixMs > now + MAX_STATUS_CLOCK_SKEW_MILLIS ||
            now - status.observedAtUnixMs > MAX_STATUS_AGE_MILLIS ||
            status.activeRequests !in 0..MAX_HOST_STATUS_ACTIVE_REQUESTS
        ) {
            return null
        }

        // HostInferenceSession serializes inference and backend transitions through one operation
        // gate; MainWindow reports one active request while Busy. These are not guessed settings.
        val capabilities =
            HostCapabilities
                .newBuilder()
                .setEphemeralHostId(endpoint.peerId)
                .setProtocolVersion(status.protocolVersion)
                .addAllModels(status.modelsList)
                .setMaxConcurrentRequests(1)
                .setMaxPromptBytes(InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES.toLong())
                .setMaxContextCapsuleBytes(ConversationContextCapsule.MAX_SERIALIZED_BYTES.toLong())
                .build()
        val announcement =
            HostAnnouncement
                .newBuilder()
                .setEphemeralHostId(endpoint.peerId)
                .setProtocolVersion(status.protocolVersion)
                .setCapabilities(capabilities)
                .setAnnouncedAtUnixMs(status.observedAtUnixMs)
                .build()
        return HostCandidate(announcement, status)
    }

    private companion object {
        const val MAX_STATUS_AGE_MILLIS = 15_000L
        const val MAX_STATUS_CLOCK_SKEW_MILLIS = 5_000L
        const val MAX_HOST_STATUS_ACTIVE_REQUESTS = 8
    }
}
