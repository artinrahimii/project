package com.whitep.client.network

import org.junit.Assert.assertEquals
import org.junit.Test

class SafeDiagnosticLogTest {
    @Test
    fun formatsOnlyTheAllowlistedNumericFields() {
        assertEquals(
            """{"schema":1,"event_code":2,"outcome_code":2,"duration_ms":27}""",
            SafeDiagnosticLog.formatRecord(
                SafeDiagnosticEventCode.INFERENCE_SUBMISSION,
                SafeDiagnosticOutcomeCode.SUCCEEDED,
                27L,
            ),
        )
    }

    @Test(expected = IllegalArgumentException::class)
    fun rejectsNegativeDurations() {
        SafeDiagnosticLog.formatRecord(
            SafeDiagnosticEventCode.INFERENCE_SUBMISSION,
            SafeDiagnosticOutcomeCode.FAILED,
            -1L,
        )
    }

    @Test
    fun diagnosticRecordDoesNotContainPromptCapsuleOrSecretCanaries() {
        val record =
            SafeDiagnosticLog.formatRecord(
                SafeDiagnosticEventCode.INFERENCE_SUBMISSION,
                SafeDiagnosticOutcomeCode.SUCCEEDED,
                27L,
            )
        listOf(
            "T0187_PROMPT_CANARY_7F2A",
            "T0187_CONTEXT_CAPSULE_CANARY_91C4",
            "T0187_SECRET_CANARY_3BD8",
        ).forEach { marker ->
            assertEquals(false, record.contains(marker))
        }
        assertEquals(true, record.contains("\"event_code\":2"))
        assertEquals(true, record.contains("\"outcome_code\":2"))
    }

    @Test
    fun usesStableNumericEventAndOutcomeCodes() {
        assertEquals(1, SafeDiagnosticEventCode.PEER_TRANSPORT_CONNECTION.code)
        assertEquals(2, SafeDiagnosticEventCode.INFERENCE_SUBMISSION.code)
        assertEquals(1, SafeDiagnosticOutcomeCode.STARTED.code)
        assertEquals(5, SafeDiagnosticOutcomeCode.CANCELLED.code)
    }
}
