from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
REQUEST_VIEW_MODEL = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt"
RESPONSE_CARDS = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/HostResponseCards.kt"
MAIN_ACTIVITY = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt"
VIEW_MODEL_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt"


class AndroidHostResponseCardsContractTests(unittest.TestCase):
    def test_response_state_is_keyed_by_request_and_attempt_and_checks_expected_peer(self):
        source = REQUEST_VIEW_MODEL.read_text(encoding="utf-8")
        self.assertIn("data class HostResponseCardState", source)
        self.assertIn("response.requestId != expectedRequestId", source)
        self.assertIn("response.attemptId != expectedAttemptId", source)
        self.assertIn("response.ephemeralHostId != expectedHostId", source)
        self.assertIn("response.modelId != expectedModelId", source)
        self.assertIn("it.requestId == card.requestId && it.attemptId == card.attemptId", source)
        self.assertIn("MutableStateFlow<List<HostResponseCardState>>", source)

    def test_ui_renders_one_card_for_each_response_without_concatenating_outputs(self):
        source = RESPONSE_CARDS.read_text(encoding="utf-8")
        self.assertIn("responses.forEach", source)
        self.assertIn("Card(", source)
        self.assertIn("response.hostId", source)
        self.assertIn("response.attemptId", source)
        self.assertIn("response.outputText", source)
        self.assertNotIn("joinToString", source)
        activity = MAIN_ACTIVITY.read_text(encoding="utf-8")
        self.assertIn("hostResponseCards(responses = requestState.hostResponses)", activity)
        self.assertIn("import com.whitep.client.ui.request.hostResponseCards", activity)
        self.assertLess(activity.index("import com.whitep.client.ui.request.RequestViewModel"), activity.index("import com.whitep.client.ui.request.hostResponseCards"))

    def test_jvm_tests_cover_distinct_hosts_duplicate_attempts_and_rejection(self):
        source = VIEW_MODEL_TEST.read_text(encoding="utf-8")
        self.assertIn("keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard", source)
        self.assertIn("rejectsResponsesThatDoNotMatchTheExpectedRequestAttemptHostOrModel", source)
        self.assertIn("clearsOnlyTheEphemeralResponseCards", source)
        self.assertIn("answer from host A", source)
        self.assertIn("answer from host B", source)


if __name__ == "__main__":
    unittest.main()
