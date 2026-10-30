from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]
MODEL = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt"


class AndroidDistributedRequestContractTests(unittest.TestCase):
    def test_domain_model_exposes_ephemeral_request_id_deadline_round_limit_and_status(self):
        source = MODEL.read_text(encoding="utf-8")

        self.assertIn("data class DistributedRequest(", source)
        self.assertRegex(source, r"\bval\s+requestId\s*:\s*String")
        self.assertRegex(source, r"\bval\s+deadlineUnixMs\s*:\s*Long")
        self.assertRegex(source, r"\bval\s+maxRounds\s*:\s*Int")
        self.assertRegex(
            source,
            r"\bval\s+status\s*:\s*DistributedRequestStatus\s*=\s*DistributedRequestStatus\.QUEUED",
        )
        self.assertIn("require(requestId.isNotBlank())", source)
        self.assertIn("require(deadlineUnixMs > 0L)", source)
        self.assertIn("require(maxRounds > 0)", source)

    def test_factory_creates_a_new_uuid_backed_request_scoped_identifier(self):
        source = MODEL.read_text(encoding="utf-8")

        self.assertRegex(source, r"import\s+java\.util\.UUID")
        self.assertIn("fun create(", source)
        self.assertIn("requestId = UUID.randomUUID().toString()", source)
        self.assertRegex(
            source,
            r"requestId.{0,240}must not be reused as a conversation or user identifier",
        )

    def test_lifecycle_statuses_match_the_existing_request_state_vocabulary(self):
        source = MODEL.read_text(encoding="utf-8")
        match = re.search(
            r"enum class DistributedRequestStatus\s*\{([^}]*)\}",
            source,
            re.DOTALL,
        )
        self.assertIsNotNone(match, "DistributedRequestStatus must be defined")

        actual = re.findall(r"\b[A-Z][A-Z0-9_]*\b", match.group(1))
        self.assertEqual(
            ["QUEUED", "CONTACTING", "WAITING", "PARTIAL", "COMPLETED", "FAILED", "CANCELLED"],
            actual,
        )

    def test_jvm_suite_covers_ephemeral_id_deadline_round_limit_and_status_validation(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        expected_tests = (
            "preservesEphemeralIdDeadlineRoundLimitAndStatus",
            "createGeneratesDistinctUuidRequestIdsAndStartsQueued",
            "rejectsBlankEphemeralRequestId",
            "rejectsNonPositiveDeadline",
            "rejectsNonPositiveRoundLimit",
            "exposesTheSevenDefinedRequestLifecycleStates",
        )
        for test_name in expected_tests:
            with self.subTest(test_name=test_name):
                self.assertIn(f"fun {test_name}(", source)

        self.assertIn("UUID.fromString(first.requestId)", source)
        self.assertIn("DistributedRequestStatus.WAITING", source)
        self.assertIn("DistributedRequestStatus.QUEUED", source)


if __name__ == "__main__":
    unittest.main()
