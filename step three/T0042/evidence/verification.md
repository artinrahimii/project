# T0042 evidence — host layer separation

## Source-level implementation
- Domain: `src/Host.Domain/WhiteP.Host.Domain.csproj`, `HostLifecycle.cs`.
- Application: `src/Host.Application/WhiteP.Host.Application.csproj`, `IInferenceBackend.cs`, `HostStartup.cs`, `UnconfiguredInferenceBackend.cs`, `HostInferenceSession.cs`.
- Infrastructure: `src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj`, `LlamaCppServerBackend.cs`, `ProtocolMessageParser.cs`.
- UI: `src/Host.Windows/WhiteP.Host.Windows.csproj` and WPF entry-point/window code.
- Shared contracts: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` owns C# Protobuf generation from the canonical schema.
- Regression checks: `tests/test_host_layer_projects.py`, updated product/protocol tests, and `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.

## Runtime/build verification
Exact-commit GitHub Actions results are pending at the time of this report. The project files and tests themselves are not evidence of successful compilation. T0042 must remain in progress unless the Windows build/smoke, C# harness, Android workflow, and graph quality gate succeed for the same commit.
