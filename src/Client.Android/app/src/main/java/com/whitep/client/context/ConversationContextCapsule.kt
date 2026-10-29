package com.whitep.client.context

import org.json.JSONArray
import org.json.JSONException
import org.json.JSONObject
import java.nio.ByteBuffer
import java.nio.charset.CharacterCodingException
import java.nio.charset.CodingErrorAction

/** Data captured locally for one conversation; it is not a network identity or a transport. */
data class ConversationContextCapsule(
    val schemaVersion: Int = CURRENT_SCHEMA_VERSION,
    val goal: String,
    val keyFacts: List<String> = emptyList(),
    val decisions: List<String> = emptyList(),
    val constraints: List<String> = emptyList(),
    val unresolvedItems: List<String> = emptyList(),
) {
    /**
     * Encodes this capsule as versioned UTF-8 JSON. The byte limit is the existing protocol
     * ceiling; later settings may choose a lower limit but must never exceed this hard ceiling.
     */
    fun encode(): ConversationContextCapsuleEncodingResult {
        if (schemaVersion != CURRENT_SCHEMA_VERSION) {
            return ConversationContextCapsuleEncodingResult(
                bytes = null,
                error = ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION,
                byteCount = null,
            )
        }
        if (goal.isBlank()) {
            return ConversationContextCapsuleEncodingResult(
                bytes = null,
                error = ConversationContextCapsuleError.EMPTY_GOAL,
                byteCount = null,
            )
        }
        if (
            listOf(keyFacts, decisions, constraints, unresolvedItems)
                .flatten()
                .any { it.isBlank() }
        ) {
            return ConversationContextCapsuleEncodingResult(
                bytes = null,
                error = ConversationContextCapsuleError.BLANK_ITEM,
                byteCount = null,
            )
        }

        val json =
            JSONObject()
                .put(FIELD_SCHEMA_VERSION, schemaVersion)
                .put(FIELD_GOAL, goal)
                .put(FIELD_KEY_FACTS, keyFacts.toJsonArray())
                .put(FIELD_DECISIONS, decisions.toJsonArray())
                .put(FIELD_CONSTRAINTS, constraints.toJsonArray())
                .put(FIELD_UNRESOLVED_ITEMS, unresolvedItems.toJsonArray())
        val bytes = json.toString().toByteArray(Charsets.UTF_8)
        return if (bytes.size > MAX_SERIALIZED_BYTES) {
            ConversationContextCapsuleEncodingResult(
                bytes = null,
                error = ConversationContextCapsuleError.TOO_LARGE,
                byteCount = bytes.size,
            )
        } else {
            ConversationContextCapsuleEncodingResult(
                bytes = bytes,
                error = null,
                byteCount = bytes.size,
            )
        }
    }

    companion object {
        const val CURRENT_SCHEMA_VERSION = 1
        const val MAX_SERIALIZED_BYTES = 4 * 1024

        private const val FIELD_SCHEMA_VERSION = "schemaVersion"
        private const val FIELD_GOAL = "goal"
        private const val FIELD_KEY_FACTS = "keyFacts"
        private const val FIELD_DECISIONS = "decisions"
        private const val FIELD_CONSTRAINTS = "constraints"
        private const val FIELD_UNRESOLVED_ITEMS = "unresolvedItems"

        private val REQUIRED_FIELDS =
            setOf(
                FIELD_SCHEMA_VERSION,
                FIELD_GOAL,
                FIELD_KEY_FACTS,
                FIELD_DECISIONS,
                FIELD_CONSTRAINTS,
                FIELD_UNRESOLVED_ITEMS,
            )

        /** Decodes only the exact v1 schema; malformed, unknown, or over-limit input is rejected. */
        fun decode(serialized: ByteArray): ConversationContextCapsuleDecodingResult {
            if (serialized.size > MAX_SERIALIZED_BYTES) {
                return ConversationContextCapsuleDecodingResult(
                    capsule = null,
                    error = ConversationContextCapsuleError.TOO_LARGE,
                    byteCount = serialized.size,
                )
            }

            val jsonText =
                try {
                    Charsets.UTF_8
                        .newDecoder()
                        .onMalformedInput(CodingErrorAction.REPORT)
                        .onUnmappableCharacter(CodingErrorAction.REPORT)
                        .decode(ByteBuffer.wrap(serialized))
                        .toString()
                } catch (exception: CharacterCodingException) {
                    return ConversationContextCapsuleDecodingResult(
                        capsule = null,
                        error = ConversationContextCapsuleError.INVALID_UTF8,
                        byteCount = serialized.size,
                    )
                }

            val json =
                try {
                    JSONObject(jsonText)
                } catch (exception: JSONException) {
                    return ConversationContextCapsuleDecodingResult(
                        capsule = null,
                        error = ConversationContextCapsuleError.MALFORMED_JSON,
                        byteCount = serialized.size,
                    )
                }

            val keys = json.keys().asSequence().toSet()
            if (keys.any { it !in REQUIRED_FIELDS }) {
                return ConversationContextCapsuleDecodingResult(
                    capsule = null,
                    error = ConversationContextCapsuleError.UNKNOWN_FIELD,
                    byteCount = serialized.size,
                )
            }
            if (keys != REQUIRED_FIELDS) {
                return ConversationContextCapsuleDecodingResult(
                    capsule = null,
                    error = ConversationContextCapsuleError.MISSING_FIELD,
                    byteCount = serialized.size,
                )
            }

            val versionNumber =
                json.opt(FIELD_SCHEMA_VERSION) as? Number ?: return invalidFieldType(serialized.size)
            val version = versionNumber.toInt()
            if (versionNumber.toDouble() != version.toDouble()) {
                return invalidFieldType(serialized.size)
            }
            if (version != CURRENT_SCHEMA_VERSION) {
                return ConversationContextCapsuleDecodingResult(
                    capsule = null,
                    error = ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION,
                    byteCount = serialized.size,
                )
            }

            val goal = json.opt(FIELD_GOAL) as? String ?: return invalidFieldType(serialized.size)
            val keyFacts = json.readStringArray(FIELD_KEY_FACTS) ?: return invalidFieldType(serialized.size)
            val decisions = json.readStringArray(FIELD_DECISIONS) ?: return invalidFieldType(serialized.size)
            val constraints = json.readStringArray(FIELD_CONSTRAINTS) ?: return invalidFieldType(serialized.size)
            val unresolvedItems = json.readStringArray(FIELD_UNRESOLVED_ITEMS) ?: return invalidFieldType(serialized.size)

            val capsule =
                ConversationContextCapsule(
                    schemaVersion = version,
                    goal = goal,
                    keyFacts = keyFacts,
                    decisions = decisions,
                    constraints = constraints,
                    unresolvedItems = unresolvedItems,
                )
            val validation = capsule.encode()
            if (!validation.isValid) {
                return ConversationContextCapsuleDecodingResult(
                    capsule = null,
                    error = validation.error,
                    byteCount = serialized.size,
                )
            }
            return ConversationContextCapsuleDecodingResult(
                capsule = capsule,
                error = null,
                byteCount = serialized.size,
            )
        }

        private fun invalidFieldType(byteCount: Int) =
            ConversationContextCapsuleDecodingResult(
                capsule = null,
                error = ConversationContextCapsuleError.INVALID_FIELD_TYPE,
                byteCount = byteCount,
            )

        private fun List<String>.toJsonArray(): JSONArray =
            JSONArray().apply {
                forEach { put(it) }
            }

        private fun JSONObject.readStringArray(name: String): List<String>? {
            val array = opt(name) as? JSONArray ?: return null
            val result = ArrayList<String>(array.length())
            for (index in 0 until array.length()) {
                val value = array.opt(index) as? String ?: return null
                result.add(value)
            }
            return result
        }
    }
}

enum class ConversationContextCapsuleError {
    UNSUPPORTED_SCHEMA_VERSION,
    EMPTY_GOAL,
    BLANK_ITEM,
    TOO_LARGE,
    MALFORMED_JSON,
    INVALID_UTF8,
    MISSING_FIELD,
    UNKNOWN_FIELD,
    INVALID_FIELD_TYPE,
}

data class ConversationContextCapsuleEncodingResult internal constructor(
    val bytes: ByteArray?,
    val error: ConversationContextCapsuleError?,
    val byteCount: Int?,
) {
    val isValid: Boolean
        get() = bytes != null && error == null
}

data class ConversationContextCapsuleDecodingResult internal constructor(
    val capsule: ConversationContextCapsule?,
    val error: ConversationContextCapsuleError?,
    val byteCount: Int,
) {
    val isValid: Boolean
        get() = capsule != null && error == null
}
