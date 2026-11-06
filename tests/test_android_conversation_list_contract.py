from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src" / "Client.Android" / "app" / "src" / "main" / "java" / "com" / "whitep" / "client"


class AndroidConversationListContractTests(unittest.TestCase):
    def test_conversation_list_uses_room_and_renders_observed_rows(self):
        screen = (ANDROID / "ui" / "conversations" / "ConversationListScreen.kt").read_text(encoding="utf-8")
        self.assertIn("ClientDatabase.getInstance(applicationContext).conversationDao()", screen)
        self.assertIn("conversationListViewModel.conversations.collectAsState", screen)
        self.assertIn("items(conversations, key = { it.conversationId })", screen)
        self.assertIn("No conversations yet.", screen)

    def test_view_model_exposes_the_real_dao_flow_without_sample_data(self):
        source = (ANDROID / "ui" / "conversations" / "ConversationListViewModel.kt").read_text(encoding="utf-8")
        self.assertIn("conversationDao.observeAll()", source)
        self.assertIn("Flow<List<ConversationEntity>>", source)
        self.assertNotIn("sampleConversations", source)

    def test_conversations_route_is_connected_to_the_bottom_navigation(self):
        destination = (ANDROID / "ui" / "navigation" / "ClientDestination.kt").read_text(encoding="utf-8")
        navigation = (ANDROID / "ui" / "navigation" / "ClientNavigation.kt").read_text(encoding="utf-8")
        activity = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
        self.assertIn('CONVERSATIONS("conversations", "Conversations"', destination)
        self.assertIn("ClientDestination.CONVERSATIONS", navigation)
        self.assertIn("composable(ClientDestination.CONVERSATIONS.route)", activity)
        self.assertIn("conversationListScreen(", activity)
        self.assertIn("onOpenConversation = { conversationId ->", activity)
        self.assertIn("import com.whitep.client.ui.conversations.conversationListScreen", activity)


if __name__ == "__main__":
    unittest.main()
