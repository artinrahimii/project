# T0046 verification evidence

- `src/Host.Application/HostStatusPresentation.cs`: maps all five host states to backend and local endpoint status.
- `src/Host.Windows/MainWindow.xaml` and `MainWindow.xaml.cs`: accessible status line wired to actual `HostStatus` and refreshed during connect/generate.
- `tests/HostLifecycle.Tests/Program.cs`: assertions cover Stopped, Starting, Ready, Busy, Error, and explicit P2P-unavailable disclosure.
- `tests/test_wpf_entrypoint.py`: checks live status wiring and the explicit transport limitation.
- Exact-main quality gate passed: https://github.com/artinrahimii/project/actions/runs/39923551335
- Exact-main product build, WPF startup smoke, lifecycle tests, Android APK and Protobuf round-trip checks passed: https://github.com/artinrahimii/project/actions/runs/39923551318

Limit: status reflects the latest observed local backend state; it is not continuous polling. P2P transport is not implemented.
