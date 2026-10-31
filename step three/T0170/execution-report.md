# T0170 — execution report

## Task outcome

Status: `in_progress`. The request-wide budget is implemented in the model, selector, dispatcher and Android runtime. Three exact-head CI attempts have exposed source-contract/KtLint issues and each has been retained in the execution history. A004 corrects the remaining KtLint indentation; exact-head CI is pending. The task is not complete yet.

The source requirement does not set numeric ceilings. The explicit first-release policy is therefore **3 rounds maximum**, **3 hosts per round**, and **9 total host attempts per request**. A request may select lower ceilings; invalid or over-cap configurations are rejected.

## Dependency review

- `T0169 → T0170`: the original `blocks` edge was retained and validated as `contract_or_interface_dependency`. T0170 extends the next-round selector contract delivered by T0169 to take request-wide progress, while continuing to consume its canonical request, previous-round attempts and completed response collection. Evidence is listed in the graph edge and in `evidence/verification.md`.
- `T0160 → T0170`: the original `blocks` edge was retained and validated as `contract_or_interface_dependency`. The bounded selector/dispatcher continue to consume the canonical generated `InferenceRequest` and payload-builder contract from T0160; no Protobuf wire-schema change was made.
- `T0170 → T0171` remains `unreviewed`. Its type and execution-order placement were not changed.

## Implementation

- Added first-release constants and validated per-request `maxRounds` / `maxTotalHosts` ceilings to `DistributedRequest`.
- Added immutable `DistributedRequestProgress` for dispatched rounds and selected host-attempt counts.
- Added bounded first-round and next-round host selection; next-round selection requires matching request identity, an empty completed response set, valid progress, and remaining round/host budgets.
- Added dispatcher rejection results for exhausted rounds, exhausted total-host capacity and inconsistent budget state before any transport submission.
- Added serialized per-request dispatch accounting in `AndroidInferenceRequestRuntime`. Reservations are retained conservatively through cancellation; ordinary rejected dispatches restore the prior budget state.
- This task does not claim an autonomous retry loop, persistent cross-process budget ledger or the cross-round duplicate-host policy assigned to T0171. Callers must pass runtime progress to the next-round selector.

## CI findings and corrections

- **A001 / V 3.0.717:** Step Three quality gate failed (run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757)): 308 Python tests ran and one source-contract assertion failed because an existing test expected the explicit `distributedRequest.requestId` key expression. Android KtLint also failed at the new `distributedRequest` helper in `InferenceAttemptDispatcherTest.kt`; APK/JVM/Protobuf steps were skipped. Windows WPF and HostLifecycle passed.
- **A002 / V 3.0.718:** Step Three quality gate passed (run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620)); HostLifecycle passed. Android KtLint reported that the helper body expression should be on the signature line. APK/JVM/Protobuf steps were skipped.
- **A003 / V 3.0.719:** Step Three quality gate passed (run [41217272330](https://github.com/artinrahimii/project/actions/runs/41217272330)); HostLifecycle passed. KtLint accepted the signature/body placement but identified argument indentation at `InferenceAttemptDispatcherTest.kt:436–441`. APK/JVM/Protobuf steps were skipped.
- **A004 correction:** aligned the helper constructor arguments and closing parenthesis with the required four-space body indentation. The exact-head Android/product and quality checks are pending; no pass is claimed yet.

## Changed files

- Four Android production Kotlin files under `src/Client.Android/app/src/main/java/com/whitep/client/requests/`.
- Four Android JVM tests, four Python source-contract tests, and T0170 report/evidence.
- Runtime contract-string compatibility was restored in A002; test helper KtLint line placement and indentation were corrected in A003/A004.
- No Protobuf wire schema was changed.

## Verification

- Regression tests cover upper bounds, reduced configured limits, partial final host batches, exhausted budgets, request-ID mismatch and invalid progress.
- Exact-head quality gates passed for A002 and A003. Android compile and JVM tests have not yet run because KtLint stopped the workflow on those revisions.
- Local Gradle/Python execution was not available in this connected GitHub session.
- T0169's selector contract revalidation remains `in_review` until the exact-head Android JVM suite passes.
- Inspect KtLint, assembleDebug, testDebugUnitTest, generated Protobuf, Windows WPF, HostLifecycle and the Step Three quality gate for A004 before marking T0170 `done`.
