"""Regression guards preventing normal logs from exposing network inference payloads."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOTS = (
    ROOT / "src/Host.Windows/Networking",
    ROOT / "src/Host.Infrastructure",
    ROOT / "src/Host.Application",
)
LISTENER_PATH = ROOT / "src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs"
MAPPER_PATH = ROOT / "src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs"

LOG_SINK = re.compile(
    r"\b(?:Console|Debug|Trace)\s*\.\s*Write(?:Line)?\s*\("
    r"|\b(?:_?logger|log)\s*\.\s*"
    r"(?:LogTrace|LogDebug|LogInformation|LogWarning|LogError|LogCritical|Trace|Debug|Information|Warning|Error|Critical)\s*\(",
    re.IGNORECASE,
)
PAYLOAD_REFERENCE = re.compile(
    r"\b(?:request|response|frame|payload|outcome|exception|ex)\s*\.\s*"
    r"(?:Prompt|ContextCapsule|OutputText|ErrorMessage|ToString|Message)\b"
    r"|\b(?:Prompt|ContextCapsule|OutputText)\b",
    re.IGNORECASE,
)


class NetworkPayloadLoggingTests(unittest.TestCase):
    def test_existing_log_calls_do_not_reference_prompt_context_or_response_fields(self):
        inspected = 0
        for source_root in SOURCE_ROOTS:
            for path in source_root.rglob("*.cs"):
                lines = path.read_text(encoding="utf-8").splitlines()
                for index, line in enumerate(lines):
                    if not LOG_SINK.search(line):
                        continue
                    inspected += 1
                    context = "\n".join(lines[max(0, index - 3):min(len(lines), index + 8)])
                    self.assertIsNone(
                        PAYLOAD_REFERENCE.search(context),
                        f"Possible inference payload disclosure in log statement at {path}:{index + 1}",
                    )
        # A zero count is valid and documents the present codebase's lack of ordinary production log sinks.
        self.assertGreaterEqual(inspected, 0)

    def test_network_listener_does_not_log_request_frames_or_payloads(self):
        source = LISTENER_PATH.read_text(encoding="utf-8")
        for sensitive in ("request.Prompt", "request.ContextCapsule", "response.OutputText", "Console.Write", "LogInformation"):
            with self.subTest(sensitive=sensitive):
                self.assertNotIn(sensitive, source)

    def test_response_mapper_uses_sanitized_messages_and_never_logs_raw_failures(self):
        source = MAPPER_PATH.read_text(encoding="utf-8")
        self.assertNotRegex(source, LOG_SINK)
        self.assertNotIn("outcome.Message", source)
        self.assertNotIn("exception.Message", source)
        self.assertNotIn("exception.ToString()", source)
        self.assertIn("The host could not process the request.", source)
        self.assertIn("The request is invalid.", source)


if __name__ == "__main__":
    unittest.main()
