# T0170 — execution report

## Task outcome

Status: `in_progress`. The first-release request budget is implemented at the request model, host-selection and dispatch boundaries. Two exact-head CI attempts exposed independent guard/formatting issues. A003 corrects the Android KtLint helper layout; its exact-head workflows are pending, so T0170 is not marked complete.

The source requirement does not state numeric ceilings. The implementation therefore makes an explicit, bounded first-release decision: at most **3 rounds**, **3 hosts per round**, and **9 host attempts per request** (3 × 3). Individual requests may choose lower limits. The request model rejects values above these ceilings and configurations whose host budget exceeds their round capacity.

## Dependency review

- `T0169 → T0170`: retained graph edge type `blocks`; validated as `contract_or_interface_dependency`. T0169 supplies deterministic three-host selection and the completed-round response/attempt result consumed by the request-budget-aware next-round selector. Evidence: `HostCandidateSelector.kt`, `HostCandidateSelectorTest.kt`, `InferenceRoundResponseCollector.kt`, and `step three/T0169/execution-report.md`.
- `T0160 → T0170`: retained graph edge type `blocks`; validated as `contract_or_interface_dependency`. The selector and dispatcher continue to consume the canonical generated `InferenceRequest` and payload-builder contract delivered by T0160. T0170 adds budget guards around those established interfaces without changing the Protobuf wire schema. Evidence: `InferenceRequestPayloadBuilder.kt`, `InferenceRequestPayloadBuilderTest.kt`, `HostCandidateSelector.kt`, `InferenceAttemptDispatcher.kt`, and `step three/T0160/execution-report.md`.
- The outgoing `T0170 → T0171` edge remains `unreviewed`; its type and execution order were not changed.

## Implementation

- Added first-release constants and validated per-request `maxRounds` / `maxTotalHosts` ceilings to `DistributedRequest`. A lower configured maximum is supported; defaults scale with the configured round count.
- Added immutable `DistributedRequestProgress` to track rounds dispatched and total selected host attempts, including queued, rejected, failed and timed-out submissions.
- Added first-round selection bounded by `maxTotalHosts`; next-round selection now requires the matching request ID, an empty completed response window, valid progress, remaining rounds and remaining host slots.
- Added fail-closed dispatcher checks before transport submission for round exhaustion, total-host exhaustion and inconsistent progress.
- The Android runtime serializes dispatches for the same request, keeps budget progress in process memory, exposes the current progress to the next selection decision, and reserves selected host slots before suspending in transport. A cancellation during a potentially partial send retains the reservation rather than allowing a retry to exceed the cap. An ordinary rejected dispatch restores the previous progress.
- No autonomous retry loop, persistent budget store, cross-installation counter or T0171 all-round duplicate-host policy is claimed. The runtime continues to expose one-round dispatch/collection primitives; a caller must use the runtime's progress snapshot when asking the selector for a subsequent batch.

## CI findings and corrections

- **A001 / V 3.0.717:** Step Three quality gate failed (run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757)): 308 Python tests ran and one source-contract assertion failed because an existing test expected the explicit `distributedRequest.requestId` key expression. Android KtLint also failed at `InferenceAttemptDispatcherTest.kt:435`; APK, JVM and Protobuf checks were skipped. Windows WPF and HostLifecycle jobs passed.
- **A002 / V 3.0.718:** Step Three quality gate passed (run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620)); HostLifecycle passed. Android KtLint failed again at `InferenceAttemptDispatcherTest.kt:435` with `First line of body expression fits on same line as function signature`, so APK, JVM tests and Protobuf validation were skipped. Windows WPF was still running when inspected.
- **A003 correction:** moved `DistributedRequest(` onto the closing signature line as `) = DistributedRequest(` in the test helper. This is a formatting-only fix; its exact-head Android and quality workflows are pending. No pass is claimed yet.

## Changed files

- Four Android production Kotlin files under `src/Client.Android/app/src/main/java/com/whitep/client/requests/`.
- Four Android JVM test files, including the native libp2p runtime acceptance test.
- Four Python source-contract tests.
- A002 restores the exact request-property expression in `AndroidInferenceRequestRuntime.kt` for the existing round-timer contract guard.
- A003 fixes the KtLint line-layout issue in `InferenceAttemptDispatcherTest.kt`.
- `step three/T0170/execution-report.md`, `step three/T0170/evidence/verification.md`, and `step three/project-graph.md`.

## Verification

- Added JVM coverage for the configured ceilings, a partial final host batch, exhausted round/host budgets, mismatched request IDs and invalid progress.
- Added Python source-contract coverage for the request policy, selector, dispatcher and runtime progress state.
- Local Gradle/Python execution was not available from this connected GitHub session; no local test pass is claimed.
- T0169's selector API was extended to consume request progress. Its contract revalidation remains `in_review` until the exact-head Android JVM suite passes on the current main commit.
- Inspect Android KtLint, APK build, JVM tests, generated Protobuf, Windows/HostLifecycle, and Step Three quality gate on the exact A003 commit before marking T0170 `done`.
