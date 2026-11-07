"""Update-task transport/trust boundary checks for the implemented first-release path.

These are source-contract checks. Runtime HTTP/peer behavior is separately exercised
by the HostLifecycle acceptance executable; neither suite proves physical-device E2E
or third-party hosting-log retention.
"""
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]


def read(relative_path: str) -> str:
    return (ROOT / relative_path).read_text(encoding="utf-8")


class UpdateTransportTrustBoundaryTests(unittest.TestCase):
    def test_rendezvous_exposes_metadata_endpoints_only(self) -> None:
        service = read("src/Rendezvous.Service/PeerAnnouncementServiceHost.cs")
        contract = read("src/Shared.Contracts/Networking/PeerAnnouncement.cs")
        self.assertIn('MapPost("/v1/peers/announcements"', service)
        self.assertIn('MapGet("/v1/peers"', service)
        self.assertIn("JsonUnmappedMemberHandling.Disallow", service)
        self.assertIn("MaximumLifetime", contract)
        self.assertIn("MaximumMessageBytes", contract)
        self.assertNotIn('MapPost("/v1/inference"', service)
        self.assertNotIn('MapPost("/v1/conversations"', service)

    def test_directory_is_volatile_latest_record_with_expiry_cleanup(self) -> None:
        directory = read("src/Rendezvous.Service/PeerAnnouncementDirectory.cs")
        publisher = read("src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs")
        self.assertIn("Dictionary<string, PeerAnnouncement> _current", directory)
        self.assertIn("_current[announcement.PeerId] = announcement", directory)
        self.assertIn("RemoveExpired(now)", directory)
        self.assertIn("ExpiresAtUtc <= now", directory)
        self.assertIn("DefaultTimeToLive = TimeSpan.FromMinutes(5)", publisher)
        self.assertIn("DefaultRenewalInterval = TimeSpan.FromMinutes(1)", publisher)
        self.assertIn("Stop renewal", publisher)

    def test_rendezvous_clients_use_https_and_bounded_non_stale_metadata_cache(self) -> None:
        windows_client = read("src/Host.Windows/Networking/PeerAnnouncementClient.cs")
        android_client = read(
            "src/Client.Android/app/src/main/java/com/whitep/client/network/"
            "RendezvousPeerDirectoryClient.kt"
        )
        self.assertIn("Rendezvous requires HTTPS", windows_client)
        self.assertIn("Rendezvous requires HTTPS", android_client)
        self.assertIn('connection.requestMethod = "GET"', android_client)
        self.assertIn('connection.setRequestProperty("Cache-Control", "no-store")', android_client)
        self.assertIn("DEFAULT_CACHE_DURATION: Duration = Duration.ofSeconds(5)", android_client)
        self.assertIn("MAXIMUM_CACHE_DURATION: Duration = Duration.ofMinutes(1)", android_client)
        self.assertIn("stale entries are never returned as a fallback", android_client)

    def test_android_and_windows_data_paths_remain_direct_and_identity_pinned(self) -> None:
        android = read(
            "src/Client.Android/app/src/main/java/com/whitep/client/network/"
            "Libp2pInferenceRequestTransport.kt"
        )
        windows = read("src/Host.Windows/Networking/Libp2pPeerTransport.cs")
        fallback = read("src/Host.Windows/Networking/PeerTransportFallbackConnector.cs")
        relay_adr = read("docs/decisions/ADR-0004-encrypted-relay-boundary.md")
        self.assertIn("tcpMultiaddresses", android)
        self.assertIn("expectedPeerIdentity", android)
        self.assertIn("remotePeerId", android)
        self.assertIn(".WithQuic()", windows)
        self.assertIn("ConnectSecureAsync", fallback)
        self.assertIn("PeerConnectionUnavailableException", fallback)
        self.assertIn("same expected peer", fallback)
        self.assertNotIn(".WithRelay(", windows)
        self.assertIn("first release does not enable a relay data path", relay_adr.lower())

    def test_normal_room_chat_is_not_misrepresented_as_the_network_request_path(self) -> None:
        chat = read(
            "src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/"
            "ConversationMessagesViewModel.kt"
        )
        self.assertIn("does not send content to a processing host", chat)
        self.assertIn("messageDao.upsert(", chat)
        decision = read("docs/decisions/ADR-0002-direct-p2p-data-path.md")
        self.assertIn("ordinary Room conversation ViewModel remains local-message-only", decision)
        self.assertIn("AC-P2P-01 remains **NOT RUN**", decision)

    def test_data_boundary_decision_discloses_infrastructure_retention_uncertainty(self) -> None:
        decision = read("docs/decisions/ADR-0002-direct-p2p-data-path.md")
        self.assertIn("No durable application database or conversation-history store", decision)
        self.assertIn("not verified", decision.lower())
        self.assertIn("source IP", decision)
        self.assertIn("end-to-end encryption would not hide", read(
            "docs/decisions/ADR-0004-encrypted-relay-boundary.md"
        ).lower())


if __name__ == "__main__":
    unittest.main()
