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

## Exact-head V 3.0.711 and V 3.0.712
- Both Step Three quality gates passed.
- Both HostLifecycle workflows passed.
- All Windows WPF formatting, Release build, generated Protobuf, startup smoke, process/policy/capability, DPAPI identity/trust and artifact steps passed.
- Android KtLint failed on test-source fluent-call layout in both revisions; the Android APK/JVM/Protobuf stages were skipped.

## V 3.0.713 correction
The three assertions now use the leading-dot continuation layout for `HostCandidateSelector.selectNextRound` and keep `.isEmpty()` on the same line as the close parenthesis. This is a formatting-only test-source change. Exact-head verification is pending.
