# T0170 — execution report

## Task outcome

Status: `in_progress`. Request-wide round and host ceilings are implemented across the model, selector, dispatcher and runtime. A001–A006 are now recorded with their exact online commit SHAs and observed CI outcomes. A006 cleared KtLint and built the APK, but Android JVM test compilation exposed a missing import in the cross-package network test. A007 adds that import and a guard against its regression. Exact-head CI for A007 is pending; T0170 is not complete.

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
- **A002 / V 3.0.718:** Step Three quality gate passed (run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620)); Android KtLint failed at helper signature/body placement. HostLifecycle and Windows WPF passed; APK/JVM/Protobuf were skipped.
- **A003 / V 3.0.719:** Step Three quality gate passed (run [41217272330](https://github.com/artinrahimii/project/actions/runs/41217272330)); Android KtLint failed on helper indentation. HostLifecycle and Windows WPF passed; APK/JVM/Protobuf were skipped.
- **A004 / V 3.0.720:** Step Three quality gate passed (run [41217532369](https://github.com/artinrahimii/project/actions/runs/41217532369)); HostLifecycle and Windows WPF passed. Android KtLint reported five production-source formatting violations; APK/JVM/Protobuf were skipped.
- **A005 / V 3.0.721:** Step Three quality gate passed (run [41217933594](https://github.com/artinrahimii/project/actions/runs/41217933594)); Windows WPF and HostLifecycle passed in product run [41217933725](https://github.com/artinrahimii/project/actions/runs/41217933725). KtLint cleared four findings but reported one remaining multiline `minOf` formatting violation at `DistributedRequest.kt:78`. APK/JVM/Protobuf were skipped.
- **A006 / V 3.0.722:** Step Three quality, Android KtLint, Android debug APK, Windows WPF and HostLifecycle passed. Android JVM test compilation failed because `Libp2pInferenceRequestTransportTest.kt` in `com.whitep.client.network` referenced `DistributedRequestProgress` but did not import it from `com.whitep.client.requests`. The failure is recorded in run [41225183725](https://github.com/artinrahimii/project/actions/runs/41225183725); JVM tests did not execute and generated-Protobuf verification was skipped.
- **A007 correction:** added the missing import and a Python contract assertion that protects the cross-package dependency. No runtime behavior or wire contract changed. Exact-head CI on V 3.0.723 is pending.

## Changed files

- Four Android production Kotlin files under `src/Client.Android/app/src/main/java/com/whitep/client/requests/`.
- Four Android JVM tests; A007 imports the request progress type into the network test.
- Four Python source-contract tests; A007 asserts that the progress type is explicitly imported.
- `step three/T0170/execution-report.md`, `step three/T0170/evidence/verification.md`, and `step three/project-graph.md`.

## Verification

- Regression tests cover upper bounds, reduced per-request limits, partial final batches, exhausted budgets, request-ID mismatch and invalid progress.
- A006's Android KtLint and debug APK succeeded; the first Android JVM test compilation then found the missing import before test execution.
- A006 Step Three quality, Windows WPF and HostLifecycle passed. Its generated-Protobuf verification was skipped after the JVM compile failure.
- Local Gradle/Python execution was not available in this connected GitHub session.
- T0169 selector contract revalidation remains `in_review` until the exact-head Android JVM suite passes.
- Inspect Android KtLint, APK build, JVM tests, Protobuf, Windows WPF, HostLifecycle and Step Three quality for A007 before marking T0170 `done`.
