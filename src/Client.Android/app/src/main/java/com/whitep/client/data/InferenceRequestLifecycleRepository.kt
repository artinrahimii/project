package com.whitep.client.data

/** Only non-content metadata needed to explain the last local inference lifecycle. */
data class InferenceRequestLifecycleState(
    val requestId: String,
    val status: PersistedRequestStatus,
    val createdAtEpochMillis: Long,
    val updatedAtEpochMillis: Long,
    val attemptNumber: Int,
    val errorCode: String? = null,
)

interface InferenceRequestLifecycleRepository {
    /**
     * On a new request-screen process/owner, mark previous in-flight rows EXPIRED and return
     * the latest local record. This does not claim that a remote host stopped processing.
     */
    suspend fun recoverLatest(nowEpochMillis: Long): InferenceRequestLifecycleState?

    suspend fun save(state: InferenceRequestLifecycleState)
}

class RoomInferenceRequestLifecycleRepository(
    private val dao: InferenceRequestStateDao,
) : InferenceRequestLifecycleRepository {
    override suspend fun recoverLatest(nowEpochMillis: Long): InferenceRequestLifecycleState? =
        dao.recoverLatestAfterProcessStart(nowEpochMillis)?.toLifecycleState()

    override suspend fun save(state: InferenceRequestLifecycleState) {
        dao.upsert(
            InferenceRequestStateEntity(
                requestId = state.requestId,
                status = state.status,
                createdAtEpochMillis = state.createdAtEpochMillis,
                updatedAtEpochMillis = state.updatedAtEpochMillis,
                attemptNumber = state.attemptNumber,
                errorCode = state.errorCode,
            ),
        )
        dao.retainMostRecent(MAX_RETAINED_REQUESTS)
    }

    private fun InferenceRequestStateEntity.toLifecycleState() =
        InferenceRequestLifecycleState(
            requestId = requestId,
            status = status,
            createdAtEpochMillis = createdAtEpochMillis,
            updatedAtEpochMillis = updatedAtEpochMillis,
            attemptNumber = attemptNumber,
            errorCode = errorCode,
        )

    private companion object {
        const val MAX_RETAINED_REQUESTS = 20
    }
}

/** Deterministic store used by ViewModel tests and preview-only construction. */
class InMemoryInferenceRequestLifecycleRepository(
    initialState: InferenceRequestLifecycleState? = null,
) : InferenceRequestLifecycleRepository {
    private var latestState: InferenceRequestLifecycleState? = initialState

    override suspend fun recoverLatest(nowEpochMillis: Long): InferenceRequestLifecycleState? {
        val current = latestState ?: return null
        if (current.status.isInFlight()) {
            latestState =
                current.copy(
                    status = PersistedRequestStatus.EXPIRED,
                    updatedAtEpochMillis = nowEpochMillis,
                    errorCode = APP_CLOSED_BEFORE_RESULT,
                )
        }
        return latestState
    }

    override suspend fun save(state: InferenceRequestLifecycleState) {
        latestState = state
    }

    private fun PersistedRequestStatus.isInFlight(): Boolean =
        this == PersistedRequestStatus.QUEUED ||
            this == PersistedRequestStatus.CONTACTING ||
            this == PersistedRequestStatus.WAITING

    private companion object {
        const val APP_CLOSED_BEFORE_RESULT = "APP_CLOSED_BEFORE_RESULT"
    }
}
