"""Contract checks for documented clean-checkout build and test entrypoints.

The CI workflow performs the real builds; these tests keep its commands,
toolchain policy, README instructions and artifact paths aligned.
"""
import json
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")
PRODUCT = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
QUALITY = (ROOT / ".github/workflows/quality.yml").read_text(encoding="utf-8")
GLOBAL = json.loads((ROOT / "global.json").read_text(encoding="utf-8"))


class ReproducibleBuildContractTests(unittest.TestCase):
    def test_windows_build_and_lifecycle_commands_match_documentation_and_ci(self):
        build = "dotnet build WhiteP.sln --configuration Release"
        lifecycle = "dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release"
        self.assertIn(build, README)
        self.assertIn(build, PRODUCT)
        self.assertIn(lifecycle, README)
        self.assertIn(lifecycle, PRODUCT)
        self.assertEqual(GLOBAL["sdk"]["version"], "10.0.100")
        self.assertEqual(GLOBAL["sdk"]["rollForward"], "latestFeature")
        self.assertIn("dotnet-version: 10.0.x", PRODUCT)

    def test_android_build_command_and_sdk_baseline_match(self):
        command = "gradle --no-daemon assembleDebug"
        self.assertIn(command, README)
        self.assertIn(command, PRODUCT)
        self.assertIn("java-version: '17'", PRODUCT)
        self.assertIn("gradle-version: '8.13'", PRODUCT)
        self.assertIn('"platforms;android-36"', PRODUCT)
        self.assertIn('"build-tools;36.0.0"', PRODUCT)
        gradle = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
        self.assertIn("compileSdk = 36", gradle)
        self.assertIn("minSdk = 26", gradle)

    def test_product_builds_publish_real_artifacts_and_fail_if_missing(self):
        self.assertEqual(PRODUCT.count("uses: actions/upload-artifact@v4"), 2)
        self.assertIn("path: src/Host.Windows/bin/Release/net10.0-windows/", PRODUCT)
        self.assertIn("path: src/Client.Android/app/build/outputs/apk/debug/app-debug.apk", PRODUCT)
        self.assertEqual(PRODUCT.count("if-no-files-found: error"), 2)
        self.assertEqual(PRODUCT.count("retention-days: 7"), 2)

    def test_python_quality_commands_are_documented_and_executed_in_ci(self):
        for command in (
            "python -m unittest discover -s tests -v",
            "python -m compileall -q tools tests",
            'python tools/graph_guard.py "step three/project-graph.md"',
        ):
            with self.subTest(command=command):
                self.assertIn(command, README)
                self.assertIn(command, QUALITY)
        self.assertIn("python-version: '3.13'", QUALITY)


if __name__ == "__main__":
    unittest.main()
