# T0127 verification evidence

## Base and dependency evidence
- Base main commit: `183603394edeefdddfe2dd118fb0591453e4a751` (`V 3.0.772`).
- Android directory/parser: `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`.
- Authenticated candidate filtering: `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`.
- No-send/duplicate dispatch guard: `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`.
- Legacy Windows listener boundary for T0115: `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs` and `tests/HostLifecycle.Tests/NetworkInferenceRequestFrameAcceptanceTests.cs`.
- T0126 feedback producer/UI is not an input to these boundary-level regression cases; the validated sequencing-only edge is recorded in the graph.

## Added test cases
- `RendezvousPeerDirectoryClientTest.rejectsDuplicatePeerIdsInOneDirectorySnapshot`
- `RendezvousPeerDirectoryClientTest.doesNotReturnCachedPeerWhenNetworkFailsAfterDirectoryCacheExpiry`
- `InferenceAttemptDispatcherTest.sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe`

## Existing cases retained
- `excludesExpiredPeersFromTheReturnedDirectory`
- `selectReachableRejectsDuplicatePeerIdsBeforeProbingAndHonorsExclusions`
- `rejectsDuplicateHostIdentityBeforeSubmitting`
- `rejectsZeroAvailableHostsWithoutSubmittingAnyRequest`

## Results
- Pending exact-head GitHub Actions. No test is stated as passed until its exact commit workflow completes.


## Exact V 3.0.773 CI result
- Step Three quality: PASS — https://github.com/artinrahimii/project/actions/runs/41313567881
- HostLifecycle: PASS — https://github.com/artinrahimii/project/actions/runs/41313567864
- Rendezvous container: PASS — https://github.com/artinrahimii/project/actions/runs/41313567782
- Android KtLint: FAIL — nine formatting findings in the added tests. Android APK, JVM tests and generated Protobuf verification were skipped. No Android runtime/test pass is claimed.

## Correction
The follow-up corrects only KtLint-reported whitespace and multiline initializer/constructor layout. The three new test assertions remain unchanged. Exact V 3.0.774 CI is pending.


## Exact V 3.0.774 acceptance evidence
- Android product workflow: https://github.com/artinrahimii/project/actions/runs/41313804544 — PASS; all three product jobs completed successfully.
- Step Three quality: https://github.com/artinrahimii/project/actions/runs/41313804501 — PASS.
- Rendezvous container: https://github.com/artinrahimii/project/actions/runs/41313804672 — PASS.
- Android JUnit artifact ID: `13086322646`; 171 tests, 0 failures, 0 ignored, 38.113 seconds.
- Android APK artifact ID: `13086661382`.
- Exact added tests:
  - `rejectsDuplicatePeerIdsInOneDirectorySnapshot` — passed.
  - `doesNotReturnCachedPeerWhenNetworkFailsAfterDirectoryCacheExpiry` — passed.
  - `sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe` — passed.
- Android KtLint, debug APK, testDebugUnitTest and generated Protobuf Java/Kotlin verification all passed.
- Windows WPF build and HostLifecycle unit/integration jobs passed.
- T0127 state: `done`. Historical V 3.0.773 KtLint failure remains recorded as attempt A002; V 3.0.774 exact CI is attempt A004 and succeeded.


## T0127-A005 — graph evidence guard correction
- V 3.0.775 Step Three quality run: https://github.com/artinrahimii/project/actions/runs/41314358746
- Python unit tests passed (312); graph_guard then failed because T0127 evidence did not include a top-level `tests/` source path. Existing Android JVM test sources are under `src/.../src/test/`, which the current guard does not recognize as an evidence path.
- Added `tests/test_android_network_failure_acceptance.py` to directly verify the named Android JVM regression cases and related production fail-closed parser/cache/selection/dispatch contracts.
- This new source is intended as supplementary repository evidence/guard, not a replacement for the already-passing Android JUnit acceptance tests.
- Exact V 3.0.776 CI is pending.


## Exact V 3.0.776 complete verification
- Step Three quality: PASS — https://github.com/artinrahimii/project/actions/runs/41314739020.
  - 317 Python tests passed.
  - compileall passed.
  - graph guard: 0 errors, one pre-existing T0001 unmapped-dependency warning.
  - repo-root actual-output/evidence validation passed.
  - five checks in `tests/test_android_network_failure_acceptance.py` passed.
- Product workflow: PASS — https://github.com/artinrahimii/project/actions/runs/41314738931.
  - Android lint, APK, 171 JVM tests, generated Protobuf Java/Kotlin passed.
  - Windows WPF build/startup/security/DPAPI and HostLifecycle passed.
  - JUnit artifact `13086841823`: 171 tests; 0 failures; 0 ignored; 39.690 seconds.
- Rendezvous container build and HTTP smoke: PASS — https://github.com/artinrahimii/project/actions/runs/41314739082.
- Top-level test-source evidence path `tests/test_android_network_failure_acceptance.py` is present in T0127's `actual_output_paths` and `evidence`.
- T0127 status: `done`. Failed attempt A002 on V 3.0.773 remains retained; failed_attempt_count remains 1.
