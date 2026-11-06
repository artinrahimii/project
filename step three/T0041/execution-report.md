# T0041 — execution report

## Dependency review
- `T0040 → T0041`: reviewed and classified as `sequencing_only`. The WPF entry point uses the existing `StartupUri` / `MainWindow` path and does not call the `ProtocolMessageParser` interface. The existing edge is retained as conservative sequence/full-host build gating; it is not claimed to be a direct message-validation API dependency.
- The review uses `src/Host.Windows/App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, product CI configuration and the T0040 task report. No unrelated graph edge was changed.

## Attempt T0041-A001 — runtime smoke implementation in progress
- Confirmed the actual WPF executable is a `WinExe` targeting `net10.0-windows` with `UseWPF=true`; `App.xaml` uses `StartupUri="MainWindow.xaml"`. The existing UI includes server URL/model controls, lifecycle status, prompt/response fields and buttons, with session cleanup on close.
- Implemented `--smoke-test` in `src/Host.Windows/App.xaml.cs`. It allows normal startup through the real `StartupUri`, waits at dispatcher application-idle priority, checks the main window has loaded and is visible, validates required named controls and initial status, then closes cleanly with exit code 0. Missing controls or exceptions return non-zero.
- Wired a bounded 15-second process-level smoke test into the Windows job in `.github/workflows/product-build.yml`; timeout, missing executable or nonzero process exit causes CI to fail.
- Added `tests/test_wpf_entrypoint.py` to guard the startup XAML, required controls, cleanup, smoke-mode contract and Windows CI invocation.
- Exact-commit tests and the real Windows smoke execution are pending. T0041 remains `in_progress` until that process-level check passes.


## Acceptance verification — T0041-A001
- Verified against the exact `main` head `d5fe343a96c524625ee650792f3548330152ae7f` (`V 3.0.123`).
- Step Three quality gate run [39862296293](https://github.com/artinrahimii/project/actions/runs/39862296293): **passed**.
- White-P product build run [39862296386](https://github.com/artinrahimii/project/actions/runs/39862296386): **passed**. This workflow runs the Windows WPF build and `--smoke-test` process, the C# lifecycle/validation harness, and the Android build/unit tests.
- The smoke test and both workflows passed on the same commit; T0041 acceptance is verified. This does not prove live installed-model inference or Android-to-host/P2P transport.
