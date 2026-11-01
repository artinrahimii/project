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
