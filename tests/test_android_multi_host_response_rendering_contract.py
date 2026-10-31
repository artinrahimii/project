from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
RESPONSE_CARDS = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/ui/request/HostResponseCards.kt"
VIEW_MODEL_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt"


class AndroidMultiHostResponseRenderingTests(unittest.TestCase):
    def test_each_host_attempt_has_its_own_stable_compose_card(self):
        source = RESPONSE_CARDS.read_text(encoding="utf-8")
        loop_index = source.index("responses.forEach { response ->")
        key_index = source.index("key(response.requestId, response.attemptId)", loop_index)
        card_index = source.index("Card(", key_index)
        output_index = source.index("response.outputText", card_index)

        self.assertLess(loop_index, key_index)
        self.assertLess(key_index, card_index)
        self.assertLess(card_index, output_index)
        self.assertIn("import androidx.compose.runtime.key", source)
        self.assertNotIn("joinToString", source)
        self.assertNotIn("responses.map", source)

    def test_jvm_acceptance_keeps_two_host_outputs_in_separate_entries(self):
        source = VIEW_MODEL_TEST.read_text(encoding="utf-8")
        self.assertIn("keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard", source)
        self.assertIn("assertEquals(2, cards.size)", source)
        self.assertIn('listOf("answer from host A", "answer from host B")', source)
        self.assertIn("listOf(ATTEMPT_A, ATTEMPT_B)", source)


if __name__ == "__main__":
    unittest.main()
