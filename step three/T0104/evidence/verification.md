# T0104 verification evidence

## Exact source and graph validation

- Repository/branch: `artinrahimii/project`, `main`.
- Base commit: `3696e6179aede840fd69112ffaaabdb2ceb5be0f` (`V 3.0.320`); exact tree SHA: `009c15ade3b100af3b6f41331ee89feeeaaa4663`.
- Graph path: `step three/project-graph.md`, Blob `5e88d1c00c0b8d2501d798eaa5b12e24976095c6`.
- Graph Blob content received as UTF-8: 1,660,798 bytes / 1,538,553 Unicode characters. The computed Git Blob SHA-1 (including the Git `blob <length>\0` header) matched the tree Blob: `5e88d1c00c0b8d2501d798eaa5b12e24976095c6`.
- Full enclosed JSON parsed successfully: 229 unique task IDs, 426 edges, and zero unresolved edge endpoints. Before this attempt statuses were 102 done, 1 in_progress, and 126 not_started.
- Existing CI on V 3.0.320: product build and quality gate were inspected and both report success (product run 40320084003; quality run 40320084041).

## Dependency review

- `T0103 -> T0104` remains `type=blocks` but is now `dependency_classification=sequencing_only`, `validation_status=validated`. Rationale and source evidence are recorded in `project-graph.md`.
- `T0104 -> T0105` remains unreviewed.

## Acceptance implementation

- `src/Host.Windows/Requests/HostRequestResponseDeliveryGate.cs`: atomic one-shot claim before invoking the callback; gate is not reopened after callback failure.
- `tests/HostLifecycle.Tests/HostRequestResponseDeliveryTests.cs`: sequential and concurrent duplicate suppression, ambiguous callback failure, pre-cancelled handoff, and invalid request ID checks.
- `tests/HostLifecycle.Tests/Program.cs` registers the suite.
- `tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj` compiles the actual production gate source into the portable harness.
- CI is pending at this attempt; no pass is claimed yet.

## Boundary and limitations

- This proves only that a shared gate instance invokes its supplied callback at most once in-process.
- There is no production P2P ingress/response sender in the inspected code, and this task does not claim wire delivery or cross-restart replay protection.
- Local .NET SDK execution is unavailable in this environment; rely on the exact-head CI results before updating status to done.
