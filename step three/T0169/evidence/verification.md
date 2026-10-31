# T0169 — verification evidence

## Source contract
- `HostCandidateSelector.kt`: next-round API, previous-round response gate, and excluded-host filtering.
- `InferenceRoundResponseCollector.kt`: completed collection carries separate validated responses and failures; `awaitAll()` settles all attempts before return.
- `AndroidInferenceRequestRuntime.kt`: `awaitRoundResponses` uses the attempt records retained by dispatch and delegates to the collector.
- `InferenceRequestPayloadBuilder.kt` and `messages.proto`: canonical request/model/protocol/prompt/context-capsule fields retained for host compatibility checks.

## Regression tests added
- `selectsNextThreeDistinctHostsWhenThePreviousRoundHasNoValidResponse`
- `doesNotSelectAnotherRoundWhenThePreviousRoundContainsAValidResponse`
- `failsClosedWhenPreviousRoundAttemptsCannotDefineASafeExclusionSet`

## Execution result
The exact-head CI outcome is pending. This file records the actual implementation and test cases, not a claim that they have passed.
