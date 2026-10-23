# T0049 verification evidence

Status: pending exact-commit GitHub Actions results.

Expected observable runtime checks on the Windows CI runner:
- Windows OS description/version are present.
- `GlobalMemoryStatusEx` successfully returns non-zero installed physical memory.
- The Windows directory resolves to a system-drive root and `DriveInfo` reports a ready drive with positive total capacity and an available capacity within the valid range.
- The production WPF startup smoke calls `WindowsHostHardwareDiscovery.Discover()` and exits non-zero if metrics are missing or inconsistent.

Additional quality checks:
- Python contract tests verify the implementation calls real Windows APIs and that the runtime smoke is wired into the actual entry point.
- The Windows Release build and C# lifecycle harness must pass; the existing Android APK/Protobuf workflow must also complete successfully.

No test is recorded as passed here until the workflow result for the implementation commit has been inspected.
