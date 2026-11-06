"""Source-contract guard for the first-release relay privacy boundary.

These checks prevent a silent relay fallback. They are not runtime evidence that
a circuit-relay byte stream exists or that an arbitrary future relay is secure.
"""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ADR_DIRECT = (ROOT / "docs/decisions/ADR-0002-direct-p2p-data-path.md").read_text(encoding="utf-8")
ADR_RELAY = (ROOT / "docs/decisions/ADR-0004-encrypted-relay-boundary.md").read_text(encoding="utf-8")
DECISION_REGISTER = (ROOT / "docs/decisions/README.md").read_text(encoding="utf-8")
TRANSPORT = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
FALLBACK = (ROOT / "src/Host.Windows/Networking/PeerTransportFallbackConnector.cs").read_text(encoding="utf-8")
REQUIREMENTS = (ROOT / "docs/requirements.md").read_text(encoding="utf-8")


class RelayPrivacyBoundaryTests(unittest.TestCase):
    def test_first_release_relay_is_disabled_and_direct_exhaustion_fails_closed(self):
        self.assertNotIn(".WithRelay(", TRANSPORT)
        self.assertIn("PeerConnectionUnavailableException", FALLBACK)
        self.assertIn("throw new PeerConnectionUnavailableException(failures)", FALLBACK)
        direct = ADR_DIRECT.lower()
        self.assertIn("must fail with an explicit connection-unavailable outcome", direct)
        self.assertIn("must not silently fall back to a server-relayed inference payload", direct)

    def test_future_relay_must_forward_opaque_end_to_end_protected_bytes(self):
        relay = ADR_RELAY.lower()
        self.assertIn("opaque byte-forwarding path", relay)
        self.assertIn("end-to-end authentication and encryption", relay)
        self.assertIn("without terminating the endpoint security layer", relay)
        self.assertIn("three-peer integration test", relay)
        self.assertIn("control-plane reserve/connect status test by itself is insufficient", relay)

    def test_relay_metadata_visibility_is_explicit_and_not_promised_away(self):
        relay = ADR_RELAY.lower()
        for item in (
            "peer ids",
            "network addresses and ports",
            "connection start/end times and duration",
            "byte counts",
            "payload sizes",
            "timing/rate patterns",
            "does not hide this connection metadata or provide anonymity",
        ):
            self.assertIn(item, relay)
            
    def test_decision_is_registered_and_keeps_the_existing_privacy_requirement(self):
        self.assertIn("ADR-0004-encrypted-relay-boundary.md", DECISION_REGISTER)
        self.assertIn("REQ-010", ADR_RELAY)
        self.assertIn("REQ-011", ADR_RELAY)
        self.assertIn("No central inference API or conversation history", REQUIREMENTS)


if __name__ == "__main__":
    unittest.main()
