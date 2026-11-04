# T0198 — verification evidence

## Exact-head source

- Repository: `artinrahimii/project`
- Branch: `main`
- Commit tested: `a074815979aff371650c5101c514989f42180968`
- Commit message: `V 3.0.922`

## Acceptance evidence

- HostCandidateSelectorTest covers distinct ready hosts, stable order, compatibility, model/capsule/payload constraints, exclusion across rounds, explicit retry allowlisting and exhausted budgets.
- InferenceRoundTimerTest covers injected clock, absolute-deadline caps, expired deadlines and bounded response timeout.
- InferenceAttemptDispatcherTest covers round/total-host limits, duplicate host/attempt identity, expired round windows and bounded dispatch.
- DistributedRequestTest covers distinct request IDs, recorded round progress and duplicate peer IDs.
- RequestViewModelTest covers duplicate correlated response rejection and manual retry with the original request identity and fresh attempt IDs.
- Android testDebugUnitTest passed in Product Build run 41886746018, job 126773137271.

## Source/test paths

- `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundTimerTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`

## CI evidence

- https://github.com/artinrahimii/project/actions/runs/41886746018/job/126773137271

## Scope note

Only the acceptance criteria supported by the listed source and successful CI runs are recorded as complete. A passing task does not imply that later network/device/release tasks have passed.
