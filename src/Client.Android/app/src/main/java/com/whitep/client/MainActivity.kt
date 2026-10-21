package com.whitep.client

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent { WhitePClientScreen() }
    }
}

@Composable
private fun WhitePClientScreen() {
    var requestText by remember { mutableStateOf("") }
    var status by remember { mutableStateOf("Not connected") }
    MaterialTheme {
        Surface(modifier = Modifier.fillMaxSize()) {
            Column(
                modifier = Modifier.fillMaxSize().padding(24.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Text("White-P", style = MaterialTheme.typography.headlineMedium)
                Text("White-P — P2P client preview (transport not implemented)")
                OutlinedTextField(
                    value = requestText,
                    onValueChange = { requestText = it },
                    modifier = Modifier.fillMaxWidth(),
                    label = { Text("Request") },
                    supportingText = { Text("Requests are not sent yet; transport is not implemented.") }
                )
                Button(
                    onClick = { status = if (requestText.isBlank()) "Enter a request first" else "Request not sent: no host transport yet" },
                    modifier = Modifier.fillMaxWidth()
                ) { Text("Prepare request") }
                Text("Status: $status")
            }
        }
    }
}
