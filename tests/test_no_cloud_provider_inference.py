"""Source-level guard for the local inference provider boundary.

This test rejects direct cloud-provider API endpoints and provider SDK usage in
first-party inference implementation files. It is intentionally not a runtime
network-capture test and does not claim to prove the absence of all outbound
traffic on a device.
"""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INFERENCE_PATHS = (
    ROOT / "src/Host.Windows/Inference/LlamaCppInferenceEngine.cs",
    ROOT / "src/Host.Infrastructure/LlamaCppServerBackend.cs",
)
CLOUD_ENDPOINT_PATTERNS = (
    re.compile(r"https?://[^\s\"'<>]*(?:api\.openai\.com|api\.anthropic\.com|generativelanguage\.googleapis\.com|api\.mistral\.ai|api\.cohere\.ai)", re.IGNORECASE),
    re.compile(r"\b(?:OpenAI|Anthropic|Google\.GenerativeAI|Mistral|Cohere)\s*\.?(?:Client|Api|SDK)\b", re.IGNORECASE),
)

class NoCloudProviderInferenceTests(unittest.TestCase):
    def test_local_inference_sources_use_only_the_configured_server_endpoint(self):
        for path in INFERENCE_PATHS:
            with self.subTest(path=path.relative_to(ROOT)):
                self.assertTrue(path.is_file(), f"Missing inference implementation: {path.relative_to(ROOT)}")
                source = path.read_text(encoding="utf-8")
                violations = []
                for pattern in CLOUD_ENDPOINT_PATTERNS:
                    violations.extend(match.group(0) for match in pattern.finditer(source))
                self.assertEqual(
                    violations,
                    [],
                    f"Cloud provider endpoint/SDK reference found in {path.relative_to(ROOT)}: {violations}",
                )

    def test_inference_engine_is_configured_with_a_host_supplied_server_uri(self):
        engine = INFERENCE_PATHS[0].read_text(encoding="utf-8")
        backend = INFERENCE_PATHS[1].read_text(encoding="utf-8")
        self.assertIn("Uri serverAddress", engine)
        self.assertIn("_serverAddress = EnsureTrailingSlash(serverAddress)", engine)
        self.assertIn("new LlamaCppServerBackend(_serverAddress, _configuredModelId, _http)", engine)
        self.assertIn('new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")', backend)
        self.assertIn('GetAsync("health"', backend)
        self.assertNotIn("api.openai.com", engine + backend)
        self.assertNotIn("api.anthropic.com", engine + backend)

if __name__ == "__main__":
    unittest.main()
