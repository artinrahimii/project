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
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
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
import com.whitep.client.ui.request.RequestRunStatus
import com.whitep.client.ui.request.RequestViewModel
import com.whitep.client.ui.request.hostResponseCards

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
                        val route = ClientDestination.conversationRoute(conversationId)
                        navController.navigate(route)
                    },
                )
            }
            composable(
                route = ClientDestination.CONVERSATION_DETAIL.route,
                arguments =
                    listOf(
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
            "Direct peer requests",
            style = MaterialTheme.typography.bodyMedium,
        )
        Text(
            "Configure an HTTPS peer directory and explicitly trust each processing host before sending.",
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
                    Text("New request")
                }
            }
        }

        Card(modifier = Modifier.fillMaxWidth()) {
            Column(
                modifier = Modifier.padding(20.dp),
                verticalArrangement = Arrangement.spacedBy(8.dp),
            ) {
                Text("Connection status", style = MaterialTheme.typography.titleMedium)
                Text("No peer has been selected for a request yet.")
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
    val context = LocalContext.current
    var rendezvousUrl by rememberSaveable { mutableStateOf("") }
    var modelId by rememberSaveable { mutableStateOf("") }
    var maxOutputTokens by rememberSaveable { mutableStateOf("256") }
    var pendingTrustPeerId by rememberSaveable { mutableStateOf<String?>(null) }

    appPage(
        title = "New request",
        subtitle = "Configure your HTTPS peer directory, explicitly trust a host, and send an inference request.",
    ) {
        OutlinedTextField(
            value = rendezvousUrl,
            onValueChange = {
                rendezvousUrl = it
                requestViewModel.clearDirectorySession()
            },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("HTTPS Rendezvous URL") },
            supportingText = {
                Text("Use the HTTPS address configured by the service operator. No server address is prefilled.")
            },
            singleLine = true,
        )
        Button(
            onClick = { requestViewModel.discoverPeers(context.applicationContext, rendezvousUrl) },
            enabled = !requestState.isDiscoveringPeers && rendezvousUrl.isNotBlank(),
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text(if (requestState.isDiscoveringPeers) "Discovering peers…" else "Discover peers")
        }
        requestState.discoveredPeers.forEach { peer ->
            Card(modifier = Modifier.fillMaxWidth()) {
                Column(
                    modifier = Modifier.padding(16.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                ) {
                    Text("Peer ID", style = MaterialTheme.typography.labelLarge)
                    Text(peer.peerId, style = MaterialTheme.typography.bodySmall)
                    Text("Lease expires: ${peer.expiresAtUtc}", style = MaterialTheme.typography.bodySmall)
                    Text(if (peer.isTrusted) "Explicitly trusted" else "Not trusted")
                    if (peer.isTrusted) {
                        OutlinedButton(
                            onClick = { requestViewModel.revokePeerTrust(peer.peerId) },
                            modifier = Modifier.fillMaxWidth(),
                        ) {
                            Text("Revoke trust")
                        }
                    } else {
                        Button(
                            onClick = { pendingTrustPeerId = peer.peerId },
                            modifier = Modifier.fillMaxWidth(),
                        ) {
                            Text("Review and trust this host")
                        }
                    }
                }
            }
        }

        OutlinedTextField(
            value = requestState.prompt,
            onValueChange = requestViewModel::updatePrompt,
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Prompt") },
            supportingText = { Text("The selected host can read your prompt and context capsule after transport decryption; encryption does not hide them from that host.") },
            minLines = 4,
            maxLines = 8,
        )
        OutlinedTextField(
            value = modelId,
            onValueChange = { modelId = it },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Exact model ID") },
            supportingText = { Text("Enter a model ID advertised by the host; model names are not guessed.") },
            singleLine = true,
        )
        OutlinedTextField(
            value = maxOutputTokens,
            onValueChange = { maxOutputTokens = it.filter(Char::isDigit).take(6) },
            modifier = Modifier.fillMaxWidth(),
            label = { Text("Maximum output tokens") },
            singleLine = true,
        )
        Button(
            onClick = {
                requestViewModel.sendRequest(
                    prompt = requestState.prompt,
                    modelId = modelId,
                    maxOutputTokens = maxOutputTokens.toIntOrNull() ?: 0,
                )
            },
            enabled = !requestState.isSendingRequest,
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text(if (requestState.isSendingRequest) "Sending and waiting for hosts…" else "Send to trusted hosts")
        }
        if (requestState.canCancelRequest || requestState.isCancellingRequest) {
            OutlinedButton(
                onClick = requestViewModel::cancelCurrentRequest,
                enabled = requestState.canCancelRequest && !requestState.isCancellingRequest,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    if (requestState.isCancellingRequest) {
                        "Sending cancellation…"
                    } else {
                        "Cancel request on selected hosts"
                    },
                )
            }
        }
        Text(
            "Request status: " +
                requestState.runStatus.name
                    .lowercase()
                    .replace('_', ' '),
            color =
                if (requestState.runStatus == RequestRunStatus.FAILED) {
                    MaterialTheme.colorScheme.error
                } else {
                    MaterialTheme.colorScheme.onSurface
                },
            style = MaterialTheme.typography.titleMedium,
        )
        if (requestState.canRetryRequest || requestState.isRetryingRequest) {
            OutlinedButton(
                onClick = requestViewModel::retryFailedAttempts,
                enabled =
                    requestState.canRetryRequest &&
                        !requestState.isSendingRequest &&
                        !requestState.isCancellingRequest,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    if (requestState.isRetryingRequest) {
                        "Retrying failed attempts…"
                    } else {
                        "Retry failed attempts manually"
                    },
                )
            }
        }
        requestState.networkStatusMessage?.let { message ->
            Text(message, style = MaterialTheme.typography.bodyMedium)
        }
        requestState.hostAvailabilityMessage?.let { message ->
            Text(message, style = MaterialTheme.typography.bodyMedium)
        }
        hostResponseCards(responses = requestState.hostResponses)
        OutlinedButton(onClick = onBack, modifier = Modifier.fillMaxWidth()) {
            Text("Back")
        }
    }

    pendingTrustPeerId?.let { peerId ->
        AlertDialog(
            onDismissRequest = { pendingTrustPeerId = null },
            title = { Text("Before you trust this host") },
            text = {
                Text(
                    "Peer ID: $peerId\n\n" +
                        "When you send a request, this processing host receives your prompt and context capsule " +
                        "and can read them after transport decryption.\n\n" +
                        "Transport encryption protects data while it travels; it does not hide request content " +
                        "from the selected host or its operator. The host may inspect or retain what it receives " +
                        "outside this app's control.\n\n" +
                        "Saving trust does not send a prompt or context now. It only makes this peer eligible " +
                        "for a request. Your request is sent only when you explicitly press Send to trusted hosts. " +
                        "You can revoke trust later.",
                )
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        requestViewModel.trustPeerAfterExplicitConfirmation(peerId)
                        pendingTrustPeerId = null
                    },
                ) {
                    Text("Trust this host")
                }
            },
            dismissButton = {
                TextButton(onClick = { pendingTrustPeerId = null }) {
                    Text("Cancel")
                }
            },
        )
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
                Text("Peer discovery is configured from the New request screen.")
                Text("Only the configured directory is queried; peer trust requires an explicit confirmation.")
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
                        "Only explicitly trusted peers are eligible for inference. " +
                            "The receiving host can read the prompt and context after transport decryption.",
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
