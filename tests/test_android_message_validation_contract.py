"""Source-contract checks for Android message text bounds and the canonical UTF-8 limit."""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
ANDROID = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client"
ANDROID_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client"
DOMAIN = (ANDROID / "domain/ConversationMessageValidator.kt").read_text(encoding="utf-8")
VIEW_MODEL = (ANDROID / "ui/conversations/ConversationMessagesViewModel.kt").read_text(encoding="utf-8")
SCREEN = (ANDROID / "ui/conversations/ConversationDetailScreen.kt").read_text(encoding="utf-8")
UNIT_TESTS = (ANDROID_TESTS / "domain/ConversationMessageValidatorTest.kt").read_text(encoding="utf-8")
VIEW_MODEL_TESTS = (
    ANDROID_TESTS / "ui/conversations/ConversationMessagesViewModelTest.kt"
).read_text(encoding="utf-8")
PROTO = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")


class AndroidMessageValidationContractTests(unittest.TestCase):
    def test_domain_validation_counts_utf8_bytes_and_matches_protocol_ceiling(self):
        self.assertIn("MAX_MESSAGE_UTF8_BYTES = 8_192", DOMAIN)
        self.assertIn("toByteArray(Charsets.UTF_8).size", DOMAIN)
        self.assertIn("byteCount > MAX_MESSAGE_UTF8_BYTES", DOMAIN)
        self.assertIn("Maximum 8,192 UTF-8 bytes", PROTO)

    def test_view_model_rejects_invalid_content_before_persisting(self):
        self.assertIn("ConversationMessageValidator.validate(content)", VIEW_MODEL)
        self.assertIn("validation.normalizedText ?: return false", VIEW_MODEL)
        self.assertIn("rejectsOverLimitMessagesWithoutPersistingThem", VIEW_MODEL_TESTS)

    def test_ui_disables_invalid_send_and_explains_over_limit_content(self):
        self.assertIn("ConversationMessageValidator.validate(draft)", SCREEN)
        self.assertIn("enabled = messageValidation.isValid", SCREEN)
        self.assertIn("Message exceeds the 8,192 UTF-8 byte limit.", SCREEN)

    def test_unit_tests_cover_blank_exact_limit_over_limit_and_normalization(self):
        for check in (
            "rejectsEmptyAndWhitespaceOnlyContent",
            "acceptsExactlyTheUtf8ByteLimit",
            "rejectsOneEmojiBeyondTheUtf8ByteLimit",
            "trimsBeforeCountingAndReturningPersistedText",
        ):
            with self.subTest(check=check):
                self.assertIn(check, UNIT_TESTS)


if __name__ == "__main__":
    unittest.main()
