# T0045 verification evidence

- Production service: `src/Host.Infrastructure/LocalHostSettingsStore.cs`
- UI integration: `src/Host.Windows/MainWindow.xaml.cs`
- Executable regression harness: `tests/HostLifecycle.Tests/Program.cs`
- Required checks: default values; versioned save/load round-trip; endpoint loopback policy; no URL credentials; timeout range; malformed JSON rejection; unsupported schema rejection.
- CI result: pending on the commit that adds this implementation. Do not mark complete until the exact-commit product build and quality gate pass.

## Attempt A001
- C# lifecycle/settings test job: passed.
- Windows WPF build: failed with CS0246 due to missing `System.IO` import in `MainWindow.xaml.cs`.
- Corrective change: add `using System.IO;`; rerun the full product workflow before acceptance.

## Graph-gate bookkeeping
- V 3.0.135 quality gate found an attempt-count mismatch (summary 1, history 2).
- Corrected the count to 2 while preserving both A001 and A002 history entries. The gate must pass on a subsequent commit before task acceptance.


## Exact-commit verification — T0045-A003
- Tested `main` commit: `2a21259407681c793fb213b7ed4ada162a739220` (V 3.0.136).
- Step Three quality gate: passed, run 39913820033: https://github.com/artinrahimii/project/actions/runs/39913820033
- White-P product build: passed, run 39913820020: https://github.com/artinrahimii/project/actions/runs/39913820020
- The exact revision passed the settings/lifecycle harness, Windows build and startup smoke, Android lint/build/Protobuf tests, graph validation and evidence-path validation.
- No claim is made about live-model inference, hardware discovery, or P2P transport.
