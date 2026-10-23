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
