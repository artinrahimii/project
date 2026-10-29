from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
SCREEN = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt"
VIEW_MODEL = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModel.kt"
VM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModelTest.kt"


class AndroidContextCapsuleUiContractTests(unittest.TestCase):
    def test_conversation_screen_exposes_create_view_edit_delete_controls(self):
        source = SCREEN.read_text(encoding="utf-8")
        for marker in (
            '"Context capsule"',
            '"Create capsule"',
            '"Edit capsule"',
            '"Delete capsule"',
            '"Save capsule"',
            "AlertDialog(",
            "contextCapsuleViewModel::beginEditing",
            "contextCapsuleViewModel::cancelEditing",
            "contextCapsuleViewModel.save()",
            "contextCapsuleViewModel.delete()",
        ):
            self.assertIn(marker, source)

    def test_screen_state_is_connected_to_room_backed_view_model(self):
        screen = SCREEN.read_text(encoding="utf-8")
        source = VIEW_MODEL.read_text(encoding="utf-8")
        self.assertIn("RoomConversationContextCapsuleRepository", screen)
        self.assertIn("contextCapsuleViewModel.uiState.collectAsState()", screen)
        self.assertIn("repository.observe(conversationId)", source)
        self.assertIn("repository.save(conversationId, capsule, clockMillis())", source)
        self.assertIn("repository.delete(conversationId)", source)

    def test_runtime_view_model_tests_cover_create_edit_validation_and_delete(self):
        source = VM_TEST.read_text(encoding="utf-8")
        self.assertIn("createsEditsAndDeletesCapsuleUsingPersistentRepository", source)
        self.assertIn("blankGoalShowsValidationAndDoesNotWriteARecord", source)
        self.assertIn("ConversationContextCapsuleLoadResult.Missing", source)


if __name__ == "__main__":
    unittest.main()
