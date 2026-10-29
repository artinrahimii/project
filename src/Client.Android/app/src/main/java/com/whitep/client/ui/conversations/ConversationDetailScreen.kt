package com.whitep.client.ui.conversations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
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
import com.whitep.client.data.MessageEntity
import com.whitep.client.domain.ConversationMessageValidationError
import com.whitep.client.domain.ConversationMessageValidator
import kotlinx.coroutines.launch

private sealed class ConversationDetailUiState {
    object Loading : ConversationDetailUiState()

    data class Found(
        val conversation: ConversationEntity,
    ) : ConversationDetailUiState()

    object Missing : ConversationDetailUiState()
}

@Composable
internal fun conversationDetailScreen(
    conversationId: String,
    onBack: () -> Unit,
) {
    val applicationContext = LocalContext.current.applicationContext
    val database =
        remember(applicationContext) {
            ClientDatabase.getInstance(applicationContext)
        }
    val conversationDao = database.conversationDao()
    val messageDao = database.messageDao()
    val conversationFactory =
        remember(conversationDao) {
            ConversationListViewModel.Factory(conversationDao)
        }
    val conversationListViewModel: ConversationListViewModel = viewModel(factory = conversationFactory)
    val messageFactory =
        remember(conversationId, messageDao) {
            ConversationMessagesViewModel.Factory(conversationId, messageDao)
        }
    val conversationMessagesViewModel: ConversationMessagesViewModel =
        viewModel(
            key = "conversation-messages-$conversationId",
            factory = messageFactory,
        )
    val messages by conversationMessagesViewModel.messages.collectAsState(initial = emptyList())
    val scope = rememberCoroutineScope()
    var draft by remember(conversationId) {
        mutableStateOf("")
    }
    var state by remember(conversationId) {
        mutableStateOf<ConversationDetailUiState>(ConversationDetailUiState.Loading)
    }
    val messageValidation =
        remember(draft) {
            ConversationMessageValidator.validate(draft)
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
                    }
                }

                Text("Messages", style = MaterialTheme.typography.titleLarge)
                if (messages.isEmpty()) {
                    Text("No messages yet. Write a message below to save it in this conversation.")
                }
                messages.forEach { message ->
                    messageCard(message = message)
                }

                OutlinedTextField(
                    value = draft,
                    onValueChange = { draft = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Message") },
                    isError = messageValidation.error == ConversationMessageValidationError.TOO_LONG,
                    supportingText = {
                        if (messageValidation.error == ConversationMessageValidationError.TOO_LONG) {
                            Text("Message exceeds the 8,192 UTF-8 byte limit.")
                        } else {
                            Text(
                                "${messageValidation.utf8ByteCount} / " +
                                    "${ConversationMessageValidator.MAX_MESSAGE_UTF8_BYTES} UTF-8 bytes",
                            )
                        }
                    },
                    minLines = 2,
                    maxLines = 5,
                )
                Button(
                    onClick = {
                        val messageText = draft
                        scope.launch {
                            if (conversationMessagesViewModel.sendMessage(messageText)) {
                                draft = ""
                            }
                        }
                    },
                    enabled = messageValidation.isValid,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text("Send")
                }
                Text(
                    "Messages are saved only on this device. Sending to a processing host is not available yet.",
                    style = MaterialTheme.typography.bodySmall,
                )
            }
            ConversationDetailUiState.Missing -> {
                Text("This conversation could not be found on this device.")
            }
        }
    }
}

@Composable
private fun messageCard(message: MessageEntity) {
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
        ) {
            val author = if (message.authorRole == "user") "You" else message.authorRole
            Text(author, style = MaterialTheme.typography.labelLarge)
            Text(message.content, style = MaterialTheme.typography.bodyLarge)
            Text(
                formatConversationTimestamp(message.createdAtEpochMillis),
                style = MaterialTheme.typography.bodySmall,
            )
        }
    }
}
