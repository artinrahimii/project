# T0177 acceptance evidence

## Source and dependency review

- Repository: `artinrahimii/project`
- Implementation branch: `main`
- Accepted implementation/test commit: `8345a037602b9cdf6f562f98cde18f6be8954da6` (`V 3.0.811`)
- The graph was read from the exact preceding commit via Git Trees API and Git Blob API, SHA-1 matched over original UTF-8 bytes, and fully parsed as 230 nodes / 422 edges.
- `T0160 → T0177`: validated `contract_or_interface_dependency`; the tested dispatcher builds real requests through the payload builder introduced by T0160.
- `T0176 → T0177`: validated `sequencing_only`; T0177's host-budget algorithm does not consume T0176's response-card/ViewModel test outputs.
- Both `blocks` edge types and the original conservative execution order were retained. Rationale/evidence are recorded in the graph's `edge_change_log`.

## Implemented acceptance cases

- Normal request: three rounds; every round selects at most three distinct hosts; prior hosts are excluded from later ordinary rounds; total attempts stop at nine.
- Custom request budget: a 4-attempt request selects three initial peers then explicitly retries one previous host as attempt four; no third selection or fifth dispatch is permitted.
- Production source was reviewed and left unchanged because its centralized first-release ceilings and selector/dispatcher budget guards already implement the required behavior. The new output is the dedicated executable acceptance coverage.

## First implementation attempt — retained failure

- `V 3.0.810`, commit `fd7fabcab361c6b8736c869b3b6d97aaa6d7d6b6`: Step Three quality passed 328 Python tests and graph/evidence checks; HostLifecycle and Rendezvous container checks passed.
- Android KtLint failed only for a redundant blank line at test source line 510; Android APK and JVM tests were skipped.
- The failure is retained as `T0177-A001` in append-only `execution_history`.

## Corrected exact-head CI — V 3.0.811

- [Android / Windows product build](https://github.com/artinrahimii/project/actions/runs/41437131408): all jobs passed.
  - Kotlin `ktlintCheck`: passed.
  - Android `assembleDebug`: passed.
  - Android `testDebugUnitTest`: passed.
  - Generated Protobuf Java/Kotlin verification: passed.
  - Windows WPF Release build, startup smoke, process/policy/capability checks and user-scoped DPAPI identity/trust checks: passed.
  - HostLifecycle unit/integration tests: passed.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41437132158): 328 tests passed, compileall passed, 230 tasks / 422 edges, zero errors and one preserved T0001 metadata warning.
- [Rendezvous container](https://github.com/artinrahimii/project/actions/runs/41437131542): production image build and non-root HTTP smoke passed.
- Android JVM report artifact: `13137490934`, 49,961 bytes.
- Android debug APK artifact: `13137520910`, 19,524,952 bytes.

All T0177 acceptance conditions are satisfied on the tested source revision. The task is therefore marked `done`; prior failed-attempt history remains intact.
