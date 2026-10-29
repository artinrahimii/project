package com.whitep.client.ui.conversations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.whitep.client.context.ConversationContextCapsule
import com.whitep.client.context.ConversationContextCapsuleError
import com.whitep.client.context.ConversationContextCapsuleSummaryResult
import com.whitep.client.context.ConversationContextSummaryMessage
import com.whitep.client.context.ConversationContextSummaryRejectionReason
import com.whitep.client.context.ConversationContextSummaryRequest
import com.whitep.client.context.SchemaValidatedConversationContextCapsuleSummarizer
import com.whitep.client.data.ConversationContextCapsuleLoadResult
import com.whitep.client.data.ConversationContextCapsuleSaveResult
import com.whitep.client.data.ConversationContextCapsuleSizeLimitStore
import com.whitep.client.data.MessageDao
import com.whitep.client.data.RoomConversationContextCapsuleRepository
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

data class ConversationContextCapsuleUiState(
    val isLoading: Boolean = true,
    val maxSerializedBytes: Int = ConversationContextCapsule.DEFAULT_MAX_SERIALIZED_BYTES,
    val capsule: ConversationContextCapsule? = null,
    val hasUnreadableCapsule: Boolean = false,
    val isEditing: Boolean = false,
    val goal: String = "",
    val keyFactsText: String = "",
    val decisionsText: String = "",
    val constraintsText: String = "",
    val unresolvedItemsText: String = "",
    val isSaving: Boolean = false,
    val isDeleting: Boolean = false,
    val isUpdatingFromHistory: Boolean = false,
    val summarySourceMessageCount: Int? = null,
    val message: String? = null,
    val errorMessage: String? = null,
)

