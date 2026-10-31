"""Source-contract and documentation checks for the request-content endpoint boundary.

These tests verify that the shared schema/source and privacy docs agree. They do
not prove live Android-to-Windows routing, end-to-end delivery, or host storage behavior.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")
ACCEPTANCE = (ROOT / "docs/acceptance.md").read_text(encoding="utf-8")
ADR = (ROOT / "docs/decisions/ADR-0019-host-visible-inference-content.md").read_text(encoding="utf-8")
PROTO = (ROOT / "protocol/whitep/v1/messages.proto").read_text(encoding="utf-8")
TRANSPORT = (ROOT / "src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs").read_text(encoding="utf-8")
PEER_HOST = (ROOT / "src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs").read_text(encoding="utf-8")
WINDOW = (ROOT / "src/Host.Windows/MainWindow.xaml.cs").read_text(encoding="utf-8")
DIRECT_PATH = (ROOT / "docs/decisions/ADR-0002-direct-p2p-data-path.md").read_text(encoding="utf-8")


class HostContentVisibilityTests(unittest.TestCase):
    def test_canonical_request_contract_contains_prompt_and_context_capsule(self):
        self.assertIn("string prompt = 4;", PROTO)
        self.assertIn("bytes context_capsule = 7;", PROTO)

    def test_host_parses_request_and_passes_it_to_local_inference_session(self):
        self.assertIn("Transport encryption and peer authentication are provided by the", TRANSPORT)
        self.assertIn("ProtocolMessageParser.ParseInferenceRequest(requestWire)", PEER_HOST)
        self.assertIn("_requestHandler(requestWire, cancellationToken)", PEER_HOST)
        self.assertIn("ProtocolMessageParser.ParseInferenceRequest(wireBytes)", WINDOW)
        self.assertIn("_session.GenerateAsync(request, token)", WINDOW)

    def test_readme_requirements_and_adr_disclose_endpoint_visibility(self):
        for phrase in (
            "InferenceRequest.prompt",
            "InferenceRequest.context_capsule",
            "not end-to-end encryption against the processing host",
            "AC-P2P-01",
            "AC-PRIV-01",
        ):
            with self.subTest(phrase=phrase):
                self.assertIn(phrase, ADR)
        self.assertIn("میزبان پردازشگر به محتوای ورودی دسترسی دارد", README)
        self.assertIn("فرایند میزبان پس از رمزگشایی کانال ارتباطی می‌تواند", README)
        self.assertIn("processing host is an endpoint that can read", REQUIREMENTS.lower())
        self.assertIn("recipient host is a security endpoint", ACCEPTANCE)
        self.assertIn("not end-to-end encryption against that endpoint", ACCEPTANCE)
        self.assertIn("must not silently fall back to a server-relayed inference payload", DIRECT_PATH)

    def test_documented_endpoint_disclosure_does_not_claim_p2p_acceptance(self):
        self.assertIn("AC-P2P-01", ACCEPTANCE)
        self.assertIn("AC-PRIV-01", ACCEPTANCE)
        self.assertIn("NOT RUN", ACCEPTANCE)
        self.assertIn("NOT RUN", ADR)
        self.assertIn("remains unverified", ADR.lower())


if __name__ == "__main__":
    unittest.main()
