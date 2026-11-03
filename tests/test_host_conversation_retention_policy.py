"""Regression guards for default-off, owner-consented host conversation retention."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POLICY = (ROOT / "src/Host.Application/Chat/HostConversationRetentionPolicy.cs").read_text(encoding="utf-8")
CONVERSATION = (ROOT / "src/Host.Application/Chat/ChatConversation.cs").read_text(encoding="utf-8")
STORE = (ROOT / "src/Host.Windows/Chat/WindowsHostConversationStore.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
WINDOW_XAML = (ROOT / "src/Host.Windows/MainWindow.xaml").read_text(encoding="utf-8")
SETTINGS = (ROOT / "src/Host.Infrastructure/LocalHostSettingsStore.cs").read_text(encoding="utf-8")
SECURITY_DOC = (ROOT / "docs/security.md").read_text(encoding="utf-8")


class HostConversationRetentionPolicyTests(unittest.TestCase):
    def test_default_is_non_persistent_and_durable_policy_requires_owner_consent(self):
        self.assertIn("ProcessSessionOnly", POLICY)
        self.assertIn("LocalDeviceWithOwnerConsent", POLICY)
        self.assertIn("PersistsPromptAndResponse", POLICY)
        self.assertIn("FromOwnerConsent(bool ownerConsent)", POLICY)
        self.assertIn("HostConversationRetentionPolicy.Default", POLICY)
        self.assertIn("HostConversationRetentionPolicy.FromOwnerConsent(_settings.PersistHostConversation)", WINDOW)
        self.assertIn("ownerConsent: true", WINDOW)

    def test_conversation_remains_in_memory_and_policy_is_explicitly_switchable(self):
        self.assertIn("private readonly List<ConversationMessage> _messages = [];", CONVERSATION)
        self.assertIn("RetentionPolicy = retentionPolicy ?? HostConversationRetentionPolicy.Default;", CONVERSATION)
        self.assertIn("UpdateRetentionPolicy(HostConversationRetentionPolicy retentionPolicy)", CONVERSATION)
        for persistence_api in ("FileStream", "StreamWriter", "File.Write", "File.Append", "JsonSerializer", "DbContext"):
            with self.subTest(persistence_api=persistence_api):
                self.assertNotIn(persistence_api, CONVERSATION)

    def test_local_history_uses_current_user_dpapi_and_clears_plaintext_buffers(self):
        self.assertIn("new WindowsDpapiSecretProtector()", STORE)
        self.assertIn("IUserSecretProtector", STORE)
        self.assertIn("CryptographicOperations.ZeroMemory(plaintext)", STORE)
        self.assertIn("CryptographicOperations.ZeroMemory(protectedPayload)", STORE)
        self.assertIn("File.Move(temporaryPath, _filePath, overwrite: true)", STORE)
        self.assertIn("public void Delete()", STORE)
        self.assertIn("DataProtectionScope.CurrentUser", (ROOT / "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs").read_text(encoding="utf-8"))

    def test_settings_store_records_only_the_boolean_preference_not_conversation_content(self):
        self.assertIn("bool PersistHostConversation = false", SETTINGS)
        settings_document = SETTINGS.split("private sealed class SettingsDocument", 1)[1]
        self.assertIn("PersistHostConversation", settings_document)
        for content_field in ("Prompt", "ResponseText", "ContextCapsule", "ConversationMessages"):
            with self.subTest(content_field=content_field):
                self.assertNotIn(content_field, settings_document)

    def test_ui_requires_a_separate_confirmation_and_allows_opt_out(self):
        self.assertIn('x:Name="HostConversationRetentionToggle"', WINDOW_XAML)
        self.assertIn('Checked="HostConversationRetentionToggle_Changed"', WINDOW_XAML)
        self.assertIn("MessageBoxButton.YesNo", WINDOW)
        self.assertIn("MessageBoxResult.Yes", WINDOW)
        self.assertIn("DisableHostConversationRetention()", WINDOW)
        self.assertIn("_conversationStore.Delete()", WINDOW)
        self.assertIn("HostConversationRetentionToggle", WINDOW)

    def test_security_document_scopes_the_guarantee_and_keeps_android_storage_separate(self):
        self.assertIn("T0179", SECURITY_DOC)
        self.assertIn("T0180", SECURITY_DOC)
        self.assertIn("T0181", SECURITY_DOC)
        self.assertIn("ADR-0020", SECURITY_DOC)
        self.assertIn("AC-PRIV-01", SECURITY_DOC)
        self.assertIn("فرایند جاری", SECURITY_DOC)
        self.assertIn("DPAPI", SECURITY_DOC)


if __name__ == "__main__":
    unittest.main()
