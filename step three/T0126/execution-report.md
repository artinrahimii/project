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


## T0126-A003 — exact V 3.0.769 discovery/capability boundary audit (blocked)

- **Base:** exact online `main` commit `c2db7d9e848567f21206bbcd55408b36ae1c7c8b` (`V 3.0.769`).
- **Live UI review:** `MainActivity.requestScreen` still invokes only `RequestViewModel.prepareRequest`; the Connections preview explicitly says peer discovery is unavailable. No production caller was found for `AndroidInferenceRequestRuntime.selectReachableHostsWithFeedback` or `RequestViewModel.recordHostSelectionFeedback`.
- **Discovery contract review:** `RendezvousPeerDirectoryClient` exposes only peer ID, direct TCP addresses and lease expiry. The metadata service advertises `inference.v1`, not model-level capability. `PeerAnnouncementHostLifecycle` publishes only this metadata tag.
- **Readiness contract review:** `MainWindow.GetPeerRuntimeStatusAsync` currently sets runtime state and active request count but does not populate `HostStatus.models`. The Android selector requires a matching current model in the announcement and fresh status, along with valid concurrency and prompt/capsule limits.
- **Decision:** T0126 is now `blocked`, not `done` or failed. The existing A001/A002 history and V 3.0.768 unit/CI passes are preserved; they prove the count/dispatch/feedback components in isolation, not a live UI flow. No fabricated HostAnnouncement/status snapshots were added and no production source was changed during this audit.
- **Required next work:** implement a validated capability publication/discovery contract from actual Windows model/readiness configuration, populate model capability data in fresh HostStatus, add bounded Android directory discovery returning typed HostAnnouncement values, and wire the real request UI to discover → live-probe → select → dispatch → display feedback. Then test the real user path for 0/1/2/3 valid peers before resuming T0126 or proceeding to T0127.
- **Tests in A003:** none run; this was a source/call-graph audit. Existing V 3.0.768 focused tests and CI remain recorded above.


## T0126-A004 — fresh host model-capability source (partial; live Android wiring remains blocked)

- **Base:** exact online `main` commit `566da8bd34e92e86935e41b01a034b7098e5d837` (`V 3.0.770`).
- Added a backend capability-provider contract and a fresh peer-runtime snapshot API. A snapshot is only returned with model data when the local session has just passed backend health and host-readiness checks; a busy, transitioning, disconnected or unverifiable backend returns no selectable model.
- The Windows llama.cpp backend now checks the configured model against `GET /v1/models` and reads the live context window from read-only `GET /props` (`default_generation_settings.n_ctx`). It does not use catalog claims or a guessed model-family limit. The reported output ceiling is capped by both the live context window and the shared protocol maximum.
- Windows `GetPeerRuntimeStatusAsync` now returns the exact current protocol version and adds the model capability only when the fresh runtime snapshot proves it. If the model snapshot is unavailable, the peer status is UNAVAILABLE rather than READY-with-guessed-capabilities.
- Added HostLifecycle acceptance coverage for exact model discovery, context parsing, missing model/context fail-closed behaviour, fresh-session capability snapshots and stale capability removal after disconnect.
- This is an upstream contract/producer repair only. The Android directory still has no production discovery caller, the Android request screen remains prepare-only, and the client still needs to combine the authenticated HostStatus with directory routing metadata, run 0/1/2/3-peer selection and wire dispatch/feedback to the UI. T0126 therefore remains `blocked`, not `done`.
- **Tests in this attempt:** the new exact-host tests are added but have not yet been executed in this authoring environment; GitHub Actions must verify the commit before this attempt can be recorded as test-passing.


## T0126-A004 — exact V 3.0.771 CI evidence reconciliation (partial)

