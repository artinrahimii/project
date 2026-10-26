# T0109 execution report

- **Task:** Fully stop the network listener when participation is disabled or the program exits.
- **Status:** Done, based on exact-head GitHub Actions.
- **Implementation commit tested:** [V 3.0.335](https://github.com/artinrahimii/project/commit/b82ce66b585c93057730225826896097845ee74f) — `b82ce66b585c93057730225826896097845ee74f`.
- **Attempts:** `T0109-2026-10-26-01` and `T0109-2026-10-26-02` failed and remain in history; `T0109-2026-10-26-03` succeeded.

## Attempts and fixes

- **Attempt 01:** Lifecycle acceptance tests and Step Three gate passed; Windows compile passed but WPF startup smoke failed with CLR exit code `-532462766`. The smoke runner called application shutdown immediately after requesting window close, racing asynchronous listener disposal.
- **Attempt 02:** Added `ExitAfterWindowCleanup` to call app shutdown after the main window's `Closed` event. Exact-head smoke still failed with `-532462766`; the Python quality gate also failed three source-contract assertions expecting literal `Shutdown(0/1/3)` calls.
- **Attempt 03:** Dispatches the final `Close()` using `Dispatcher.BeginInvoke`, after the first `Closing` event unwinds, even if disposal completes synchronously. The Python source-contract tests were updated to verify the new close/exit contract. This attempt passed all relevant gates.

## Dependency review

- `T0108 -> T0109`: `type=blocks` preserved and validated as `hard_dependency`, because this task consumes the listener stop/dispose API introduced by T0108.
- `T0107 -> T0109`: `type=blocks` preserved and validated as `sequencing_only`; coordinator tests do not provide a T0107-specific API or data consumed by T0109.
- `T0109 -> T0110` remains unreviewed and must be validated before T0110 begins.

## Implementation

- `ParticipationNetworkListenerLifecycle` serializes activation, deactivation and disposal.
- The WPF UI exposes explicit opt-in participation. The listener is loopback-only and disabled by default.
- Disabling participation awaits listener stop; application close waits for lifecycle disposal; final window close is dispatched after the first Closing handler unwinds.
- `ParticipationNetworkListenerLifecycleTests` verify initial disabled state, activation, deactivation, port reuse, restart, disposal while active, endpoint clearing and rejection after disposal.
- Inbound TCP connections still close without parsing or logging payloads until T0110 adds bounded Protobuf parsing. No P2P request/response or encryption capability is claimed.

## Verification — all PASS on `b82ce66b585c93057730225826896097845ee74f`

- **Step Three quality gate:** [PASS](https://github.com/artinrahimii/project/actions/runs/40357312350).
- **HostLifecycle acceptance:** [PASS](https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693129382).
- **Windows Release build and WPF startup smoke:** [PASS](https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693130018).
- **Android lint, APK build, Protobuf tests and generated-source checks:** [PASS](https://github.com/artinrahimii/project/actions/runs/40357312422/job/121693129952).
- **Complete White-P product build:** [PASS](https://github.com/artinrahimii/project/actions/runs/40357312422).
- The local environment has no .NET SDK; these results came from the linked GitHub Actions jobs, not from a claimed local run.
