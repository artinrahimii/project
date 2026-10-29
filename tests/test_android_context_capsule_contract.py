from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
MODEL = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/context/ConversationContextCapsule.kt"
UNIT_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleTest.kt"


class AndroidContextCapsuleContractTests(unittest.TestCase):
    def test_model_has_explicit_schema_and_required_context_sections(self):
        source = MODEL.read_text(encoding="utf-8")
        for field in ("schemaVersion", "goal", "keyFacts", "decisions", "constraints", "unresolvedItems"):
            self.assertIn(field, source)
        self.assertIn("const val CURRENT_SCHEMA_VERSION = 1", source)
        self.assertIn("UNSUPPORTED_SCHEMA_VERSION", source)

    def test_json_codec_checks_utf8_byte_ceiling_and_rejects_unknown_fields(self):
        source = MODEL.read_text(encoding="utf-8")
        self.assertIn("const val MAX_SERIALIZED_BYTES = 4 * 1024", source)
        self.assertIn("toByteArray(Charsets.UTF_8)", source)
        self.assertIn("bytes.size > maxSerializedBytes", source)
        self.assertIn("serialized.size > MAX_SERIALIZED_BYTES", source)
        self.assertIn("isValidConfiguredSizeLimit", source)
        self.assertIn("INVALID_SIZE_LIMIT", source)
        self.assertIn("CodingErrorAction.REPORT", source)
        self.assertIn("UNKNOWN_FIELD", source)
        self.assertIn("MISSING_FIELD", source)

    def test_android_json_tests_use_robolectric_runtime(self):
        source = UNIT_TEST.read_text(encoding="utf-8")
        self.assertIn("import org.junit.runner.RunWith", source)
        self.assertIn("import org.robolectric.RobolectricTestRunner", source)
        self.assertIn("@RunWith(RobolectricTestRunner::class)", source)
        self.assertIn("@Config(sdk = [35])", source)

    def test_android_unit_tests_cover_versioning_round_trip_and_bounds(self):
        source = UNIT_TEST.read_text(encoding="utf-8")
        for test in (
            "serializesAllVersionedFieldsAndRoundTripsJsonEscapes",
            "defaultsToSchemaVersionOneAndRejectsUnsupportedVersions",
            "acceptsSerializedPayloadAtExactlyTheFourKilobyteCeiling",
            "rejectsOversizedPayloadUsingUtf8EncodedByteCount",
            "rejectsMissingUnknownAndIncorrectlyTypedFields",
            "enforcesConfigurableSizeLimitBelowTheHardProtocolCeiling",
            "rejectsInvalidConfiguredSizeLimits",
        ):
            self.assertIn(test, source)


if __name__ == "__main__":
    unittest.main()
