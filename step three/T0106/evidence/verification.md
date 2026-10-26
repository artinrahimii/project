# T0106 verification evidence

## Exact source snapshot

- Repository/branch: `artinrahimii/project`, `main`.
- Base commit: `684b47473e6d06d519bbe92ae3f2de5515f2754c` (`V 3.0.325`); base tree SHA: `19adad55a47c62182ca48b1451c35acc850ca955`.
- Graph path: `step three/project-graph.md`; expected Blob SHA: `2faec3f1e93ecf503192b1054cae796fd14687dd`.
- Graph received from Git Blob API was **1679657 UTF-8 bytes**, matching the tree entry; Git Blob SHA-1 computed from the original bytes matched the expected Blob ID. The fenced JSON parsed fully and contained 229 unique task nodes and 426 edges.
- T0105 and T0097 were both `done` before T0106 began; T0106 was `not_started`.

## Reviewed edge

- `T0105 -> T0106`: `type=blocks` preserved; classification `unreviewed -> sequencing_only`; validation `unreviewed -> validated`.
- Inspected: production `HostRequestCoordinator.cs`, existing `HostRequestCoordinatorTests.cs`, `HostRequestCoordinatorBusyTests.cs`, and T0105 execution/verification records.
- Rationale: T0105 checks cleanup after an exception; T0106 independently exercises the already-existing one-slot admission mechanism under simultaneous contention. The edge remains as conservative ordering only.

## Acceptance test added

- `tests/HostLifecycle.Tests/HostRequestCoordinatorConcurrencyTests.cs`: 64 contenders, a shared launch barrier, and a held winning inference allow the test to measure concurrent active operations rather than infer concurrency from source alone.
- Assertions: exactly one operation invoked; maximum active inference count equals one; exactly one completed outcome and 63 Busy outcomes; all work exits and the coordinator no longer reports Busy.
- `tests/HostLifecycle.Tests/Program.cs` registers the new test in the real lifecycle harness. The test project includes `HostRequestCoordinator.cs` from production source.

## Result pending

The .NET SDK is unavailable in this environment, so local execution was not performed. The implementation commit's exact-head CI results must pass before this task can be marked done.
