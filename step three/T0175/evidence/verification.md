# T0175 — Verification evidence

## Source revision and integrity

- Repository: `artinrahimii/project`
- Branch: `main`
- Exact code/CI revision: `aed7d2c3c49e04b596c457e5f38f680d88027b04` (`V 3.0.803`)
- Tree: `ce331a8cb4b153138765e4fea7bfb10abc296581`
- Graph path: `step three/project-graph.md`
- Graph Blob from the exact tree: `3f9f259b1dbed523c652f5103b6b200fc03a7824`
- Graph retrieval: Git Blob API, because ordinary `fetch_file` returned empty base64 content.
- Graph size: 3,850,038 UTF-8 bytes; retrieved content: 3,850,038 bytes / 3,723,481 Unicode characters.
- Calculated Git Blob SHA-1 over `blob <byte-length>\0<original UTF-8 bytes>`: `3f9f259b1dbed523c652f5103b6b200fc03a7824` — exact match.
- JSON validation: full fenced JSON parsed; 230 task nodes, 422 edges, no duplicate task IDs and no unresolved task references.

## Prerequisites and dependency edges

- `T0174`: `done`, latest acceptance attempt `T0174-A021-v3.0.799-exact-head-acceptance`.
- `T0160`: `done`, latest acceptance attempt `T0160-A006`.
- `T0174 -> T0175`: `contract_or_interface_dependency`, `validated`. The terminal correlated `CANCELLED` result is handled as non-retryable.
- `T0160 -> T0175`: `contract_or_interface_dependency`, `validated`. Retry keeps the logical request identity and uses a new attempt identity.
- Outgoing `T0175 -> T0176`: `unreviewed` / `unreviewed`; preserved unchanged.

## Exact production source blobs at V 3.0.803

- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt` — `10054297017e6468f6addb0319ce30b19c70b993`.
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt` — `b27133434c38f6d8d010c89dc0dab5cac4ba8dd9`.
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt` — `1833a0d334dd5b8bc7839554a87b6fa7caba196b`.

## Request-level regression tests

- **PASS** — `showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds`: the first dispatch reports FAILED and exposes the manual action; no second dispatch occurs until `retryFailedAttempts()` is called; both rounds use the same request ID; retry attempts have fresh attempt IDs; successful retry responses move the request to SUCCEEDED and disable further retry.
- **PASS** — `manualRetryExcludesHostsWhoseLatestAttemptAlreadySucceeded`: a partial first round enables manual retry; a host whose latest attempt succeeded is excluded; only the failed peers are selected in the next round; successful retry results move the request to SUCCEEDED.
- **PASS** — `cancelActionSendsCorrelatedCancellationForEveryQueuedAttemptFromTheLiveDispatch`: existing cancellation regression remains green, with aggregate CANCELLED status and retry disabled for cancelled requests.

## Exact-head CI results — V 3.0.803

- [Android product job](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183843391): KtLint PASS; `assembleDebug` PASS; `testDebugUnitTest` PASS; generated Protobuf Java/Kotlin verification PASS.
- [Android Gradle report artifact 13127773181](https://github.com/artinrahimii/project/actions/runs/41417197342/artifacts/13127773181): 183 tests, 0 failures, 0 ignored; 100% success; 38.912 seconds. `RequestViewModelTest`: 14 tests, 0 failures, 0 ignored.
- [Android debug APK artifact 13128670755](https://github.com/artinrahimii/project/actions/runs/41417197342/artifacts/13128670755): uploaded successfully (19,524,950 bytes).
- [Windows WPF job 125183844243](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183844243): Release build, Protobuf C#, WPF startup smoke, agent-tool process/policy/capability checks and DPAPI identity/trust checks PASS.
- [Windows host artifact 13128885203](https://github.com/artinrahimii/project/actions/runs/41417197342/artifacts/13128885203): uploaded successfully (14,266,137 bytes).
- [HostLifecycle job 125183843834](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183843834): PASS.
- [Step Three quality run 41417197306](https://github.com/artinrahimii/project/actions/runs/41417197306): 317 Python tests, `compileall`, graph structure and recorded-output/evidence validation PASS.
- [Rendezvous container run 41417197168](https://github.com/artinrahimii/project/actions/runs/41417197168): production image build and non-root HTTP contract smoke PASS.

## Historical failures retained in the task ledger

- **A001 / V 3.0.801**: one of 317 Python tests failed because the required exact phrase `No inference payload was sent` was absent; Android KtLint reported the status-chain and parameter-formatting violations. Windows/HostLifecycle and container smoke passed.
- **A002 / V 3.0.802**: all 317 Python tests and graph/evidence quality passed; Android KtLint still reported the formatting violations. Windows/HostLifecycle and container smoke passed.
- **A003 / V 3.0.803**: the exact-head rerun passed all acceptance gates above.

## Acceptance boundary

T0175 is accepted for the real Android UI/ViewModel failure-status and explicit manual-retry path. The implementation retains the same request ID, fresh attempt IDs, per-host latest outcomes, successful-host exclusion and bounded retry budget. Cancellation is terminal and is not retryable. No claim is made that physical two-device or independent-public-network final acceptance has passed.


## A004 — graph guard result on V 3.0.804

- Exact graph commit tested: `1010fe74f1a3722026148c1a291bca7dd414e1a3`.
- All 317 Python tests passed and `python -m compileall -q tools tests` passed.
- `tools/graph_guard.py` found 230 tasks / 422 edges but rejected T0175 with `DONE_WITHOUT_TEST_FILE_EVIDENCE`.
- Inspection of `tools/graph_guard.py` confirms that evidence qualification requires a source-path string starting exactly with `tests/` or `test/`. The Android JVM test file exists under `src/Client.Android/.../src/test/`, but that path is outside the validator's accepted evidence prefixes.
- The existing `UNMAPPED_DEPENDENCY` warning for T0001 is metadata preserved by the guard; it is not the new error.

## A005 — supplemental test-source evidence repair

- Added `tests/test_android_manual_retry_contract.py` as a top-level test source. It reads the actual production Kotlin files and Android JVM test source; it does not replace or simulate the real Gradle/JVM acceptance.
- Added the path to T0175 `actual_output_paths` and `evidence`.
- The new source test verifies aggregate status exposure, same-requestId/fresh-attemptId bounded manual retry, latest-outcome filtering, cancellation terminal behavior, explicit Compose Retry callback and the exact passing Android JVM regression names.
- The exact V 3.0.805 Python/graph quality run is pending. T0175 is intentionally retained as `in_progress` until the new tests and graph guard pass.
