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

## Exact-head V 3.0.711, V 3.0.712 and V 3.0.713
- Step Three quality passed in all three revisions.
- HostLifecycle passed in all three revisions.
- All Windows WPF formatting, Release build, generated Protobuf, startup smoke, process/policy/capability, DPAPI identity/trust and artifact steps passed in all three revisions.
- Android KtLint failed in each revision on new test-source layout; all APK/JVM/Protobuf stages were skipped.

## V 3.0.714 correction
The three nested assertions use the leading-dot receiver layout and now indent all arguments and closing lines one additional four-space level as required by the prior exact-head KtLint diagnostics. Production behavior is unchanged. Exact-head verification is pending.
