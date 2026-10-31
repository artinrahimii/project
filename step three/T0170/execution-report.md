# T0170 — execution report

## Task outcome

Status: `in_progress`. Request-wide round and host ceilings are implemented across the model, selector, dispatcher and runtime. Four CI attempts exposed test-contract or KtLint issues. A005 now corrects the five production-source KtLint findings reported on A004; exact-head CI for A005 is pending. The task is not marked complete.

The source requirement does not specify numeric ceilings. The explicit first-release policy is **3 rounds maximum**, **3 hosts per round**, and **9 total host attempts per request**. A request may configure lower limits; invalid or over-cap configurations are rejected.

## Dependency review

- `T0169 → T0170`: the original `blocks` edge is retained and validated as `contract_or_interface_dependency`. T0170 extends T0169's next-round selector contract with request-wide progress while continuing to consume the canonical request, previous-round attempts and completed response collection.
- `T0160 → T0170`: the original `blocks` edge is retained and validated as `contract_or_interface_dependency`. The bounded selector/dispatcher continue to consume T0160's canonical generated `InferenceRequest` and payload-builder contract; no Protobuf wire-schema change is made.
- `T0170 → T0171` remains `unreviewed`; its type and execution order are unchanged.

## Implementation

- Added first-release ceilings and per-request `maxRounds` / `maxTotalHosts` validation to `DistributedRequest`.
- Added immutable `DistributedRequestProgress` for dispatched rounds and selected host-attempt counts.
- Added bounded first-round and next-round selection; later selection requires matching request identity, an empty completed response set, valid progress, and remaining round/host budget.
- Added dispatcher rejection results for exhausted rounds, exhausted total-host capacity and inconsistent budget state before transport submission.
- Added serialized per-request dispatch accounting in `AndroidInferenceRequestRuntime`. Reservations remain conservative through cancellation; normal rejected dispatches restore the previous state.
- No autonomous retry loop, persistent cross-process budget ledger or T0171 cross-round duplicate-host policy is claimed. Callers must pass runtime progress to next-round selection.

## CI findings and corrections

- **A001 / V 3.0.717:** Step Three quality gate failed (run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757)): 308 Python tests, one source-contract assertion failure. Android KtLint failed; APK/JVM/Protobuf were skipped. Windows WPF and HostLifecycle passed.
- **A002 / V 3.0.718:** Step Three quality gate passed (run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620)); Android KtLint failed at the helper's signature/body placement. HostLifecycle and Windows WPF passed; APK/JVM/Protobuf were skipped.
- **A003 / V 3.0.719:** Step Three quality gate passed (run [41217272330](https://github.com/artinrahimii/project/actions/runs/41217272330)); Android KtLint failed on excess indentation in the same helper. HostLifecycle and Windows WPF passed; APK/JVM/Protobuf were skipped.
- **A004 / V 3.0.720:** Step Three quality gate passed (run [41217532369](https://github.com/artinrahimii/project/actions/runs/41217532369)); HostLifecycle and Windows WPF passed. Android KtLint identified five production-source formatting violations: two multiline default expressions in `DistributedRequest.kt`, the `maxHosts` expression in `HostCandidateSelector.kt`, and the boolean expression in `AndroidInferenceRequestRuntime.kt`. APK/JVM/Protobuf were skipped.
- **A005 correction:** adjusted those expressions to KtLint's multiline layout without changing the budget policy or request/transport contracts. Exact-head A005 CI remains pending.

## Changed files

- Four Android production Kotlin files under `src/Client.Android/app/src/main/java/com/whitep/client/requests/`.
- Four Android JVM tests and four Python source-contract tests.
- `step three/T0170/execution-report.md`, `step three/T0170/evidence/verification.md`, and `step three/project-graph.md`.
- A002 restored the literal expression required by an existing source contract; A003/A004 fixed helper layout; A005 fixes five KtLint formatting rules in production code.

## Verification

- Regression tests cover upper bounds, reduced per-request limits, partial final batches, exhausted budgets, request-ID mismatch and invalid progress.
- Exact-head Step Three quality gates passed for A002, A003 and A004. Windows WPF and HostLifecycle also passed for A004.
- Android KtLint has failed on A001–A004; consequently `assembleDebug`, `testDebugUnitTest` and Protobuf validation were skipped on those revisions.
- Local Gradle/Python execution was not available in this connected GitHub session.
- T0169 selector contract revalidation remains `in_review` until the exact-head Android JVM suite passes.
- Inspect KtLint, APK build, JVM tests, Protobuf, Windows WPF, HostLifecycle and Step Three quality for A005 before marking T0170 `done`.
