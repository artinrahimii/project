package com.whitep.client.ui.conversations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.whitep.client.data.ClientDatabase
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.time.format.FormatStyle
import java.util.Locale

@Composable
internal fun conversationListScreen() {
    val applicationContext = LocalContext.current.applicationContext
    val conversationDao =
        remember(applicationContext) {
            ClientDatabase.getInstance(applicationContext).conversationDao()
        }
    val factory = remember(conversationDao) { ConversationListViewModel.Factory(conversationDao) }
    val conversationListViewModel: ConversationListViewModel = viewModel(factory = factory)
    val conversations by conversationListViewModel.conversations.collectAsState(initial = emptyList())

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
            }
        }

        if (conversations.isEmpty()) {
            item {
                Text(
                    "No conversations yet. New conversations will appear here after they are created.",
                    style = MaterialTheme.typography.bodyMedium,
                )
            }
        } else {
            items(conversations, key = { it.conversationId }) { conversation ->
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(4.dp),
                    ) {
                        Text(
                            conversation.title,
                            style = MaterialTheme.typography.titleMedium,
                        )
                        Text(
                            "Updated ${formatConversationTimestamp(conversation.updatedAtEpochMillis)}",
                            style = MaterialTheme.typography.bodySmall,
                        )
                    }
                }
            }
        }
    }
}

private fun formatConversationTimestamp(epochMillis: Long): String =
    DateTimeFormatter
        .ofLocalizedDateTime(FormatStyle.MEDIUM)
        .withLocale(Locale.getDefault())
        .withZone(ZoneId.systemDefault())
        .format(Instant.ofEpochMilli(epochMillis))
