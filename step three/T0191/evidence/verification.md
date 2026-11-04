# T0191 — Verification evidence

## Snapshot and graph integrity
- Branch: `main`.
- Base commit: `d70ce891c61e0f85143be68b6af42ab1de4a0a79` (`V 3.0.894`).
- Tree selected `step three/project-graph.md` Blob: `491b69c01a878aa00ababbaa7f8b43cf2606da4a`.
- Recomputed Git Blob SHA-1: `491b69c01a878aa00ababbaa7f8b43cf2606da4a` (match).
- Original content: 3,503,313 UTF-8 bytes; 3,375,853 JavaScript string code units.
- JSON parsed successfully; 230 unique task nodes and 422 edges.
- Prerequisites T0188 and T0190 are recorded `done`; their task workspaces and evidence files were confirmed in the same base commit.

## Incoming dependency validation
- `T0188 → T0191`: `sequencing_only / validated`. The security/threat-model output is normative guidance; no source-level dependency on a produced API or artifact was found.
- `T0190 → T0191`: `sequencing_only / validated`. Structured status presentation consumes lifecycle/status state but T0191's coordinator does not consume the T0190 presentation implementation.
- Original `type=blocks` values and conservative execution order are preserved. The edge review is appended to `edge_change_log`.

## Code-level finding and correction
- Before correction, `HostRequestCoordinator.TryExecuteAsync` evaluated the request deadline before starting inference and after inference returned. There was no timer or cancellation source that signalled the active operation when the deadline elapsed.
- The coordinator now combines caller cancellation with deadline cancellation; the deadline monitor signals the linked operation token at expiry.
- Deadline-triggered `OperationCanceledException` becomes a typed `HostRequestOutcome.Rejected(DeadlineExpired, ...)`; cancellation from the caller or `CancelActiveRequest` is not converted into a timeout.
- Cleanup stops and joins the deadline monitor, clears the active cancellation reference, disposes both cancellation sources, and releases the single-inference semaphore.
- The existing post-operation deadline check remains as a guard against timer scheduling delay and engines that return after the deadline despite cancellation.

## Test coverage
- Added `RequestDeadlineCancelsActiveOperationAsync` to the existing HostLifecycle executable test harness through its already-registered `HostRequestCoordinatorFailureAcceptanceTests.RunAsync`.
- The new test checks deadline-token notification, typed deadline rejection, release of the coordinator gate, clearing of the active cancellation reference, and successful admission of a subsequent request.
- Existing tests in the same class continue to cover duplicate admission, pre-expired deadlines, explicit in-flight cancellation with late-result withholding, and exception cleanup.
- **Local .NET execution:** not run; `.NET SDK` is absent from the available environment.
- **Exact-head CI:** pending after commit. No test is marked passed until the corresponding GitHub Actions result is inspected.

## Acceptance decision
- Task state remains `in_progress` until exact-head CI verifies the source and tests.
- This change addresses the confirmed host inference deadline-cancellation gap; it does not claim that all product workflows or independent-network acceptance are complete.
