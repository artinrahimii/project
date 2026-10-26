# T0112 verification evidence — pending CI

## Baseline and edge review
- Repository: `artinrahimii/project`; branch `main`; base `9f5441d3b4617ab936cecb058018f44bda96b75a`.
- `T0111 → T0112`: validated as `contract_or_interface_dependency`; mapper translates the T0111 parser/validator error codes into concrete protocol response enums.
- `T0107 → T0112`: validated as `contract_or_interface_dependency`; mapper directly consumes `HostRequestOutcome<T>`, `HostRequestStatus` and `HostRequestAdmissionFailure`.
- Both edge types remain `blocks`; the outgoing `T0112 → T0113` edge is unchanged and unreviewed.

## Implementation
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`: centralizes protocol failure mapping, generates future retry time for HostBusy, preserves request/model/host IDs where the contract has them, and uses fixed safe public messages.
- `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`: covers Busy, not-ready, unsupported model, invalid request, unsupported version, expired deadline, capacity limits, and internal-error sanitization.
- `tests/HostLifecycle.Tests/Program.cs` registers the acceptance suite.
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj` compiles the same production mapper source into the cross-platform lifecycle harness.
- ADR-0003 explicitly distinguishes typed mapping from actual TCP response serialization/delivery.

## Verification
- HostLifecycle, quality gate, Windows build and Android build: **pending exact-head CI**.
- The task must remain partial until the acceptance suite and relevant CI jobs succeed.
