package com.whitep.client.ui.navigation

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
    REQUEST("request", "New request", "＋"),
    CONNECTIONS("connections", "Connections", "↔"),
    PRIVACY("privacy", "Privacy", "i"),
}
