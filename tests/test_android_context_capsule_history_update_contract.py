from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
VIEW_MODEL = ANDROID / "ui/conversations/ConversationContextCapsuleViewModel.kt"
SCREEN = ANDROID / "ui/conversations/ConversationDetailScreen.kt"
MAIN_ACTIVITY = ANDROID / "MainActivity.kt"
KOTLIN_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModelTest.kt"


class AndroidContextCapsuleHistoryUpdateContractTests(unittest.TestCase):
    def test_summary_reads_only_the_selected_conversation_room_flow(self):
        source = VIEW_MODEL.read_text(encoding="utf-8")
        self.assertIn("messageDao", source)
        self.assertIn(".observeForConversation(conversationId)", source)
        self.assertIn("ConversationContextSummaryMessage(", source)
        self.assertIn("SchemaValidatedConversationContextCapsuleSummarizer", source)
        self.assertIn("maxSerializedBytes = current.maxSerializedBytes", source)
        self.assertIn("Review and save to persist.", source)

    def test_suggestion_is_a_draft_and_does_not_send_network_requests(self):
        source = VIEW_MODEL.read_text(encoding="utf-8")
        screen = SCREEN.read_text(encoding="utf-8")
        main = MAIN_ACTIVITY.read_text(encoding="utf-8")
        self.assertIn("suspend fun updateFromLocalHistory()", source)
        self.assertIn("isEditing = true", source)
        self.assertIn("Refresh draft from local history", screen)
        self.assertIn("Uses only this conversation’s local messages.", screen)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("Socket(", source)
        self.assertIn("Requests are not sent yet; transport is not implemented.", main)

    def test_robolectric_test_covers_conversation_scope_and_manual_save(self):
        source = KOTLIN_TEST.read_text(encoding="utf-8")
        self.assertIn("suggestsCapsuleFromOnlyThisConversationsLocalHistoryAndRequiresSave", source)
        self.assertIn("conversation-other", source)
        self.assertIn("assertEquals(2, draft.summarySourceMessageCount)", source)
        self.assertIn("assertTrue(viewModel.save())", source)


if __name__ == "__main__":
    unittest.main()
