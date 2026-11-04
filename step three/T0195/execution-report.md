# T0195 — Failure-path acceptance tests

## Scope and implementation

T0195 covers five failure classes across the existing Windows and Android implementations. Existing executable tests already cover network interruption, host disconnect, inference-server disconnect and app restart. This attempt adds the missing explicit low-disk acceptance coverage rather than duplicating existing scenarios.

Updated `tests/HostLifecycle.Tests/VerifiedModelInstallerAcceptanceTests.cs` with two executable cases:

- `RejectsInsufficientDiskSpaceBeforeNetworkAccess`: injects an available-space provider reporting one byte less than the required model staging size; verifies rejection before contacting the HTTP source, creating a final model or leaving a staging file.
- `PreservesExistingModelWhenReplacementSpaceIsInsufficient`: repeats the preflight for an update; verifies the previous installed model remains byte-for-byte unchanged, no HTTP request is made, and no staging/backup file remains.

The production installer already performs this free-space check before download and staging; no new runtime dependency or behavior was added. The tests protect the existing failure boundary.

## Existing scenario coverage mapped to T0195

- **Internet interruption:** `tests/HostLifecycle.Tests/IncompleteModelDownloadAcceptanceTests.cs` — `InterruptedTransferRemovesPartialStagingAndLeavesNoFinalModel`; the fixture provides partial bytes then throws IOException, verifying cleanup and no final model.
- **Host disconnect:** `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt` — `sendsNothingWhenEveryDiscoveredPeerDisconnectsBeforeAuthenticatedStatusProbe`; verifies no request is sent after all selected peers disconnect.
- **Inference server/process failure:** `tests/HostLifecycle.Tests/Program.cs` injects a local inference transport failure and verifies a stable “server process stopped” message without exposing the underlying exception; `HostRequestCoordinatorFailureAcceptanceTests` verifies an inference failure releases the single-request gate.
- **Application shutdown/restart:** delivered under T0194 through `RequestViewModelTest.processRestartExpiresPersistedInFlightRequestAndRestoresExplicitExpiredState` and encrypted Room/SQLCipher instrumentation. Exact Product run 41882320680 passed all 9 emulator instrumentation tests (0 skipped, 0 failed).

## Dependencies and privacy

Incoming T0194 and T0188 edges are already validated in the graph. Their original `type=blocks` values and execution order are preserved. Failure tests continue to respect the fixed diagnostic allowlist and do not log prompt, response, capsule, key or peer identifiers.

## Verification state

The new C# tests are registered in `VerifiedModelInstallerAcceptanceTests.Run()`, invoked by `tests/HostLifecycle.Tests/Program.cs`. Source-level inspection of the added methods passed. Exact-head Quality and Product CI are pending; T0195 remains `in_progress`.
