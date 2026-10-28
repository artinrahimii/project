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
    REQUEST("request", "New request", "＋"),
    CONNECTIONS("connections", "Connections", "↔"),
    PRIVACY("privacy", "Privacy", "i"),
}

internal object ClientNavigation {
    val startDestination: ClientDestination = ClientDestination.HOME
    val bottomNavigation: List<ClientDestination> =
        listOf(ClientDestination.HOME, ClientDestination.CONNECTIONS, ClientDestination.PRIVACY)
}
