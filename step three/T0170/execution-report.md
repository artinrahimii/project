# T0170 — execution report

## Task outcome

Status: `in_progress`. The first-release request budget is implemented at the request model, host-selection and dispatch boundaries. One exact-head quality-gate assertion failed in A001 and was corrected in A002. The A002 exact-head CI result is pending; T0170 is not marked complete.

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

## CI findings and correction

- **A001 / V 3.0.717 — Step Three quality gate FAILED:** run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757), job 124583012254. Of 308 Python tests, one failed: `test_runtime_creates_a_fresh_round_window_and_tracks_queued_attempts` expected the source expression `AttemptKey(distributedRequest.requestId, attempt.attemptId)`; the new equivalent code used the local immutable `requestId`. This was a source-contract text assertion, not an observed runtime behavior failure.
- **A002 correction:** restored the exact request-property expression in `AndroidInferenceRequestRuntime.kt` to keep the existing regression guard intact. The full quality and product workflows must be rerun on the A002 commit; no pass is claimed yet.
- The product workflow for A001 was still running when inspected. Its Android, Windows and HostLifecycle results are not treated as passed.

## Changed files

- Four Android production Kotlin files under `src/Client.Android/app/src/main/java/com/whitep/client/requests/`.
- Four Android JVM test files, including the native libp2p runtime acceptance test.
- Four Python source-contract tests.
- A002 also makes the compatibility-only source-expression correction in `AndroidInferenceRequestRuntime.kt`.
- `step three/T0170/execution-report.md`, `step three/T0170/evidence/verification.md`, and `step three/project-graph.md`.

## Verification

- Added JVM coverage for the configured ceilings, a partial final host batch, exhausted round/host budgets, mismatched request IDs and invalid progress.
- Added Python source-contract coverage for the request policy, selector, dispatcher and runtime progress state.
- Local Gradle/Python execution was not available from this connected GitHub session; no local test pass is claimed.
- T0169's selector API was extended to consume request progress. Its contract revalidation remains `in_review` until the exact-head Android JVM suite passes on the current main commit.
- Inspect Android KtLint, APK build, JVM tests, generated Protobuf, Windows/HostLifecycle, and Step Three quality gate on the exact A002 commit before marking T0170 `done`.
