package com.whitep.client

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.whitep.client.ui.conversations.conversationDetailScreen
import com.whitep.client.ui.conversations.conversationListScreen
import com.whitep.client.ui.navigation.ClientDestination
import com.whitep.client.ui.navigation.ClientNavigation
import com.whitep.client.ui.request.RequestViewModel

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                whitePClientApp()
            }
        }
    }
}

@Composable
private fun whitePClientApp() {
    val navController = rememberNavController()
    val currentEntry by navController.currentBackStackEntryAsState()
    val currentDestination = currentEntry?.destination

    Scaffold(
        bottomBar = {
            NavigationBar {
                ClientNavigation.bottomNavigation.forEach { item ->
                    val selected =
                        currentDestination?.hierarchy?.any { it.route == item.route } == true
                    NavigationBarItem(
                        selected = selected,
                        onClick = {
                            navController.navigate(item.route) {
                                popUpTo(navController.graph.findStartDestination().id) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        },
                        icon = { Text(item.glyph) },
                        label = { Text(item.label) },
                    )
                }
            }
        },
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = ClientNavigation.startDestination.route,
            modifier = Modifier.padding(innerPadding),
        ) {
            composable(ClientDestination.HOME.route) {
                homeScreen(
                    onOpenRequest = { navController.navigate(ClientDestination.REQUEST.route) },
                    onOpenConnections = {
                        navController.navigate(ClientDestination.CONNECTIONS.route)
                    },
                    onOpenPrivacy = { navController.navigate(ClientDestination.PRIVACY.route) },
                )
            }
            composable(ClientDestination.CONVERSATIONS.route) {
                conversationListScreen(
                    onOpenConversation = { conversationId ->
                        navController.navigate(
                            ClientDestination.conversationRoute(conversationId),
                        )
                    },
                )
            }
            composable(
                route = ClientDestination.CONVERSATION_DETAIL.route,
                arguments = listOf(
                    navArgument(ClientDestination.CONVERSATION_ID_ARGUMENT) {
                        type = NavType.StringType
                    },
                ),
            ) { backStackEntry ->
                val conversationId =
                    requireNotNull(
                        backStackEntry.arguments?.getString(
                            ClientDestination.CONVERSATION_ID_ARGUMENT,
                        ),
                    )
                conversationDetailScreen(
                    conversationId = conversationId,
                    onBack = { navController.popBackStack() },
                )
            }
            composable(ClientDestination.REQUEST.route) {
                requestScreen(onBack = { navController.popBackStack() })
            }
            composable(ClientDestination.CONNECTIONS.route) {
                connectionsScreen()
            }
            composable(ClientDestination.PRIVACY.route) {
                privacyScreen()
            }
        }
    }
}

@Composable
private fun homeScreen(
    onOpenRequest: () -> Unit,
    onOpenConnections: () -> Unit,
    onOpenPrivacy: () -> Unit,
) {
    appPage(
        title = "White-P",
        subtitle = "A focused workspace for preparing local AI requests.",
    ) {
        Text(
            "P2P client preview (transport not implemented)",
            style = MaterialTheme.typography.bodyMedium,
        )
        Text(
            "Requests are not sent yet; transport is not implemented.",
            style = MaterialTheme.typography.bodySmall,
        )
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                Text(
                    text = "Start with a request",
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.SemiBold,
                )
                Text(
                    text = "Draft a prompt and review the privacy boundary before choosing a host.",
                    style = MaterialTheme.typography.bodyMedium,
                )
                Button(
                    onClick = onOpenRequest,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text("Prepare request")
                }
            }
        }

        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
            ) {
                Text("Connection status", style = MaterialTheme.typography.titleMedium)
                Text("Not connected. Android-to-host transport is not available yet.")
                OutlinedButton(
                    onClick = onOpenConnections,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text("View connections")
                }
            }
        }

        OutlinedButton(
            onClick = onOpenPrivacy,
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text("Read privacy information")
        }
    }
}

@Composable
private fun requestScreen(
    onBack: () -> Unit,
    requestViewModel: RequestViewModel = viewModel(),
) {
    val requestState by requestViewModel.uiState.collectAsState()

    appPage(
        title = "New request",
        subtitle = "Prepare text only. This preview does not send it to a host.",
    ) {
        OutlinedTextField(
            value = requestState.prompt,
            onValueChange = requestViewModel::updatePrompt,
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Prompt") },
            supportingText = { Text("Text remains in this screen's in-memory state.") },
            minLines = 4,
            maxLines = 8,
        )
        Button(
            onClick = requestViewModel::prepareRequest,
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text("Prepare request")
        }
        Text("Status: ${requestState.statusMessage}", style = MaterialTheme.typography.bodyMedium)
        OutlinedButton(onClick = onBack, modifier = Modifier.fillMaxWidth()) {
            Text("Back")
        }
    }
}

@Composable
private fun connectionsScreen() {
    appPage(
        title = "Connections",
        subtitle = "Not connected",
    ) {
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
            ) {
                Text("Peer discovery is not available in this client preview.")
                Text("No connection attempt or network request is made from this screen.")
            }
        }
    }
}

@Composable
private fun privacyScreen() {
    appPage(
        title = "Privacy",
        subtitle = "Understand the processing-host trust boundary.",
    ) {
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                Text(
                    text = "The processing host can read your prompt and context capsule after channel decryption.",
                    style = MaterialTheme.typography.bodyLarge,
                )
                Text("Transport encryption protects data in transit, not from the receiving host.")
                Text(
                    text = "Only share content you are willing to disclose to the host you choose.",
                )
                Text(
                    text =
                        "This preview does not send requests yet. " +
                            "Android-to-Windows delivery and no-storage behavior have not been verified.",
                    style = MaterialTheme.typography.bodyMedium,
                )
            }
        }
    }
}

@Composable
private fun appPage(
    title: String,
    subtitle: String,
    content: @Composable ColumnScope.() -> Unit,
) {
    Column(
        modifier =
            Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
        content = {
            Text(title, style = MaterialTheme.typography.headlineMedium)
            Text(subtitle, style = MaterialTheme.typography.bodyLarge)
            content()
        },
    )
}
