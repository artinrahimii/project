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
import com.whitep.client.data.ConversationContextCapsuleChangeSource
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
import java.util.Locale

enum class ConversationContextCapsuleConflictField(
    val label: String,
) {
    GOAL("Goal"),
    KEY_FACT("Key fact"),
    DECISION("Decision"),
    CONSTRAINT("Constraint"),
    UNRESOLVED_ITEM("Unresolved item"),
}

enum class ConversationContextCapsuleConflictDecision(
    val label: String,
) {
    KEEP_CURRENT("Keep current value"),
    USE_SUGGESTION("Use suggestion"),
}

data class ConversationContextCapsuleConflictSuggestion(
    val field: ConversationContextCapsuleConflictField,
    val currentValue: String,
    val proposedValue: String,
)

data class ConversationContextCapsuleConflictReview(
    val suggestion: ConversationContextCapsuleConflictSuggestion,
    val decision: ConversationContextCapsuleConflictDecision? = null,
)

private data class CapsuleConflictReconciliation(
    val capsule: ConversationContextCapsule,
    val suggestions: List<ConversationContextCapsuleConflictSuggestion>,
)

private val capsuleConflictWordPattern = Regex("[\\p{L}\\p{N}]+(?:['’][\\p{L}]+)?")
private val capsuleConflictNegations =
    setOf(
        "not",
        "no",
        "never",
        "without",
        "cannot",
        "can't",
        "don't",
        "doesn't",
        "isn't",
        "aren't",
        "won't",
        "shouldn't",
        "mustn't",
    )
private val capsuleConflictOpposites =
    mapOf(
        "enabled" to "disabled",
        "disabled" to "enabled",
        "enable" to "disable",
        "disable" to "enable",
        "active" to "inactive",
        "inactive" to "active",
        "on" to "off",
        "off" to "on",
        "online" to "offline",
        "offline" to "online",
        "connected" to "disconnected",
        "disconnected" to "connected",
        "available" to "unavailable",
        "unavailable" to "available",
        "allowed" to "disallowed",
        "disallowed" to "allowed",
        "allow" to "deny",
        "deny" to "allow",
        "required" to "optional",
        "optional" to "required",
        "present" to "absent",
        "absent" to "present",
        "included" to "excluded",
        "excluded" to "included",
        "include" to "exclude",
        "exclude" to "include",
        "true" to "false",
        "false" to "true",
        "yes" to "no",
        "no" to "yes",
        "safe" to "unsafe",
        "unsafe" to "safe",
        "supported" to "unsupported",
        "unsupported" to "supported",
    )

private fun normalizeCapsuleConflictValue(value: String): String =
    value.trim().lowercase(Locale.ROOT).replace(Regex("\\s+"), " ")

private fun directlyContradictsCapsuleValue(
    currentValue: String,
    proposedValue: String,
): Boolean {
    val currentWords =
        capsuleConflictWordPattern
            .findAll(currentValue.lowercase(Locale.ROOT))
            .map { it.value }
            .toList()
    val proposedWords =
        capsuleConflictWordPattern
            .findAll(proposedValue.lowercase(Locale.ROOT))
            .map { it.value }
            .toList()
    if (currentWords.isEmpty() || proposedWords.isEmpty()) return false

    val currentNegated = currentWords.any { it in capsuleConflictNegations }
    val proposedNegated = proposedWords.any { it in capsuleConflictNegations }
    if (
        currentWords.filterNot { it in capsuleConflictNegations } ==
        proposedWords.filterNot { it in capsuleConflictNegations } &&
        currentNegated != proposedNegated
    ) {
        return true
    }
    if (currentWords.size != proposedWords.size) return false

    var oppositeCount = 0
    for (index in currentWords.indices) {
        val currentWord = currentWords[index]
        val proposedWord = proposedWords[index]
        if (currentWord == proposedWord) continue
        if (capsuleConflictOpposites[currentWord] != proposedWord) return false
        oppositeCount++
    }
    return oppositeCount == 1
}

