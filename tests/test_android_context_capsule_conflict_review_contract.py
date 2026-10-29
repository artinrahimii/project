from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
VIEW_MODEL = ANDROID / "ui/conversations/ConversationContextCapsuleViewModel.kt"
SCREEN = ANDROID / "ui/conversations/ConversationDetailScreen.kt"
KOTLIN_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModelTest.kt"


class AndroidContextCapsuleConflictReviewContractTests(unittest.TestCase):
    def test_conflicts_are_explicit_suggestions_in_the_compose_editor(self):
        source = SCREEN.read_text(encoding="utf-8")
        self.assertIn("Potential conflicts", source)
        self.assertIn("Keep current value", source)
        self.assertIn("Use suggestion", source)
        self.assertIn("state.conflictReviews.all { it.decision != null }", source)

    def test_view_model_keeps_current_values_and_gates_persistence(self):
        source = VIEW_MODEL.read_text(encoding="utf-8")
        self.assertIn("reconcileCapsuleConflicts", source)
        self.assertIn("directlyContradictsCapsuleValue", source)
        self.assertIn("Review every potential conflict before saving.", source)
        self.assertIn("ConversationContextCapsuleConflictDecision.USE_SUGGESTION", source)
        screen = SCREEN.read_text(encoding="utf-8")
        self.assertIn("ConversationContextCapsuleConflictDecision.KEEP_CURRENT", screen)
        self.assertIn("ConversationContextCapsuleConflictDecision.USE_SUGGESTION", screen)

    def test_robolectric_covers_conflict_review_and_no_write_until_confirmation(self):
        source = KOTLIN_TEST.read_text(encoding="utf-8")
        self.assertIn("conflictingHistoryAnswersNeedAnExplicitChoiceBeforeSaving", source)
        self.assertIn("assertFalse(viewModel.save())", source)
        self.assertIn("repository.find(\"conversation-ui\")", source)
        self.assertIn("ConversationContextCapsuleConflictDecision.KEEP_CURRENT", source)
        self.assertIn("ConversationContextCapsuleConflictDecision.USE_SUGGESTION", source)


if __name__ == "__main__":
    unittest.main()
