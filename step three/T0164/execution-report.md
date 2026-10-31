# T0164 execution report

## Attempt T0164-A001 — injectable round timer and shared deadline budget

- Base commit: fa9f061f8abef825857d9977535696ae06e8f0c2 (V 3.0.682).
- Prerequisites: T0160 and T0163 are verified complete. T0163's three-native-.NET-peer test passed on V 3.0.682; all 105 Android JVM tests passed.
- Edge review: T0160 -> T0164 and T0163 -> T0164 are validated contract_or_interface_dependency edges. The first carries absolute request-deadline semantics; the second supplies the real round-dispatch/attempt-correlation interface.
- Implementation: InferenceRoundTimer uses an injected java.time.Clock, configurable Duration and an immutable per-round deadline capped by the request's absolute deadline. AndroidInferenceRequestRuntime creates one fresh window per dispatch and tracks it for QUEUED attempts. InferenceAttemptDispatcher bounds each host submission by the same remaining budget. awaitResponse caps the requested timeout by the remaining round budget and removes its attempt timer entry on completion/failure.
- If a round is already expired, dispatch is rejected before any submit. If the deadline expires mid-dispatch, already queued attempts remain QUEUED and unsubmitted remaining attempts become TIMED_OUT; no remote delivery is claimed for those attempts.
- Production runtime factory and internal test constructor accept injectable Clock and roundTimeout; default is 60 seconds.
- Tests added: five timer JVM tests, two dispatcher timeout tests and six Python source-contract checks.
- Local Gradle/JVM/Python execution was unavailable in this connected editing session. Exact-head CI is required before T0164 can be marked done.
- Task status remains in_progress pending CI.
## Attempt T0164-A001 — V 3.0.683 quality result

- Step Three quality workflow [41185790866](https://github.com/artinrahimii/project/actions/runs/41185790866) failed: 295 tests ran, 294 passed and one new source-contract assertion failed. The test expected `roundWindow.remainingTimeoutMillis()` but the dispatcher correctly uses the nullable-safe `roundWindow?.remainingTimeoutMillis()`.
- HostLifecycle passed in product workflow [41185790885](https://github.com/artinrahimii/project/actions/runs/41185790885). Android fixture compilation and Windows build were still running when the quality failure was recorded.
- The failure is in the new Python expectation, not evidence of a Kotlin/runtime failure. Attempt A002 corrects only the assertion and reruns full CI.
## Attempt T0164-A002 — V 3.0.684 exact-head result

- Step Three quality [41186026119](https://github.com/artinrahimii/project/actions/runs/41186026119) passed all 295 Python tests, compileall and graph/evidence guards.
- Product workflow [41186026207](https://github.com/artinrahimii/project/actions/runs/41186026207): HostLifecycle passed. Android KtLint failed before APK/JVM/Protobuf tests. KtLint identified formatting issues in InferenceAttemptDispatcherTest.kt (long status assertion; MutableClock and RecordingTransport declarations), InferenceRoundTimerTest.kt (MutableClock declaration), and InferenceRoundTimer.kt (long IllegalArgumentException constructor). No Android JVM result is claimed for V 3.0.684.
- A003 applies only the exact reported formatting changes and schedules the full exact-head CI again.
## Attempt T0164-A002 — V 3.0.684 exact-head result

- Step Three quality [41186026119](https://github.com/artinrahimii/project/actions/runs/41186026119) passed all 295 Python tests, compileall and graph/evidence guards.
- Product workflow [41186026207](https://github.com/artinrahimii/project/actions/runs/41186026207): HostLifecycle passed. Android KtLint failed before APK/JVM/Protobuf tests. Reported formatting violations were limited to the new timer-related Kotlin files. No Android JVM result is claimed.
- A003 applied the exact KtLint fixes and reran the full workflow.

## Attempt T0164-A003 — V 3.0.685 exact-head result

- Step Three quality [41186430811](https://github.com/artinrahimii/project/actions/runs/41186430811) passed 295 Python tests, compileall and graph/evidence guards.
- Android KtLint failed with import-order and multiline-expression violations in InferenceAttemptDispatcherTest.kt and InferenceRoundTimerTest.kt, plus two formatting violations in AndroidInferenceRequestRuntime.kt.
- Because KtLint failed, the Android APK build, JVM tests and generated Protobuf checks were skipped. No Android test result is claimed for V 3.0.685.
- Windows WPF Release/build/Protobuf/startup/security/DPAPI passed. HostLifecycle unit/integration passed. Product run: [41186430719](https://github.com/artinrahimii/project/actions/runs/41186430719).
- A004 fixes only the lint-reported import and multiline-expression formatting.
## Attempt T0164-A004 — V 3.0.686 exact-head result

- Step Three quality [41186743157](https://github.com/artinrahimii/project/actions/runs/41186743157) passed.
- Android KtLint [41186743095](https://github.com/artinrahimii/project/actions/runs/41186743095) still failed only in InferenceAttemptDispatcherTest.kt: import order and argument indentation for the two added dispatchRound calls. APK, JVM and Protobuf were skipped.
- Windows WPF and HostLifecycle passed in the same product workflow.
- A005 fixes the final two KtLint findings only.

## Attempt T0164-A005 — V 3.0.687 final exact-head result

- Final source-fix commit: c73c64e284347a4ebc7ca77594be64fd9522dd17 (`V 3.0.687`).
- Step Three quality workflow [41187096865](https://github.com/artinrahimii/project/actions/runs/41187096865) completed successfully. The automated Python tests, Python syntax check, graph validation and recorded-output/evidence guards passed.
- Product workflow [41187096859](https://github.com/artinrahimii/project/actions/runs/41187096859) completed successfully. Android KtLint, debug APK build, Android JVM unit tests, generated Protobuf checks and artifact uploads passed.
- The Windows WPF Release build, C# formatting, Protobuf, startup, process/policy/capability and DPAPI identity/peer-trust checks passed.
- HostLifecycle unit and integration tests passed.
- This is exact-head evidence for `c73c64e284347a4ebc7ca77594be64fd9522dd17`, not a result copied from an earlier commit. Local Gradle was not run in this editing session; GitHub CI executed the actual build and tests.
- All acceptance criteria for T0164 are now verified. Status: **done**. Four earlier attempts failed and remain in the preserved history; A005 is the successful fifth attempt.

