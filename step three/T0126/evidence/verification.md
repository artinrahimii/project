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


## Exact V 3.0.771 workflow reconciliation

- Step Three quality — PASS: [41312296046](https://github.com/artinrahimii/project/actions/runs/41312296046).
- Rendezvous container and HTTP smoke — PASS: [41312296044](https://github.com/artinrahimii/project/actions/runs/41312296044).
- Windows WPF, Protobuf, startup, security/DPAPI and policy/capability checks — PASS: [41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- HostLifecycle unit/integration, including the new host capability provider acceptance tests — PASS: [41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- Android KtLint — PASS: [41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- Live local llama.cpp/GGUF inference — PASS: [41312296077](https://github.com/artinrahimii/project/actions/runs/41312296077).
- Android APK build, JVM unit tests and generated Protobuf verification were still pending; no pass is claimed for them here.


## T0126-A005 — bounded directory enumeration (partial; CI pending)

- Added `RendezvousPeerDirectoryClient.discoverAll()` to expose all unexpired direct-TCP metadata endpoints in stable PeerId order.
- The method reuses the bounded parser, shared cache lock, TTL/announcement-expiry limit and fail-closed refresh path; `resolve()` also uses the extracted common refresh helper.
- Added Robolectric/JVM test `discoversFreshPeersInStableIdentityOrderAndExcludesPeersWithoutTcpRoute`, covering all-peer enumeration, deterministic order, QUIC-only exclusion and cache reuse.
- This does not produce typed HostAnnouncement model/capability data and is not yet called by the production request screen. T0126 remains blocked. Exact-head CI results are pending.


## T0126-A005 — exact V 3.0.779 verification results

- Exact tested commit: `ee9692a3d34543dd159f48ca4ef72e29f5981db8` (`V 3.0.779`).
- Android KtLint, debug APK build and JVM testDebugUnitTest passed. The JVM run includes `discoversFreshPeersInStableIdentityOrderAndExcludesPeersWithoutTcpRoute`. APK artifact: `13087940513`.
- Generated Protobuf Java/Kotlin verification passed.
- Windows WPF build/startup/security/DPAPI and HostLifecycle unit/integration passed.
- Step Three quality gate and Rendezvous container build/HTTP smoke passed.
- Workflow references: [product](https://github.com/artinrahimii/project/actions/runs/41316384983), [quality](https://github.com/artinrahimii/project/actions/runs/41316384981), [Rendezvous](https://github.com/artinrahimii/project/actions/runs/41316385018).
- These results validate discoverAll and its metadata-only contract, not production Android inference request UI integration. T0126 remains blocked.


## T0126-A006 — authenticated discovery verification

- Added `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`.
- The implementation consumes metadata-only `RendezvousPeerEndpoint` values only as routes; it calls the existing `probeHostStatus` function, whose transport checks explicit trust, canonical PeerId, authenticated remote PeerId, returned HostStatus PeerId, timestamp freshness and directory lease expiry.
- It converts the returned HostStatus model list into the local HostCapabilities candidate required by the existing selector. Fixed request limits are tied to `InferenceRequestPayloadBuilder.MAX_PROMPT_BYTES` (8,192) and `ConversationContextCapsule.MAX_SERIALIZED_BYTES` (4,096); concurrency is one, matching the Windows `HostInferenceSession` single-operation gate and MainWindow active-request status.
- Added `src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt`. Six tests exercise deterministic three-host selection, 0/1/2/3 feedback counts, expired/untrusted/busy/stale/incompatible peers, duplicate identities, cancellation, and status identity mismatch.
- Exact-head build, lint and JVM test results remain pending until GitHub Actions runs against the resulting commit. The task is not complete until the request UI calls this API and dispatch/feedback are verified end-to-end.


## T0126-A006 exact CI result

- Commit: `175962bded9e8942bf040c253f0b9765149a267a` (`V 3.0.781`).
- Android main-source KtLint failed on the two chained `newBuilder()` line breaks in `AuthenticatedHostDiscovery.kt` (lines 109 and 118).
- Android APK build, Android JVM tests and generated Android Protobuf verification were not reached on that commit.
- Windows WPF formatting/build/startup/security/DPAPI checks passed; HostLifecycle tests passed; Step Three graph/Python quality gate passed; Rendezvous container build and HTTP smoke passed.
- A007 changes only those two Kotlin line breaks. The Android checks must be rerun on A007's commit before they can be recorded as passing.



## T0126-A007 graph-ledger correction

- Exact V 3.0.782 graph guard error: `FAILED_ATTEMPT_COUNT_MISMATCH: T0126: summary=2, history=1`.
- The attempt A006 overall result is `partial`, although its KtLint sub-check failed. The separate test failure is retained, but it does not increase the count of fully failed attempts.
- Set `failed_attempt_count=1` to match the one `execution_history` entry whose overall result is `failed`. The T0001 unmapped-dependency notice remains a non-fatal warning.
- A007 code commit: `c618b7778f0ccdcb5152ce346141228c5519c6f1`. Graph correction commit is the next version and will be verified on `main`.



## T0126-A007 exact Android lint result and A008 correction

- On V 3.0.782, KtLint passed the main production source set, including `AuthenticatedHostDiscovery.kt`.
- KtLint then failed the test source set on four chained Java Protobuf builders in `AuthenticatedHostDiscoveryTest.kt`; no Android APK, JVM unit test or generated Android Protobuf verification ran.
- A008 replaces those nested builder chains with the generated Kotlin DSL already used in `HostCandidateSelectorTest.kt`. Test cases and their behavioral assertions are unchanged.
- Re-run all Android checks on the new commit before recording any pass.



## T0126-A008 exact CI reconciliation and A009 fix

- Exact V 3.0.784: production-source KtLint passed.
- Test-source KtLint failed only on unused import `ModelCapability` at `AuthenticatedHostDiscoveryTest.kt:13`.
- Android APK build, JVM tests and generated Android Protobuf verification were not reached. They are not counted as passes.
- Windows WPF build/startup/security/DPAPI, HostLifecycle tests, Step Three quality gate, and Rendezvous container smoke all passed on the exact V 3.0.784 commit.
- A009 removes that unused import only; its resulting commit must still pass a new exact-head CI run before Android verification can be marked successful.

