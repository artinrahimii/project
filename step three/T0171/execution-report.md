# T0171 — execution report

## Current outcome

Status: `in_progress`. The next-round selector now excludes every peer ID in the request's cumulative attempt history, not only the immediately preceding round. A previously attempted peer can be selected again only when its ID is explicitly supplied in the per-selection `retryableEphemeralHostIds` allowlist. No autonomous retry scheduler or end-to-end multi-round orchestration is claimed.

## Dependency review

- `T0170 → T0171`: validated as `contract_or_interface_dependency`. T0170 supplies request-scoped progress from the Android runtime; T0171 extends the progress contract with the complete ordered peer-attempt history consumed by the selector.
- `T0160 → T0171`: validated as `contract_or_interface_dependency`. The selector verifies the canonical generated `InferenceRequest.request_id` matches `DistributedRequest.requestId` before applying per-request history.
- Both original `blocks` edge types and their conservative ordering are preserved. No unrelated graph edge was changed.

## Implementation

- Added ordered `attemptedEphemeralHostIds` to `DistributedRequestProgress`. The ordered list preserves repeated IDs as separate attempts.
- Added `afterDispatch(hostIds)`, validating one to three nonblank, distinct host IDs per round.
- Updated `AndroidInferenceRequestRuntime` to reserve valid selected peer IDs before transport suspension and to record returned peer IDs after successful dispatch. Cancellation remains fail-closed.
- Updated `HostCandidateSelector.selectNextRound` to require the prior-round attempts to match the suffix of the cumulative history and exclude all historical peers by default.
- Added explicit `retryableEphemeralHostIds` for a one-selection retry exception. Allowlisted IDs must already exist in this request's history; the allowlist is not carried forward automatically.
- Preserved a counter-only compatibility overload for legacy first-round callers. It cannot safely represent multi-round history; selection fails closed after more than one round if complete history is absent.
- Added Kotlin/JVM cases for earlier-round exclusion, explicit retry, invalid retry IDs, missing multi-round history, ordered history, blank IDs and duplicate IDs in one round. Updated runtime integration assertions and its Python source-contract guard.

## Changed files

- `src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`
- `tests/test_android_libp2p_transport_contract.py`
- `step three/T0171/execution-report.md`
- `step three/T0171/evidence/verification.md`
- `step three/T0170/execution-report.md`
- `step three/T0170/evidence/verification.md`
- `step three/project-graph.md`

## Verification

No local Gradle execution is claimed from this connected GitHub session. Exact-head CI is pending for this implementation commit. Required checks: Android KtLint, debug APK build, Android JVM tests, generated Protobuf Java/Kotlin validation, Step Three Python/graph/evidence quality, Windows WPF and HostLifecycle. T0171 and the affected T0169/T0170 contract revalidation will remain in progress until those checks pass.


## A001 — exact-head V 3.0.724 CI result

- Step Three quality gate [41230294822](https://github.com/artinrahimii/project/actions/runs/41230294822): PASS. Python automated tests, syntax, graph structure and recorded-output/evidence validation all succeeded.
- HostLifecycle job 124622910470 in [product workflow 41230294791](https://github.com/artinrahimii/project/actions/runs/41230294791): PASS.
- Windows WPF job 124622910280: C# formatting, Release build, generated Protobuf, startup smoke, agent-tool process/policy/capability checks, DPAPI identity/trust storage and artifact upload all passed.
- Android job 124622910620: KtLint failed in `HostCandidateSelectorTest.kt` only. The log reported five inline `DistributedRequestProgress` argument layouts at lines 195, 217, 237, 257 and 288, plus multiline-expression indentation at lines 357 and 410. The Android APK, JVM tests and generated Protobuf checks were skipped because KtLint is an earlier gate. No Android test success is claimed for V 3.0.724.
- A002 fixes only those reported test-source formatting violations. A fresh product workflow must pass KtLint, APK assembly, JVM tests and generated Java/Kotlin Protobuf checks before T0171 is complete.


## A003 — exact-head V 3.0.725 CI result

- Step Three quality gate [41230521477](https://github.com/artinrahimii/project/actions/runs/41230521477): PASS. Python automated tests, syntax, graph structure and recorded-output/evidence validation passed.
- HostLifecycle job 124623603658 in [product workflow 41230521490](https://github.com/artinrahimii/project/actions/runs/41230521490): PASS.
- Windows WPF job 124623603615: C# formatting, Release build, generated Protobuf C#, startup smoke, process/policy/capability checks, DPAPI identity/trust and artifact upload all passed.
- Android job 124623603397: KtLint failed with seven continuation-indentation findings in `HostCandidateSelectorTest.kt` lines 385–389 and 440–441. APK, JVM tests and generated Protobuf checks were skipped. No Android test pass is claimed for V 3.0.725.
- A004 corrects only those reported indentation findings. A new exact-head product run is required before completing T0171.


## A004 — exact-head V 3.0.726 CI result

- Step Three quality gate [41230929453](https://github.com/artinrahimii/project/actions/runs/41230929453): PASS. Python automated tests, syntax, graph structure and recorded-output/evidence validation passed.
- HostLifecycle job 124624805496 in [product workflow 41230929279](https://github.com/artinrahimii/project/actions/runs/41230929279): PASS.
- Android job 124624805452: KtLint failed on one remaining indentation finding at `HostCandidateSelectorTest.kt:389:1`: the closing delimiter of the multi-round candidate `listOf` was at 16 spaces but required 20. Android APK, JVM tests and generated Protobuf checks were skipped. No Android test pass is claimed for V 3.0.726.
- The Windows WPF job was still running when this correction was prepared; its final result will be recorded separately after workflow completion.
- A005 moves only the reported list-closing delimiter to the required indentation. T0171 remains in progress until Android lint, APK, JVM and Protobuf checks pass.


## A005 — exact-head V 3.0.727 acceptance

T0171 acceptance is satisfied on exact online `main` commit `2590ff32c1e4340672374dceb8930b566c6c0f7b`:

- Step Three quality gate [41231116594](https://github.com/artinrahimii/project/actions/runs/41231116594): PASS — automated Python tests, syntax compilation, graph structure and recorded-output/evidence validation.
- Android KtLint: PASS; `assembleDebug`: PASS; `testDebugUnitTest`: PASS. The Android job ended successfully in [product workflow 41231116520](https://github.com/artinrahimii/project/actions/runs/41231116520). The log records `BUILD SUCCESSFUL` for all three Gradle commands.
- Generated Protobuf Java and Kotlin validation: PASS. The generated Java `InferenceResponseOrBuilder.java` and Kotlin `CancelRequestKt.kt` were both present and validated.
- Windows WPF: PASS — C# formatting, Release build, generated Protobuf, startup smoke, process restrictions, policy rejection, backend/model capability, DPAPI identity/trust and artifact upload.
- HostLifecycle unit/integration tests: PASS.
- Android debug APK and unit-test report artifacts were uploaded by CI.

Three failed KtLint attempts remain in execution history (V 3.0.724, V 3.0.725 and V 3.0.726). The successful V 3.0.727 run closes the formatting gap; those historical failures remain preserved. T0171 is complete; the T0169/T0170 consumer-contract revalidation is passed.
