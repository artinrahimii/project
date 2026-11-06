package com.whitep.client.requests

import android.content.Context
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.network.RendezvousPeerEndpoint
import org.whitep.shared.contracts.v1.InferenceRequest
import java.io.Closeable
import java.net.URI

/** Application boundary used by the request UI to discover, trust and contact real peers. */
interface RequestExecutionGateway : Closeable {
    fun isPeerTrusted(peerId: String): Boolean

    fun trustPeerAfterExplicitConfirmation(
        peerId: String,
        explicitlyConfirmed: Boolean,
    )

    fun revokePeerTrust(peerId: String)

    suspend fun discoverPeersForTrust(): List<RendezvousPeerEndpoint>

    suspend fun discoverAndSelectReachableHosts(
        request: InferenceRequest,
        excludedEphemeralHostIds: Set<String> = emptySet(),
        maxHosts: Int = HostCandidateSelector.MAX_HOSTS_PER_ROUND,
    ): HostSelectionResult

    suspend fun dispatchRound(
        distributedRequest: DistributedRequest,
        selectedHosts: List<HostCandidate>,
        modelId: String,
        currentMessage: String,
        capsule: ConversationContextCapsule,
        maxOutputTokens: Int,
        protocolVersion: String,
    ): InferenceRoundDispatchResult

    suspend fun cancelSelectedHosts(
        requestId: String,
        selectedAttempts: List<InferenceHostAttempt>,
    ): List<InferenceAttemptCancellationResult>

    suspend fun awaitRoundResponses(
        requestId: String,
        attempts: List<InferenceHostAttempt>,
    ): InferenceRoundResponseCollection
}

fun interface RequestExecutionGatewayFactory {
    fun create(
        context: Context,
        rendezvousServiceBaseUri: URI,
    ): RequestExecutionGateway
}

/** Production factory; it never invents a directory URL or silently trusts peers. */
object AndroidRequestExecutionGatewayFactory : RequestExecutionGatewayFactory {
    override fun create(
        context: Context,
        rendezvousServiceBaseUri: URI,
    ): RequestExecutionGateway =
        AndroidInferenceRequestRuntime.create(
            context = context.applicationContext,
            rendezvousServiceBaseUri = rendezvousServiceBaseUri,
        )
}
