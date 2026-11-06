from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsuleSummarizer.kt"
KOTLIN_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleSummarizerTest.kt"


class AndroidContextCapsuleSummarizerContractTests(unittest.TestCase):
    def test_provider_is_replaceable_and_result_is_codec_validated(self):
        source = SOURCE.read_text(encoding="utf-8")
        for marker in (
            "fun interface ConversationContextCapsuleSummaryProvider",
            "SchemaValidatedConversationContextCapsuleSummarizer",
            "provider.summarize(",
            "candidate.encode(request.maxSerializedBytes)",
            "ConversationContextCapsule.decode(bytes)",
            "ConversationContextSummaryRejectionReason.SIZE_LIMIT_EXCEEDED",
            "ConversationContextSummaryRejectionReason.INVALID_SIZE_LIMIT",
        ):
            self.assertIn(marker, source)

    def test_default_strategy_is_extractive_and_does_not_add_network_transport(self):
        source = SOURCE.read_text(encoding="utf-8")
        kotlin_test = KOTLIN_TEST.read_text(encoding="utf-8")
        self.assertIn("ExtractiveConversationContextCapsuleSummaryProvider", source)
        self.assertIn("Unmarked transcript", kotlin_test)
        self.assertNotIn("HttpClient", source)
        self.assertNotIn("Socket(", source)
        self.assertNotIn("Retrofit", source)

    def test_runtime_tests_cover_schema_limits_and_empty_history_safely(self):
        source = KOTLIN_TEST.read_text(encoding="utf-8")
        for marker in (
            "defaultProviderExtractsExplicitMarkersWithoutCopyingUnmarkedHistory",
            "invalidProviderSchemaIsRejectedByTheCodecBoundary",
            "invalidConfiguredLimitIsRejectedWithoutCallingProvider",
            "oversizeProviderOutputIsRejectedAtTheSelectedLimit",
            "existingCapsuleIsPreservedWhenAllInputMessagesAreBlank",
        ):
            self.assertIn(marker, source)


if __name__ == "__main__":
    unittest.main()
