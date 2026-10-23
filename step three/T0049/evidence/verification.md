# T0049 verification evidence

## Attempt A001 / V 3.0.150 — failed build
- [Step Three quality gate run 39939563867](https://github.com/artinrahimii/project/actions/runs/39939563867): passed.
- [Host lifecycle job 120367370545](https://github.com/artinrahimii/project/actions/runs/39939563803/job/120367370545): passed.
- [Windows WPF build job 120367370825](https://github.com/artinrahimii/project/actions/runs/39939563803/job/120367370825): failed before runtime smoke with `SYSLIB1062` / `CS0227`, because the `LibraryImport` source generator required unsafe compilation.
- Corrective action: use conventional `DllImport` for the Windows native API while leaving unsafe compilation disabled.

## Attempt A003 — pending
Required checks on the corrected implementation commit:
1. Python quality gate and graph/evidence validation.
2. Windows WPF Release build and startup smoke that calls `WindowsHostHardwareDiscovery.Discover()` on the Windows runner and rejects absent or inconsistent OS/RAM/disk values.
3. C# lifecycle harness and Android APK/Protobuf workflow.

No result is marked passed for the corrected declaration until the new exact-commit workflow completes.
