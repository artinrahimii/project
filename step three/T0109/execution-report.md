# T0109 execution report

- **Task:** Fully stop the network listener when participation is disabled or the program exits.
- **Base:** `f8b08f814f28c604fa5f8141ff3b2bd3d137e86b` (`V 3.0.333`); current revision is a corrective attempt.
- **Status:** In progress pending corrected exact-head CI.
- **Attempt 1:** `T0109-2026-10-26-01` — lifecycle acceptance test and Step Three quality gate passed; Windows WPF build compiled, but startup smoke failed with exit code `-532462766`.
- **Failure cause:** `App.xaml.cs` called `window.Close()` followed immediately by `Shutdown(0)`. The new `MainWindow_Closing` handler cancels the first close and asynchronously waits for listener disposal, so the smoke runner could shut down the WPF dispatcher before the second close completed.
- **Attempt 2:** `T0109-2026-10-26-02` adds `ExitAfterWindowCleanup`, which subscribes to the actual `Closed` event and calls application shutdown only after that event. All smoke-test exit paths use the helper. Exact-head validation is pending.

## Dependency review

- `T0108 -> T0109`: preserved `type=blocks`; validated as `hard_dependency`, because this task consumes the concrete listener `StopAsync/DisposeAsync` lifecycle introduced by T0108.
- `T0107 -> T0109`: preserved `type=blocks`; validated as `sequencing_only`; the request-coordinator fault tests provide no API/data consumed by listener shutdown.

## Implementation

- `src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs` serializes participation activation, deactivation and disposal.
- WPF uses an explicit checkbox for a loopback-only listener and waits for lifecycle disposal before closing.
- Added `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`; exact-head log from attempt 1 confirmed activation, deactivation, endpoint release/restart, disposal and no-reuse-after-disposal.
- Inbound TCP connections are still closed without parsing/logging payloads until T0110 implements the shared Protobuf parser. No P2P request/response or encryption capability is claimed.

## Verification

- Attempt 1 HostLifecycle lifecycle tests: **PASS** — https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685894262
- Attempt 1 Step Three quality gate: **PASS** — https://github.com/artinrahimii/project/actions/runs/40355264399
- Attempt 1 Windows build: compile **PASS**, startup smoke **FAIL** — https://github.com/artinrahimii/project/actions/runs/40355264305/job/121685895265
- Corrective attempt 2 Windows/Android/product build, real inference integration and graph gate: **pending**. No successful result is pre-claimed.
