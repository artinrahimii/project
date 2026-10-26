# T0109 execution report

- **Task:** Fully stop the network listener when participation is disabled or the program exits.
- **Base:** `3bc81b6feab2fa3d5b927c9b4ef13ea1f7f626cc` (`V 3.0.334`); corrective attempt 03 is in progress.
- **Status:** In progress; final CI pending.

## Attempts

- **Attempt 01 — `T0109-2026-10-26-01`:** Lifecycle acceptance tests and Step Three gate passed; Windows compile passed but WPF startup smoke failed with CLR exit code `-532462766`. The smoke runner immediately called application shutdown after requesting window close, racing asynchronous listener disposal.
- **Attempt 02 — `T0109-2026-10-26-02`:** Added `ExitAfterWindowCleanup` to call application shutdown after the main window's `Closed` event. Exact-head smoke still failed with `-532462766`; the Python quality gate also failed three source-contract assertions because they expected literal `Shutdown(0/1/3)` at the old call sites.
- **Attempt 03 — `T0109-2026-10-26-03`:** Defers the final `Close()` using `Dispatcher.BeginInvoke` so it runs only after the first `Closing` event unwinds, even if listener disposal completes synchronously. Updates source-contract tests to verify the new orderly-close contract.

## Dependency review

- `T0108 -> T0109`: `type=blocks` preserved, validated as `hard_dependency`, because this task consumes the listener stop/dispose API introduced by T0108.
- `T0107 -> T0109`: `type=blocks` preserved, validated as `sequencing_only`; the coordinator tests do not provide a T0107-specific API or data consumed by T0109.

## Implementation

- `src/Host.Windows/Networking/ParticipationNetworkListenerLifecycle.cs` serializes activation/deactivation/disposal.
- `src/Host.Windows/MainWindow.xaml` and `MainWindow.xaml.cs` expose an explicit loopback-only participation checkbox and wait for listener disposal before closing.
- `src/Host.Windows/App.xaml.cs` defers smoke-mode application shutdown until the actual `Closed` event.
- `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs` verifies activation, disable, released-port reuse, restart, disposal, and rejection after disposal.
- `tests/test_windows_hardware_discovery.py` and `tests/test_wpf_entrypoint.py` now verify the updated shutdown contract.
- Inbound connections still close without parsing/logging payloads until T0110. This task does not claim P2P request processing or encryption.

## Verification status

- Attempt 01 lifecycle test: PASS; Step Three gate: PASS; Windows startup smoke: FAIL.
- Attempt 02 Step Three gate: FAIL (three stale source-contract assertions); Windows startup smoke: FAIL.
- Attempt 03 CI: pending. No final completion is claimed yet.
