# T0177 verification evidence

## Graph and source validation

- Repository/branch: `artinrahimii/project`, `main`.
- Base implementation commit: `fd7fabcab361c6b8736c869b3b6d97aaa6d7d6b6` (`V 3.0.810`).
- Graph at the base commit was fetched by its tree-derived Blob SHA and SHA-1 validated against the original 3,897,496 UTF-8 bytes; full fenced JSON parsed as 230 nodes and 422 edges.
- Incoming edges were reviewed and recorded: `T0176 → T0177` = `sequencing_only`; `T0160 → T0177` = `contract_or_interface_dependency`. Both remain `blocks` edges and remain in the conservative order.

## Acceptance implementation

- Android JVM tests cover three rounds, a maximum of three distinct peers per round, nine total attempts, no ordinary peer reuse across rounds, and a previously used peer retry consuming the last slot in a smaller request budget.
- Python source-contract tests cover centralized policy limits and the selector/dispatcher guard paths.
- Production source is unchanged.

## Exact-head V 3.0.810 results

- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41436616226): passed 328 Python tests, compileall, 230 nodes / 422 edges, zero graph/evidence errors and one preserved metadata warning.
- [HostLifecycle tests](https://github.com/artinrahimii/project/actions/runs/41436616179): passed.
- [Rendezvous container build/smoke](https://github.com/artinrahimii/project/actions/runs/41436616214): passed.
- [Android product build](https://github.com/artinrahimii/project/actions/runs/41436616179): failed at `ktlintTestSourceSetCheck` because of one needless blank line at test source line 510. APK build and JVM tests were skipped.

## Correction and current state

- A002 removed the extra blank line between the previous test and the first new acceptance test.
- Exact-head CI for `V 3.0.811` is pending.
- T0177 remains `in_progress`; the Android tests are not considered accepted until the corrected commit passes KtLint, APK build and `testDebugUnitTest`.
