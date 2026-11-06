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
ANDROID_GRADLE = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
ANDROID_MANIFEST = (ROOT / "src/Client.Android/app/src/main/AndroidManifest.xml").read_text(encoding="utf-8")


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
        self.assertIn("dotnet restore WhiteP.sln", PRODUCT)
        publish = "dotnet publish src/Host.Windows/WhiteP.Host.Windows.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/windows-host"
        self.assertIn(publish, README)
        self.assertIn(publish, PRODUCT)

    def test_android_build_command_and_sdk_baseline_match(self):
        command = "gradle --no-daemon assembleDebug"
        self.assertIn(command, README)
        self.assertIn(command, PRODUCT)
        self.assertIn("java-version: '21'", PRODUCT)
        self.assertIn("JDK 21", README)
        self.assertIn("gradle-version: '8.13'", PRODUCT)
        self.assertIn('"platforms;android-36"', PRODUCT)
        self.assertIn('"build-tools;36.0.0"', PRODUCT)
        gradle = (ROOT / "src/Client.Android/app/build.gradle.kts").read_text(encoding="utf-8")
        self.assertIn("compileSdk = 36", gradle)
        self.assertIn("minSdk = 26", gradle)

    def test_product_builds_publish_real_artifacts_and_fail_if_missing(self):
        self.assertEqual(PRODUCT.count("uses: actions/upload-artifact@v4"), 8)
        self.assertIn("whitep-android-keystore-instrumentation-reports", PRODUCT)
        self.assertIn("path: src/Host.Windows/bin/Release/net10.0-windows/", PRODUCT)
        self.assertIn("path: src/Client.Android/app/build/outputs/apk/debug/app-debug.apk", PRODUCT)
        self.assertEqual(PRODUCT.count("if-no-files-found: error"), 6)
        self.assertEqual(PRODUCT.count("retention-days: 7"), 6)
        self.assertIn("whitep-windows-installer", PRODUCT)
        self.assertIn("whitep-windows-publish", PRODUCT)
        self.assertIn("artifacts/windows-host/", PRODUCT)
        self.assertIn("artifacts/releases/WhiteP-Setup-0.1.0-win-x64.exe", PRODUCT)
        self.assertIn("retention-days: 30", PRODUCT)
        self.assertIn("tools/build_windows_installer.ps1", PRODUCT)
        self.assertIn("name: whitep-android-unit-test-reports", PRODUCT)
        self.assertIn("if: always()", PRODUCT)
        self.assertIn("src/Client.Android/app/build/reports/tests/testDebugUnitTest/", PRODUCT)
        self.assertIn("if-no-files-found: ignore", PRODUCT)
        self.assertIn("whitep-android-coverage-and-critical-results", PRODUCT)
        self.assertIn("whitep-host-lifecycle-coverage-and-critical-results", PRODUCT)
        self.assertIn("createDebugUnitTestCoverageReport", PRODUCT)
        self.assertIn("createDebugAndroidTestCoverageReport", PRODUCT)
        self.assertIn("dotnet-coverage collect", PRODUCT)

    def test_windows_installer_verifies_publisher_from_isolated_uninstall_registration(self):
        script = (ROOT / "tools/build_windows_installer.ps1").read_text(encoding="utf-8")
        installer = (ROOT / "tools/installer/WhiteP.iss").read_text(encoding="utf-8")
        self.assertIn('AppPublisher="{#Publisher}"', installer)
        self.assertIn('VersionInfoCompany="{#Publisher}"', installer)
        self.assertIn("Get-ItemProperty -Path", script)
        self.assertIn("HKCU:", script)
        self.assertIn('$_.DisplayName -ne "White-P"', script)
        self.assertIn("$_.InstallLocation", script)
        self.assertIn("$publisherRegistrations[0].Publisher", script)
        self.assertIn("Installed White-P publisher registration mismatch", script)
        self.assertIn("remainingPublisherRegistrations.Count -ne 0", script)
        self.assertIn("Installer PE CompanyName metadata", script)

    def test_android_emulator_builds_installs_and_launches_real_main_activity(self):
        self.assertIn('applicationId = "com.whitep.client"', ANDROID_GRADLE)
        self.assertIn('android:name=".MainActivity"', ANDROID_MANIFEST)
        self.assertIn("connectedDebugAndroidTest createDebugAndroidTestCoverageReport", PRODUCT)
        instrumentation_step = PRODUCT.split(
            "- name: Run Android Keystore instrumentation tests", 1
        )[1].split("- name: Upload Android Keystore instrumentation reports", 1)[0]
        self.assertIn("script: |", instrumentation_step)
        self.assertIn("set -eu", instrumentation_step)
        self.assertNotIn("pipefail", instrumentation_step)
        self.assertIn("adb shell am force-stop com.whitep.client", PRODUCT)
        self.assertIn("adb shell pm path com.whitep.client | grep -F 'package:'", PRODUCT)
        reinstall = "adb install -r src/Client.Android/app/build/outputs/apk/debug/app-debug.apk"
        self.assertIn(reinstall, instrumentation_step)
        self.assertLess(
            instrumentation_step.index("connectedDebugAndroidTest createDebugAndroidTestCoverageReport"),
            instrumentation_step.index(reinstall),
        )
        self.assertLess(
            instrumentation_step.index(reinstall),
            instrumentation_step.index("adb shell pm path com.whitep.client | grep -F 'package:'"),
        )
        component_file = "/tmp/whitep-launcher-component.txt"
        resolve_launcher = (
            "adb shell cmd package resolve-activity --brief -a android.intent.action.MAIN "
            "-c android.intent.category.LAUNCHER com.whitep.client | tr -d '\\r' | "
            "grep -E '^com[.]whitep[.]client/.*MainActivity$' | tee " + component_file
        )
        self.assertIn(resolve_launcher, instrumentation_step)
        self.assertIn(f"test -s {component_file}", instrumentation_step)
        launch_resolved = f'adb shell am start -n "$(cat {component_file})"'
        self.assertIn(launch_resolved, instrumentation_step)
        self.assertNotIn("launch_activity=", instrumentation_step)
        self.assertNotIn('case "$launch_activity" in', instrumentation_step)
        self.assertNotIn("adb shell am start -n com.whitep.client/.MainActivity", PRODUCT)
        self.assertNotIn("adb shell am start -W", PRODUCT)
        self.assertEqual(instrumentation_step.count('while [ "$i" -lt 30 ]'), 2)
        self.assertIn("app_pid=", instrumentation_step)
        self.assertIn("adb shell pidof com.whitep.client", instrumentation_step)
        self.assertIn("if [ -z", instrumentation_step)
        self.assertIn("resumed_activity=", instrumentation_step)
        self.assertIn("adb shell dumpsys activity activities", instrumentation_step)
        self.assertEqual(instrumentation_step.count("adb logcat -d -t 300"), 2)
        self.assertIn("MainActivity process did not appear within 30 seconds", instrumentation_step)
        self.assertIn("MainActivity was not resumed within 30 seconds", instrumentation_step)
        self.assertNotIn("adb shell pidof com.whitep.client | grep", PRODUCT)
        self.assertLess(instrumentation_step.index(launch_resolved), instrumentation_step.index("app_pid="))
        self.assertLess(instrumentation_step.index("app_pid="), instrumentation_step.index("resumed_activity="))

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

    def test_sample_model_license_notice_and_full_license_text_are_in_installer(self):
        script = (ROOT / "tools/build_windows_installer.ps1").read_text(encoding="utf-8")
        generator = (ROOT / "tools/generate_model_license_notice.py").read_text(encoding="utf-8")
        installer = (ROOT / "tools/installer/WhiteP.iss").read_text(encoding="utf-8")
        self.assertIn("Test sample model license notice", PRODUCT)
        self.assertIn("test_generate_model_license_notice.py", PRODUCT)
        self.assertIn("sample-model-catalog.json", script)
        self.assertIn("generate_model_license_notice.py", script)
        self.assertIn("https://www.apache.org/licenses/LICENSE-2.0.txt", script)
        self.assertIn("ModelLicenses.md", script)
        self.assertIn("WhiteP-Sample-Model-Apache-2.0.txt", script)
        self.assertIn("WhiteP-Sample-Model-Apache-2.0.txt", generator)
        self.assertIn("LicensesDir", installer)
        self.assertIn("licenses", installer)


if __name__ == "__main__":
    unittest.main()
