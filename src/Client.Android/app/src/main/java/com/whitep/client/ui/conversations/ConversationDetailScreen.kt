package com.whitep.client.ui.conversations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Slider
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.data.ClientDatabase
import com.whitep.client.data.ConversationEntity
import com.whitep.client.data.MessageEntity
import com.whitep.client.data.RequestStateEntity
import com.whitep.client.data.RoomConversationContextCapsuleRepository
import com.whitep.client.data.RoomInferenceRequestLifecycleRepository
import com.whitep.client.data.SharedPreferencesConversationContextCapsuleSizeLimitStore
import com.whitep.client.domain.ConversationMessageValidationError
import com.whitep.client.domain.ConversationMessageValidator
import com.whitep.client.errors.RequestErrorCode
import com.whitep.client.ui.request.HostResponseOutcome
import com.whitep.client.ui.request.RequestRunStatus
import com.whitep.client.ui.request.RequestViewModel
import com.whitep.client.ui.request.RequestViewModelFactory
import com.whitep.client.ui.request.toPresentation
import kotlinx.coroutines.launch
import kotlin.math.roundToInt

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
    rendezvousServiceUrl: String,
    privacyConsentAccepted: Boolean,
    onPrivacyConsentAccepted: () -> Unit,
    onConfigureConnection: () -> Unit,
    onBack: () -> Unit,
) {
    val applicationContext = LocalContext.current.applicationContext
    val database =
        remember(applicationContext) {
            ClientDatabase.getInstance(applicationContext)
        }
    val conversationDao = database.conversationDao()
    val messageDao = database.messageDao()
    val requestStateDao = database.requestStateDao()
    val requestLifecycleRepository =
        remember(database) {
            RoomInferenceRequestLifecycleRepository(database.inferenceRequestStateDao())
        }
    val requestViewModelFactory =
        remember(requestLifecycleRepository) {
            RequestViewModelFactory(requestLifecycleRepository)
        }
    val requestViewModel: RequestViewModel =
        viewModel(
            key = "conversation-request-$conversationId",
            factory = requestViewModelFactory,
        )
    val contextCapsuleRepository =
        remember(database) {
            RoomConversationContextCapsuleRepository(database.conversationContextCapsuleDao())
        }
    val contextCapsuleSizeLimitStore =
        remember(applicationContext) {
            SharedPreferencesConversationContextCapsuleSizeLimitStore(applicationContext)
        }
    val contextCapsuleFactory =
        remember(conversationId, contextCapsuleRepository, contextCapsuleSizeLimitStore, messageDao) {
            ConversationContextCapsuleViewModel.Factory(
                conversationId,
                contextCapsuleRepository,
                contextCapsuleSizeLimitStore,
                messageDao,
            )
        }
    val contextCapsuleViewModel: ConversationContextCapsuleViewModel =
        viewModel(
            key = "conversation-context-capsule-$conversationId",
            factory = contextCapsuleFactory,
        )
    val contextCapsuleState by contextCapsuleViewModel.uiState.collectAsState()
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
    val draft by conversationMessagesViewModel.draft.collectAsState()
    val requestStates by requestStateDao.observeForConversation(conversationId).collectAsState(initial = emptyList())
    val liveRequestState by requestViewModel.uiState.collectAsState()
    val hostResponses by requestViewModel.hostResponses.collectAsState()
    val scope = rememberCoroutineScope()
    var pendingTrustPeerId by rememberSaveable { mutableStateOf<String?>(null) }
    var pendingPrivacyPrompt by rememberSaveable { mutableStateOf<String?>(null) }
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

    LaunchedEffect(rendezvousServiceUrl, requestViewModel) {
        if (rendezvousServiceUrl.isNotBlank()) {
            requestViewModel.discoverPeers(applicationContext, rendezvousServiceUrl)
        }
    }

    LaunchedEffect(hostResponses, conversationMessagesViewModel) {
        hostResponses.forEach { response ->
            when (response.outcome) {
                HostResponseOutcome.SUCCEEDED -> {
                    conversationMessagesViewModel.saveAssistantResponse(
                        requestId = response.requestId,
                        attemptId = response.attemptId,
                        content = response.outputText,
                    )
                }
                else -> {
                    if (response.errorMessage.isNotBlank()) {
                        conversationMessagesViewModel.saveRequestNotice(
                            requestId = response.requestId,
                            attemptId = response.attemptId,
                            content = response.errorMessage,
                        )
                    }
                }
            }
        }
    }

    fun submitMessageToHost(messageText: String) {
        scope.launch {
            if (conversationMessagesViewModel.sendMessage(messageText)) {
                conversationMessagesViewModel.updateDraft("")
                requestViewModel.sendRequest(messageText)
            }
        }
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

                contextCapsuleSection(
                    state = contextCapsuleState,
                    onBeginEditing = contextCapsuleViewModel::beginEditing,
                    onCancelEditing = contextCapsuleViewModel::cancelEditing,
                    onMaxSerializedBytesChange = contextCapsuleViewModel::updateMaxSerializedBytes,
                    onGoalChange = contextCapsuleViewModel::updateGoal,
                    onKeyFactsChange = contextCapsuleViewModel::updateKeyFacts,
                    onDecisionsChange = contextCapsuleViewModel::updateDecisions,
                    onConstraintsChange = contextCapsuleViewModel::updateConstraints,
                    onUnresolvedItemsChange = contextCapsuleViewModel::updateUnresolvedItems,
                    onSave = { scope.launch { contextCapsuleViewModel.save() } },
                    onDelete = { scope.launch { contextCapsuleViewModel.delete() } },
                    onUpdateFromHistory = {
                        scope.launch { contextCapsuleViewModel.updateFromLocalHistory() }
                    },
                    onResolveConflict = contextCapsuleViewModel::resolveConflict,
                )

                Text("Processing host", style = MaterialTheme.typography.titleLarge)
                if (rendezvousServiceUrl.isBlank()) {
                    Text("Connect a trusted processing host before sending a message.")
                    OutlinedButton(
                        onClick = onConfigureConnection,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text("Configure host directory")
                    }
                } else if (liveRequestState.isDiscoveringPeers) {
                    Text("Finding available processing hosts…")
                } else if (liveRequestState.discoveredPeers.isEmpty()) {
                    Text("No available processing host was found.")
                    OutlinedButton(
                        onClick = {
                            requestViewModel.discoverPeers(applicationContext, rendezvousServiceUrl)
                        },
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text("Search again")
                    }
                } else {
                    val trustedHostCount = liveRequestState.discoveredPeers.count { it.isTrusted }
                    Text(
                        if (trustedHostCount == 0) {
                            "Review and trust a host before sending. No host is trusted automatically."
                        } else {
                            "$trustedHostCount trusted processing host(s) are available."
                        },
                        style = MaterialTheme.typography.bodySmall,
                    )
                    liveRequestState.discoveredPeers.forEachIndexed { index, peer ->
                        Card(modifier = Modifier.fillMaxWidth()) {
                            Column(
                                modifier = Modifier.padding(12.dp),
                                verticalArrangement = Arrangement.spacedBy(6.dp),
                            ) {
                                Text("Processing host ${index + 1}", style = MaterialTheme.typography.titleMedium)
                                Text(if (peer.isTrusted) "Trusted" else "Approval required")
                                if (!peer.isTrusted) {
                                    OutlinedButton(
                                        onClick = { pendingTrustPeerId = peer.peerId },
                                        modifier = Modifier.fillMaxWidth(),
                                    ) {
                                        Text("Review host and trust")
                                    }
                                }
                            }
                        }
                    }
                }
                if (liveRequestState.isSelectingModel) {
                    Text("Checking trusted-host compatibility and verified model readiness…")
                }
                if (liveRequestState.isSendingRequest) {
                    Text("Secure connection and model processing are in progress…")
                }
                if (liveRequestState.isCancellingRequest) {
                    Text("Cancellation is being sent to the selected host…")
                }
                liveRequestState.networkStatusMessage?.let { message ->
                    Text(message, style = MaterialTheme.typography.bodySmall)
                }
                liveRequestState.hostAvailabilityMessage?.let { message ->
                    Text(message, color = MaterialTheme.colorScheme.error)
                }
                Text(
                    text =
                        when (liveRequestState.runStatus) {
                            RequestRunStatus.IDLE -> "No request sent yet."
                            RequestRunStatus.RUNNING -> "Request is running."
                            RequestRunStatus.SUCCEEDED -> "Request completed."
                            RequestRunStatus.PARTIAL -> "Request returned a partial result."
                            RequestRunStatus.FAILED -> "The request failed."
                            RequestRunStatus.CANCELLED -> "The request was cancelled."
                            RequestRunStatus.EXPIRED -> "The request expired before a result was saved."
                        },
                    color =
                        if (liveRequestState.runStatus == RequestRunStatus.FAILED) {
                            MaterialTheme.colorScheme.error
                        } else {
                            MaterialTheme.colorScheme.onSurface
                        },
                    style = MaterialTheme.typography.bodySmall,
                )
                if (liveRequestState.canCancelRequest) {
                    OutlinedButton(
                        onClick = requestViewModel::cancelCurrentRequest,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text("Cancel request")
                    }
                }
                if (liveRequestState.canRetryRequest) {
                    OutlinedButton(
                        onClick = requestViewModel::retryFailedAttempts,
                        enabled = !liveRequestState.isSendingRequest && !liveRequestState.isCancellingRequest,
                        modifier = Modifier.fillMaxWidth(),
                    ) {
                        Text("Retry failed host attempt")
                    }
                }
                hostResponses
                    .filter { response -> response.outcome != HostResponseOutcome.SUCCEEDED }
                    .forEach { response ->
                        Text(
                            response.errorMessage.ifBlank { "The processing host could not complete this request." },
                            color = MaterialTheme.colorScheme.error,
                            style = MaterialTheme.typography.bodySmall,
                        )
                    }
                requestStates.forEach { requestState ->
                    requestStatusCard(requestState)
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
                    onValueChange = { conversationMessagesViewModel.updateDraft(it) },
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
                        if (privacyConsentAccepted) {
                            submitMessageToHost(messageText)
                        } else {
                            pendingPrivacyPrompt = messageText
                        }
                    },
                    enabled =
                        messageValidation.isValid &&
                            liveRequestState.discoveredPeers.any { it.isTrusted } &&
                            !liveRequestState.isSelectingModel &&
                            !liveRequestState.isSendingRequest,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text(
                        when {
                            liveRequestState.isSelectingModel -> "Checking model…"
                            liveRequestState.isSendingRequest -> "Sending…"
                            else -> "Send"
                        },
                    )
                }
                if (!liveRequestState.discoveredPeers.any { it.isTrusted }) {
                    Text(
                        "A trusted processing host is required before a message can be sent.",
                        style = MaterialTheme.typography.bodySmall,
                    )
                }
            }
            ConversationDetailUiState.Missing -> {
                Text("This conversation could not be found on this device.")
            }
        }
    }

    pendingTrustPeerId?.let { peerId ->
        val hostNumber = liveRequestState.discoveredPeers.indexOfFirst { it.peerId == peerId } + 1
        AlertDialog(
            onDismissRequest = { pendingTrustPeerId = null },
            title = { Text("Before you trust this processing host") },
            text = {
                Text(
                    text =
                        "Processing host ${hostNumber.coerceAtLeast(1)} can read prompts and context " +
                            "after transport decryption. " +
                            "Encryption protects data in transit, but not from the receiving host or its operator. " +
                            "Trust is saved only after your confirmation. You can revoke it later from host settings.",
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

    pendingPrivacyPrompt?.let { prompt ->
        AlertDialog(
            onDismissRequest = { pendingPrivacyPrompt = null },
            title = { Text("Privacy before your first request") },
            text = {
                Text(
                    "The selected processing host can read your message and sent context after transport decryption. " +
                        "Its operator may inspect or retain received content outside this app's control. " +
                        "Only continue with content you are willing to disclose to a host you explicitly trust.",
                )
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        pendingPrivacyPrompt = null
                        onPrivacyConsentAccepted()
                        submitMessageToHost(prompt)
                    },
                ) {
                    Text("I understand and agree")
                }
            },
            dismissButton = {
                TextButton(onClick = { pendingPrivacyPrompt = null }) {
                    Text("Cancel")
                }
            },
        )
    }
}

@Composable
private fun contextCapsuleSection(
    state: ConversationContextCapsuleUiState,
    onBeginEditing: () -> Unit,
    onCancelEditing: () -> Unit,
    onMaxSerializedBytesChange: (Int) -> Unit,
    onGoalChange: (String) -> Unit,
    onKeyFactsChange: (String) -> Unit,
    onDecisionsChange: (String) -> Unit,
    onConstraintsChange: (String) -> Unit,
    onUnresolvedItemsChange: (String) -> Unit,
    onSave: () -> Unit,
    onDelete: () -> Unit,
    onUpdateFromHistory: () -> Unit,
    onResolveConflict: (Int, ConversationContextCapsuleConflictDecision) -> Unit,
) {
    var confirmDelete by remember { mutableStateOf(false) }
    val minSizeLimit = ConversationContextCapsule.MIN_CONFIGURABLE_SERIALIZED_BYTES
    val maxSizeLimit = ConversationContextCapsule.MAX_SERIALIZED_BYTES
    val sizeLimitStep = ConversationContextCapsule.CONFIGURED_SIZE_LIMIT_STEP_BYTES
    val sizeLimitSteps = (maxSizeLimit - minSizeLimit) / sizeLimitStep - 1

    Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
        Text("Context capsule", style = MaterialTheme.typography.titleLarge)
        Text(
            "A short set of goals and decisions for this conversation. It stays on this device and is not sent automatically.",
            style = MaterialTheme.typography.bodySmall,
        )
        Text(
            "Maximum capsule size on this device: ${state.maxSerializedBytes} bytes",
            style = MaterialTheme.typography.labelLarge,
        )
        Slider(
            value = state.maxSerializedBytes.toFloat(),
            onValueChange = { onMaxSerializedBytesChange(it.roundToInt()) },
            valueRange = minSizeLimit.toFloat()..maxSizeLimit.toFloat(),
            steps = sizeLimitSteps,
            enabled = !state.isSaving && !state.isDeleting && !state.isUpdatingFromHistory,
            modifier = Modifier.fillMaxWidth(),
        )
        Text(
            "This device setting applies to future saves. Existing capsules are not deleted when you lower it; " +
                "the protocol ceiling remains 4,096 bytes.",
            style = MaterialTheme.typography.bodySmall,
        )
        if (state.isLoading) {
            Text("Loading context capsule…")
        } else if (state.isEditing) {
            OutlinedTextField(
                value = state.goal,
                enabled = !state.isUpdatingFromHistory,
                onValueChange = onGoalChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Goal (required)") },
                minLines = 2,
                maxLines = 3,
            )
            OutlinedTextField(
                value = state.keyFactsText,
                enabled = !state.isUpdatingFromHistory,
                onValueChange = onKeyFactsChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Key facts — one per line") },
                minLines = 2,
                maxLines = 4,
            )
            OutlinedTextField(
                value = state.decisionsText,
                enabled = !state.isUpdatingFromHistory,
                onValueChange = onDecisionsChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Decisions — one per line") },
                minLines = 2,
                maxLines = 4,
            )
            OutlinedTextField(
                value = state.constraintsText,
                enabled = !state.isUpdatingFromHistory,
                onValueChange = onConstraintsChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Constraints — one per line") },
                minLines = 2,
                maxLines = 4,
            )
            OutlinedTextField(
                value = state.unresolvedItemsText,
                enabled = !state.isUpdatingFromHistory,
                onValueChange = onUnresolvedItemsChange,
                modifier = Modifier.fillMaxWidth(),
                label = { Text("Unresolved items — one per line") },
                minLines = 2,
                maxLines = 4,
            )
            if (state.conflictReviews.isNotEmpty()) {
                Text("Potential conflicts", style = MaterialTheme.typography.titleMedium)
                Text(
                    "These changes are suggestions only. Choose a value for every conflict before saving.",
                    style = MaterialTheme.typography.bodySmall,
                )
                state.conflictReviews.forEachIndexed { index, review ->
                    Card(modifier = Modifier.fillMaxWidth()) {
                        Column(
                            modifier = Modifier.padding(12.dp),
                            verticalArrangement = Arrangement.spacedBy(6.dp),
                        ) {
                            Text(review.suggestion.field.label, style = MaterialTheme.typography.labelLarge)
                            Text("Current value: " + review.suggestion.currentValue)
                            Text("Suggested value: " + review.suggestion.proposedValue)
                            review.decision?.let { choice ->
                                Text("Selected: " + choice.label, style = MaterialTheme.typography.bodySmall)
                            }
                            Button(
                                onClick = {
                                    onResolveConflict(index, ConversationContextCapsuleConflictDecision.KEEP_CURRENT)
                                },
                                enabled = !state.isSaving && !state.isDeleting && !state.isUpdatingFromHistory,
                                modifier = Modifier.fillMaxWidth(),
                            ) {
                                Text("Keep current value")
                            }
                            OutlinedButton(
                                onClick = {
                                    onResolveConflict(index, ConversationContextCapsuleConflictDecision.USE_SUGGESTION)
                                },
                                enabled = !state.isSaving && !state.isDeleting && !state.isUpdatingFromHistory,
                                modifier = Modifier.fillMaxWidth(),
                            ) {
                                Text("Use suggestion")
                            }
                        }
                    }
                }
            }
            Button(
                onClick = onSave,
                enabled =
                    !state.isSaving &&
                        !state.isDeleting &&
                        !state.isUpdatingFromHistory &&
                        state.conflictReviews.all { it.decision != null },
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(if (state.isSaving) "Saving…" else "Save capsule")
            }
            OutlinedButton(
                onClick = onCancelEditing,
                enabled = !state.isSaving && !state.isDeleting,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text("Cancel")
            }
        } else {
            val capsule = state.capsule
            if (capsule != null) {
                Text("Goal", style = MaterialTheme.typography.labelLarge)
                Text(capsule.goal)
                if (capsule.keyFacts.isNotEmpty()) {
                    Text("Key facts", style = MaterialTheme.typography.labelLarge)
                    Text(capsule.keyFacts.joinToString("\n") { "• $it" })
                }
                if (capsule.decisions.isNotEmpty()) {
                    Text("Decisions", style = MaterialTheme.typography.labelLarge)
                    Text(capsule.decisions.joinToString("\n") { "• $it" })
                }
                if (capsule.constraints.isNotEmpty()) {
                    Text("Constraints", style = MaterialTheme.typography.labelLarge)
                    Text(capsule.constraints.joinToString("\n") { "• $it" })
                }
                if (capsule.unresolvedItems.isNotEmpty()) {
                    Text("Unresolved items", style = MaterialTheme.typography.labelLarge)
                    Text(capsule.unresolvedItems.joinToString("\n") { "• $it" })
                }
            } else if (state.hasUnreadableCapsule) {
                Text("A saved capsule exists but could not be read. You can replace it or delete it.")
            } else {
                Text("No context capsule has been saved for this conversation.")
            }
            Button(
                onClick = onBeginEditing,
                enabled = !state.isSaving && !state.isDeleting,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    when {
                        capsule != null -> "Edit capsule"
                        state.hasUnreadableCapsule -> "Replace unreadable capsule"
                        else -> "Create capsule"
                    },
                )
            }
            if (capsule != null || state.hasUnreadableCapsule) {
                OutlinedButton(
                    onClick = { confirmDelete = true },
                    enabled = !state.isSaving && !state.isDeleting,
                    modifier = Modifier.fillMaxWidth(),
                ) {
                    Text(if (state.isDeleting) "Deleting…" else "Delete capsule")
                }
            }
        }
        if (!state.isLoading) {
            OutlinedButton(
                onClick = onUpdateFromHistory,
                enabled = !state.isSaving && !state.isDeleting && !state.isUpdatingFromHistory,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Text(
                    when {
                        state.isUpdatingFromHistory -> "Preparing draft…"
                        state.isEditing -> "Refresh draft from local history"
                        else -> "Suggest capsule from local history"
                    },
                )
            }
            Text(
                "Uses only this conversation’s local messages. The draft is not sent automatically; review and save it to persist.",
                style = MaterialTheme.typography.bodySmall,
            )
        }
        state.errorMessage?.let { message ->
            Text(message, color = MaterialTheme.colorScheme.error, style = MaterialTheme.typography.bodySmall)
        }
        state.message?.let { message ->
            Text(message, style = MaterialTheme.typography.bodySmall)
        }
    }

    if (confirmDelete) {
        AlertDialog(
            onDismissRequest = { confirmDelete = false },
            title = { Text("Delete context capsule?") },
            text = {
                Text(
                    "This removes the saved capsule from this device. The conversation and its messages remain.",
                )
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        confirmDelete = false
                        onDelete()
                    },
                ) {
                    Text("Delete")
                }
            },
            dismissButton = {
                TextButton(onClick = { confirmDelete = false }) {
                    Text("Cancel")
                }
            },
        )
    }
}

@Composable
private fun requestStatusCard(requestState: RequestStateEntity) {
    val presentation = requestState.status.toPresentation()
    Card(modifier = Modifier.fillMaxWidth()) {
        Column(
            modifier = Modifier.padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
        ) {
            Text(presentation.label, style = MaterialTheme.typography.titleMedium)
            Text(presentation.detail, style = MaterialTheme.typography.bodyMedium)
            if (requestState.attemptNumber > 0) {
                Text("Attempt ${requestState.attemptNumber}", style = MaterialTheme.typography.bodySmall)
            }
            requestState.errorCode?.let { storedCode ->
                val errorCode = RequestErrorCode.fromWireCode(storedCode)
                Text(
                    "Error code: ${errorCode.wireCode}",
                    style = MaterialTheme.typography.labelMedium,
                )
                Text(errorCode.userMessage, style = MaterialTheme.typography.bodySmall)
            }
            Text(
                "Updated ${formatConversationTimestamp(requestState.updatedAtEpochMillis)}",
                style = MaterialTheme.typography.bodySmall,
            )
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
