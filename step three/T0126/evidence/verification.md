# T0126 verification evidence

## Dependencies checked

- `T0125 → T0126`: `blocks` retained, `contract_or_interface_dependency`, validated against `HostCandidateSelector.kt`, `InferenceAttemptDispatcher.kt`, `HostCandidateSelectorTest.kt` and the T0125 report. The downstream task consumes selected candidate count and the 1–3/empty dispatch contract.
- `T0115 → T0126`: `blocks` retained, `sequencing_only`, validated against the T0115 execution report and `NetworkInferenceRequestFrameAcceptanceTests.cs`. T0126 does not consume that legacy TCP listener API.

## Source evidence

- `HostSelectionFeedback.kt`: constrains available count to 0..requested (requested count at most three), derives shortfall, dispatch permission and exact user-facing text.
- `AndroidInferenceRequestRuntime.kt`: `selectReachableHostsWithFeedback` derives the feedback only from the final selected candidate list produced by live status probing.
- `InferenceAttemptDispatcher.kt`: empty selected-host lists return `NO_SELECTED_HOSTS` before submission; one/two distinct hosts are not rejected as an underfilled round.
- `RequestViewModel.kt` and `MainActivity.kt`: store the feedback message in transient UI state and render it when a live caller supplies it. No peer IDs are stored in the message.
- `MainActivity.requestScreen` remains prepare-only, so the producer is not yet connected to the live screen/dispatch path. This prevents a completion claim for real user notification.

## Tests added; status pending

- `HostSelectionFeedbackTest.zeroHostsNeverAllowsDispatchAndStatesThatTheRequestWasNotSent`
- `HostSelectionFeedbackTest.oneHostAllowsDispatchAndReportsTheExactShortfall`
- `HostSelectionFeedbackTest.twoHostsAllowDispatchAndReportTheExactShortfall`
- `HostSelectionFeedbackTest.threeHostsReportThatTheRequestedCapacityIsAvailable`
- `InferenceAttemptDispatcherTest.continuesWithTwoAvailableDistinctHostsAndSubmitsBoth`
- `InferenceAttemptDispatcherTest.continuesWithOneAvailableHostAndSubmitsIt`
- `InferenceAttemptDispatcherTest.rejectsZeroAvailableHostsWithoutSubmittingAnyRequest`
- `RequestViewModelTest.reportsWhenTwoOfThreeReachableHostsAreAvailableWithoutStoringPeerMetadata`
- `RequestViewModelTest.reportsOneAvailableHostAndAllowsTheRequestToContinue`
- `RequestViewModelTest.reportsNoAvailableHostAndExplainsThatNothingWasSent`

No CI/test outcome is claimed until the exact commit workflow is inspected.


## Exact V 3.0.767 CI outcome

- Android main/test KtLint: passed.
- Android `assembleDebug`: passed.
- Android `compileDebugUnitTestKotlin`: failed at `RequestViewModelTest.kt:47,58,69` because the test source did not import `HostSelectionFeedback`; JVM test assertions and generated Protobuf verification were not executed.
- Step Three Python test suite: 312 passed; syntax check passed.
- Step Three graph guard: failed on `DONE_WITHOUT_TEST_FILE_EVIDENCE: T0125`; T0125's evidence list lacked a `tests/` or `test/` source path.
- Windows WPF, HostLifecycle and Rendezvous container smoke: passed.
- Exact logs: [Android job](https://github.com/artinrahimii/project/actions/runs/41308558286/job/124853943405), [Step Three quality job](https://github.com/artinrahimii/project/actions/runs/41308558249/job/124853943020), [Rendezvous workflow](https://github.com/artinrahimii/project/actions/runs/41308558375).

## A002 correction

- The missing `com.whitep.client.requests.HostSelectionFeedback` import is added to `RequestViewModelTest.kt`.
- T0125 graph evidence now references both Python contract-test source files required by `graph_guard.py`.
- Retest exact commit V 3.0.768; no test result is presumed.
