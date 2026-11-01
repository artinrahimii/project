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

## Tests added and exact-head execution

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

The exact V 3.0.768 outcomes are recorded below. Passing unit/build checks do not prove live request-screen integration.


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


## Exact V 3.0.768 CI outcome (2026-11-01)

- Android main/test KtLint — PASS.
- Android `assembleDebug` — PASS.
- Android `compileDebugUnitTestKotlin` and `testDebugUnitTest` — PASS; the three listed T0126 test classes were included in this test task.
- Generated Protobuf Java/Kotlin verification — PASS.
- Windows WPF build/startup/security/DPAPI and agent-tool policy/capability checks — PASS.
- HostLifecycle unit/integration — PASS.
- Step Three quality gate — PASS: 312 Python tests, syntax compilation and graph/recorded-output/evidence checks passed (0 errors, 1 existing warning).
- Rendezvous container build/HTTP smoke — PASS.
- Exact workflow links: [product workflow](https://github.com/artinrahimii/project/actions/runs/41309472601), [Step Three quality](https://github.com/artinrahimii/project/actions/runs/41309472680), [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41309472603).

The live acceptance gap remains: `MainActivity.requestScreen` is prepare-only, and there is no production caller passing complete validated `HostAnnouncement` capability snapshots into `selectReachableHostsWithFeedback` / dispatch and recording feedback in the request UI. T0126 stays `in_progress`.


## T0126-A003 — exact V 3.0.769 live discovery/feedback audit (blocked)

- **Base code:** `c2db7d9e848567f21206bbcd55408b36ae1c7c8b` (`V 3.0.769`).
- `MainActivity.requestScreen` calls only `prepareRequest`; no production call site invokes `selectReachableHostsWithFeedback` or `recordHostSelectionFeedback`.
- The Android Rendezvous directory result is metadata-only (peer ID, TCP route, expiry). The Windows peer announcement publishes only `inference.v1`; it carries no model/concurrency/payload capability snapshot.
- The Windows `GetPeerRuntimeStatusAsync` provider currently returns state and active request count without `HostStatus.models`. This cannot satisfy the selector's live matching-model requirement.
- No live acceptance test was possible because the production flow does not exist. No fake candidate data was introduced. T0126 is recorded as `blocked` pending the real capability/discovery and request-screen integration described in the execution report.
- Incoming dependency classifications remain unchanged. No outgoing `unreviewed` edge was reclassified or removed.


## T0126-A004 — capability-source implementation (pending exact-head CI)

- Added `IInferenceModelCapabilityProvider` and `HostPeerRuntimeSnapshot` in the application layer.
- `LlamaCppServerBackend.GetModelCapabilityAsync` verifies the configured model is present in `/v1/models` and reads `n_ctx` from `GET /props`; malformed, missing, zero or unavailable values return no capability.
- The Windows peer status callback publishes the Protobuf `protocol_version` and only includes `ModelCapability` after a fresh READY/readiness-checked snapshot.
- Added `HostPeerRuntimeCapabilitiesAcceptanceTests` to the HostLifecycle test executable.
- Exact-head build and test results are pending. This attempt does not complete the Android discovery/selection/request UI integration, so T0126 remains blocked.
