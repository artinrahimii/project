# T0170 — verification evidence

## Base revision and scope

- Branch: `main`
- Base commit: `6338ed92c0c7508a9b07b3ee0eafd3a1abaff20a` (`V 3.0.716`)
- Task: cap per-request round count and total attempted hosts at first-release limits.

## Reviewed source evidence

- `step one/V1.0.md` identifies T0170 but does not prescribe numeric limits. The chosen first-release ceilings (3 rounds, 3 hosts per round, 9 total hosts) are recorded in the execution report and source constants.
- `HostCandidateSelector.kt` previously limited each round to three and supported selecting the next batch after an empty completed response window.
- `DistributedRequest.kt` previously validated only that `maxRounds` was positive; it had no first-release upper ceiling or total-host budget.
- `InferenceAttemptDispatcher.kt` previously enforced the three-host per-round maximum but had no request-wide round/host progress checks.
- `InferenceRequestPayloadBuilder.kt` and its tests provide the canonical generated `InferenceRequest` contract consumed by selection and dispatch. T0170 does not change the Protobuf wire schema.
- `step three/T0169/execution-report.md` bounded T0169 to the selector boundary and did not claim a complete automatic multi-round coordinator.

## Verification status

- Request-model, selector, dispatcher and runtime regression tests: added; awaiting exact-head CI.
- Python source-contract tests: updated; awaiting exact-head quality gate.
- Local Gradle/build execution: not run; this session did not have a checked-out repository or working Git network access.
- No task-wide acceptance or product-release completion is claimed at this stage.
