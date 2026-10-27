"""Regression checks for persistent host identity and Windows DPAPI integration."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STORE = (ROOT / "src/Shared.Security/HostIdentityStore.cs").read_text(encoding="utf-8")
DPAPI = (ROOT / "src/Host.Windows/Security/WindowsDpapiSecretProtector.cs").read_text(encoding="utf-8")
PROVIDER = (ROOT / "src/Host.Windows/Security/WindowsHostIdentityProvider.cs").read_text(encoding="utf-8")
TEST_PROJECT = (ROOT / "tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj").read_text(encoding="utf-8")
PROGRAM = (ROOT / "tests/HostLifecycle.Tests/Program.cs").read_text(encoding="utf-8")
WORKFLOW = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
PORTABLE_TEST = (ROOT / "tests/HostLifecycle.Tests/HostIdentityStoreAcceptanceTests.cs").read_text(encoding="utf-8")
WINDOWS_TEST = (ROOT / "tests/HostLifecycle.Tests/WindowsHostIdentityStoreAcceptanceTests.cs").read_text(encoding="utf-8")
ADR = (ROOT / "docs/decisions/ADR-0017-windows-user-scoped-host-identity-storage.md").read_text(encoding="utf-8")


class HostIdentityStoreTests(unittest.TestCase):
    def test_private_key_is_protected_before_atomic_persistence_and_plaintext_buffers_are_cleared(self):
        self.assertIn("_protector.Protect(serializedPrivateKey)", STORE)
        self.assertIn("File.Move(temporaryPath, _filePath)", STORE)
        self.assertIn("FileOptions.WriteThrough", STORE)
        self.assertIn("CryptographicOperations.ZeroMemory(serializedPrivateKey)", STORE)
        self.assertIn("Only a valid, non-empty Ed25519 host identity", STORE)

    def test_production_provider_uses_windows_current_user_dpapi_and_local_appdata(self):
        self.assertIn("ProtectedData.Protect", DPAPI)
        self.assertIn("ProtectedData.Unprotect", DPAPI)
        self.assertIn("DataProtectionScope.CurrentUser", DPAPI)
        self.assertIn("OperatingSystem.IsWindows()", DPAPI)
        self.assertIn("Environment.SpecialFolder.LocalApplicationData", PROVIDER)
        self.assertIn("HostIdentityStore(path, new WindowsDpapiSecretProtector())", PROVIDER)
        self.assertIn('PackageReference Include="System.Security.Cryptography.ProtectedData" Version="10.0.14"', TEST_PROJECT)

    def test_real_dpapi_test_is_registered_in_windows_product_job(self):
        self.assertIn("--test-host-identity-store", PROGRAM)
        self.assertIn("--test-host-identity-store", WORKFLOW)
        self.assertIn("Concurrent first-run callers must observe a single persisted PeerId", PORTABLE_TEST)
        self.assertIn("Windows current-user DPAPI preserves the host PeerId", WINDOWS_TEST)
        self.assertIn("same user", ADR.lower())
        self.assertIn("tamper", ADR.lower())


if __name__ == "__main__":
    unittest.main()
