import hashlib
import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
VERIFIER = ROOT / "src" / "Host.Windows" / "Inference" / "LocalInferenceBinaryVerifier.cs"
LAUNCHER = ROOT / "src" / "Host.Windows" / "Inference" / "LocalInferenceProcessLauncher.cs"

class LocalInferenceBinaryVerificationSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.verifier = VERIFIER.read_text(encoding="utf-8")
        cls.launcher = LAUNCHER.read_text(encoding="utf-8")

    def test_hash_must_match_before_version_probe(self):
        hash_check = self.verifier.index("CryptographicOperations.FixedTimeEquals(expectedHash, actualHash)")
        probe = self.verifier.index('startInfo.ArgumentList.Add("--version")')
        self.assertLess(hash_check, probe)
        self.assertIn("SHA256.HashData(stream)", self.verifier)

    def test_version_and_hash_are_rechecked_before_return(self):
        self.assertIn("output.Contains(expectedVersion.Trim()", self.verifier)
        self.assertIn("The llama.cpp executable changed during version verification", self.verifier)

    def test_probe_is_shell_free_and_time_bounded(self):
        self.assertIn("UseShellExecute = false", self.verifier)
        self.assertIn("WaitForExitAsync(timeoutSource.Token)", self.verifier)
        self.assertIn("process.Kill(entireProcessTree: true)", self.verifier)

    def test_launcher_fails_closed_before_server_start(self):
        verify = self.launcher.index("LocalInferenceBinaryVerifier.Verify")
        start = self.launcher.index("if (!process.Start())")
        self.assertLess(verify, start)
        self.assertIn("WHITEP_LLAMA_SERVER_VERSION", self.launcher)
        self.assertIn("WHITEP_LLAMA_SERVER_SHA256", self.launcher)

if __name__ == "__main__":
    unittest.main()
