package com.whitep.client.ui.request

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Card
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.key
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/** Renders each correlated host attempt independently; response bodies are never concatenated. */
@Composable
internal fun hostResponseCards(responses: List<HostResponseCardState>) {
    Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
        Text("Host responses", style = MaterialTheme.typography.titleLarge)
        if (responses.isEmpty()) {
            Text(
                "No validated host responses have been received yet.",
                style = MaterialTheme.typography.bodyMedium,
            )
        } else {
            responses.forEach { response ->
                key(response.requestId, response.attemptId) {
                    Card(modifier = Modifier.fillMaxWidth()) {
                        Column(
                            modifier = Modifier.padding(16.dp),
                            verticalArrangement = Arrangement.spacedBy(6.dp),
                        ) {
                            Text(
                                "Host: ${response.hostId}",
                                style = MaterialTheme.typography.titleMedium,
                            )
                            Text(
                                "Outcome: ${response.outcome.name.lowercase().replace('_', ' ')}",
                                style = MaterialTheme.typography.labelLarge,
                            )
                            Text("Model: ${response.modelId}", style = MaterialTheme.typography.bodySmall)
                            Text("Request: ${response.requestId}", style = MaterialTheme.typography.bodySmall)
                            Text("Attempt: ${response.attemptId}", style = MaterialTheme.typography.bodySmall)
                            Text("Response", style = MaterialTheme.typography.labelLarge)
                            Text(
                                response.outputText.ifBlank { "This host returned no response text." },
                                style = MaterialTheme.typography.bodyLarge,
                            )
                            if (response.errorMessage.isNotBlank()) {
                                Text(
                                    response.errorMessage,
                                    color = MaterialTheme.colorScheme.error,
                                    style = MaterialTheme.typography.bodySmall,
                                )
                            }
                            Text(
                                "Completed at: ${response.completedAtUnixMs} ms UTC",
                                style = MaterialTheme.typography.bodySmall,
                            )
                        }
                    }
                }
            }
        }
    }
}
