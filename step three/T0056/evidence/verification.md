# T0056 verification evidence

## Source and dependency evidence

- Hardware discovery/UI source: `src/Host.Windows/WindowsHostHardwareDiscovery.cs`, `src/Host.Windows/MainWindow.xaml.cs`, `src/Host.Windows/MainWindow.xaml`.
- Tri-state capacity contract: `src/Host.Domain/HostCapabilities.cs`.
- Lifecycle guard: `src/Host.Application/HostInferenceSession.cs` and `src/Host.Domain/HostLifecycle.cs`.
- Regression contracts: `tests/test_windows_hardware_discovery.py`, `tests/test_wpf_entrypoint.py`, `tests/HostLifecycle.Tests/Program.cs`.

## Acceptance checks

- A visible WPF capacity status distinguishes Unknown, Insufficient and Ready.
- Unknown measurements or unvalidated requirements do not allow the UI to present lifecycle Ready or enable Generate.
- The Ready label is limited to the checks actually performed and explicitly discloses that GPU compute/VRAM has not been validated.
- The Windows startup smoke checks that the new status control is present.

## Exact-commit CI

Pending for implementation attempt T0056-A001. T0056 remains in progress until the exact implementation commit's quality and product workflows are inspected; no test result is inferred from source existence.
