from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"


class AndroidConversationManagementContractTests(unittest.TestCase):
    def test_view_model_performs_real_room_create_rename_delete_and_lookup(self):
        source = (ANDROID / "ui/conversations/ConversationListViewModel.kt").read_text(encoding="utf-8")
        self.assertIn("suspend fun createConversation", source)
        self.assertIn("conversationDao.upsert(conversation)", source)
        self.assertIn("suspend fun renameConversation", source)
        self.assertIn("conversationDao.findById(conversationId)", source)
        self.assertIn("suspend fun deleteConversation", source)
        self.assertIn("conversationDao.deleteById(conversationId)", source)
        self.assertIn("UUID.randomUUID().toString()", source)
        self.assertIn("normalizedTitle.isEmpty()", source)

    def test_list_screen_wires_user_actions_to_room_mutations_and_detail_navigation(self):
        source = (ANDROID / "ui/conversations/ConversationListScreen.kt").read_text(encoding="utf-8")
        self.assertIn('Text("New conversation")', source)
        self.assertIn('Text("Open")', source)
        self.assertIn('Text("Rename")', source)
        self.assertIn('Text("Delete")', source)
        self.assertIn("conversationListViewModel.createConversation()", source)
        self.assertIn("conversationListViewModel.renameConversation(", source)
        self.assertIn("conversationListViewModel.deleteConversation(", source)
        self.assertIn("onOpenConversation(conversation.conversationId)", source)
        self.assertIn("AlertDialog(", source)

    def test_open_route_loads_the_selected_local_conversation(self):
        destinations = (ANDROID / "ui/navigation/ClientDestination.kt").read_text(encoding="utf-8")
        activity = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
        detail = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
        self.assertIn('CONVERSATION_DETAIL("conversation/{conversationId}"', destinations)
        self.assertIn("fun conversationRoute(conversationId: String)", destinations)
        self.assertIn("navArgument(ClientDestination.CONVERSATION_ID_ARGUMENT)", activity)
        self.assertIn("conversationDetailScreen(", activity)
        self.assertIn("getConversation(conversationId)", detail)
        self.assertIn("currentState.conversation.title", detail)

    def test_room_regression_tests_cover_crud_cascade_and_destination_routing(self):
        view_model_tests = (TESTS / "ui/conversations/ConversationListViewModelTest.kt").read_text(encoding="utf-8")
        navigation_tests = (TESTS / "ui/navigation/ClientDestinationTest.kt").read_text(encoding="utf-8")
        self.assertIn("supportsConversationCreateRenameDeleteAndCascade", view_model_tests)
        self.assertIn("conversationDetailRouteCanCarryAnEncodedConversationId", navigation_tests)
        self.assertIn("countForConversation(created.conversationId)", view_model_tests)


if __name__ == "__main__":
    unittest.main()
