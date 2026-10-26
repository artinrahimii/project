# T0107 verification evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `34ba1f33e32534cfd39c4dc40e682e09e66e9b27` (`V 3.0.328`); tree SHA: `ed23c9943e0301ab4f10b9850db12e8a3a9b959c`.
- Graph path: `step three/project-graph.md`; Blob ID: `1017d4d5f93812b0bdb112f27c7658d4f250b683`.
- The full Git Blob response measured **1689305 UTF-8 bytes**, matching the exact tree entry; calculated Git Blob SHA-1 matched the Blob ID. The original fenced JSON parsed fully and contained 229 task nodes and 426 edges.
- T0106 and T0097 were `done` before T0107 began.

## Dependency edge

- `T0106 -> T0107`: `type=blocks` preserved; class `unreviewed -> sequencing_only`; validation `unreviewed -> validated`.
- Evidence inspected: production coordinator and admission policy; T0106 concurrency test/report; existing coordinator, Busy, and admission tests.
- Rationale: T0106 tests the shared one-active-inference gate, while T0107 tests repeated admission and failure-path contracts using the same existing APIs. No newly produced artifact/API from T0106 is consumed as a technical prerequisite.
- `T0107 -> T0108` remains unreviewed.

## New acceptance suite

- `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`, registered in `tests/HostLifecycle.Tests/Program.cs`.
- Repeated active admission is rejected Busy without a second invocation.
- Expired deadline is rejected before inference begins.
- Cancellation after operation start prevents a late success even when the operation ignores the token and eventually returns.
- Inference exception propagates; coordinator gate and active cancellation state are cleaned; a subsequent request succeeds.

## Pending results

The .NET SDK is unavailable locally. Exact-head Step Three quality gate, HostLifecycle test execution, Windows WPF build, Android APK build, and Protobuf checks must complete successfully before T0107 can be marked done.
