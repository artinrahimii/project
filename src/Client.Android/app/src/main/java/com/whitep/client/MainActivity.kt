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
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.whitep.client.ui.navigation.ClientDestination

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                WhitePClientApp()
            }
        }
    }
}

@Composable
private fun WhitePClientApp() {
    val navController = rememberNavController()
    val currentEntry by navController.currentBackStackEntryAsState()
    val currentDestination = currentEntry?.destination

    Scaffold(
        bottomBar = {
            NavigationBar {
                ClientDestination.bottomNavigation.forEach { item ->
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
            startDestination = ClientDestination.startDestination.route,
            modifier = Modifier.padding(innerPadding),
        ) {
            composable(ClientDestination.HOME.route) {
                HomeScreen(
                    onOpenRequest = { navController.navigate(ClientDestination.REQUEST.route) },
                    onOpenConnections = {
                        navController.navigate(ClientDestination.CONNECTIONS.route)
                    },
                    onOpenPrivacy = { navController.navigate(ClientDestination.PRIVACY.route) },
                )
            }
            composable(ClientDestination.REQUEST.route) {
                RequestScreen(onBack = { navController.popBackStack() })
            }
            composable(ClientDestination.CONNECTIONS.route) {
                ConnectionsScreen()
            }
            composable(ClientDestination.PRIVACY.route) {
                PrivacyScreen()
            }
        }
    }
}

@Composable
private fun HomeScreen(
    onOpenRequest: () -> Unit,
    onOpenConnections: () -> Unit,
    onOpenPrivacy: () -> Unit,
) {
    AppPage(
        title = "White-P",
        subtitle = "A focused workspace for preparing local AI requests.",
    ) {
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
private fun RequestScreen(onBack: () -> Unit) {
    var requestText by remember { mutableStateOf("") }
    var status by remember { mutableStateOf("Nothing has been sent.") }

    AppPage(
        title = "New request",
        subtitle = "Prepare text only. This preview does not send it to a host.",
    ) {
        OutlinedTextField(
            value = requestText,
            onValueChange = { requestText = it },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Prompt") },
            supportingText = { Text("Text remains in this screen's in-memory state.") },
            minLines = 4,
            maxLines = 8,
        )
        Button(
            onClick = {
                status =
                    if (requestText.isBlank()) {
                        "Enter a prompt first."
                    } else {
                        "Not sent: Android-to-host transport is not implemented."
                    }
            },
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text("Prepare request")
        }
        Text("Status: $status", style = MaterialTheme.typography.bodyMedium)
        OutlinedButton(onClick = onBack, modifier = Modifier.fillMaxWidth()) {
            Text("Back")
        }
    }
}

@Composable
private fun ConnectionsScreen() {
    AppPage(
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
private fun PrivacyScreen() {
    AppPage(
        title = "Privacy",
        subtitle = "Understand the processing-host trust boundary.",
    ) {
        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                Text(
                    text = "The processing host can read your prompt and context capsule " +
                        "after it decrypts the connection.",
                    style = MaterialTheme.typography.bodyLarge,
                )
                Text(
                    text = "Transport encryption protects data in transit. It does not hide " +
                        "the message from the receiving host.",
                )
                Text(
                    text = "Only share content you are willing to disclose to the host you choose.",
                )
                Text(
                    text = "This preview does not send requests yet. Android-to-Windows delivery and no-storage behavior have not been verified.",
                    style = MaterialTheme.typography.bodyMedium,
                )
            }
        }
    }
}

@Composable
private fun AppPage(
    title: String,
    subtitle: String,
    content: @Composable ColumnScope.() -> Unit,
) {
    Column(
        modifier = Modifier
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
