"""Keep README restore/build/test/package commands aligned with repository tooling and CI."""
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
README = (ROOT / "README.md").read_text(encoding="utf-8")
PRODUCT_WORKFLOW = (ROOT / ".github/workflows/product-build.yml").read_text(encoding="utf-8")
QUALITY_WORKFLOW = (ROOT / ".github/workflows/quality.yml").read_text(encoding="utf-8")


class ReadmeBuildCommandTests(unittest.TestCase):
    def test_dotnet_restore_build_lifecycle_test_and_publish_are_documented(self):
        for command in (
            "dotnet restore WhiteP.sln",
            "dotnet build WhiteP.sln --configuration Release",
            "dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release",
            "dotnet publish src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/windows-host",
        ):
            with self.subTest(command=command):
                self.assertIn(command, README)

    def test_android_lint_and_apk_package_commands_match_ci(self):
        for command in ("gradle --no-daemon ktlintCheck", "gradle --no-daemon assembleDebug"):
            with self.subTest(command=command):
                self.assertIn(command, README)
                self.assertIn(command, PRODUCT_WORKFLOW)
        self.assertIn("src/Client.Android/app/build/outputs/apk/debug/app-debug.apk", README)
        self.assertIn("path: src/Client.Android/app/build/outputs/apk/debug/app-debug.apk", PRODUCT_WORKFLOW)

    def test_python_checks_match_quality_workflow(self):
        for command in (
            "python -m unittest discover -s tests -v",
            "python -m compileall -q tools tests",
            'python tools/graph_guard.py "step three/project-graph.md"',
            'python tools/graph_guard.py "step three/project-graph.md" --repo-root .',
        ):
            with self.subTest(command=command):
                self.assertIn(command, README)
                self.assertIn(command, QUALITY_WORKFLOW)

    def test_toolchain_prerequisites_match_checked_in_configuration(self):
        self.assertIn('"version": "10.0.100"', (ROOT / "global.json").read_text(encoding="utf-8"))
        android = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
        self.assertIn("compileSdk = 36", android)
        self.assertIn("minSdk = 26", android)
        self.assertIn("JDK 17", README)
        self.assertIn("Gradle 8.13", README)


if __name__ == "__main__":
    unittest.main()
