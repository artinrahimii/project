package com.whitep.client.ui.navigation

import android.net.Uri

/**
 * Destinations owned by the Android Compose shell.
 *
 * These routes are navigation only; they do not imply that peer transport has been implemented.
 */
internal enum class ClientDestination(
    val route: String,
    val label: String,
    val glyph: String,
) {
    HOME("home", "Home", "⌂"),
    CONVERSATIONS("conversations", "Conversations", "▤"),
    CONVERSATION_DETAIL("conversation/{conversationId}", "Conversation", "↩"),
    REQUEST("request", "New request", "＋"),
    CONNECTIONS("connections", "Connections", "↔"),
    PRIVACY("privacy", "Privacy", "i"), ;

    companion object {
        const val CONVERSATION_ID_ARGUMENT = "conversationId"

        fun conversationRoute(conversationId: String): String = "conversation/${Uri.encode(conversationId)}"
    }
}
