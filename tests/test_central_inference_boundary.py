"""Prevent central prompt/response/history route handlers in first-party source.

This is a narrow source-level guard. It excludes the Windows host because the
host is the intended local inference endpoint; it does not ban direct P2P
transport routes or prove what future network traffic contains.
"""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "src"
SOURCE_SUFFIXES = {".cs", ".kt", ".java", ".js", ".jsx", ".ts", ".tsx", ".py", ".go", ".rs"}
CENTRAL_ROUTE_HANDLER_PATTERNS = (
    re.compile(
        r'''\bMap(?:Post|Put|Patch)\s*\(\s*["'][^"']*(?:^|[/_-])(?:inference|prompt|prompts|chat|completion|completions|response|responses|conversation|conversations|history)(?:[/_-]|$)[^"']*["']''',
        re.IGNORECASE,
    ),
    re.compile(
        r'''\[(?:HttpPost|HttpPut|HttpPatch)\s*\(\s*["'][^"']*(?:^|[/_-])(?:inference|prompt|prompts|chat|completion|completions|response|responses|conversation|conversations|history)(?:[/_-]|$)[^"']*["']''',
        re.IGNORECASE,
    ),
    re.compile(
        r'''@(?:app|router)\.(?:post|put|patch)\s*\(\s*["'][^"']*(?:^|[/_-])(?:inference|prompt|prompts|chat|completion|completions|response|responses|conversation|conversations|history)(?:[/_-]|$)[^"']*["']''',
        re.IGNORECASE,
    ),
)

def first_party_non_host_sources():
    """Return source files outside Host.Windows; local host APIs are allowed."""
    if not SRC.exists():
        return []
    return [
        path for path in SRC.rglob("*")
        if path.is_file()
        and path.suffix.lower() in SOURCE_SUFFIXES
        and "Host.Windows" not in path.relative_to(SRC).parts
    ]


class CentralInferenceBoundaryTests(unittest.TestCase):
    def test_no_central_inference_or_conversation_route_handlers_in_non_host_source(self):
        violations = []
        for path in first_party_non_host_sources():
            try:
                source = path.read_text(encoding="utf-8")
            except (OSError, UnicodeError) as exc:
                self.fail(f"Cannot inspect owned source file {path.relative_to(ROOT)}: {exc}")
            for pattern in CENTRAL_ROUTE_HANDLER_PATTERNS:
                for match in pattern.finditer(source):
                    line = source.count("\n", 0, match.start()) + 1
                    violations.append(f"{path.relative_to(ROOT)}:{line}")
        self.assertEqual(
            violations,
            [],
            "Central prompt/inference/chat/response/conversation/history handler "
            "declaration found outside the local Windows host: " + ", ".join(violations),
        )

    def test_current_android_client_does_not_claim_prompt_delivery(self):
        client = (SRC / "Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt").read_text(encoding="utf-8")
        self.assertIn("Requests are not sent yet; transport is not implemented.", client)
        self.assertIn("Request not sent: no host transport yet", client)

    def test_requirement_requires_network_evidence_beyond_source_search(self):
        requirements = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
        self.assertIn("No central inference API or conversation history", requirements)
        self.assertIn("source-code search alone is insufficient", requirements)


if __name__ == "__main__":
    unittest.main()
