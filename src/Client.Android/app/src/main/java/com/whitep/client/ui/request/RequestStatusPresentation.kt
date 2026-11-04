package com.whitep.client.ui.request

import com.whitep.client.data.PersistedRequestStatus
import com.whitep.client.data.RequestDraftStatus

data class RequestStatusPresentation(
    val label: String,
    val detail: String,
)

/** User-facing meanings for locally persisted request lifecycle states. */
fun PersistedRequestStatus.toPresentation(): RequestStatusPresentation =
    when (this) {
        PersistedRequestStatus.QUEUED ->
            RequestStatusPresentation(
                label = "Queued",
                detail = "The request is waiting in the local queue.",
            )
        PersistedRequestStatus.CONTACTING ->
            RequestStatusPresentation(
                label = "Contacting",
                detail = "The client is attempting to contact the selected host.",
            )
        PersistedRequestStatus.WAITING ->
            RequestStatusPresentation(
                label = "Waiting",
                detail = "The request is marked as waiting for a result.",
            )
        PersistedRequestStatus.PARTIAL ->
            RequestStatusPresentation(
                label = "Partial",
                detail = "The request has a partial-result status.",
            )
        PersistedRequestStatus.COMPLETED ->
            RequestStatusPresentation(
                label = "Completed",
                detail = "The request has been marked as completed.",
            )
        PersistedRequestStatus.FAILED ->
            RequestStatusPresentation(
                label = "Failed",
                detail = "The request has been marked as failed.",
            )
        PersistedRequestStatus.CANCELLED ->
            RequestStatusPresentation(
                label = "Cancelled",
                detail = "The request has been marked as cancelled.",
            )
    }

data class NetworkInferenceStatusPresentation(
    val networkStatus: String,
    val inferenceStatus: String,
)

/** Presents lifecycle state only; request prompts, peer messages and response bodies are excluded. */
fun RequestUiState.toNetworkInferenceStatusPresentation(): NetworkInferenceStatusPresentation {
    val trustedPeerCount = discoveredPeers.count { peer -> peer.isTrusted }
    val networkStatus =
        when {
            isDiscoveringPeers -> "Discovering peers."
            isCancellingRequest -> "Sending cancellation to selected peers."
            isSendingRequest -> "Request transport is active."
            discoveredPeers.isNotEmpty() ->
                "Peers discovered: ${discoveredPeers.size}; trusted: $trustedPeerCount."
            status == RequestDraftStatus.TRANSPORT_UNAVAILABLE -> "Transport unavailable."
            else -> "No peers discovered."
        }
    val inferenceStatus =
        when {
            isCancellingRequest -> "Cancellation requested."
            isRetryingRequest -> "Retrying failed attempts."
            else ->
                when (runStatus) {
                    RequestRunStatus.IDLE -> "Not started."
                    RequestRunStatus.RUNNING -> "Running."
                    RequestRunStatus.SUCCEEDED -> "Completed."
                    RequestRunStatus.PARTIAL -> "Partial result."
                    RequestRunStatus.FAILED -> "Failed."
                    RequestRunStatus.CANCELLED -> "Cancelled."
                }
        }
    return NetworkInferenceStatusPresentation(
        networkStatus = networkStatus,
        inferenceStatus = inferenceStatus,
    )
}
