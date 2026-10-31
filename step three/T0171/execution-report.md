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
