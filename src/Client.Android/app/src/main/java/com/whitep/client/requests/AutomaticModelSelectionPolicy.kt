package com.whitep.client.requests

import org.whitep.shared.contracts.v1.InferenceRequest
import org.whitep.shared.contracts.v1.ModelCapability

enum class AutomaticModelSelectionFailure(val userMessage: String) {
    NO_READY_HOST(
        "No explicitly trusted, reachable host is ready for this request.",
    ),
    NO_VERIFIED_MODEL(
        "No ready host reports a catalog-verified installed model.",
    ),
    INVALID_MODEL_METADATA(
        "Ready hosts returned invalid or ambiguous model capability metadata.",
    ),
    NO_COMPATIBLE_MODEL(
        "No catalog-verified model can accept this request within the product limits.",
    ),
}

sealed interface AutomaticModelSelectionResult {
    data class Selected(
        val hostId: String,
        val modelId: String,
        val maxOutputTokens: Int,
    ) : AutomaticModelSelectionResult

    data class Rejected(
        val failure: AutomaticModelSelectionFailure,
    ) : AutomaticModelSelectionResult
}

/**
 * Deterministic first-release model policy.
 *
 * Windows advertises a model capability only after the active backend model ID matches the
 * persisted catalog selection and the installed file SHA-256 matches the value verified by the
 * catalog installer. Android uses fresh authenticated HostStatus data, never rendezvous labels,
 * as the source of READY model capabilities.
 */
object AutomaticModelSelectionPolicy {
    const val PREFERRED_OUTPUT_TOKENS = 1_024
    const val MAX_PRODUCT_OUTPUT_TOKENS = 4_096
    const val MAX_PROTOCOL_OUTPUT_TOKENS = 131_072

    private data class Option(
        val hostId: String,
        val modelId: String,
        val maxContextTokens: Long,
        val maxOutputTokens: Long,
        val selectedOutputTokens: Int,
    )

    fun resolve(
        request: InferenceRequest,
        candidates: Collection<HostCandidate>,
    ): AutomaticModelSelectionResult {
        val ready =
            candidates.filter { candidate ->
                HostCandidateSelector.isReadyForRequestIgnoringModel(request, candidate)
            }
        if (ready.isEmpty()) {
            return AutomaticModelSelectionResult.Rejected(AutomaticModelSelectionFailure.NO_READY_HOST)
        }

        var sawVerifiedCatalogModel = false
        var sawInvalidMetadata = false
        val options = ArrayList<Option>()

        for (candidate in ready) {
            val models = candidate.status.modelsList
            if (models.isEmpty()) continue
            sawVerifiedCatalogModel = true

            val ids = models.map { model -> model.modelId }
            if (ids.any(String::isBlank) || ids.distinct().size != ids.size) {
                sawInvalidMetadata = true
                continue
            }

            for (model in models) {
                if (!isValidMetadata(model)) {
                    sawInvalidMetadata = true
                    continue
                }

                val effectiveCeiling =
                    minOf(
                        model.maxOutputTokens,
                        MAX_PRODUCT_OUTPUT_TOKENS.toLong(),
                        MAX_PROTOCOL_OUTPUT_TOKENS.toLong(),
                    )
                val selectedTokens = minOf(PREFERRED_OUTPUT_TOKENS.toLong(), effectiveCeiling).toInt()
                if (selectedTokens <= 0) {
                    sawInvalidMetadata = true
                    continue
                }

                options +=
                    Option(
                        hostId = candidate.announcement.ephemeralHostId,
                        modelId = model.modelId,
                        maxContextTokens = model.maxContextTokens,
                        maxOutputTokens = effectiveCeiling,
                        selectedOutputTokens = selectedTokens,
                    )
            }
        }

        if (options.isEmpty()) {
            val failure = when {
                !sawVerifiedCatalogModel -> AutomaticModelSelectionFailure.NO_VERIFIED_MODEL
                sawInvalidMetadata -> AutomaticModelSelectionFailure.INVALID_MODEL_METADATA
                else -> AutomaticModelSelectionFailure.NO_COMPATIBLE_MODEL
            }
            return AutomaticModelSelectionResult.Rejected(failure)
        }

        // Preference: largest usable context, then output headroom. Model ID and PeerId ordinal
        // order are stable tie-breakers; no display-name parsing or random/default model is used.
        val selected =
            options
                .sortedWith(
                    compareByDescending<Option> { it.maxContextTokens }
                        .thenByDescending { it.maxOutputTokens }
                        .thenBy { it.modelId }
                        .thenBy { it.hostId },
                )
                .first()

        return AutomaticModelSelectionResult.Selected(
            hostId = selected.hostId,
            modelId = selected.modelId,
            maxOutputTokens = selected.selectedOutputTokens,
        )
    }

    private fun isValidMetadata(model: ModelCapability): Boolean {
        val modelId = model.modelId
        return modelId.isNotBlank() &&
            modelId == modelId.trim() &&
            modelId.length <= 128 &&
            modelId.none { character -> character.code < 32 || character.code == 127 } &&
            model.displayName.isNotBlank() &&
            model.maxContextTokens > 0L &&
            model.maxOutputTokens > 0L
    }
}
