"""Source-contract checks for the persisted Android request-status UI."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
ANDROID_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"
SCREEN = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
REQUEST_SCREEN = (ANDROID / "MainActivity.kt").read_text(encoding="utf-8")
PRESENTATION = (ANDROID / "ui/request/RequestStatusPresentation.kt").read_text(encoding="utf-8")
PRESENTATION_TEST = (
    ANDROID_TESTS / "ui/request/RequestStatusPresentationTest.kt"
).read_text(encoding="utf-8")
DATABASE_TEST = (ANDROID_TESTS / "data/ClientDatabaseTest.kt").read_text(encoding="utf-8")
ENTITY = (ANDROID / "data/RequestStateEntity.kt").read_text(encoding="utf-8")
LIFECYCLE_ENTITY = (ANDROID / "data/InferenceRequestStateEntity.kt").read_text(encoding="utf-8")
LIFECYCLE_REPOSITORY = (ANDROID / "data/InferenceRequestLifecycleRepository.kt").read_text(encoding="utf-8")
REQUEST_VIEW_MODEL = (ANDROID / "ui/request/RequestViewModel.kt").read_text(encoding="utf-8")


class AndroidRequestStatusContractTests(unittest.TestCase):
    def test_conversation_detail_observes_real_room_status_rows(self):
        self.assertIn("database.requestStateDao()", SCREEN)
        self.assertIn("requestStateDao.observeForConversation(conversationId)", SCREEN)
        self.assertIn("requestStates.forEach { requestState ->", SCREEN)
        self.assertIn("requestStatusCard(requestState)", SCREEN)
        self.assertIn("RequestRunStatus.FAILED", SCREEN)
        self.assertIn("liveRequestState.runStatus", SCREEN)
        self.assertIn("hostResponses", SCREEN)
        self.assertIn("Request status", SCREEN)

    def test_all_eight_persisted_states_have_explicit_display_labels(self):
        for status in (
            "QUEUED",
            "CONTACTING",
            "WAITING",
            "PARTIAL",
            "COMPLETED",
            "FAILED",
            "CANCELLED",
            "EXPIRED",
        ):
            with self.subTest(status=status):
                self.assertIn(f"    {status},", ENTITY)
                self.assertIn(f"PersistedRequestStatus.{status}", PRESENTATION)
                self.assertIn(f"PersistedRequestStatus.{status} to", PRESENTATION_TEST)

    def test_status_flow_is_database_backed_not_synthetic(self):
        self.assertIn("fun observeForConversation(conversationId: String): Flow<List<RequestStateEntity>>", ENTITY)
        self.assertIn("allPersistedRequestStatusesCanBeReadFromRoomInCreationOrder", DATABASE_TEST)
        self.assertNotIn("PersistedRequestStatus.values()", SCREEN)

    def test_app_restart_expires_in_flight_state_and_restores_it_in_the_request_view_model(self):
        self.assertIn("recoverLatest(clockMillis())", REQUEST_VIEW_MODEL)
        self.assertIn("persistLifecycleState(context, PersistedRequestStatus.QUEUED)", REQUEST_VIEW_MODEL)
        self.assertIn("persistLifecycleState(nextContext, PersistedRequestStatus.WAITING)", REQUEST_VIEW_MODEL)
        self.assertIn("APP_CLOSED_BEFORE_RESULT", LIFECYCLE_ENTITY + LIFECYCLE_REPOSITORY)
        self.assertIn("RoomInferenceRequestLifecycleRepository", REQUEST_SCREEN)
        self.assertIn("RequestViewModelFactory", REQUEST_SCREEN)
        self.assertIn("status = 'EXPIRED'", LIFECYCLE_ENTITY)
        for sensitive_field in ("val prompt:", "contextCapsule", "responseText", "val payload:", "ephemeralHostId"):
            self.assertNotIn(sensitive_field, LIFECYCLE_ENTITY)

    def test_live_network_and_inference_status_displays_structured_state_not_free_form_messages(self):
        self.assertIn("toNetworkInferenceStatusPresentation()", REQUEST_SCREEN)
        self.assertIn("statusPresentation.networkStatus", REQUEST_SCREEN)
        self.assertIn("statusPresentation.inferenceStatus", REQUEST_SCREEN)
        self.assertNotIn("requestState.networkStatusMessage", REQUEST_SCREEN)
        self.assertNotIn("requestState.hostAvailabilityMessage", REQUEST_SCREEN)
        self.assertNotIn("networkStatusMessage", PRESENTATION)
        self.assertNotIn("hostAvailabilityMessage", PRESENTATION)


if __name__ == "__main__":
    unittest.main()
