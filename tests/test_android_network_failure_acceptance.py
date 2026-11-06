from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[1]
RENDEZVOUS = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt"
SELECTOR = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt"
DISPATCHER = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt"
RENDEZVOUS_JVM_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt"
DISPATCHER_JVM_TESTS = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt"


class AndroidNetworkFailureAcceptanceEvidenceTests(unittest.TestCase):
    def test_jvm_suite_contains_duplicate_and_expired_announcement_regressions(self):
        source = RENDEZVOUS_JVM_TESTS.read_text(encoding="utf-8")
        expected = (
            "rejectsDuplicatePeerIdsInOneDirectorySnapshot",
            "doesNotReturnCachedPeerWhenNetworkFailsAfterDirectoryCacheExpiry",
            "excludesExpiredPeersFromTheReturnedDirectory",
        )
        for test_name in expected:
            with self.subTest(test_name=test_name):
                self.assertIn(f"fun {test_name}(", source)

    def test_jvm_suite_covers_peer_disconnect_and_no_send_behavior(self):
        source = DISPATCHER_JVM_TESTS.read_text(encoding="utf-8")
        self.assertIn(
            "fun sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe(",
            source,
        )
        self.assertIn("InferenceRoundDispatchRejection.NO_SELECTED_HOSTS", source)
        self.assertIn("assertTrue(transport.requests.isEmpty())", source)

    def test_rendezvous_parser_rejects_duplicate_and_expired_peer_announcements(self):
        source = RENDEZVOUS.read_text(encoding="utf-8")
        self.assertIn("val seenPeerIds = HashSet<String>()", source)
        self.assertIn("require(seenPeerIds.add(peerId))", source)
        self.assertIn("if (!expiresAtUtc.isAfter(now))", source)

    def test_directory_cache_refresh_does_not_fall_back_to_stale_snapshot(self):
        source = RENDEZVOUS.read_text(encoding="utf-8")
        refresh = source.split("suspend fun resolve(", 1)[1].split("private fun fetchDirectory(", 1)[0]
        self.assertIn("if (!now.isBefore(cacheExpiresAt))", refresh)
        self.assertIn("val freshPeers =", refresh)
        self.assertIn("fetchDirectory(now)", refresh)
        self.assertIn("cachedPeers = freshPeers", refresh)
        self.assertLess(refresh.index("fetchDirectory(now)"), refresh.index("cachedPeers = freshPeers"))

    def test_authenticated_selection_fails_closed_and_dispatch_rejects_empty_hosts(self):
        selector = SELECTOR.read_text(encoding="utf-8")
        dispatcher = DISPATCHER.read_text(encoding="utf-8")
        self.assertIn("statusProbe(hostId)", selector)
        self.assertIn("catch (_: Exception)", selector)
        self.assertIn("InferenceRoundDispatchRejection.NO_SELECTED_HOSTS", dispatcher)
        self.assertIn("transport.submit(hostId, payloads[index])", dispatcher)


if __name__ == "__main__":
    unittest.main()
