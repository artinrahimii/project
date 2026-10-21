"""Static contract checks for reproducible Windows and Android product builds.

These tests verify repository configuration only. They do not replace CI builds,
runtime/device testing, local inference, or P2P transport validation.
"""
import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class ProductBuildContractTests(unittest.TestCase):
    def test_windows_host_targets_wpf_on_dotnet_10(self):
        project = (ROOT / "src/Host.Windows/WhiteP.Host.Windows.csproj").read_text(encoding="utf-8")
        sdk = json.loads((ROOT / "global.json").read_text(encoding="utf-8"))
        self.assertEqual(sdk["sdk"]["version"], "10.0.100")
        self.assertIn("<TargetFramework>net10.0-windows</TargetFramework>", project)
        self.assertIn("<UseWPF>true</UseWPF>", project)

    def test_android_sdk_levels_and_gradle_entrypoint_are_explicit(self):
        gradle = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
        self.assertIn("compileSdk = 36", gradle)
        self.assertIn("minSdk = 26", gradle)
        self.assertIn("gradle --no-daemon assembleDebug", (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8"))

    def test_ci_builds_both_platforms_and_installs_android_sdk(self):
        workflow = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
        self.assertIn("dotnet build src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release", workflow)
        self.assertIn('"platforms;android-36"', workflow)
        self.assertIn('"build-tools;36.0.0"', workflow)
        self.assertIn("Build Windows WPF host", workflow)
        self.assertIn("Build Android client APK", workflow)


if __name__ == "__main__":
    unittest.main()
