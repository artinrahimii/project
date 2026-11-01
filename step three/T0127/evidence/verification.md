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
