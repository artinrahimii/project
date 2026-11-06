package com.whitep.client.context

import org.json.JSONArray
import org.json.JSONObject
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import org.robolectric.annotation.Config

@RunWith(RobolectricTestRunner::class)
@Config(sdk = [35])
class ConversationContextCapsuleTest {
    @Test
    fun serializesAllVersionedFieldsAndRoundTripsJsonEscapes() {
        val capsule =
            ConversationContextCapsule(
                goal = "Prepare for launch: \"safe\"",
                keyFacts = listOf("Local model", "Use UTF-8"),
                decisions = listOf("Do not send to a central service"),
                constraints = listOf("Offline-capable", "Direct path only"),
                unresolvedItems = listOf("Validate on a second device"),
            )

        val encoded = capsule.encode()
        assertTrue(encoded.isValid)
        assertNotNull(encoded.bytes)
        assertEquals(encoded.bytes?.size, encoded.byteCount)

        val decoded = ConversationContextCapsule.decode(encoded.bytes!!)
        assertTrue(decoded.isValid)
        assertEquals(capsule, decoded.capsule)

        val objectJson = JSONObject(String(encoded.bytes, Charsets.UTF_8))
        assertEquals(ConversationContextCapsule.CURRENT_SCHEMA_VERSION, objectJson.getInt("schemaVersion"))
        assertEquals(capsule.goal, objectJson.getString("goal"))
        assertEquals(capsule.keyFacts, objectJson.getJSONArray("keyFacts").toStringList())
        assertEquals(capsule.decisions, objectJson.getJSONArray("decisions").toStringList())
        assertEquals(capsule.constraints, objectJson.getJSONArray("constraints").toStringList())
        assertEquals(capsule.unresolvedItems, objectJson.getJSONArray("unresolvedItems").toStringList())
    }

    @Test
    fun defaultsToSchemaVersionOneAndRejectsUnsupportedVersions() {
        assertEquals(1, ConversationContextCapsule.CURRENT_SCHEMA_VERSION)
        assertEquals(1, ConversationContextCapsule(goal = "goal").schemaVersion)
        assertEquals(
            ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION,
            ConversationContextCapsule(schemaVersion = 2, goal = "goal").encode().error,
        )
        assertEquals(
            ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION,
            ConversationContextCapsule.decode(encodedObject(version = 2)).error,
        )
    }

    @Test
    fun rejectsBlankGoalsAndBlankItems() {
        assertEquals(
            ConversationContextCapsuleError.EMPTY_GOAL,
            ConversationContextCapsule(goal = " \n ").encode().error,
        )
        assertEquals(
            ConversationContextCapsuleError.BLANK_ITEM,
            ConversationContextCapsule(goal = "goal", keyFacts = listOf(" \t ")).encode().error,
        )
        assertEquals(
            ConversationContextCapsuleError.BLANK_ITEM,
            ConversationContextCapsule(goal = "goal", unresolvedItems = listOf("")).encode().error,
        )
    }

    @Test
    fun acceptsSerializedPayloadAtExactlyTheFourKilobyteCeiling() {
        val baselineBytes = ConversationContextCapsule(goal = "x").encode().bytes!!.size
        val exactGoalLength = ConversationContextCapsule.MAX_SERIALIZED_BYTES - baselineBytes + 1
        val encoded = ConversationContextCapsule(goal = "x".repeat(exactGoalLength)).encode()

        assertTrue(encoded.isValid)
        assertEquals(ConversationContextCapsule.MAX_SERIALIZED_BYTES, encoded.byteCount)
        assertTrue(ConversationContextCapsule.decode(encoded.bytes!!).isValid)
    }

    @Test
    fun rejectsOversizedPayloadUsingUtf8EncodedByteCount() {
        val encoded = ConversationContextCapsule(goal = "🙂".repeat(1_024)).encode()
        assertFalse(encoded.isValid)
        assertEquals(ConversationContextCapsuleError.TOO_LARGE, encoded.error)
        assertTrue(encoded.byteCount!! > ConversationContextCapsule.MAX_SERIALIZED_BYTES)
    }

    @Test
    fun rejectsOversizedInputBeforeJsonParsing() {
        val result =
            ConversationContextCapsule.decode(
                ByteArray(ConversationContextCapsule.MAX_SERIALIZED_BYTES + 1),
            )

        assertFalse(result.isValid)
        assertEquals(ConversationContextCapsuleError.TOO_LARGE, result.error)
        assertEquals(ConversationContextCapsule.MAX_SERIALIZED_BYTES + 1, result.byteCount)
    }

    @Test
    fun rejectsMalformedJsonAndInvalidUtf8() {
        assertEquals(
            ConversationContextCapsuleError.MALFORMED_JSON,
            ConversationContextCapsule.decode("{bad-json".toByteArray(Charsets.UTF_8)).error,
        )
        assertEquals(
            ConversationContextCapsuleError.INVALID_UTF8,
            ConversationContextCapsule.decode(byteArrayOf(0xC3.toByte(), 0x28)).error,
        )
    }

    @Test
    fun rejectsMissingUnknownAndIncorrectlyTypedFields() {
        assertEquals(
            ConversationContextCapsuleError.MISSING_FIELD,
            ConversationContextCapsule.decode("""{"schemaVersion":1}""".toByteArray(Charsets.UTF_8)).error,
        )

        val unknown = JSONObject(String(encodedObject(), Charsets.UTF_8)).put("futureField", true)
        assertEquals(
            ConversationContextCapsuleError.UNKNOWN_FIELD,
            ConversationContextCapsule.decode(unknown.toString().toByteArray(Charsets.UTF_8)).error,
        )

        val invalidType =
            JSONObject(String(encodedObject(), Charsets.UTF_8))
                .put("keyFacts", JSONArray().put(17))
        assertEquals(
            ConversationContextCapsuleError.INVALID_FIELD_TYPE,
            ConversationContextCapsule.decode(invalidType.toString().toByteArray(Charsets.UTF_8)).error,
        )
    }

    @Test
    fun enforcesConfigurableSizeLimitBelowTheHardProtocolCeiling() {
        val capsule = ConversationContextCapsule(goal = "x".repeat(700))
        val rejected = capsule.encode(maxSerializedBytes = 512)
        assertFalse(rejected.isValid)
        assertEquals(ConversationContextCapsuleError.TOO_LARGE, rejected.error)
        assertTrue(rejected.byteCount!! > 512)

        val accepted = capsule.encode(maxSerializedBytes = 1_024)
        assertTrue(accepted.isValid)
        assertTrue(accepted.byteCount!! <= 1_024)
    }

    @Test
    fun rejectsInvalidConfiguredSizeLimits() {
        for (limit in listOf(0, 511, 513, ConversationContextCapsule.MAX_SERIALIZED_BYTES + 256)) {
            assertEquals(
                ConversationContextCapsuleError.INVALID_SIZE_LIMIT,
                ConversationContextCapsule(goal = "goal").encode(maxSerializedBytes = limit).error,
            )
        }
    }

    private fun encodedObject(version: Int = 1): ByteArray =
        JSONObject()
            .put("schemaVersion", version)
            .put("goal", "goal")
            .put("keyFacts", JSONArray())
            .put("decisions", JSONArray())
            .put("constraints", JSONArray())
            .put("unresolvedItems", JSONArray())
            .toString()
            .toByteArray(Charsets.UTF_8)

    private fun JSONArray.toStringList(): List<String> = (0 until length()).map { getString(it) }
}
