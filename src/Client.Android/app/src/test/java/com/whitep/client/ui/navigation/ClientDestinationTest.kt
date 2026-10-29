package com.whitep.client.ui.navigation

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class ClientDestinationTest {
    @Test
    fun everyDestinationHasAUniqueStableRoute() {
        val destinations = ClientDestination.values()

        assertEquals(destinations.size, destinations.map { it.route }.toSet().size)
        assertEquals(ClientDestination.HOME, ClientNavigation.startDestination)
    }

    @Test
    fun bottomNavigationExposesOnlyTopLevelDestinations() {
        assertEquals(
            listOf(
                ClientDestination.HOME,
                ClientDestination.CONVERSATIONS,
                ClientDestination.CONNECTIONS,
                ClientDestination.PRIVACY,
            ),
            ClientNavigation.bottomNavigation,
        )
        assertTrue(ClientNavigation.bottomNavigation.contains(ClientDestination.HOME))
        assertFalse(ClientNavigation.bottomNavigation.contains(ClientDestination.REQUEST))
        assertFalse(ClientNavigation.bottomNavigation.contains(ClientDestination.CONVERSATION_DETAIL))
    }

    @Test
    fun conversationDetailRouteCanCarryAnEncodedConversationId() {
        assertEquals(
            "conversation/conversation-123",
            ClientDestination.conversationRoute("conversation-123"),
        )
        assertEquals(
            "conversation/one%20two",
            ClientDestination.conversationRoute("one two"),
        )
        assertEquals("conversation/{conversationId}", ClientDestination.CONVERSATION_DETAIL.route)
    }
}
