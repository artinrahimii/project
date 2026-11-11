from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android"
GRADLE = (ANDROID / "app/build.gradle.kts").read_text(encoding="utf-8")
ACTIVITY = (ANDROID / "app/src/main/java/com/whitep/client/MainActivity.kt").read_text(encoding="utf-8")
CONVERSATION = (ANDROID / "app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
VIEW_MODEL = (ANDROID / "app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt").read_text(encoding="utf-8")


class UpdateS1011RendezvousConfigurationTests(unittest.TestCase):
    def test_packaged_https_directory_is_injected_without_a_guessed_default(self):
        self.assertIn('gradleProperty("whitepRendezvousServiceBaseUrl")', GRADLE)
        self.assertIn('environmentVariable("WHITEP_RENDEZVOUS_SERVICE_BASE_URL")', GRADLE)
        self.assertIn('.orElse("")', GRADLE)
        self.assertIn('buildConfigField(\n            "String",\n            "RENDEZVOUS_SERVICE_BASE_URL"', GRADLE)
        self.assertIn("buildConfig = true", GRADLE)
        self.assertIn("BuildConfig.RENDEZVOUS_SERVICE_BASE_URL", ACTIVITY)
        self.assertIn('uri.scheme?.equals("https", ignoreCase = true) != true', GRADLE)
        self.assertIn("uri.userInfo != null", GRADLE)
        self.assertIn("uri.query != null", GRADLE)
        self.assertIn("uri.fragment != null", GRADLE)

    def test_custom_directory_entry_is_confined_to_advanced_host_settings(self):
        self.assertIn('title = "Advanced host settings"', ACTIVITY)
        self.assertIn('label = { Text("Custom HTTPS Rendezvous URL (optional)") }', ACTIVITY)
        self.assertIn("Clear this field to return to that default", ACTIVITY)
        self.assertIn("customOverride.ifBlank { configuredRendezvousUrl }", ACTIVITY)
        self.assertIn("remove(RENDEZVOUS_URL_KEY)", ACTIVITY)
        self.assertNotIn("Rendezvous URL", CONVERSATION)
        self.assertNotIn("Peer ID", CONVERSATION)
        self.assertNotIn("Model ID", CONVERSATION)
        self.assertNotIn("Port", CONVERSATION)

    def test_first_chat_send_is_gated_by_declineable_privacy_consent(self):
        self.assertIn("if (privacyConsentAccepted)", CONVERSATION)
        self.assertIn("pendingPrivacyPrompt = messageText", CONVERSATION)
        self.assertIn('title = { Text("Privacy before your first request") }', CONVERSATION)
        self.assertIn("onPrivacyConsentAccepted()", CONVERSATION)
        self.assertIn("submitMessageToHost(prompt)", CONVERSATION)
        self.assertIn("fun sendRequest(prompt: String)", VIEW_MODEL)
        self.assertIn("resolveAutomaticModel(probe)", VIEW_MODEL)
        self.assertIn("maxOutputTokens = resolution.maxOutputTokens", VIEW_MODEL)


if __name__ == "__main__":
    unittest.main()
