package com.whitep.client.ui.conversations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity
import kotlinx.coroutines.launch

@Composable
internal fun conversationListScreen(onOpenConversation: (String) -> Unit) {
    val applicationContext = LocalContext.current.applicationContext
    val conversationDao =
        remember(applicationContext) {
            ClientDatabase.getInstance(applicationContext).conversationDao()
        }
    val factory = remember(conversationDao) { ConversationListViewModel.Factory(conversationDao) }
    val conversationListViewModel: ConversationListViewModel = viewModel(factory = factory)
    val conversations by conversationListViewModel.conversations.collectAsState(initial = emptyList())
    val scope = rememberCoroutineScope()
    var renameTarget by remember { mutableStateOf<ConversationEntity?>(null) }
    var renameTitle by remember { mutableStateOf("") }
    var deleteTarget by remember { mutableStateOf<ConversationEntity?>(null) }

    LazyColumn(
        modifier = Modifier.padding(horizontal = 24.dp, vertical = 20.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        item {
            Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                Text("Conversations", style = MaterialTheme.typography.headlineMedium)
                Text(
                    "Conversation history saved on this device.",
                    style = MaterialTheme.typography.bodyLarge,
                )
                Button(
                    onClick = {
                        scope.launch {
                            val conversation = conversationListViewModel.createConversation()
                            onOpenConversation(conversation.conversationId)
                        }
                    },
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text("New conversation")
                }
            }
        }

        if (conversations.isEmpty()) {
            item {
                Text(
                    "No conversations yet. Create a new conversation to get started.",
                    style = MaterialTheme.typography.bodyMedium,
                )
            }
        } else {
            items(conversations, key = { it.conversationId }) { conversation ->
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp),
                    ) {
                        Text(
                            conversation.title,
                            style = MaterialTheme.typography.titleMedium,
                        )
                        Text(
                            "Updated ${formatConversationTimestamp(conversation.updatedAtEpochMillis)}",
                            style = MaterialTheme.typography.bodySmall,
                        )
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            Button(onClick = { onOpenConversation(conversation.conversationId) }) {
                                Text("Open")
                            }
                            OutlinedButton(
                                onClick = {
                                    renameTarget = conversation
                                    renameTitle = conversation.title
                                },
                            ) {
                                Text("Rename")
                            }
                            OutlinedButton(onClick = { deleteTarget = conversation }) {
                                Text("Delete")
                            }
                        }
                    }
                }
            }
        }
    }

    renameTarget?.let { target ->
        AlertDialog(
            onDismissRequest = { renameTarget = null },
            title = { Text("Rename conversation") },
            text = {
                OutlinedTextField(
                    value = renameTitle,
                    onValueChange = { renameTitle = it },
                    label = { Text("Conversation title") },
                    singleLine = true,
                )
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        val newTitle = renameTitle
                        scope.launch {
                            conversationListViewModel.renameConversation(
                                target.conversationId,
                                newTitle,
                            )
                            renameTarget = null
                        }
                    },
                    enabled = renameTitle.isNotBlank(),
                ) {
                    Text("Save")
                }
            },
            dismissButton = {
                TextButton(onClick = { renameTarget = null }) {
                    Text("Cancel")
                }
            },
        )
    }

    deleteTarget?.let { target ->
        AlertDialog(
            onDismissRequest = { deleteTarget = null },
            title = { Text("Delete conversation?") },
            text = {
                Text(
                    "Delete \"${target.title}\" and its locally stored messages and request states? " +
                        "This cannot be undone.",
                )
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        scope.launch {
                            conversationListViewModel.deleteConversation(target.conversationId)
                            deleteTarget = null
                        }
                    },
                ) {
                    Text("Delete")
                }
            },
            dismissButton = {
                TextButton(onClick = { deleteTarget = null }) {
                    Text("Cancel")
                }
            },
        )
    }
}

internal fun formatConversationTimestamp(epochMillis: Long): String =
    java.time.format.DateTimeFormatter
        .ofLocalizedDateTime(java.time.format.FormatStyle.MEDIUM)
        .withLocale(java.util.Locale.getDefault())
        .withZone(java.time.ZoneId.systemDefault())
        .format(java.time.Instant.ofEpochMilli(epochMillis))
