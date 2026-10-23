# T0042 evidence — host layer separation

## Source-level implementation
- Domain: `src/Host.Domain/WhiteP.Host.Domain.csproj`, `HostLifecycle.cs`.
- Application: `src/Host.Application/WhiteP.Host.Application.csproj`, `IInferenceBackend.cs`, `HostStartup.cs`, `UnconfiguredInferenceBackend.cs`, `HostInferenceSession.cs`.
- Infrastructure: `src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj`, `LlamaCppServerBackend.cs`, `ProtocolMessageParser.cs`.
- UI: `src/Host.Windows/WhiteP.Host.Windows.csproj`, `App.xaml.cs`, `MainWindow.xaml.cs`.
- Shared contracts: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` owns C# Protobuf generation from the canonical schema.

## Exact-commit evidence
- [Quality gate 39907571842](https://github.com/artinrahimii/project/actions/runs/39907571842): passed.
- The C# host lifecycle/integration job passed on commit `eae94f3f93ad437f94994c180f5c715a5ee6b083`.
- [Product build 39907571806](https://github.com/artinrahimii/project/actions/runs/39907571806): Windows build failed because of namespace/type ambiguity in `App.xaml.cs` and stale WPF copies of moved infrastructure classes. This was not an accepted build.
- Attempt A003 fixes those concrete failures; rerun all product gates before acceptance.
