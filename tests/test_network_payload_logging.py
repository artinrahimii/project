"""Regression guards for the product's allowlist-only diagnostic logging boundary."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOTS = (
    ROOT / "src/Host.Windows",
    ROOT / "src/Host.Infrastructure",
    ROOT / "src/Host.Application",
    ROOT / "src/Shared.Security",
    ROOT / "src/Client.Android/app/src/main/java",
)
CSHARP_LOG = ROOT / "src/Shared.Security/SafeDiagnosticLog.cs"
ANDROID_LOG = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/SafeDiagnosticLog.kt"
ANDROID_TRANSPORT = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt"
WINDOWS_TRANSPORT = ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs"

LOG_SINK = re.compile(
    r"\b(?:Console|Debug|Trace)\s*\.\s*Write(?:Line)?\s*\("
    r"|\b(?:_?logger|log)\s*\.\s*"
    r"(?:LogTrace|LogDebug|LogInformation|LogWarning|LogError|LogCritical|Trace|Debug|Information|Warning|Error|Critical)\s*\("
    r"|\bLog\s*\.\s*(?:v|d|i|w|e|wtf)\s*\(",
    re.IGNORECASE,
)
SENSITIVE_REFERENCE = re.compile(
    r"\b(?:request|response|frame|payload|outcome|exception|ex|endpoint|identity|key)\s*\.\s*"
    r"(?:Prompt|ContextCapsule|SerializedCapsule|OutputText|ErrorMessage|Message|RequestId|AttemptId|RoundId|ModelId|"
    r"EphemeralHostId|PeerId|PrivateKey|Secret|Token|Address)\b"
    r"|\b(?:Prompt|ContextCapsule|SerializedCapsule|OutputText|ErrorMessage|PrivateKey|SecretKey|AccessToken|"
    r"RefreshToken|password|apiKey)\b",
    re.IGNORECASE,
)


class NetworkPayloadLoggingTests(unittest.TestCase):
    def test_existing_product_log_sinks_do_not_reference_sensitive_payload_fields(self):
        inspected = 0
        for source_root in SOURCE_ROOTS:
            for pattern in ("*.cs", "*.kt", "*.java"):
                for path in source_root.rglob(pattern):
                    lines = path.read_text(encoding="utf-8").splitlines()
                    for index, line in enumerate(lines):
                        if not LOG_SINK.search(line):
                            continue
                        inspected += 1
                        context = "\n".join(lines[max(0, index - 3):min(len(lines), index + 8)])
                        self.assertIsNone(
                            SENSITIVE_REFERENCE.search(context),
                            f"Possible sensitive diagnostic disclosure at {path}:{index + 1}",
                        )
        self.assertGreater(inspected, 0, "The production log-sink audit did not inspect any sink.")

    def test_dotnet_diagnostic_api_accepts_only_numeric_allowlist_values(self):
        source = CSHARP_LOG.read_text(encoding="utf-8")
        signature = source.split("public static void Write(", 1)[1].split(")", 1)[0]
        self.assertIn("SafeDiagnosticEventCode eventCode", signature)
        self.assertIn("SafeDiagnosticOutcomeCode outcomeCode", signature)
        self.assertIn("long durationMilliseconds", signature)
        self.assertNotIn("string", signature)
        self.assertIn("MaximumRecordsPerProcess = 80", source)
        self.assertIn("new object?[] { eventCode, outcomeCode, durationMilliseconds }", source)

    def test_android_diagnostic_api_accepts_only_numeric_allowlist_values(self):
        source = ANDROID_LOG.read_text(encoding="utf-8")
        signature = source.split("internal fun formatRecord(", 1)[1].split("): String", 1)[0]
        self.assertIn("SafeDiagnosticEventCode", signature)
        self.assertIn("SafeDiagnosticOutcomeCode", signature)
        self.assertIn("durationMilliseconds: Long", signature)
        self.assertNotIn("String", signature)
        self.assertIn("MAXIMUM_RECORDS_PER_PROCESS = 80", source)
        self.assertIn("Log.i(TAG, formatRecord(eventCode, outcomeCode, durationMilliseconds))", source)

    def test_product_transports_emit_only_fixed_codes_and_elapsed_time(self):
        windows = WINDOWS_TRANSPORT.read_text(encoding="utf-8")
        android = ANDROID_TRANSPORT.read_text(encoding="utf-8")
        self.assertIn("SafeDiagnosticOutcomeCode.Started", windows)
        self.assertIn("SafeDiagnosticOutcomeCode.Succeeded", windows)
        self.assertIn("SafeDiagnosticOutcomeCode.Failed", windows)
        self.assertIn("SafeDiagnosticEventCode.PeerTransportConnection", windows)
        self.assertIn("SafeDiagnosticEventCode.INFERENCE_SUBMISSION", android)
        self.assertIn("SafeDiagnosticOutcomeCode.STARTED", android)
        self.assertIn("SafeDiagnosticOutcomeCode.SUCCEEDED", android)
        self.assertIn("SafeDiagnosticOutcomeCode.REJECTED", android)
        self.assertIn("SafeDiagnosticOutcomeCode.CANCELLED", android)
        for call in re.findall(r"SafeDiagnosticLog\.write\((.*?)\)", android, re.DOTALL):
            self.assertNotRegex(call, r"\b(request|response|peer|model|exception|payload)\b")


if __name__ == "__main__":
    unittest.main()
