package com.whitep.client.ui.navigation

/**
 * Defines the initial route and the destinations shown in the app's bottom navigation.
 *
 * The request editor is intentionally reachable from Home, not shown as a top-level tab.
 * These routes do not imply that peer discovery or request transport has been implemented.
 */
internal object ClientNavigation {
    val startDestination: ClientDestination = ClientDestination.HOME

    val bottomNavigation: List<ClientDestination> =
        listOf(
            ClientDestination.HOME,
            ClientDestination.CONNECTIONS,
            ClientDestination.PRIVACY,
        )
}
