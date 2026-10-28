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
        assertEquals(ClientDestination.HOME, ClientDestination.startDestination)
    }

    @Test
    fun bottomNavigationExposesOnlyTopLevelDestinations() {
        assertEquals(
            listOf(
                ClientDestination.HOME,
                ClientDestination.CONNECTIONS,
                ClientDestination.PRIVACY,
            ),
            ClientDestination.bottomNavigation,
        )
        assertTrue(ClientDestination.bottomNavigation.contains(ClientDestination.HOME))
        assertFalse(ClientDestination.bottomNavigation.contains(ClientDestination.REQUEST))
    }
}
