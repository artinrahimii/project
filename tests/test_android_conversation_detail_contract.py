from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
UNIT_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"


class AndroidConversationDetailContractTests(unittest.TestCase):
    def test_detail_screen_observes_messages_and_sends_only_to_local_room(self):
        screen = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
        self.assertIn("conversationMessagesViewModel.messages.collectAsState", screen)
        self.assertIn("messages.forEach { message ->", screen)
        self.assertIn("conversationMessagesViewModel.sendMessage(messageText)", screen)
        self.assertIn('Text("Send")', screen)
        self.assertIn("Messages are saved only on this device", screen)
        self.assertIn("Sending to a processing host is not available yet", screen)

    def test_message_view_model_persists_ordered_user_messages_without_network_calls(self):
        source = (ANDROID / "ui/conversations/ConversationMessagesViewModel.kt").read_text(encoding="utf-8")
        self.assertIn("messageDao.observeForConversation(conversationId)", source)
        self.assertIn("messageDao.upsert(", source)
        self.assertIn('authorRole = "user"', source)
        self.assertIn("normalizedContent.isEmpty()", source)
        self.assertNotIn("Socket(", source)
        self.assertNotIn("HttpClient", source)

    def test_room_tests_cover_ordered_persistence_and_blank_rejection(self):
        test_source = (UNIT_TESTS / "ui/conversations/ConversationMessagesViewModelTest.kt").read_text(encoding="utf-8")
        self.assertIn("persistsMessagesInChronologicalOrder", test_source)
        self.assertIn("rejectsBlankMessagesWithoutPersistingThem", test_source)
        self.assertIn("createdAtEpochMillis", test_source)


if __name__ == "__main__":
    unittest.main()
