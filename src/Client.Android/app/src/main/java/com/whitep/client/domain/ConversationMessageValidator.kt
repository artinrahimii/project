package com.whitep.client.domain

enum class ConversationMessageValidationError {
    EMPTY,
    TOO_LONG,
}

data class ConversationMessageValidationResult internal constructor(
    val normalizedText: String?,
    val utf8ByteCount: Int,
    val error: ConversationMessageValidationError?,
) {
    val isValid: Boolean
        get() = normalizedText != null && error == null
}

/**
 * Enforces the client-side prompt rule defined by protocol/whitep/v1/messages.proto.
 * Length is measured after trimming in UTF-8 bytes, not Kotlin UTF-16 code units.
 */
object ConversationMessageValidator {
    const val MAX_MESSAGE_UTF8_BYTES = 8_192

    fun validate(content: String): ConversationMessageValidationResult {
        val normalizedText = content.trim()
        val byteCount = normalizedText.toByteArray(Charsets.UTF_8).size

        return when {
            normalizedText.isEmpty() ->
                ConversationMessageValidationResult(
                    normalizedText = null,
                    utf8ByteCount = byteCount,
                    error = ConversationMessageValidationError.EMPTY,
                )
            byteCount > MAX_MESSAGE_UTF8_BYTES ->
                ConversationMessageValidationResult(
                    normalizedText = null,
                    utf8ByteCount = byteCount,
                    error = ConversationMessageValidationError.TOO_LONG,
                )
            else ->
                ConversationMessageValidationResult(
                    normalizedText = normalizedText,
                    utf8ByteCount = byteCount,
                    error = null,
                )
        }
    }
}
