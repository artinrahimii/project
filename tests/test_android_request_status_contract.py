"""Source-contract checks for the persisted Android request-status UI."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
ANDROID_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"
SCREEN = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
PRESENTATION = (ANDROID / "ui/request/RequestStatusPresentation.kt").read_text(encoding="utf-8")
PRESENTATION_TEST = (
    ANDROID_TESTS / "ui/request/RequestStatusPresentationTest.kt"
).read_text(encoding="utf-8")
DATABASE_TEST = (ANDROID_TESTS / "data/ClientDatabaseTest.kt").read_text(encoding="utf-8")
ENTITY = (ANDROID / "data/RequestStateEntity.kt").read_text(encoding="utf-8")


class AndroidRequestStatusContractTests(unittest.TestCase):
    def test_conversation_detail_observes_real_room_status_rows(self):
        self.assertIn("database.requestStateDao()", SCREEN)
        self.assertIn("requestStateDao.observeForConversation(conversationId)", SCREEN)
        self.assertIn("requestStates.forEach { requestState ->", SCREEN)
        self.assertIn("requestStatusCard(requestState)", SCREEN)
        self.assertIn("No request status has been recorded", SCREEN)
        self.assertIn("Requests are not sent to a host in this preview.", SCREEN)

    def test_all_seven_persisted_states_have_explicit_display_labels(self):
        for status in (
            "QUEUED",
            "CONTACTING",
            "WAITING",
            "PARTIAL",
            "COMPLETED",
            "FAILED",
            "CANCELLED",
        ):
            with self.subTest(status=status):
                self.assertIn(f"    {status},", ENTITY)
                self.assertIn(f"PersistedRequestStatus.{status}", PRESENTATION)
                self.assertIn(f"PersistedRequestStatus.{status} to", PRESENTATION_TEST)

    def test_status_flow_is_database_backed_not_synthetic(self):
        self.assertIn("fun observeForConversation(conversationId: String): Flow<List<RequestStateEntity>>", ENTITY)
        self.assertIn("allPersistedRequestStatusesCanBeReadFromRoomInCreationOrder", DATABASE_TEST)
        self.assertNotIn("PersistedRequestStatus.values()", SCREEN)


if __name__ == "__main__":
    unittest.main()
