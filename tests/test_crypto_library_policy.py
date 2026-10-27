"""Regression guards for the maintained libp2p cryptographic boundary."""
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HOST_PROJECT = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
SECURITY_PROJECT = (ROOT / "src/Shared.Security/WhiteP.Shared.Security.csproj").read_text(encoding="utf-8")
POLICY = (ROOT / "src/Shared.Security/PeerSecurityPolicy.cs").read_text(encoding="utf-8")
TRANSPORT = (ROOT / "src/Host.Windows/Networking/Libp2pPeerTransport.cs").read_text(encoding="utf-8")
MESSAGE_PROTOCOL = (ROOT / "src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs").read_text(encoding="utf-8")
ADR = (ROOT / "docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md").read_text(encoding="utf-8")


class CryptoLibraryPolicyTests(unittest.TestCase):
    def test_host_uses_pinned_maintained_libp2p_stack_and_shared_security_policy(self):
        self.assertRegex(HOST_PROJECT, r'<PackageReference Include="Nethermind\.Libp2p" Version="1\.1\.0"')
        self.assertIn("WhiteP.Shared.Security.csproj", HOST_PROJECT)
        self.assertRegex(SECURITY_PROJECT, r'<PackageReference Include="Nethermind\.Libp2p\.Core" Version="1\.1\.0"')
        self.assertIn("PeerSecurityPolicy.ValidatePeerIdentity(identity);", TRANSPORT)
        self.assertIn("Noise/TLS", MESSAGE_PROTOCOL)

    def test_shared_identity_policy_delegates_parsing_and_contains_no_custom_crypto_primitive(self):
        self.assertIn("new PeerId(identity)", POLICY)
        self.assertIn("ArgumentException or FormatException or IndexOutOfRangeException", POLICY)
        for forbidden in (
            "Aes.Create(",
            "RSA.Create(",
            "ECDiffieHellman.Create(",
            "HMACSHA",
            "SHA256.HashData(",
            "ChaCha20Poly1305(",
        ):
            self.assertNotIn(forbidden, POLICY)

    def test_decision_discloses_boundary_and_unverified_runtime_gates(self):
        self.assertIn("ADR-0016", ADR)
        self.assertIn("not an independent cryptographic audit", ADR.lower())
        self.assertIn("AC-P2P-01", ADR)
        self.assertIn("NOT RUN", ADR)


if __name__ == "__main__":
    unittest.main()
