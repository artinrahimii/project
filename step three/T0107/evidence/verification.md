# T0107 verification evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Implementation commit: `b034e7df70e8ab37bf0c9aac4314d4cd32c52051` (`V 3.0.329`), parent `34ba1f33e32534cfd39c4dc40e682e09e66e9b27`.
- Graph path: `step three/project-graph.md`; Blob `d3c13e9b97b974d9cd5bcdefe1bd3e35815731b3`.
- The Git Blob API response measured **1693962 UTF-8 bytes**, matching the tree's declared size; calculated Git Blob SHA-1 matched. The full fenced JSON parsed and contained 229 task nodes and 426 edges.
- T0106 and T0097 were `done` before T0107 started.

## Reviewed dependency edge

- `T0106 -> T0107`: `type=blocks` preserved; classification `unreviewed -> sequencing_only`; validation `unreviewed -> validated`.
- Inspected production `HostRequestCoordinator.cs`, `HostRequestAdmissionPolicy.cs`, the T0106 concurrency suite, prior coordinator/admission/busy tests, and T0106 task evidence.
- Rationale: T0106 proves the one-active-inference gate under contention; T0107 exercises related fault cases via the same existing API, consuming no T0106-specific new API or data output.
- `T0107 -> T0108` and other outgoing edges remain unreviewed.

## Acceptance suite

- New file: `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`.
- Registered in the executable `tests/HostLifecycle.Tests/Program.cs` harness.
- Four scenarios:
  - Repeat the same admission while an original request is active; expect `Busy` and no second invocation.
  - Expired deadline; expect explicit rejection before inference is invoked.
  - Cancel active inference and then let a cancellation-ignoring operation return; expect no successful result delivery and release of the request gate.
  - Throw an inference-engine exception; expect propagation, cleanup, and successful admission of the next request.
- HostLifecycle passed: https://github.com/artinrahimii/project/actions/runs/40336520459/job/121619458211
- Step Three quality gate passed: https://github.com/artinrahimii/project/actions/runs/40336520434
- Full Windows/Android product build passed: https://github.com/artinrahimii/project/actions/runs/40336520459
- Real local inference integration passed: https://github.com/artinrahimii/project/actions/runs/40336520612

## Scope boundary

The request admission context does not contain a request ID. The repeated-request test validates fail-fast handling while a request is in flight; it does not assert deduplication or idempotency for a completed request ID.