private fun reconcileCapsuleConflicts(
    current: ConversationContextCapsule?,
    proposed: ConversationContextCapsule,
): CapsuleConflictReconciliation {
    if (current == null) return CapsuleConflictReconciliation(proposed, emptyList())
    val suggestions = mutableListOf<ConversationContextCapsuleConflictSuggestion>()
    val goalChanged =
        current.goal.isNotBlank() &&
            normalizeCapsuleConflictValue(current.goal) != normalizeCapsuleConflictValue(proposed.goal)
    val safeGoal =
        if (goalChanged) {
            suggestions.add(
                ConversationContextCapsuleConflictSuggestion(
                    field = ConversationContextCapsuleConflictField.GOAL,
                    currentValue = current.goal,
                    proposedValue = proposed.goal,
                ),
            )
            current.goal
        } else {
            proposed.goal
        }

    fun mergeField(
        field: ConversationContextCapsuleConflictField,
        currentValues: List<String>,
        proposedValues: List<String>,
    ): List<String> {
        val approved = currentValues.filter { it.isNotBlank() }.distinctBy(::normalizeCapsuleConflictValue)
        val merged = approved.toMutableList()
        val additions =
            proposedValues.filter { proposedValue ->
                approved.none { normalizeCapsuleConflictValue(it) == normalizeCapsuleConflictValue(proposedValue) }
            }
        for (addition in additions) {
            val conflict = approved.firstOrNull { directlyContradictsCapsuleValue(it, addition) }
            if (conflict != null) {
                suggestions.add(
                    ConversationContextCapsuleConflictSuggestion(
                        field = field,
                        currentValue = conflict,
                        proposedValue = addition,
                    ),
                )
            } else if (merged.none { normalizeCapsuleConflictValue(it) == normalizeCapsuleConflictValue(addition) }) {
                merged.add(addition)
            }
        }
        return merged
    }

    return CapsuleConflictReconciliation(
        capsule =
            proposed.copy(
                goal = safeGoal,
                keyFacts =
                    mergeField(
                        ConversationContextCapsuleConflictField.KEY_FACT,
                        current.keyFacts,
                        proposed.keyFacts,
                    ),
                decisions =
                    mergeField(
                        ConversationContextCapsuleConflictField.DECISION,
                        current.decisions,
                        proposed.decisions,
                    ),
                constraints =
                    mergeField(
                        ConversationContextCapsuleConflictField.CONSTRAINT,
                        current.constraints,
                        proposed.constraints,
                    ),
                unresolvedItems =
                    mergeField(
                        ConversationContextCapsuleConflictField.UNRESOLVED_ITEM,
                        current.unresolvedItems,
                        proposed.unresolvedItems,
                    ),
            ),
        suggestions = suggestions,
    )
}

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
    val hasUserEditedSummaryDraft: Boolean = false,
    val conflictReviews: List<ConversationContextCapsuleConflictReview> = emptyList(),
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
                conflictReviews = emptyList(),
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
                conflictReviews = emptyList(),
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

    fun resolveConflict(
        index: Int,
        decision: ConversationContextCapsuleConflictDecision,
    ): Boolean {
        val current = _uiState.value
        val review = current.conflictReviews.getOrNull(index) ?: return false
        var next = current
        if (review.decision != decision) {
            if (review.decision == ConversationContextCapsuleConflictDecision.USE_SUGGESTION) {
                next =
                    replaceConflictValue(
                        next,
                        review.suggestion,
                        review.suggestion.proposedValue,
                        review.suggestion.currentValue,
                    )
            }
            if (decision == ConversationContextCapsuleConflictDecision.USE_SUGGESTION) {
                next =
                    replaceConflictValue(
                        next,
                        review.suggestion,
                        review.suggestion.currentValue,
                        review.suggestion.proposedValue,
                    )
            }
        }
        val reviews = current.conflictReviews.toMutableList()
        reviews[index] = review.copy(decision = decision)
        val unresolvedCount = reviews.count { it.decision == null }
        _uiState.value =
            next.copy(
                conflictReviews = reviews,
                message =
                    if (unresolvedCount == 0) {
                        "All potential conflicts reviewed. Save to persist your choices."
                    } else {
                        "Review every potential conflict before saving."
                    },
                errorMessage = null,
            )
        return true
    }

    private fun replaceConflictValue(
        state: ConversationContextCapsuleUiState,
        suggestion: ConversationContextCapsuleConflictSuggestion,
        fromValue: String,
        toValue: String,
    ): ConversationContextCapsuleUiState {
        fun replaceLineItems(text: String): String {
            val items =
                text
                    .toItems()
                    .filterNot {
                        normalizeCapsuleConflictValue(it) == normalizeCapsuleConflictValue(fromValue)
                    }.toMutableList()
            if (
                items.none {
                    normalizeCapsuleConflictValue(it) == normalizeCapsuleConflictValue(toValue)
                }
            ) {
                items.add(toValue)
            }
            return items.joinToString("\n")
        }
        return when (suggestion.field) {
            ConversationContextCapsuleConflictField.GOAL -> state.copy(goal = toValue)
            ConversationContextCapsuleConflictField.KEY_FACT ->
                state.copy(
                    keyFactsText = replaceLineItems(state.keyFactsText),
                )
            ConversationContextCapsuleConflictField.DECISION ->
                state.copy(
                    decisionsText = replaceLineItems(state.decisionsText),
                )
            ConversationContextCapsuleConflictField.CONSTRAINT ->
                state.copy(
                    constraintsText = replaceLineItems(state.constraintsText),
                )
            ConversationContextCapsuleConflictField.UNRESOLVED_ITEM ->
                state.copy(unresolvedItemsText = replaceLineItems(state.unresolvedItemsText))
        }
    }

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
                    val reconciliation = reconcileCapsuleConflicts(draftCapsule, result.capsule)
                    val capsule = reconciliation.capsule
                    val reviews = reconciliation.suggestions.map { ConversationContextCapsuleConflictReview(it) }
                    val draftMessage =
                        if (reviews.isEmpty()) {
                            "Draft from ${result.sourceMessageCount} local messages. Review and save to persist."
                        } else {
                            "Found " + reviews.size + " potential conflicts. Choose a value for each before saving."
                        }
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
                            hasUserEditedSummaryDraft = false,
                            conflictReviews = reviews,
                            message = draftMessage,
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
        if (current.conflictReviews.any { it.decision == null }) {
            _uiState.value =
                current.copy(
                    message = null,
                    errorMessage = "Review every potential conflict before saving.",
                )
            return false
        }
        val changeSource =
            when {
                current.summarySourceMessageCount == null ->
                    ConversationContextCapsuleChangeSource.USER_EDIT
                current.conflictReviews.isNotEmpty() || current.hasUserEditedSummaryDraft ->
                    ConversationContextCapsuleChangeSource.LOCAL_HISTORY_SUMMARY_REVIEWED
                else -> ConversationContextCapsuleChangeSource.LOCAL_HISTORY_SUMMARY
            }
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
            when (val result = repository.save(conversationId, capsule, clockMillis(), current.maxSerializedBytes, changeSource)) {
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
                            conflictReviews = emptyList(),
                            summarySourceMessageCount = null,
                            hasUserEditedSummaryDraft = false,
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
                    conflictReviews = emptyList(),
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
        val current = _uiState.value
        _uiState.value =
            transform(current).copy(
                hasUserEditedSummaryDraft =
                    current.hasUserEditedSummaryDraft || current.summarySourceMessageCount != null,
                message = null,
                errorMessage = null,
            )
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
