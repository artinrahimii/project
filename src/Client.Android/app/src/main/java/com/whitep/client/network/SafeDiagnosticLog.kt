package com.whitep.client.network

import android.util.Log
import java.util.concurrent.atomic.AtomicInteger

/** Fixed numeric diagnostic categories shared with the Windows host schema. */
internal enum class SafeDiagnosticEventCode(
    val code: Int,
) {
    PEER_TRANSPORT_CONNECTION(1),
    INFERENCE_SUBMISSION(2),
}

internal enum class SafeDiagnosticOutcomeCode(
    val code: Int,
) {
    STARTED(1),
    SUCCEEDED(2),
    REJECTED(3),
    FAILED(4),
    CANCELLED(5),
}

/**
 * Emits only schema, event_code, outcome_code and duration_ms.
 * The API accepts no request, peer, model, address, key, exception or message strings.
 */
internal object SafeDiagnosticLog {
    private const val TAG = "WhiteP.SafeDiagnostics"
    private const val MAXIMUM_RECORDS_PER_PROCESS = 80
    private val written = AtomicInteger(0)

    internal fun formatRecord(
        eventCode: SafeDiagnosticEventCode,
        outcomeCode: SafeDiagnosticOutcomeCode,
        durationMilliseconds: Long = 0L,
    ): String {
        require(durationMilliseconds >= 0L) { "Diagnostic duration cannot be negative." }
        return buildString {
            append("""{"schema":1,"event_code":""")
            append(eventCode.code)
            append(""","outcome_code":""")
            append(outcomeCode.code)
            append(""","duration_ms":""")
            append(durationMilliseconds)
            append('}')
        }
    }

    internal fun write(
        eventCode: SafeDiagnosticEventCode,
        outcomeCode: SafeDiagnosticOutcomeCode,
        durationMilliseconds: Long = 0L,
    ) {
        val previous =
            written.getAndUpdate { count ->
                if (count < MAXIMUM_RECORDS_PER_PROCESS) count + 1 else count
            }
        if (previous >= MAXIMUM_RECORDS_PER_PROCESS) return
        Log.i(TAG, formatRecord(eventCode, outcomeCode, durationMilliseconds))
    }
}
