# T0197 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- ConversationContextCapsuleTest covers exact 4 KiB acceptance, UTF-8 byte-count overflow, oversized input before JSON parsing, invalid UTF-8/JSON and configured limits.
- InferenceRequestPayloadBuilderTest covers prompt UTF-8 byte limits, invalid capsules and the 16 KiB serialized request ceiling.
- InferenceResponseCorrelationValidatorTest rejects stale request/attempt/round correlation and late responses past the request deadline.
- RequestViewModelTest rejects a duplicate response for the same request and attempt without replacing the first response card.
- InferenceAttemptDispatcherTest rejects duplicate host identities and duplicate attempt IDs; InferenceRoundResponseCollectorTest covers concurrent response collection and per-host failure outcomes.
- Android testDebugUnitTest passed in Product Build run 41886746018, job 126773137271.

## Source/test paths

- `src/Client.Android/app/src/test/java/com/whitep/client/context/ConversationContextCapsuleTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/protocol/InferenceRequestPayloadBuilderTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/InferenceResponseCorrelationValidatorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundResponseCollectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137271

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.
