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
        self.assertIn("ConversationMessageValidator.validate(content)", source)
        self.assertIn("validation.normalizedText ?: return false", source)
        self.assertNotIn("Socket(", source)
        self.assertNotIn("HttpClient", source)

    def test_draft_state_uses_saved_state_handle_for_recreation(self):
        view_model = (ANDROID / "ui/conversations/ConversationMessagesViewModel.kt").read_text(encoding="utf-8")
        screen = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
        unit_test = (UNIT_TESTS / "ui/conversations/ConversationMessagesViewModelTest.kt").read_text(encoding="utf-8")
        self.assertIn('getStateFlow(DRAFT_STATE_KEY, "")', view_model)
        self.assertIn("savedStateHandle[DRAFT_STATE_KEY] = value", view_model)
        self.assertIn("extras.createSavedStateHandle()", view_model)
        self.assertIn("conversationMessagesViewModel.draft.collectAsState()", screen)
        self.assertIn("conversationMessagesViewModel.updateDraft(it)", screen)
        self.assertNotIn("var draft by remember(conversationId)", screen)
        self.assertIn("draftIsRestoredAfterViewModelRecreation", unit_test)

    def test_file_backed_messages_and_request_states_survive_database_reopen(self):
        test_source = (UNIT_TESTS / "data/ClientDatabaseTest.kt").read_text(encoding="utf-8")
        self.assertIn("messagesAndRequestStatusesSurviveClosingAndReopeningFileBackedDatabase", test_source)
        self.assertIn("Room", test_source)
        self.assertIn(".databaseBuilder(", test_source)
        self.assertIn("firstDatabase.close()", test_source)
        self.assertIn("reopenedDatabase", test_source)
        self.assertIn("reopenedDatabase", test_source)
        self.assertIn(".messageDao()", test_source)
        self.assertIn(".requestStateDao()", test_source)
        self.assertIn("PersistedRequestStatus.WAITING", test_source)
        self.assertIn("PersistedRequestStatus.FAILED", test_source)
        self.assertIn('application.deleteDatabase(databaseName)', test_source)


    def test_end_to_end_local_conversation_workflow_has_repository_and_ui_coverage(self):
        workflow = (UNIT_TESTS / "ui/conversations/ConversationWorkflowAcceptanceTest.kt").read_text(encoding="utf-8")
        list_screen = (ANDROID / "ui/conversations/ConversationListScreen.kt").read_text(encoding="utf-8")
        detail_screen = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")

        self.assertIn("createsConversationSendsMessagesAndRestoresHistoryAcrossDatabaseReopen", workflow)
        self.assertIn("conversationListViewModel.createConversation(", workflow)
        self.assertIn("messagesViewModel.sendMessage(", workflow)
        self.assertIn("firstDatabase.close()", workflow)
        self.assertIn("reopenedDatabase", workflow)
        self.assertIn("restoredMessagesViewModel.messages.first()", workflow)
        self.assertIn(".messageDao()", workflow)
        self.assertIn(".requestStateDao()", workflow)
        self.assertIn("conversationListViewModel.createConversation()", list_screen)
        self.assertIn("onOpenConversation(conversation.conversationId)", list_screen)
        self.assertIn("conversationMessagesViewModel.messages.collectAsState", detail_screen)
        self.assertIn("conversationMessagesViewModel.sendMessage(messageText)", detail_screen)
        self.assertIn("Messages are saved only on this device", detail_screen)
        self.assertIn("Sending to a processing host is not available yet", detail_screen)

    def test_room_tests_cover_ordered_persistence_and_blank_rejection(self):
        test_source = (UNIT_TESTS / "ui/conversations/ConversationMessagesViewModelTest.kt").read_text(encoding="utf-8")
        self.assertIn("persistsMessagesInChronologicalOrder", test_source)
        self.assertIn("rejectsBlankMessagesWithoutPersistingThem", test_source)
        self.assertIn("createdAtEpochMillis", test_source)


if __name__ == "__main__":
    unittest.main()
