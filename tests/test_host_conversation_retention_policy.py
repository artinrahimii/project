"""Regression guards for the host's default in-memory conversation retention boundary."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
POLICY = (ROOT / "src/Host.Application/Chat/HostConversationRetentionPolicy.cs").read_text(encoding="utf-8")
CONVERSATION = (ROOT / "src/Host.Application/Chat/ChatConversation.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
SETTINGS = (ROOT / "src/Host.Infrastructure/LocalHostSettingsStore.cs").read_text(encoding="utf-8")
SECURITY_DOC = (ROOT / "docs/security.md").read_text(encoding="utf-8")


class HostConversationRetentionPolicyTests(unittest.TestCase):
    def test_default_policy_is_process_session_only_and_non_persistent(self):
        self.assertIn("ProcessSessionOnly", POLICY)
        self.assertIn("PersistsPromptAndResponse => false", POLICY)
        self.assertIn("HostConversationRetentionPolicy.Default", WINDOW)
        self.assertIn("new(HostConversationRetentionPolicy.Default)", WINDOW)

    def test_conversation_uses_only_an_in_memory_collection(self):
        self.assertIn("private readonly List<ConversationMessage> _messages = [];", CONVERSATION)
        self.assertIn("RetentionPolicy = retentionPolicy ?? HostConversationRetentionPolicy.Default;", CONVERSATION)
        for persistence_api in ("FileStream", "StreamWriter", "File.Write", "File.Append", "JsonSerializer", "DbContext"):
            with self.subTest(persistence_api=persistence_api):
                self.assertNotIn(persistence_api, CONVERSATION)

    def test_persisted_host_settings_do_not_contain_prompt_or_response_content(self):
        settings_document = SETTINGS.split("private sealed class SettingsDocument", 1)[1]
        for setting in ("ServerUrl", "ModelId", "RequestTimeoutSeconds", "EngineExecutablePath", "ModelFilePath", "ActiveCatalogModelId"):
            with self.subTest(setting=setting):
                self.assertIn(setting, settings_document)
        for content_field in ("Prompt", "ResponseText", "ContextCapsule", "ConversationMessages"):
            with self.subTest(content_field=content_field):
                self.assertNotIn(content_field, settings_document)

    def test_security_document_scopes_the_claim_and_separates_android_storage(self):
        self.assertIn("T0179", SECURITY_DOC)
        self.assertIn("T0180", SECURITY_DOC)
        self.assertIn("ADR-0020", SECURITY_DOC)
        self.assertIn("AC-PRIV-01", SECURITY_DOC)
        self.assertIn("فرایند جاری", SECURITY_DOC)
        self.assertIn("رمزگذاری", SECURITY_DOC)


if __name__ == "__main__":
    unittest.main()
