# T0109 execution report

- **Task:** Fully stop the network listener when participation is disabled or the program exits.
- **Attempt:** `T0109-2026-10-26-01`.
- **Base:** `main` at `908efce321623898ec3cb8023ec8ba3f7b165b2a` (`V 3.0.332`).
- **Status:** In progress pending exact-head CI.

## Dependency review

- `T0108 -> T0109`: preserved `type=blocks`; classified as `hard_dependency` and validated. T0109 uses the actual `ParticipationGatedNetworkListener.StopAsync/DisposeAsync` and its listener state to release a listener created by T0108.
- `T0107 -> T0109`: preserved `type=blocks`; classified as `sequencing_only` and validated. T0107 tests in-process inference coordinator fault paths; its output/API is not consumed by listener shutdown. Both decisions have code/test/evidence paths in the graph's edge change log.

## Implementation

- Added `src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs` to serialize enable/disable/dispose and keep the listener inactive by default.
- Added a WPF checkbox to enable the current loopback-only listener explicitly. The UI displays its actual localhost endpoint and states that inbound requests are not processed yet.
- On uncheck, the lifecycle turns participation off and awaits listener stop. Stop does not take a cancellable UI token after deactivation begins.
- Added `MainWindow_Closing` cleanup: window close is deferred while the lifecycle is disposed; only after cleanup completes is the window closed and the existing `Closed` resource cleanup run.
- Added `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`, registered it in `Program.cs`, and linked the production lifecycle controller into the test project.
- The listener still closes connections without reading payloads until T0110 adds Protobuf parsing. This task does not claim peer networking, request processing, or encryption.

## Verification

- Local .NET SDK execution: unavailable in the current environment; not claimed.
- Exact-head GitHub Actions HostLifecycle, Step Three quality gate, and Windows/Android build: pending.
- The task will be marked done only after the new tests and relevant build gates pass on the actual implementation commit.
