from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
CLIENT = ROOT / "src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt"
MANIFEST = ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml"
JVM_TEST = ROOT / "src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt"


class AndroidRendezvousDirectoryContractTests(unittest.TestCase):
    def test_directory_client_is_metadata_only_and_uses_bounded_https_control_plane(self):
        source = CLIENT.read_text(encoding="utf-8")

        self.assertIn("/v1/peers", source)
        self.assertIn('"capability=inference.v1"', source)
        self.assertIn('scheme == "https"', source)
        self.assertIn("instanceFollowRedirects = false", source)
        self.assertIn("MAXIMUM_DIRECTORY_BYTES = 16 * 1024", source)
        self.assertIn("REQUEST_TIMEOUT_MILLIS = 10_000", source)
        self.assertIn("tcpMultiaddresses", source)
        self.assertIn("addressPeerId == peerId", source)
        self.assertNotIn("InferenceRequest", source)
        self.assertNotIn("InferenceResponse", source)
        self.assertNotIn("currentMessage", source)
        self.assertNotIn("outputText", source)

    def test_android_manifest_declares_internet_access(self):
        manifest = MANIFEST.read_text(encoding="utf-8")
        self.assertIn('android.permission.INTERNET', manifest)

    def test_jvm_tests_cover_directory_metadata_validation_and_cache(self):
        source = JVM_TEST.read_text(encoding="utf-8")
        self.assertIn("cachesOneDirectoryFetchForSeveralHostLookupsInTheSameRound", source)
        self.assertIn("rejectsAnAddressThatClaimsAnotherPeerIdentity", source)
        self.assertIn("rejectsPublicHttpAndAllowsLoopbackOnlyHttp", source)
        self.assertIn("excludesExpiredPeersFromTheReturnedDirectory", source)


if __name__ == "__main__":
    unittest.main()
