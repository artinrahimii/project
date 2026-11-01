# T0126 execution report — shortage when fewer than three compatible hosts exist

- **Base:** exact online `main` commit `bd2a3deecfaa0798709177fa04db4606f62c4755` (`V 3.0.766`).
- **Task state:** `in_progress`; implementation, focused acceptance tests and exact V 3.0.768 CI are verified, but the current Android request screen remains a prepare-only preview with no live discovery/selection/dispatch caller.
- **T0125 prerequisite:** verified complete on the exact V 3.0.766 commit. Its live-selector test chooses three distinct PeerIds, Android main/test KtLint, debug APK, JVM tests, Protobuf verification and Step Three quality all passed. The full product/host and Rendezvous smoke jobs also passed. See `step three/T0125/execution-report.md`.
- **T0115 prerequisite:** verified complete from `step three/T0115/execution-report.md` and `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`; the report records the disconnect-recovery, invalid-frame, sequential-connection and shutdown acceptance tests passing.

## Dependency-edge review

- `T0125 → T0126`: original edge type remains `blocks`; classification is now `contract_or_interface_dependency`. T0126 consumes the exact count/list of validated selected `HostCandidate` records from the selector and the dispatcher's established rule: one to three distinct candidates can be dispatched, while an empty list returns `NO_SELECTED_HOSTS` before any send. This is a direct output/interface dependency.
- `T0115 → T0126`: original edge type remains `blocks`; classification is now `sequencing_only`. T0115's acceptance suite validates the separate legacy length-framed TCP listener lifecycle; T0126 does not call that listener or consume its frame-reader interface. Preserve the conservative edge/order, but do not assert a technical dependency.
- No edge was removed and neither original `blocks` edge label was rewritten.

## Behaviour implemented

1. The graph and requirements do not define three as a mandatory minimum. The existing dispatcher explicitly accepts a non-empty list containing one or two candidates; it only rejects an empty list as `NO_SELECTED_HOSTS`.
2. Added `HostSelectionFeedback`, calculated from the *post-probe* compatible/reachable candidate count, not raw Rendezvous entries. It reports the exact available count and shortfall.
3. Added `HostSelectionResult` to couple the exact selected candidates to the matching feedback count and validate distinct, nonblank PeerIds.
4. Added `AndroidInferenceRequestRuntime.selectReachableHostsWithFeedback`, preserving the existing API and returning the candidate list plus feedback.
5. Added a feedback field and recording/clearing methods to `RequestViewModel`; `MainActivity` renders that message when supplied. The message is in-memory only and contains no PeerIds or peer metadata.
6. Policy: when one or two valid hosts exist, proceed with those available hosts and inform the user that the round will continue with that number. With zero hosts, state that no compatible/reachable host is available and the request was not sent; the dispatcher already returns `NO_SELECTED_HOSTS` and submits nothing.

## Changed outputs

The expected generic Shared.Networking/Windows path is not the real consumer in this repository. The selector and dispatch boundary are in the Android client, and the relevant user state and Compose screen are under `com.whitep.client.ui.request`; implementation was placed at those actual architecture boundaries rather than adding an unrelated networking project or moving code into the Windows host.

- `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostSelectionFeedback.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostSelectionFeedbackTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`

## Acceptance tests and exact-head CI result

- `HostSelectionFeedbackTest`: zero, one, two and three available candidates; exact message and continue/no-continue decisions; reject counts above the requested ceiling.
- `InferenceAttemptDispatcherTest`: one host dispatches, two hosts dispatch, zero hosts returns `NO_SELECTED_HOSTS` and performs no transport submit.
- `RequestViewModelTest`: the two-of-three shortage message propagates into UI state without storing host identity data; one and zero host cases produce the intended policy messages.
- Exact V 3.0.768 CI passed Android KtLint, debug APK build, JVM `testDebugUnitTest`, generated Protobuf Java/Kotlin verification and Step Three quality checks. Exact logs are recorded in A002 below. This verifies automated contracts, not the live request-screen path.

## Remaining integration boundary

