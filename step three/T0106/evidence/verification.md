# T0106 verification evidence

## Source snapshot and graph integrity

- Repository/branch: `artinrahimii/project`, `main`.
- Implementation commit: `d83cf79f79e9852ad6b065f522208f6022abec74` (`V 3.0.326`); parent `684b47473e6d06d519bbe92ae3f2de5515f2754c`.
- Graph Blob at implementation commit: `530611955da6ff3c43dfa57504f90e3ec564341f`; received **1682483 UTF-8 bytes** matched the tree's declared size. Calculated Git Blob SHA-1 matched the Blob ID; the full fenced JSON parsed and contained 229 unique task IDs and 426 edges.
- T0105 and T0097 were done before starting T0106.

## Reviewed edge

- `T0105 -> T0106`: `type=blocks` preserved; class `unreviewed -> sequencing_only`; status `unreviewed -> validated`.
- Evidence inspected: production `HostRequestCoordinator.cs`, existing coordinator and busy tests, and T0105 execution/evidence reports.
- Rationale: T0105 checks exception-path cleanup; T0106 exercises the existing one-slot admission rule under simultaneous contention. No new API/data artifact is consumed from T0105. `T0106 -> T0107` remains unreviewed.

## Acceptance test

- Added `tests/HostLifecycle.Tests/HostRequestCoordinatorConcurrencyTests.cs` and registered it in `tests/HostLifecycle.Tests/Program.cs`.
- The test releases 64 requests from a common barrier against the production coordinator, holds the winning operation active, and asserts exactly one invocation, one completed result, 63 Busy results, maximum active inference count one, and gate release.
- HostLifecycle, product build, and real local-inference workflows passed on `d83cf79f79e9852ad6b065f522208f6022abec74`.

## Quality-gate discrepancy and remediation

The first Step Three quality run [40331493667](https://github.com/artinrahimii/project/actions/runs/40331493667) passed automated tests and Python syntax, then failed graph structure validation because the task summary claimed one attempt while its execution history was empty. This attempt is now preserved as `partial` in the graph, matching the declared attempt count. The quality gate must be rerun on the correction commit; T0106 remains in progress until the corrected exact-head result passes.

The hosted product build [40331494036](https://github.com/artinrahimii/project/actions/runs/40331494036) passed all three jobs. The live inference workflow [40331493535](https://github.com/artinrahimii/project/actions/runs/40331493535) also passed. No local .NET execution is claimed.
