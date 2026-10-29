package com.whitep.client.ui.request

import com.whitep.client.data.PersistedRequestStatus

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
