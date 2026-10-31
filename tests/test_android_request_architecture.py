"""Source-contract checks for the Android request ViewModel/repository boundary."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/data/ClientRequestRepository.kt"
).read_text(encoding="utf-8")
VIEW_MODEL = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt"
).read_text(encoding="utf-8")
ACTIVITY = (
    ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt"
).read_text(encoding="utf-8")
ANDROID_BUILD = (
    ROOT / "src/Client.Android/app/build.gradle.kts"
).read_text(encoding="utf-8")


class AndroidRequestArchitectureTests(unittest.TestCase):
    def test_repository_exposes_a_flow_and_suspendable_data_contract(self):
        self.assertIn("interface ClientRequestRepository", REPOSITORY)
        self.assertIn("val draft: Flow<ClientRequestDraft>", REPOSITORY)
        self.assertIn("suspend fun updatePrompt(prompt: String)", REPOSITORY)
        self.assertIn("suspend fun prepareRequest()", REPOSITORY)
        self.assertIn("suspend fun clearDraft()", REPOSITORY)
        self.assertIn("class InMemoryClientRequestRepository", REPOSITORY)

    def test_viewmodel_observes_repository_state_and_owns_screen_actions(self):
        self.assertIn("class RequestViewModel(", VIEW_MODEL)
        self.assertIn("repository.draft", VIEW_MODEL)
        self.assertIn("StateFlow<RequestUiState>", VIEW_MODEL)
        self.assertIn("viewModelScope", VIEW_MODEL)
        self.assertIn("fun updatePrompt(prompt: String)", VIEW_MODEL)
        self.assertIn("fun prepareRequest()", VIEW_MODEL)
        self.assertIn("fun clearDraft()", VIEW_MODEL)

    def test_compose_screen_uses_viewmodel_and_keeps_transport_boundary_truthful(self):
        self.assertIn('implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.9.3")', ANDROID_BUILD)
        self.assertIn("RequestViewModel", ACTIVITY)
        self.assertIn("viewModel()", ACTIVITY)
        self.assertIn("collectAsState()", ACTIVITY)
        self.assertIn("requestViewModel::updatePrompt", ACTIVITY)
        self.assertIn("requestViewModel::prepareRequest", ACTIVITY)
        self.assertIn("Android-to-host transport is not implemented.", VIEW_MODEL)
        self.assertIn("The processing host can read your prompt and context capsule", ACTIVITY)
        self.assertNotIn("OkHttpClient(", REPOSITORY + VIEW_MODEL)
        self.assertNotIn("Socket(", REPOSITORY + VIEW_MODEL)


if __name__ == "__main__":
    unittest.main()