- **Exact tested commit:** `390ffad4739b7e045639c9e19219f3863361b22c` (`V 3.0.771`).
- **Step Three quality gate:** PASS — 312 Python tests, syntax, graph and evidence checks — [run 41312296046](https://github.com/artinrahimii/project/actions/runs/41312296046).
- **Rendezvous:** PASS — production container build and non-root HTTP smoke — [run 41312296044](https://github.com/artinrahimii/project/actions/runs/41312296044).
- **Windows WPF:** PASS — C# formatting, Release build, Protobuf verification, startup smoke, security/policy/capability gates and DPAPI checks — [run 41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- **HostLifecycle:** PASS — full unit/integration suite, including `HostPeerRuntimeCapabilitiesAcceptanceTests` — [run 41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- **Android KtLint:** PASS — [run 41312296076](https://github.com/artinrahimii/project/actions/runs/41312296076).
- **Real local inference:** PASS — pinned llama.cpp CPU binary, validated GGUF model and a real local inference response — [run 41312296077](https://github.com/artinrahimii/project/actions/runs/41312296077).
- **Still pending at reconciliation:** Android `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin verification and APK artifact upload. No result is claimed for those pending steps.
- **Task status:** `blocked`. These CI checks do not establish Android live discovery, request-screen selection/dispatch or user-visible shortage handling.


## T0126-A005 — bounded Rendezvous discover-all API (partial)

- **Exact base:** `main` commit `513df9233b703a1f7f2da945ca9a7c0cfc0af3a9` (`V 3.0.778`).
- Added `RendezvousPeerDirectoryClient.discoverAll()`, returning all unexpired metadata endpoints with direct TCP routes in stable PeerId order. It uses the existing strict parser, maximum 16 KiB response bound, shared mutex/cache refresh and fail-closed network-refresh behavior. `resolve(ephemeralHostId)` now uses the same private refresh helper, preventing the two entry points from drifting in cache semantics.
- Added JVM/Robolectric regression `discoversFreshPeersInStableIdentityOrderAndExcludesPeersWithoutTcpRoute`: it verifies stable PeerId ordering, omission of a QUIC-only endpoint for this TCP transport client, and one network fetch shared by `discoverAll()` and a subsequent `resolve()`.
- This is a real directory API and test addition. It still returns metadata-only `RendezvousPeerEndpoint` values, not typed model/capacity `HostAnnouncement` values. It does not authenticate peers, invoke HostStatus probes, instantiate `AndroidInferenceRequestRuntime`, dispatch inference, or connect the Request screen. No UI flow is claimed.
- Exact V 3.0.779 Android KtLint, APK/JVM/Protobuf, Windows/HostLifecycle, Step Three and Rendezvous CI are pending at authoring time. No test pass is claimed until exact-head workflows finish.
- **Outcome:** partial implementation; T0126 remains `blocked` because production candidate construction and request-screen integration are outstanding.
- **Follow-up:** implement the typed capability/status-to-candidate path and connect discovery, explicit trust, live selection, dispatch and feedback to the real Android request UI. Verify 0/1/2/3 peers and no-send on zero before unblocking T0126.


## T0126-A005 — exact V 3.0.779 CI reconciliation

- **Exact tested online main commit:** `ee9692a3d34543dd159f48ca4ef72e29f5981db8` (`V 3.0.779`). All listed runs target this exact SHA.
- **Android KtLint and build:** PASS — [product workflow 41316384983](https://github.com/artinrahimii/project/actions/runs/41316384983). Kotlin main/test lint passed and `assembleDebug` succeeded; debug APK artifact `13087940513` (19,480,629 bytes) was uploaded.
- **Android JVM tests:** PASS — [product workflow 41316384983](https://github.com/artinrahimii/project/actions/runs/41316384983). `testDebugUnitTest` completed successfully, including the new `discoversFreshPeersInStableIdentityOrderAndExcludesPeersWithoutTcpRoute` regression.
- **Generated Protobuf Java/Kotlin:** PASS — [product workflow 41316384983](https://github.com/artinrahimii/project/actions/runs/41316384983).
- **Windows WPF:** PASS — [product workflow 41316384983](https://github.com/artinrahimii/project/actions/runs/41316384983). C# formatting, Release build, generated Protobuf C#, startup smoke, agent process/policy/capability restrictions and DPAPI peer identity/trust checks passed.
- **HostLifecycle:** PASS — [product workflow 41316384983](https://github.com/artinrahimii/project/actions/runs/41316384983). Unit and integration job completed successfully.
- **Step Three quality gate:** PASS — [workflow 41316384981](https://github.com/artinrahimii/project/actions/runs/41316384981).
- **Rendezvous:** PASS — [container/smoke workflow 41316385018](https://github.com/artinrahimii/project/actions/runs/41316385018).
- **Outcome:** A005's bounded metadata directory API and its JVM regression are verified on exact V 3.0.779. T0126 remains `blocked`, not `done`: no typed model/capacity HostAnnouncement contract or production Android request UI flow (authenticated status → selection → dispatch → feedback) exists yet.


## T0126-A006 — authenticated discovery and candidate construction (partial)

- **Base:** exact online `main` commit `baf446c5e3b1e0ee70a62096341257655210d9a5` (`V 3.0.780`).
- Added `AuthenticatedHostDiscovery` in the Android request layer. It enumerates the existing bounded `RendezvousPeerDirectoryClient.discoverAll()` snapshot, orders peers deterministically, and obtains a fresh authenticated `HostStatus` using the existing libp2p transport for each explicitly trusted peer.
- A candidate is built only when the authenticated status PeerId equals the directory PeerId, the route lease remains current, the status is READY and fresh, and the status includes the active model/protocol details. Public `inference.v1` metadata is not treated as model capability.
- Capability limits are sourced from existing contracts: one concurrent inference because `HostInferenceSession` serializes operations; 8,192 UTF-8 prompt bytes and 4,096 context-capsule bytes from the shared protocol constants. Model IDs and token ceilings come only from the fresh host status produced from its active backend.
- The production `AndroidInferenceRequestRuntime` now exposes `discoverAndSelectReachableHosts()`, using the same directory and authenticated transport instances as normal request delivery. Untrusted/unreachable/non-ready/stale/incompatible hosts are excluded; coroutine cancellation is propagated.
- Added six JVM tests for stable three-host selection, exact 0/1/2/3 post-probe counts, expired/untrusted/busy/stale/model-incompatible peers, duplicate directory identities, cancellation propagation, and authenticated identity mismatch.
- **Current boundary:** this adds and tests the real discovery-to-candidate API, but the Compose request screen still has no user-configured Rendezvous URL, explicit peer-trust interaction, or production caller that dispatches and records feedback. T0126 therefore remains blocked/partial; no inference request or prompt is sent by this new API. T0174 remains blocked on the complete request UI flow.
- **Tests:** pending exact-head GitHub Actions verification for this commit. No test pass is claimed before the workflow result is inspected.
- **Next:** connect configured service URL and explicit trust confirmation to the request UI, construct the payload from the user's actual request, call authenticated discovery/selection, dispatch only when one or more candidates exist, display the feedback, and collect correlated responses. Verify this full path before marking T0126 done.


## Exact V 3.0.781 CI reconciliation and T0126-A007 correction

- The online commit was `175962bded9e8942bf040c253f0b9765149a267a` (`V 3.0.781`) on `main`.
- **Android lint failed** in `AuthenticatedHostDiscovery.kt` at lines 109 and 118. KtLint required a newline before each chained `.newBuilder()`. This was a real style failure; it was not reported as a passing build.
- Because lint stopped the Android job, `assembleDebug`, `testDebugUnitTest`, and generated Android Protobuf Java/Kotlin verification were **not run** on V 3.0.781. No Android APK or JVM pass is claimed for that commit.
- The separate Windows WPF job passed C# formatting, Release build, generated Protobuf C#, startup, process/policy/capability and DPAPI identity/trust checks. HostLifecycle unit/integration tests passed. Step Three graph/Python quality gate passed. Rendezvous production container build and HTTP smoke passed.
- **T0126-A007** applies only the two KtLint-required chain-call line breaks in `AuthenticatedHostDiscovery.kt`. The discovery behavior and acceptance scope are unchanged. Android lint/build/JVM/Protobuf checks are pending rerun on the corrected commit.
- T0126 remains blocked: the Compose request screen still does not connect user-configured Rendezvous, explicit peer trust, payload construction, dispatch, shortage feedback and correlated response collection.



## T0126-A007 graph-ledger correction after V 3.0.782 quality run

- The Android formatting correction is committed as `c618b7778f0ccdcb5152ce346141228c5519c6f1` (`V 3.0.782`).
- The V 3.0.782 graph validator reported `FAILED_ATTEMPT_COUNT_MISMATCH: T0126: summary=2, history=1`. This came from counting A006's failed lint sub-check as a fully failed task attempt even though A006's overall result is correctly `partial`.
- Reconciled `execution_summary.failed_attempt_count` to `1`, which matches the single historical attempt whose overall `result` is `failed`. The A006 lint failure remains preserved in `tests_failed` and its own test record; it was not erased or relabeled as a pass.
- The unrelated `UNMAPPED_DEPENDENCY` warning for T0001 is a pre-existing metadata warning; it is not a blocking structural error.
- This graph correction is being recorded in `V 3.0.783`. The Android product workflow for V 3.0.782 has not yet been reconciled and remains pending.



## T0126-A008 — Android test-source lint correction

- Exact V 3.0.782 CI confirmed the production Kotlin source-set KtLint check passed after A007.
- The test source-set then found four style violations in `AuthenticatedHostDiscoveryTest.kt` at lines 177, 184, 192 and 195. The test task stopped before `assembleDebug`, `testDebugUnitTest` and generated Android Protobuf verification.
- Replaced the nested Protobuf Java builder chains in the new fixture helpers with the repository's already-used generated Protobuf Kotlin DSL (`hostStatus { }`, `modelCapability { }`, `inferenceRequest { }`). This is a formatting/API-style correction only; assertions and behavior were not changed.
- A008 remains partial pending exact-head lint/build/JVM/Protobuf and graph-gate reruns. T0126 remains blocked because the Compose request screen still does not invoke discovery, explicit trust, dispatch and response collection.



## T0126-A009 — remove the final unused test import

- Exact V 3.0.784 Windows WPF Release build, generated C# Protobuf, startup, process/policy/capability and DPAPI checks passed. HostLifecycle tests passed. Step Three quality gate and Rendezvous container smoke also passed.
- Android main production-source KtLint passed, including `AuthenticatedHostDiscovery.kt`.
- Android test-source KtLint failed only on one unused import: `org.whitep.shared.contracts.v1.ModelCapability` at `AuthenticatedHostDiscoveryTest.kt:13`. No Android APK, JVM unit tests or generated Android Protobuf checks ran because lint stopped first.
- A009 removes only that unused import. The test logic, fixtures and production code are unchanged. All Android verification is pending on the next exact-head commit.
- T0126 remains blocked because the Compose request screen still does not connect configured Rendezvous, explicit peer trust, actual request construction, dispatch, shortage feedback and correlated response collection.



## T0126-A009 final status reconciliation

- The exact V 3.0.785 Android test report is inspected: 178 total JVM tests, zero failures and zero ignored; `AuthenticatedHostDiscoveryTest` has six passed tests.
- The debug APK artifact SHA-256 from GitHub Actions is `dfd55299a6662d0a7aa18a9b38b586b8b134195df51461322326302b5e34ec13` (artifact ID `13088671959`, 19,484,737 bytes).
- All Android KtLint/build/JVM/Protobuf, Windows WPF/startup/security/DPAPI, HostLifecycle, Step Three quality and Rendezvous smoke checks on V 3.0.785 passed.
- No automated test in A009 is pending now. The remaining blocker is implementation scope, not CI: the request Compose UI does not yet invoke the authenticated discovery and dispatch path or present live feedback/collected responses.
- Next work is the production UI-to-host flow described in the graph's T0126 follow-up. T0126 remains blocked until that end-to-end path passes.



## T0126-A010 — production request-screen integration

- **Base revision:** `93e5c5d40bc1e4f652b2427b9314c209df45f3ae` (`V 3.0.786`), branch `main`. The concurrent A009 CI-summary/report commit was retained.
- **Implementation:** added the `RequestExecutionGateway` application boundary and made `AndroidInferenceRequestRuntime` its production implementation. The runtime exposes current, unexpired directory peer endpoints for a trust decision without trusting them automatically.
- **User flow:** the Compose request screen accepts an explicitly configured HTTPS Rendezvous URL (no placeholder/default endpoint), lists eligible direct-TCP peer IDs and lease expiry, requires a separate confirmation before persisting peer trust, and offers trust revocation. The request form accepts the exact model ID and output-token limit.
- **Execution path:** `RequestViewModel` builds a validated Protobuf selection request using the existing payload builder and protocol version `1.0.0`; the production runtime performs fresh authenticated status probes and selects only explicitly trusted, ready and compatible peers. Zero selected peers returns before dispatch; one or two peers remain dispatchable with the existing shortage feedback. A dispatched round is awaited through the existing per-attempt response collector and correlated responses are rendered as separate cards.
- **Privacy and identity boundary:** the directory is used for peer metadata only. No prompt is sent to Rendezvous. Peer trust still requires explicit user confirmation; the receiving host can read the prompt and capsule after transport decryption. No server endpoint or peer identity is fabricated.
- **New UI-flow tests authored:** `discoveryDoesNotTrustPeersAndOnlyExplicitlyApprovedPeersReceiveRequests` and `clearingDirectorySessionRemovesItsPeerListAndClosesTheGateway` in `RequestViewModelTest.kt`. They use an injected fake gateway to test UI orchestration and no-send-before-trust; they are not real-network or two-device tests.
- **Validation status:** no exact-head CI result for A010 exists yet. The V 3.0.785 passes recorded above do not apply to A010. No A010 test pass is claimed.
- **Result:** `partial`; task status changes from `blocked` to `in_progress` because the previously missing production UI-to-runtime path is now implemented. Do not mark T0126 `done` until exact-head CI and the new tests pass.
- **Changed implementation files:** `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/RequestExecutionGateway.kt`, and `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`.
- **Next:** inspect exact V 3.0.787 CI, fix failures and rerun affected checks. Keep independent public-network/two-device acceptance in deferred task `T0128`; this attempt does not claim it.


## T0126-A010 — exact V 3.0.787 failure reconciliation and correction

- **Exact tested commit:** `264532ea8d276f84725cfcbd8d2b762b7214d741` (`V 3.0.787`).
- **Android KtLint — FAILED:** the main Kotlin parser reported `RequestViewModel.kt:201:90 Unexpected tokens`. The offending trailing comma after the last expression in the `else` branch of the peer-discovery message was removed. Because KtLint stopped first, `assembleDebug`, `testDebugUnitTest` and generated Android Protobuf verification did not run on V 3.0.787. No Android build/JVM pass is claimed for that commit. [Product workflow 41321109193](https://github.com/artinrahimii/project/actions/runs/41321109193).
- **Step Three Python quality gate — FAILED:** 317 Python tests ran; seven assertions in existing source-contract tests still required the former prepare-only/transport-not-implemented preview strings. These assertions conflicted with the newly implemented request UI. The failures were in `test_android_compose_navigation.py`, `test_android_context_capsule_history_update_contract.py`, `test_android_host_response_cards_contract.py`, `test_android_request_architecture.py`, `test_central_inference_boundary.py`, `test_claims_audit.py` and `test_decision_traceability.py`. The later syntax, graph-structure and recorded-evidence steps were skipped because the workflow stops when the automated test step fails. [Quality workflow 41321109219](https://github.com/artinrahimii/project/actions/runs/41321109219).
- **Other checks on V 3.0.787:** Windows WPF Release build/startup/security/DPAPI and HostLifecycle unit/integration passed; Rendezvous container build and HTTP smoke passed. [Product workflow 41321109193](https://github.com/artinrahimii/project/actions/runs/41321109193) · [Rendezvous workflow 41321109208](https://github.com/artinrahimii/project/actions/runs/41321109208).
- **Corrections prepared for V 3.0.788:** fixed the single Kotlin syntax error and updated the seven obsolete source-contract tests rather than deleting them. Their new assertions check the configured HTTPS directory, explicit peer confirmation/revocation, real request dispatch/correlated response path, no dispatch when the selected-host list is empty, local-only context-capsule draft behaviour and continuing disclosure of the receiving-host privacy boundary. The response-card import-order check now matches the repository's alphabetical import order.
- **Result:** A010 remains `partial` and T0126 remains `in_progress` until exact V 3.0.788 tests pass. Neither the Android test suite nor the corrected Python tests have yet been rerun.


## T0126-A010 — exact V 3.0.788 CI reconciliation and test-format correction

- **Exact tested commit:** `0f97f55ff60f701debe855f13072c2f4854ce81d` (`V 3.0.788`).
- **Step Three quality gate — PASS:** all 317 Python tests passed; Python syntax checks, graph-structure validation and recorded-output/evidence validation also passed. [Workflow 41321728773](https://github.com/artinrahimii/project/actions/runs/41321728773).
- **Android main-source KtLint — PASS:** the previous `RequestViewModel.kt:201:90` parser error is resolved.
- **Android test-source KtLint — FAILED:** lint reached the new `RequestViewModelTest.kt` and reported 14 style diagnostics (one import-order issue, 12 chain-newline diagnostics and one blank line before the class closing brace). The diagnostics are limited to import ordering and line wrapping in the two new fake-gateway tests; no compilation or runtime failure was observed because the build stopped at lint. The test source has been reformatted to match the repository's existing Kotlin import groups and chained-call style.
- **Not reached on V 3.0.788:** Android `assembleDebug`, `testDebugUnitTest`, and generated Protobuf Java/Kotlin checks, because test-source lint failed first. No APK/JVM-test/Protobuf pass is claimed for this revision. [Product workflow 41321728687](https://github.com/artinrahimii/project/actions/runs/41321728687).
- **Other exact-head checks:** Windows WPF Release build/startup/security/DPAPI and HostLifecycle unit/integration passed; the Windows .NET peer interoperability fixture built successfully; Rendezvous production container and HTTP smoke passed. [Product workflow 41321728687](https://github.com/artinrahimii/project/actions/runs/41321728687) · [Rendezvous workflow 41321728849](https://github.com/artinrahimii/project/actions/runs/41321728849).
- **Correction prepared for V 3.0.789:** imports reordered to the existing `com, kotlinx, org, java` layout; chained calls in the new ViewModel tests moved to the next line; unnecessary blank line removed. The exact-head rerun is still pending.
- **Result:** A010 remains `partial`; T0126 remains `in_progress` until Android KtLint, APK build, JVM tests, generated Protobuf checks and the remaining runtime acceptance are complete.


## T0126-A010 — exact V 3.0.789 CI reconciliation and import-order correction

- **Exact tested commit:** `f4e19951b3a6b1e8d687d773468d55b5a0637ddd` (`V 3.0.789`).
- **Step Three quality gate — PASS:** all 317 Python tests passed; Python syntax, graph-structure validation and recorded-output/evidence validation also passed. [Workflow 41322274185](https://github.com/artinrahimii/project/actions/runs/41322274185).
- **Android KtLint — FAILED:** exact-head lint found import-order violations in `MainActivity.kt`, `AndroidInferenceRequestRuntime.kt`, `AuthenticatedHostDiscovery.kt`, `RequestExecutionGateway.kt` and `RequestViewModel.kt`; it also reported the chained-call wrapping at `RequestViewModel.kt:500`. The imports had been sorted case-insensitively, not according to KtLint's case-sensitive lexicographic order and its final `java/javax/kotlin` group. The APK, Android JVM tests and generated Protobuf checks were not reached. [Product workflow 41322274224](https://github.com/artinrahimii/project/actions/runs/41322274224).
- **Other exact-head checks — PASS:** Windows WPF Release build, startup, security/DPAPI, HostLifecycle unit/integration, Rendezvous production container build and HTTP smoke passed. [Product workflow 41322274224](https://github.com/artinrahimii/project/actions/runs/41322274224) · [Rendezvous workflow 41322274194](https://github.com/artinrahimii/project/actions/runs/41322274194).
- **Correction prepared for V 3.0.790:** sorted all five production Kotlin import blocks case-sensitively with `java/javax/kotlin` imports at the end; split the two ViewModel chained expressions according to KtLint's wrapping rule; restored the case-sensitive import-order contract assertion for `RequestViewModel` before `hostResponseCards`; updated the legacy ViewModel test's expected missing-directory status message to the current truthful text. The two new fake-gateway test cases remain; they are not represented as network integration tests.
- **Not yet validated:** V 3.0.790 exact-head Android KtLint, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin and the quality gate rerun.
- **Result:** A010 remains `partial`; T0126 remains `in_progress`. Do not mark it `done` before all required checks pass; public-network and two-device acceptance remains deferred to T0128.

## T0126-A011 — exact V 3.0.790 failure analysis and V 3.0.791 correction

- **Base:** exact online main commit `c893ace7d4b34aaffd82b61a693773d4582f6970` (`V 3.0.790`).
- **Workflow review:** Step Three quality gate passed (317 Python tests, Python syntax, graph structure and recorded-output/evidence validation): [run 41323436364](https://github.com/artinrahimii/project/actions/runs/41323436364). Windows WPF and HostLifecycle jobs passed in [product run 41323436382](https://github.com/artinrahimii/project/actions/runs/41323436382); Rendezvous container build and HTTP smoke passed in [run 41323436381](https://github.com/artinrahimii/project/actions/runs/41323436381).
- **Android failure:** the Android job failed at `ktlintMainSourceSetCheck` on six diagnostics in `RequestViewModel.kt`: four multiline-condition operator-wrap diagnostics at lines 168–169 and missing trailing comma/newline before arrows at lines 488 and 490. Consequently `assembleDebug`, `testDebugUnitTest` and generated Protobuf Java/Kotlin verification were skipped. No Android build or JVM test pass is claimed for V 3.0.790.
- **Correction:** moved the HTTPS URI validation boolean operators to KtLint-required continuation lines and moved the two multi-label `when` arrows after a trailing comma/newline. No runtime condition or outcome mapping was changed.
- **Regression strengthening:** the existing injected-gateway ViewModel test now asserts the exact user-visible zero-host message and no-send outcome before trust, then asserts the exact one-of-three shortage message after explicit trust and dispatch. Existing dedicated feedback/dispatcher/discovery tests retain zero/one/two/three-host coverage.
- **Task status:** `in_progress`. T0126 remains incomplete pending exact V 3.0.791 Android lint/build/JVM/Protobuf and quality-gate results. Public independent-network/two-device acceptance is the separate deferred T0128 and is not claimed here.
- **Next:** inspect the exact V 3.0.791 workflow; record only checks that actually pass and fix any remaining failure.
## T0126-A011 — exact V 3.0.791 formatting findings and V 3.0.792 correction

- Exact tested commit: `05354729bf2a36aa3c45ed26ff0329fcd3fd3db7` (`V 3.0.791`).
- Step Three graph/Python/syntax/evidence quality — PASS — [run 41336524524](https://github.com/artinrahimii/project/actions/runs/41336524524). HostLifecycle unit/integration — PASS — product [run 41336524479](https://github.com/artinrahimii/project/actions/runs/41336524479). Rendezvous production container/HTTP smoke — PASS — [run 41336524490](https://github.com/artinrahimii/project/actions/runs/41336524490).
- Android test-source KtLint completed successfully, but main-source KtLint failed. The multiline HTTPS condition had operators at the start of following lines, contrary to this repository's rule requiring each `||` at the end of the previous line. Three `message`/`discoveredPeers` assignment right-hand expressions also needed to begin on a new line.
- Android APK, JVM tests and generated Protobuf Java/Kotlin checks were skipped after main-source KtLint failed. No Android build or JVM test pass is claimed for V 3.0.791.
- V 3.0.792 changes only those source-formatting layouts. The previously added zero-host no-send and one-host shortage ViewModel assertions remain unchanged.
- T0126 remains `in_progress`; exact V 3.0.792 Android and full quality/product workflow results are pending.
## T0126-A011 — exact V 3.0.792 findings and V 3.0.793 correction

- Exact tested commit: `30065ac8b367d2639cb5faea2363ba852e34f05a` (`V 3.0.792`).
- Step Three quality — PASS — [run 41336799535](https://github.com/artinrahimii/project/actions/runs/41336799535). HostLifecycle — PASS — product [run 41336799515](https://github.com/artinrahimii/project/actions/runs/41336799515). Rendezvous container/HTTP smoke — PASS — [run 41336799520](https://github.com/artinrahimii/project/actions/runs/41336799520). Android test-source KtLint passed.
- Android main-source KtLint failed on three multiline expression body-indent findings at the peer-discovery and trust-map expressions and one remaining inline `discoveredPeers` map in the revoke-trust path. Android APK, JVM and Protobuf steps were skipped.
- V 3.0.793 corrects only the reported indentation and moves the revoke-trust map to its own continuation line. Request/selection runtime semantics and the A011 assertions are unchanged.
- T0126 stays `in_progress`; exact V 3.0.793 CI must verify Android main/test lint, APK, JVM tests and generated Protobuf.