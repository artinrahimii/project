# T0162 verification evidence — implementation and exact-head CI audit

## Baseline and dependency review

- **Baseline:** `main` at `f4848a6b0a5358ab76e2fa8a1257f18016c9e610` (`V 3.0.608`).
- **Implementation commit:** `2388d218e5622dd545ddd9296fb3a866059f2715` (`V 3.0.609`).
- `T0160 → T0162`: `contract_or_interface_dependency`, validated. The selector reads the canonical `InferenceRequest` created through the T0160 payload-builder contract to compare model, protocol, output-token and byte-size requirements.
- `T0161 → T0162`: `sequencing_only`, validated. The selector consumes no `DistributedRequest` API/field. Deadline and round scheduling remain in later orchestration tasks.
- Original edge type `blocks` and execution order were preserved.

## Implementation

- `HostCandidateSelector.kt` selects up to three distinct, READY and capacity-available peers; validates matching peer identities and protocol versions; requires the requested model in advertised and live host status; and checks output, prompt, capsule and serialized-request bounds.
- Eight Android JVM tests were added in `HostCandidateSelectorTest.kt`.
- Four repository-root source-contract tests were added in `tests/test_android_host_candidate_selector_contract.py`.
- Scope is a deterministic pure selection policy over supplied snapshots. No discovery, remote query, socket connection, dispatch, retry or Android-to-Windows delivery is claimed.

## Exact V 3.0.609 CI

- Step Three quality gate **PASS**: https://github.com/artinrahimii/project/actions/runs/41039680210
- HostLifecycle **PASS**: https://github.com/artinrahimii/project/actions/runs/41039680143 (job 124009119312)
- Android KtLint **FAIL**: https://github.com/artinrahimii/project/actions/runs/41039680143 (job 124009118998), three chained-access formatting violations in `supportsModel`.
- Android APK/JVM/Protobuf steps did not execute because KtLint failed first.
- Windows WPF was still running at the time of the correction; this evidence does not infer its result.

## Correction state

The chained property expression was split into local values and an explicit match-count guard. The code has been updated in the next candidate, `V 3.0.610`; all Android and quality checks must pass on that exact commit before T0162 may be marked done.
