# T0042 evidence — accepted host layer separation

## Buildable production layers
- Domain: `src/Host.Domain/WhiteP.Host.Domain.csproj`, `HostLifecycle.cs`.
- Application: `src/Host.Application/WhiteP.Host.Application.csproj`, `IInferenceBackend.cs`, `HostStartup.cs`, `UnconfiguredInferenceBackend.cs`, `HostInferenceSession.cs`.
- Infrastructure: `src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj`, `LlamaCppServerBackend.cs`, `ProtocolMessageParser.cs`.
- UI: `src/Host.Windows/WhiteP.Host.Windows.csproj`, `App.xaml.cs`, `MainWindow.xaml.cs`.
- Shared contract generation: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` owns the single C# Protobuf generation from `protocol/whitep/v1/messages.proto`.
- Regression tests: `tests/test_host_layer_projects.py`, updated Python contract checks, and `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj` referencing the actual product projects.

## Exact-commit test evidence
- [Step Three quality gate run 39907751870](https://github.com/artinrahimii/project/actions/runs/39907751870): success.
- [White-P product build run 39907751958](https://github.com/artinrahimii/project/actions/runs/39907751958): success.
- Windows job: solution build, C# format, generated Protobuf C# verification, real WPF `--smoke-test`, artifact upload: passed.
- C# lifecycle/integration tests: passed.
- Android job: Kotlin formatting/lint, debug APK build, Protobuf round-trip tests, generated Java/Kotlin source verification and APK upload: passed.

The earlier failed compilation attempts are preserved in `step three/project-graph.md` and the execution report. This acceptance covers layer separation and CI build/runtime entry-point verification; it does not claim installed-model live inference or peer-to-peer Android transport is implemented.
