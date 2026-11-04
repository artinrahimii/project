# T0191 — Async cancellation, timeout and exception boundaries

- **Status:** In progress; implementation and regression coverage are committed in this increment, while exact-head CI acceptance is pending.
- **Base:** `main` / `d70ce891c61e0f85143be68b6af42ab1de4a0a79` (`V 3.0.894`).
- **Graph source:** `step three/project-graph.md`; base Blob `491b69c01a878aa00ababbaa7f8b43cf2606da4a`; 230 nodes / 422 edges. The complete UTF-8 payload was fetched from the base commit's tree-selected Blob and its Git SHA-1 was recomputed before parsing.
- **Attempt:** `T0191-A001`; result `partial` pending CI.
- **Prerequisites:** T0188 and T0190 were inspected in the base commit. Both are recorded `done`; their reports and verification artifacts are present in the same commit.
- **Incoming edge review:**
  - `T0188 → T0191`: retained the original `blocks` edge, reclassified as `sequencing_only / validated`. T0188 documents threat/privacy constraints but provides no executable interface or artifact consumed by T0191's coordinator implementation.
  - `T0190 → T0191`: retained the original `blocks` edge, reclassified as `sequencing_only / validated`. T0190 changes structured status presentation, but T0191 does not consume its presentation API or files; the existing lifecycle/status contract remains unchanged.
  - Conservative task ordering is preserved. Both rationales and source evidence are appended to `edge_change_log`.
- **Verified defect:** `HostRequestCoordinator` checked the absolute deadline before invoking inference and after it returned, but did not signal its active operation token when the deadline elapsed. A long-running cancellation-aware backend could therefore retain the single-inference gate beyond the request deadline.
- **Implementation:** the coordinator now links caller cancellation with a deadline cancellation source, monitors the absolute deadline while inference is running (using bounded 24-hour timer slices for long deadlines), maps deadline-triggered cancellation to a typed `DeadlineExpired` rejection, and stops/joins the monitor before releasing cancellation state and the semaphore. Caller-initiated cancellation and unrelated operation exceptions retain their existing propagation behavior.
- **Regression test added:** `RequestDeadlineCancelsActiveOperationAsync` verifies that deadline expiry signals the operation token, returns `DeadlineExpired`, clears active cancellation state, releases the gate, and allows a subsequent request.
- **Changed files:** `src/Host.Windows/Requests/HostRequestCoordinator.cs`, `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`, `step three/T0191/execution-report.md`, `step three/T0191/evidence/verification.md`, `step three/project-graph.md`.
- **Local execution limitation:** the execution environment has Git but no `.NET SDK`; a clean GitHub clone also could not be resolved from this environment. No local .NET test/build result is claimed. The exact-head GitHub Actions results must be inspected before changing this task to `done`.
- **Next action:** inspect the exact `V 3.0.895` quality, Windows HostLifecycle/WPF, Android and rendezvous workflows. If they pass, reconcile the task as `done`; otherwise preserve the failure and fix the reported issue.
