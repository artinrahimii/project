# T0107 execution report

- **Task:** Add tests for duplicate requests, expired deadline, cancellation during execution, and inference-engine failure.
- **Attempt:** `T0107-2026-10-26-01`.
- **Base:** `main` at `34ba1f33e32534cfd39c4dc40e682e09e66e9b27` (`V 3.0.328`).
- **Status:** In progress pending exact-head CI.
- **Local .NET run:** Not available; the .NET SDK is not installed in this execution environment.

## Dependency review

- Reviewed `T0106 -> T0107`; kept `type=blocks`, changed classification from `unreviewed` to `sequencing_only`, and marked it `validated`. T0106 proves one-active-inference admission under contention; T0107 adds explicit fault-path acceptance around the same existing coordinator. No T0106-specific API/data artifact is consumed, so the edge remains conservative ordering, not a hard technical dependency.
- `T0097 -> T0107` remains validated as `sequencing_only`.
- `T0107 -> T0108` remains `unreviewed`; review it before T0108 starts.

## Implementation

- Added `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`, registered in the existing `tests/HostLifecycle.Tests/Program.cs` harness.
- The suite asserts:
  1. A repeated admission with the same admission context while the original request is active returns `Busy` and does not invoke inference a second time.
  2. An expired deadline is rejected as `DeadlineExpired` before inference starts.
  3. Cancellation during active inference prevents a successful result even if the simulated operation returns after cancellation was signalled.
  4. An inference exception propagates, releases the coordinator gate, clears active cancellation state, and permits the next request.
- The production `HostRequestCoordinator` and admission policy were not rewritten; this task adds explicit executable acceptance coverage for the existing contracts.

## Verification

- New suite: **pending exact-head CI**.
- Step Three quality gate and full Windows/Android/HostLifecycle product build: **pending**.
