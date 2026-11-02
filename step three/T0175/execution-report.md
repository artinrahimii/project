# T0175 — Manual retry and terminal request status

## Result

**Status: done.** The feature is implemented in the production Android request flow and accepted by exact-head CI on `main` commit `aed7d2c3c49e04b596c457e5f38f680d88027b04` (`V 3.0.803`).

The request screen now shows aggregate `IDLE`, `RUNNING`, `SUCCEEDED`, `PARTIAL`, `FAILED` or `CANCELLED` status. When the latest outcome for one or more hosts is retryable and the original request budget still permits another round, the UI exposes an explicit **Retry failed attempts manually** action. No retry runs automatically.

A manual retry:
- preserves the same logical `requestId`, prompt, model, output limit and context capsule;
- creates fresh per-attempt identifiers through the existing dispatcher;
- uses the existing `DistributedRequestProgress` budget and the existing first-release limits (up to 3 rounds / 9 total host attempts);
- retries hosts whose latest attempt failed, timed out, was rejected or otherwise did not complete successfully; hosts whose latest attempt succeeded or was cancelled are excluded;
- receives and displays a separate correlated response card for each fresh attempt;
- becomes unavailable when the retry budget is exhausted, the draft changes, the directory session is cleared, or a cancellation outcome is recorded.

A `CANCELLED` outcome is terminal and is not presented as a retryable failure.

## Production outputs

- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt` — retained logical request context, aggregate result classification, retry eligibility, request/attempt history and bounded explicit retry operation.
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt` — aggregate status text and manual Retry button.
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt` — request-level retry regressions plus terminal cancellation assertion.
- `step three/T0175/evidence/verification.md` — exact source, dependency and CI evidence.

## Exact-head acceptance — V 3.0.803

All required workflows completed successfully on the same tested commit:

- [Android product job](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183843391): KtLint, debug APK, JVM suite, generated Protobuf Java/Kotlin and artifact uploads passed.
- [Android JVM report artifact](https://github.com/artinrahimii/project/actions/runs/41417197342/artifacts/13127773181): **183 tests, 0 failures, 0 ignored, 100% success**, 38.912 seconds. The `RequestViewModelTest` class contains 14 tests, 0 failures and 0 ignored. Both `showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds` and `manualRetryExcludesHostsWhoseLatestAttemptAlreadySucceeded` passed.
- [Windows WPF build/startup/security/DPAPI job](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183844243): passed.
- [HostLifecycle unit/integration job](https://github.com/artinrahimii/project/actions/runs/41417197342/job/125183843834): passed.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41417197306): all 317 Python tests, Python syntax compilation, graph structure and recorded-output/evidence validation passed.
- [Rendezvous container build and HTTP smoke](https://github.com/artinrahimii/project/actions/runs/41417197168): production image build and non-root HTTP contract smoke passed.

## Retained failed attempts

The execution ledger keeps the two earlier failures rather than deleting or rewriting them:

- **A001 / V 3.0.801** — Python claims-audit test required the exact no-payload disclosure phrase; Android KtLint also identified status-chain and function-signature formatting. The no-host wording was restored in V 3.0.802 and the Kotlin formatting was fixed in V 3.0.803.
- **A002 / V 3.0.802** — all 317 Python/graph/evidence checks passed, but Android KtLint still rejected the two formatting patterns. V 3.0.803 corrected them and reran the full product pipeline.

## Dependency and scope notes

The incoming T0174 and T0160 edges were already validated as `contract_or_interface_dependency` and their prerequisites are done. No dependency edge was changed. The outgoing `T0175 -> T0176` edge remains `unreviewed`; it was not reclassified or removed.

This acceptance proves the Android request UI/ViewModel flow and its bounded retry behavior under the test suite. It does not claim physical two-device or independent-public-network acceptance; the separate final-acceptance tasks remain deferred.

## Next action

Review all incoming edges and evidence for T0176 before starting it. Keep unreviewed edges unchanged until the affected contracts are actually inspected.
