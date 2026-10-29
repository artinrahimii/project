package com.whitep.client.ui.conversations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity

private sealed class ConversationDetailUiState {
    object Loading : ConversationDetailUiState()

    data class Found(val conversation: ConversationEntity) : ConversationDetailUiState()

    object Missing : ConversationDetailUiState()
}

@Composable
internal fun conversationDetailScreen(conversationId: String, onBack: () -> Unit) {
    val applicationContext = LocalContext.current.applicationContext
    val conversationDao =
        remember(applicationContext) {
            ClientDatabase.getInstance(applicationContext).conversationDao()
        }
    val factory = remember(conversationDao) { ConversationListViewModel.Factory(conversationDao) }
    val conversationListViewModel: ConversationListViewModel = viewModel(factory = factory)
    var state by remember(conversationId) {
        mutableStateOf<ConversationDetailUiState>(ConversationDetailUiState.Loading)
    }

    LaunchedEffect(conversationId, conversationListViewModel) {
        state =
            conversationListViewModel.getConversation(conversationId)?.let {
                ConversationDetailUiState.Found(it)
            } ?: ConversationDetailUiState.Missing
    }

    Column(
        modifier =
            Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(24.dp),
        verticalArrangement = Arrangement.spacedBy(16.dp),
    ) {
        Text("Conversation", style = MaterialTheme.typography.headlineMedium)
        OutlinedButton(onClick = onBack, modifier = Modifier.fillMaxWidth()) {
            Text("Back to conversations")
        }

        when (val currentState = state) {
            ConversationDetailUiState.Loading -> {
                Text("Loading conversation…")
            }
            is ConversationDetailUiState.Found -> {
                Card(modifier = Modifier.fillMaxWidth()) {
                    Column(
                        modifier = Modifier.padding(20.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp),
                    ) {
                        Text(
                            currentState.conversation.title,
                            style = MaterialTheme.typography.titleLarge,
                        )
                        Text(
                            "Created ${formatConversationTimestamp(currentState.conversation.createdAtEpochMillis)}",
                            style = MaterialTheme.typography.bodySmall,
                        )
                        Text(
                            "Updated ${formatConversationTimestamp(currentState.conversation.updatedAtEpochMillis)}",
                            style = MaterialTheme.typography.bodySmall,
                        )
                        Text(
                            "Messages and sending are not available in this screen yet.",
                            style = MaterialTheme.typography.bodyMedium,
                        )
                    }
                }
            }
            ConversationDetailUiState.Missing -> {
                Text("This conversation could not be found on this device.")
            }
        }
    }
}