/** Owns the local create/edit/delete lifecycle for one conversation's context capsule. */
class ConversationContextCapsuleViewModel(
    private val conversationId: String,
    private val repository: RoomConversationContextCapsuleRepository,
    private val sizeLimitStore: ConversationContextCapsuleSizeLimitStore,
    private val messageDao: MessageDao,
    private val summarizer: SchemaValidatedConversationContextCapsuleSummarizer =
        SchemaValidatedConversationContextCapsuleSummarizer(),
    private val clockMillis: () -> Long = System::currentTimeMillis,
) : ViewModel() {
    private val _uiState =
        MutableStateFlow(
            ConversationContextCapsuleUiState(
                maxSerializedBytes = sizeLimitStore.loadMaxSerializedBytes(),
            ),
        )
    val uiState: StateFlow<ConversationContextCapsuleUiState> = _uiState.asStateFlow()

    init {
        viewModelScope.launch {
            repository.observe(conversationId).collect { result ->
                val current = _uiState.value
                _uiState.value =
                    when (result) {
                        is ConversationContextCapsuleLoadResult.Found -> {
                            if (current.isEditing) {
                                current.copy(
                                    isLoading = false,
                                    capsule = result.capsule,
                                    hasUnreadableCapsule = false,
                                )
                            } else {
                                current.copy(
                                    isLoading = false,
                                    capsule = result.capsule,
                                    hasUnreadableCapsule = false,
                                    goal = result.capsule.goal,
                                    keyFactsText = result.capsule.keyFacts.joinToString("\n"),
                                    decisionsText = result.capsule.decisions.joinToString("\n"),
                                    constraintsText = result.capsule.constraints.joinToString("\n"),
                                    unresolvedItemsText = result.capsule.unresolvedItems.joinToString("\n"),
                                    errorMessage = null,
                                )
                            }
                        }
                        ConversationContextCapsuleLoadResult.Missing -> {
                            if (current.isEditing) {
                                current.copy(
                                    isLoading = false,
                                    capsule = null,
                                    hasUnreadableCapsule = false,
                                )
                            } else {
                                current.copy(
                                    isLoading = false,
                                    capsule = null,
                                    hasUnreadableCapsule = false,
                                    goal = "",
                                    keyFactsText = "",
                                    decisionsText = "",
                                    constraintsText = "",
                                    unresolvedItemsText = "",
                                    errorMessage = null,
                                )
                            }
                        }
                        is ConversationContextCapsuleLoadResult.InvalidStoredCapsule -> {
                            current.copy(
                                isLoading = false,
                                hasUnreadableCapsule = true,
                                errorMessage = "The saved context capsule could not be read. Replace or delete it.",
                            )
                        }
                    }
            }
        }
    }

    fun beginEditing() {
        val current = _uiState.value
        val capsule = current.capsule
        _uiState.value =
            current.copy(
                isEditing = true,
                goal = capsule?.goal.orEmpty(),
                keyFactsText = capsule?.keyFacts?.joinToString("\n").orEmpty(),
                decisionsText = capsule?.decisions?.joinToString("\n").orEmpty(),
                constraintsText = capsule?.constraints?.joinToString("\n").orEmpty(),
                unresolvedItemsText = capsule?.unresolvedItems?.joinToString("\n").orEmpty(),
                message = null,
                errorMessage = null,
            )
    }

    fun cancelEditing() {
        val current = _uiState.value
        val capsule = current.capsule
        _uiState.value =
            current.copy(
                isEditing = false,
                goal = capsule?.goal.orEmpty(),
                keyFactsText = capsule?.keyFacts?.joinToString("\n").orEmpty(),
                decisionsText = capsule?.decisions?.joinToString("\n").orEmpty(),
                constraintsText = capsule?.constraints?.joinToString("\n").orEmpty(),
                unresolvedItemsText = capsule?.unresolvedItems?.joinToString("\n").orEmpty(),
                message = null,
                errorMessage = null,
            )
    }

    fun updateMaxSerializedBytes(value: Int) {
        if (!ConversationContextCapsule.isValidConfiguredSizeLimit(value)) return
        try {
            if (sizeLimitStore.saveMaxSerializedBytes(value)) {
                _uiState.value =
                    _uiState.value.copy(
                        maxSerializedBytes = value,
                        message = null,
                        errorMessage = null,
                    )
            } else {
                _uiState.value =
                    _uiState.value.copy(
                        errorMessage = "Choose a capsule limit from 512 to 4,096 bytes in 256-byte steps.",
                        message = null,
                    )
            }
        } catch (_: Exception) {
            _uiState.value =
                _uiState.value.copy(
                    errorMessage = "The capsule size setting could not be saved.",
                    message = null,
                )
        }
    }

    fun updateGoal(value: String) = updateDraft { it.copy(goal = value) }

    fun updateKeyFacts(value: String) = updateDraft { it.copy(keyFactsText = value) }

    fun updateDecisions(value: String) = updateDraft { it.copy(decisionsText = value) }

    fun updateConstraints(value: String) = updateDraft { it.copy(constraintsText = value) }

    fun updateUnresolvedItems(value: String) = updateDraft { it.copy(unresolvedItemsText = value) }

    /**
     * Builds a reviewable capsule draft from this conversation's local Room history.
     * This operation does not send history to a host or network.
     */
    suspend fun updateFromLocalHistory(): Boolean {
        val current = _uiState.value
        if (current.isUpdatingFromHistory || current.isSaving || current.isDeleting) return false
        _uiState.value = current.copy(isUpdatingFromHistory = true, message = null, errorMessage = null)

        return try {
            val localMessages =
                messageDao
                    .observeForConversation(conversationId)
                    .first()
                    .filter { it.content.isNotBlank() }
                    .map { message ->
                        ConversationContextSummaryMessage(
                            messageId = message.messageId,
                            authorRole = message.authorRole,
                            content = message.content,
                            createdAtEpochMillis = message.createdAtEpochMillis,
                        )
                    }
            if (localMessages.isEmpty()) {
                _uiState.value =
                    _uiState.value.copy(
                        isUpdatingFromHistory = false,
                        errorMessage = "Add a non-empty local message before creating a capsule draft.",
                    )
                return false
            }

            val draftCapsule =
                current.toCapsuleDraftOrNull()
                    ?: (repository.find(conversationId) as? ConversationContextCapsuleLoadResult.Found)?.capsule
            val result =
                summarizer.summarize(
                    ConversationContextSummaryRequest(
                        messages = localMessages,
                        existingCapsule = draftCapsule,
                        maxSerializedBytes = current.maxSerializedBytes,
                    ),
                )
            when (result) {
                is ConversationContextCapsuleSummaryResult.Accepted -> {
                    val capsule = result.capsule
                    _uiState.value =
                        _uiState.value.copy(
                            isUpdatingFromHistory = false,
                            isEditing = true,
                            goal = capsule.goal,
                            keyFactsText = capsule.keyFacts.joinToString("\n"),
                            decisionsText = capsule.decisions.joinToString("\n"),
                            constraintsText = capsule.constraints.joinToString("\n"),
                            unresolvedItemsText = capsule.unresolvedItems.joinToString("\n"),
                            summarySourceMessageCount = result.sourceMessageCount,
                            message =
                                "Draft prepared from ${result.sourceMessageCount} local messages. Review and save to persist.",
                            errorMessage = null,
                        )
                    true
                }
                is ConversationContextCapsuleSummaryResult.Rejected -> {
                    val errorMessage =
                        when (result.reason) {
                            ConversationContextSummaryRejectionReason.NO_USABLE_MESSAGES ->
                                "Add a non-empty local message before creating a capsule draft."
                            ConversationContextSummaryRejectionReason.INVALID_SIZE_LIMIT ->
                                "The selected capsule size limit is invalid."
                            ConversationContextSummaryRejectionReason.PROVIDER_FAILURE ->
                                "Local capsule summarization failed. Your saved capsule was not changed."
                            ConversationContextSummaryRejectionReason.INVALID_CAPSULE ->
                                "The suggested capsule did not match the supported format."
                            ConversationContextSummaryRejectionReason.SIZE_LIMIT_EXCEEDED ->
                                "The suggested capsule exceeds the selected size limit. Shorten the history or raise the limit."
                        }
                    _uiState.value =
                        _uiState.value.copy(
                            isUpdatingFromHistory = false,
                            errorMessage = errorMessage,
                            message = null,
                        )
                    false
                }
            }
        } catch (exception: CancellationException) {
            _uiState.value = _uiState.value.copy(isUpdatingFromHistory = false)
            throw exception
        } catch (_: Exception) {
            _uiState.value =
                _uiState.value.copy(
                    isUpdatingFromHistory = false,
                    errorMessage = "Local conversation history could not be read. Your saved capsule was not changed.",
                )
            false
        }
    }

    suspend fun save(): Boolean {
        val current = _uiState.value
        if (current.isSaving || current.isDeleting || current.isUpdatingFromHistory) return false
        _uiState.value = current.copy(isSaving = true, message = null, errorMessage = null)
        val capsule =
            ConversationContextCapsule(
                goal = current.goal.trim(),
                keyFacts = current.keyFactsText.toItems(),
                decisions = current.decisionsText.toItems(),
                constraints = current.constraintsText.toItems(),
                unresolvedItems = current.unresolvedItemsText.toItems(),
            )
        return try {
            when (val result = repository.save(conversationId, capsule, clockMillis(), current.maxSerializedBytes)) {
                is ConversationContextCapsuleSaveResult.Saved -> {
                    val savedState = _uiState.value
                    _uiState.value =
                        savedState.copy(
                            isSaving = false,
                            isEditing = false,
                            capsule = capsule,
                            hasUnreadableCapsule = false,
                            goal = capsule.goal,
                            keyFactsText = capsule.keyFacts.joinToString("\n"),
                            decisionsText = capsule.decisions.joinToString("\n"),
                            constraintsText = capsule.constraints.joinToString("\n"),
                            unresolvedItemsText = capsule.unresolvedItems.joinToString("\n"),
                            message = "Context capsule saved on this device.",
                            errorMessage = null,
                        )
                    true
                }
                is ConversationContextCapsuleSaveResult.Rejected -> {
                    _uiState.value =
                        _uiState.value.copy(
                            isSaving = false,
                            errorMessage = result.error.toUserMessage(),
                        )
                    false
                }
            }
        } catch (exception: CancellationException) {
            throw exception
        } catch (_: Exception) {
            _uiState.value =
                _uiState.value.copy(
                    isSaving = false,
                    errorMessage = "The context capsule could not be saved. Your draft is still on screen.",
                )
            false
        }
    }

    suspend fun delete(): Boolean {
        val current = _uiState.value
        if (current.isDeleting || current.isSaving || current.isUpdatingFromHistory) return false
        _uiState.value = current.copy(isDeleting = true, message = null, errorMessage = null)
        return try {
            val deleted = repository.delete(conversationId) > 0
            val latest = _uiState.value
            _uiState.value =
                latest.copy(
                    isDeleting = false,
                    isEditing = false,
                    capsule = null,
                    hasUnreadableCapsule = false,
                    goal = "",
                    keyFactsText = "",
                    decisionsText = "",
                    constraintsText = "",
                    unresolvedItemsText = "",
                    message = if (deleted) "Context capsule deleted from this device." else null,
                    errorMessage = if (deleted) null else "There is no saved context capsule to delete.",
                )
            deleted
        } catch (exception: CancellationException) {
            throw exception
        } catch (_: Exception) {
            _uiState.value =
                _uiState.value.copy(
                    isDeleting = false,
                    errorMessage = "The context capsule could not be deleted.",
                )
            false
        }
    }

    private fun ConversationContextCapsuleUiState.toCapsuleDraftOrNull(): ConversationContextCapsule? {
        if (!isEditing) return capsule
        if (goal.isBlank()) return null
        return ConversationContextCapsule(
            goal = goal.trim(),
            keyFacts = keyFactsText.toItems(),
            decisions = decisionsText.toItems(),
            constraints = constraintsText.toItems(),
            unresolvedItems = unresolvedItemsText.toItems(),
        )
    }

    private fun updateDraft(transform: (ConversationContextCapsuleUiState) -> ConversationContextCapsuleUiState) {
        _uiState.value = transform(_uiState.value).copy(message = null, errorMessage = null)
    }

    private fun String.toItems(): List<String> =
        lineSequence()
            .map { it.trim() }
            .filter { it.isNotEmpty() }
            .toList()

    private fun ConversationContextCapsuleError.toUserMessage(): String =
        when (this) {
            ConversationContextCapsuleError.EMPTY_GOAL -> "Enter a goal before saving the context capsule."
            ConversationContextCapsuleError.BLANK_ITEM -> "Remove blank items before saving."
            ConversationContextCapsuleError.TOO_LARGE ->
                "The context capsule exceeds the selected size limit. Shorten it or raise the limit."
            ConversationContextCapsuleError.INVALID_SIZE_LIMIT ->
                "Choose a capsule limit from 512 to 4,096 bytes in 256-byte steps."
            ConversationContextCapsuleError.UNSUPPORTED_SCHEMA_VERSION ->
                "This context capsule version is not supported."
            ConversationContextCapsuleError.INVALID_UTF8,
            ConversationContextCapsuleError.MALFORMED_JSON,
            ConversationContextCapsuleError.MISSING_FIELD,
            ConversationContextCapsuleError.UNKNOWN_FIELD,
            ConversationContextCapsuleError.INVALID_FIELD_TYPE,
            -> "The context capsule is invalid. Review the fields and try again."
        }

    class Factory(
        private val conversationId: String,
        private val repository: RoomConversationContextCapsuleRepository,
        private val sizeLimitStore: ConversationContextCapsuleSizeLimitStore,
        private val messageDao: MessageDao,
    ) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T {
            if (!modelClass.isAssignableFrom(ConversationContextCapsuleViewModel::class.java)) {
                throw IllegalArgumentException("Unknown ViewModel class: ${modelClass.name}")
            }
            return ConversationContextCapsuleViewModel(conversationId, repository, sizeLimitStore, messageDao) as T
        }
    }
}
