package com.whitep.client.domain

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test

class ConversationMessageValidatorTest {
    @Test
    fun rejectsEmptyAndWhitespaceOnlyContent() {
        listOf("", "   ", " \n\t ").forEach { input ->
            val result = ConversationMessageValidator.validate(input)

            assertFalse(result.isValid)
            assertEquals(ConversationMessageValidationError.EMPTY, result.error)
            assertNull(result.normalizedText)
        }
    }

    @Test
    fun acceptsExactlyTheUtf8ByteLimit() {
        val input = "🙂".repeat(ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES / 4)
        val result = ConversationMessageValidator.validate(input)

        assertTrue(result.isValid)
        assertEquals(ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES, result.utf8ByteCount)
        assertEquals(input, result.normalizedText)
    }

    @Test
    fun rejectsOneEmojiBeyondTheUtf8ByteLimit() {
        val input = "🙂".repeat(ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES / 4 + 1)
        val result = ConversationMessageValidator.validate(input)

        assertFalse(result.isValid)
        assertEquals(ConversationMessageValidationError.TOO_LONG, result.error)
        assertEquals(ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES + 4, result.utf8ByteCount)
        assertNull(result.normalizedText)
    }

    @Test
    fun trimsBeforeCountingAndReturningPersistedText() {
        val result = ConversationMessageValidator.validate("  hello \n")

        assertTrue(result.isValid)
        assertEquals("hello", result.normalizedText)
        assertEquals(5, result.utf8ByteCount)
    }
}
