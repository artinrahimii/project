# T0127 execution report — network loss, announcement expiry, duplicate peers and no-host behavior

- **Base:** exact online `main` commit `183603394edeefdddfe2dd118fb0591453e4a751` (`V 3.0.772`).
- **Current result:** Android regression tests added; exact-head CI is pending. The task is recorded as `in_progress` until Android JVM/build and graph-quality checks pass on the exact commit.
- **Scope:** tests target the Android Rendezvous parser/cache, authenticated peer-status selection and dispatch boundary. No fake peer capabilities, mocked success status or production UI claim was introduced.

## Dependency review

- **T0126 → T0127:** original `type=blocks` retained; validated as `sequencing_only`. T0127 consumes the candidate-selector/dispatcher contracts from T0124/T0125. It does not consume T0126's user-facing `HostSelectionFeedback`, RequestViewModel producer or Compose integration. The conservative order remains in the graph; these lower-level failure-path tests can be validated without pretending the T0126 user flow is ready.
- **T0115 → T0127:** original `type=blocks` retained; validated as `sequencing_only`. T0115 covers the legacy Windows length-prefixed `ParticipationGatedNetworkListener`; these tests cover Android HTTP Rendezvous refresh, libp2p status-probe failure, candidate selection and no-send dispatch. They do not consume the T0115 listener/frame-reader interface.
- Both edge reviews were appended to `edge_change_log`. Neither `blocks` label nor edge direction was changed; no edge was deleted.

## Test changes

1. `RendezvousPeerDirectoryClientTest` rejects duplicate PeerIds in a single directory snapshot.
2. A cache-refresh regression first resolves a real parsed directory entry, advances a controlled clock past the cache TTL, then simulates an HTTP-directory network interruption. The expired cache must not be returned as a successful resolution.
3. `InferenceAttemptDispatcherTest` simulates every discovered peer disconnecting during its authenticated status probe. Selection must return no candidates; dispatch must return `NO_SELECTED_HOSTS`; the transport must receive zero inference submissions.

Existing relevant cases retained:
- expired announcement omitted by the directory parser/resolver;
- duplicate PeerIds excluded before status probes by the selector;
- duplicate host identity rejected by the dispatcher before submission;
- empty candidate list rejected as `NO_SELECTED_HOSTS` without sending;
- T0115's separate listener disconnect-recovery test remains a different boundary.

## Validation

- Changed product files are Android JVM tests only.
- No local Gradle/Android runtime is available in this connector-only editing session; no local Android pass is claimed.
- Exact-commit GitHub Actions must verify Kotlin formatting, `assembleDebug`, `testDebugUnitTest`, generated Protobuf checks and the Step Three guard before T0127 can become `done`.

## Next

Inspect exact-head workflow results. If all relevant Android JVM/build and Step Three checks pass, append exact CI evidence and mark T0127 `done` in a follow-up version commit. If any check fails, preserve this attempt as partial/failed, correct the test code, and rerun.


## T0127-A002 — exact V 3.0.773 CI failure and correction

- The Step Three quality gate passed: [41313567881](https://github.com/artinrahimii/project/actions/runs/41313567881).
- HostLifecycle unit/integration passed: [product workflow 41313567864](https://github.com/artinrahimii/project/actions/runs/41313567864).
- Rendezvous container build and HTTP smoke passed: [41313567782](https://github.com/artinrahimii/project/actions/runs/41313567782).
- Android KtLint failed nine style findings in the two new test files: extra blank lines, multiline initializer layout and constructor parameter layout. APK, JVM tests and generated Protobuf verification were skipped; no Android test assertion is claimed as passed.
- T0127-A003 removes only the reported whitespace/line-layout violations. Test behavior is unchanged; exact V 3.0.774 CI is pending.


## T0127-A004 — exact V 3.0.774 acceptance and completion

- **Exact tested main commit:** `98560c02287a028f6e8eb867e5cf0c058b7a12c1` (`V 3.0.774`).
- **Android KtLint:** PASS — test-source formatting/lint succeeded.
- **Android debug APK:** PASS — `assembleDebug` succeeded and APK artifact `13086661382` was uploaded.
- **Android JVM acceptance:** PASS — `testDebugUnitTest` succeeded. The JUnit HTML report records **171 tests, 0 failures, 0 ignored, 38.113 seconds**; artifact `13086322646`.
- **New test evidence:** all three added test methods are individually listed as passed in that exact JUnit artifact:
  - `RendezvousPeerDirectoryClientTest.rejectsDuplicatePeerIdsInOneDirectorySnapshot`
  - `RendezvousPeerDirectoryClientTest.doesNotReturnCachedPeerWhenNetworkFailsAfterDirectoryCacheExpiry`
  - `InferenceAttemptDispatcherTest.sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe`
- **Existing regressions:** expired announcement, duplicate selector/dispatcher identity and empty-host no-send cases also passed within the same JVM report.
- **Generated Protobuf Java/Kotlin verification:** PASS.
- **Windows WPF and HostLifecycle:** both jobs passed in [product workflow 41313804544](https://github.com/artinrahimii/project/actions/runs/41313804544).
- **Step Three quality gate:** PASS — [41313804501](https://github.com/artinrahimii/project/actions/runs/41313804501).
- **Rendezvous container build and HTTP smoke:** PASS — [41313804672](https://github.com/artinrahimii/project/actions/runs/41313804672).
- **Outcome:** T0127 is now `done`, supported by exact-commit source, test report and workflow evidence. The failed V 3.0.773 lint attempt remains preserved; it is counted as one failed attempt, not hidden or overwritten.

## Next dependency-aware work

The next task in the graph order is T0175, but it depends on T0174, which remains blocked by the missing live Android discovery/dispatch/UI integration. T0126 also remains blocked on that production flow. Resume that integration and revalidate those blockers before starting downstream UI tasks.
