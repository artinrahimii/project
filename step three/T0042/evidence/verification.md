# T0042 evidence — host layer separation

## Source-level implementation
- Domain: `src/Host.Domain/WhiteP.Host.Domain.csproj`, `HostLifecycle.cs`.
- Application: `src/Host.Application/WhiteP.Host.Application.csproj`, `IInferenceBackend.cs`, `HostStartup.cs`, `UnconfiguredInferenceBackend.cs`, `HostInferenceSession.cs`.
- Infrastructure: `src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj`, `LlamaCppServerBackend.cs`, `ProtocolMessageParser.cs`.
- UI: `src/Host.Windows/WhiteP.Host.Windows.csproj` and WPF entry-point/window code.
- Shared contracts: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj` owns C# Protobuf generation from the canonical schema.
- Regression checks: `tests/test_host_layer_projects.py`, updated product/protocol tests, and `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj`.

## Attempt A001 — observed failure
- [Step Three quality gate run 39907451222](https://github.com/artinrahimii/project/actions/runs/39907451222) passed.
- [Product build run 39907451227](https://github.com/artinrahimii/project/actions/runs/39907451227): C# lifecycle job failed because `HostStatus` resolved ambiguously to the domain record and generated Protobuf message.
- The compile failure is recorded; it is not treated as a successful test.

## Attempt A002 — correction pending
The domain type is now explicitly aliased in `HostInferenceSession.cs`. New exact-commit product/quality workflow outcomes are pending; this file must be revised with the observed conclusions before T0042 can be marked done.