The actual `MainActivity.requestScreen` still describes itself as a prepare-only preview; it calls `prepareRequest`, and there is no production caller that obtains a complete `HostAnnouncement` collection and then invokes `selectReachableHostsWithFeedback` / dispatch. The Connections screen also explicitly says peer discovery is not available. The message-rendering state is wired to Compose, but the runtime producer is not called by that preview screen. This is tracked by the separate blocked request-screen/discovery integration work (including T0174); do not mark T0126 complete or claim a live user notification until a real production flow supplies this feedback.

## Next

The exact V 3.0.768 Android/Windows/HostLifecycle/Step Three/Rendezvous workflows passed. Next, wire the feedback producer into a real Android request flow backed by validated discovery snapshots, then test the end-to-end user notification before setting T0126 to `done`.


## T0126-A001 — exact V 3.0.767 CI result

- **Commit:** `431d25136b0031523e4190cf3a5cb6660080112c` (`V 3.0.767`).
- **Android KtLint:** passed.
- **Android Debug APK:** `assembleDebug` passed.
- **Android JVM:** failed at test-source compilation. `RequestViewModelTest.kt` referenced `HostSelectionFeedback` at lines 47, 58 and 69 without importing `com.whitep.client.requests.HostSelectionFeedback`. The JVM assertions did not execute; generated Protobuf Java/Kotlin verification was skipped.
- **Step Three:** all 312 Python tests passed and Python syntax validation passed. The graph guard failed with `DONE_WITHOUT_TEST_FILE_EVIDENCE: T0125`, because the T0125 `evidence` list did not include a source path under `tests/` or `test/`. The existing T0001 unmapped-dependency metadata warning was preserved.
- **Windows/HostLifecycle:** WPF build/startup/security/DPAPI checks and HostLifecycle unit/integration passed.
- **Rendezvous:** container build and HTTP smoke passed.
- **Outcome:** A001 is recorded as failed verification. No Android test assertion for T0126 is claimed as passed.

## T0126-A002 — import and evidence repair

- Added the missing `HostSelectionFeedback` import to `RequestViewModelTest.kt`.
- Added `tests/test_android_host_candidate_selector_contract.py` and `tests/test_android_inference_attempt_dispatcher_contract.py` to T0125's graph evidence. These are the actual Python source-contract tests for the completed candidate selection and pre-send dispatch invariants.
- No production runtime behaviour was changed in this correction.
- Exact V 3.0.768 CI is pending. T0126 remains `in_progress`: the request screen still has no live production caller that obtains complete `HostAnnouncement` snapshots and invokes `selectReachableHostsWithFeedback`/dispatch. Conditional message rendering alone is not proof that a real user was notified.


## T0126-A002 — exact V 3.0.768 CI result (2026-11-01)

- **Tested commit:** `ffa77ceeb53648f9e450f87820dace9d1b48ffff` (`V 3.0.768`), exact online `main` head.
- **Android:** main/test KtLint, `assembleDebug`, `compileDebugUnitTestKotlin`, `testDebugUnitTest` and generated Protobuf Java/Kotlin verification all passed. The test task includes the T0126 `HostSelectionFeedbackTest`, `InferenceAttemptDispatcherTest` and `RequestViewModelTest` cases.
- **Windows WPF:** Release build, startup, security/DPAPI and agent-tool policy/capability checks passed.
- **HostLifecycle:** unit and integration suite passed.
- **Step Three:** 312 Python tests passed; `compileall` and graph/recorded-output/evidence guard passed (0 errors, 1 existing warning).
- **Rendezvous:** container build and HTTP smoke passed.
- **Workflow evidence:** [Android/Windows/HostLifecycle](https://github.com/artinrahimii/project/actions/runs/41309472601), [Step Three quality](https://github.com/artinrahimii/project/actions/runs/41309472680), [Rendezvous container](https://github.com/artinrahimii/project/actions/runs/41309472603).
- **Outcome:** the A001 missing-import failure is repaired and exact-head tests pass. T0126 remains `in_progress`, not `done`: `MainActivity.requestScreen` is still prepare-only and no production caller obtains validated `HostAnnouncement` snapshots, invokes selection/dispatch, and records feedback. Unit-test success does not prove a user can receive this message in a live request.
