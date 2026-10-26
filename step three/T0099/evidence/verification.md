# T0099 verification evidence

- **Repository / branch / base:** `artinrahimii/project`, `main`, `712243254c86aee70f1510b560f3c5e6f0d5fc4c` (`V 3.0.308`).
- **Graph integrity:** exact-commit `step three/project-graph.md` Blob `8ba3f8944f787e41e3b91a082f5961ac64edd706`, 1,596,993 bytes / 1,474,887 characters; computed Git Blob SHA-1 matched. The complete enclosed JSON parsed; 229 unique task IDs and 426 edges were found.
- **Prerequisite evidence:** T0097's capability-gate status and CI evidence remain recorded in `step three/T0097/evidence/verification.md`; T0098's capacity-one semaphore test passed on V 3.0.307 in [HostLifecycle run 40308296192](https://github.com/artinrahimii/project/actions/runs/40308296192).
- **Contract change:** the prior semaphore `WaitAsync(cancellationToken)` could queue requests. T0099 changes admission to zero-timeout `WaitAsync(0, cancellationToken)`; Busy callers receive `HostRequestStatus.Busy`, and `ExecuteAsync` raises `HostRequestBusyException` with the same status.
- **New automated checks:** `HostRequestCoordinatorBusyTests` sends 32 concurrent admission attempts while one request is held, verifies all are immediately Busy and none invoke the operation, preserves pre-cancelled cancellation, and confirms service resumes after release. `HostRequestCoordinatorTests` verifies explicit Busy behavior and no overlap.
- **Execution status:** implemented and registered in the existing CI test harness. CI is pending for this new commit; tests are not yet marked passed.
