# T0169 — verification evidence

## Source contract
- `HostCandidateSelector.kt`: next-round API, previous-round response gate and excluded-host filtering.
- `InferenceRoundResponseCollector.kt`: completed collection carries separate validated responses and failures; `awaitAll()` settles all attempts before return.
- `AndroidInferenceRequestRuntime.kt`: `awaitRoundResponses` uses the attempt records retained by dispatch and delegates to the collector.
- `InferenceRequestPayloadBuilder.kt` and `messages.proto`: canonical request/model/protocol/prompt/context-capsule fields retained for host compatibility checks.

## Regression tests added
- `selectsNextThreeDistinctHostsWhenThePreviousRoundHasNoValidResponse`
- `doesNotSelectAnotherRoundWhenThePreviousRoundContainsAValidResponse`
- `failsClosedWhenPreviousRoundAttemptsCannotDefineASafeExclusionSet`

## Exact-head V 3.0.711
- Step Three quality: passed — Python tests, syntax, graph structure and evidence validation.
- HostLifecycle: passed.
- Windows WPF: all workflow checks passed, including formatting, Release build, generated Protobuf, startup smoke, process/policy/capability and DPAPI identity/trust.
- Android KtLint: failed — three continuation-line formatting findings in the newly-added test methods. APK/JVM/Protobuf were skipped.

## V 3.0.712 correction
The three `.isEmpty()` chain calls were moved to the continuation line required by KtLint. No production behavior was changed. Exact-head verification is pending.
