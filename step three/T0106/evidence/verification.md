# T0106 verification evidence

## Exact source snapshot and graph integrity

- Repository: `artinrahimii/project`; branch: `main`.
- Implementation commit: `d83cf79f79e9852ad6b065f522208f6022abec74` (`V 3.0.326`).
- Corrected validation commit: `126168388f54d075949d4a46b87ce926a266eb9b` (`V 3.0.327`).
- Graph at the validation commit: Blob `3ceed69a3fb4c5ff8e47632a3ceba0e1be36f331`, **1686704 UTF-8 bytes**. The received byte size matched the Git tree entry, calculated Git Blob SHA-1 matched, and the complete fenced JSON parsed successfully.
- The graph has 229 tasks and 426 edges. T0105 and T0097 were done before T0106 began.

## Dependency review

- `T0105 -> T0106`: `type=blocks` preserved; classification `sequencing_only`; validation `validated`.
- Inspected the production `HostRequestCoordinator.cs`, existing coordinator/busy tests, and T0105 execution and verification reports.
- Rationale: T0105 verifies exception-path cleanup; T0106 independently validates the existing one-slot admission mechanism under simultaneous contention. T0106 consumes no new API/data artifact from T0105.
- `T0106 -> T0107` remains unreviewed.

## Acceptance test and results

- Added `tests/HostLifecycle.Tests/HostRequestCoordinatorConcurrencyTests.cs` and registered it in `tests/HostLifecycle.Tests/Program.cs`.
- 64 contenders start from a common barrier against the production coordinator. A winning operation is held open while all competing requests attempt admission.
- The passing test asserts exactly one operation invocation, one completed result, 63 Busy results, a maximum active inference count of one, and release of all active work.
- HostLifecycle job, full Windows/Android product build and Step Three quality gate passed on corrected exact head `126168388f54d075949d4a46b87ce926a266eb9b`: [product](https://github.com/artinrahimii/project/actions/runs/40332219205), [quality](https://github.com/artinrahimii/project/actions/runs/40332219306).
- Real local inference integration passed on implementation commit `d83cf79f79e9852ad6b065f522208f6022abec74`: [workflow](https://github.com/artinrahimii/project/actions/runs/40331493535).

## First-run audit trail

The first quality gate [https://github.com/artinrahimii/project/actions/runs/40331493667] failed at graph structure validation because `execution_summary.attempt_count=1` while `execution_history` was empty. Automated tests and Python syntax passed. The attempt is retained as partial and the missing history entry was recorded in `V 3.0.327`; the corrected exact-head quality gate and product build passed. No failure evidence was deleted.

No local .NET run is claimed because the SDK was unavailable in this tool environment.
