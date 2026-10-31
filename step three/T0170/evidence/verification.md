# T0170 — verification evidence

## Revisions

- Initial base: `6338ed92c0c7508a9b07b3ee0eafd3a1abaff20a` (`V 3.0.716`), branch `main`.
- A001 implementation commit: `17077b464d0946d28789d0a4f8378f80bee09845` (`V 3.0.717`).
- A002 guard-expression correction: `63be6aa51b229ecf4f9d49565bee04e9c0a8a4d8` (`V 3.0.718`).
- A003 KtLint helper-layout correction: pending commit.

## Reviewed source evidence

- `step one/V1.0.md` identifies T0170 but does not prescribe numeric limits. The chosen first-release ceilings (3 rounds, 3 hosts per round, 9 total hosts) are recorded in the execution report and source constants.
- `HostCandidateSelector.kt` previously limited each round to three and supported selecting the next batch after an empty completed response window.
- `DistributedRequest.kt` previously validated only that `maxRounds` was positive; it had no first-release upper ceiling or total-host budget.
- `InferenceAttemptDispatcher.kt` previously enforced the three-host per-round maximum but had no request-wide round/host progress checks.
- `InferenceRequestPayloadBuilder.kt` and its tests provide the canonical generated `InferenceRequest` contract consumed by selection and dispatch. T0170 does not change the Protobuf wire schema.
- `step three/T0169/execution-report.md` bounded T0169 to the selector boundary and did not claim a complete automatic multi-round coordinator.

## Exact-head CI evidence

### A001 — V 3.0.717

- Step Three quality gate: **FAILED**, run [41216834757](https://github.com/artinrahimii/project/actions/runs/41216834757): 308 Python tests, one assertion failure in `test_android_inference_round_timer_contract`.
- Android KtLint: **FAILED**, product run [41216834811](https://github.com/artinrahimii/project/actions/runs/41216834811); line 435 in `InferenceAttemptDispatcherTest.kt`. APK, JVM tests and Protobuf checks were skipped.
- Windows WPF: passed. HostLifecycle: passed.

### A002 — V 3.0.718

- Step Three quality gate: **PASSED**, run [41217032620](https://github.com/artinrahimii/project/actions/runs/41217032620).
- Android KtLint: **FAILED**, product run [41217032694](https://github.com/artinrahimii/project/actions/runs/41217032694); the same helper expression needed to be placed on the signature line. APK, JVM tests and Protobuf checks were skipped.
- HostLifecycle: passed. Windows WPF was still running at the last inspection.
- Corrected the helper expression in A003.

### A003 — pending

- Exact-head Step Three quality gate and White-P product build have not yet run on A003.
- Android KtLint, assembleDebug, testDebugUnitTest, generated Protobuf, Windows WPF and HostLifecycle are not yet confirmed for A003.
- Local Gradle/build execution: not run; this connected session has no checked-out repository with a working Git network path.
- No task-wide acceptance or product-release completion is claimed.
