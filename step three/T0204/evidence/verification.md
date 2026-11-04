# T0204 verification evidence

## Incoming edge validation

1. `T0203 → T0204` — `contract_or_interface_dependency`, validated. T0204 extends the Android/.NET authenticated peer interop fixture and correlation contracts. Evidence: T0203 execution report/evidence, `Libp2pInferenceRequestTransportTest.kt`, `AndroidInteropPeerServer.cs`, `Program.cs`, and `AndroidInferenceRequestRuntime.kt`.
2. `T0195 → T0204` — `sequencing_only`, validated. T0204 does not consume T0195-specific output; the direct contracts are the request-round timer, response collector, candidate selector and authenticated transport. Evidence: T0195 execution report/evidence plus `InferenceRoundTimer.kt`, `InferenceRoundResponseCollector.kt`, and `HostCandidateSelector.kt`.

Original edge types `blocks` and conservative order were retained.

## Acceptance scenario

The new test is `dispatchesSecondRoundToThreeNewPeersAfterFirstThreeRemainSilent`.

- Three distinct authenticated .NET peers receive first-round inference requests.
- In `silent` mode, each peer deliberately withholds its response and remains alive through the Android shared round timeout.
- The response collector observes zero valid first-round responses and three `RESPONSE_TIMED_OUT` failures.
- The first three processes are stopped; three fresh .NET processes start with new PeerIds.
- `HostCandidateSelector.selectNextRound` selects the three fresh hosts and excludes every first-round host ID.
- The Android runtime dispatches round two and receives three successful responses correlated to the same request/model and the correct host and attempt IDs.
- Dispatch progress is checked for two rounds and six attempted hosts.

## Exact-head CI evidence

- Commit: `28d43ec6c5cf42a26926a5a50238d2f7a1ce044b` (`V 3.0.937`).
- The Product Build Android job first built `tests/HostLifecycle.Tests/bin/Release/net10.0/HostLifecycle.Tests.dll`.
- The JVM test step explicitly set `WHITEP_INTEROP_SERVER_DLL` to that path and ran `gradle --no-daemon testDebugUnitTest`.
- Job `126859276431` completed `Verify Kotlin formatting and lint`, `Build debug APK`, APK permission validation and `Run Android JVM unit tests` with success.
- Product Build Windows job `126859276762`, Host Lifecycle job `126859276787`, Step Three Quality Gate job `126859275712`, and Rendezvous job `126859275697` also passed.

The parent Product Build run `41911308402` was still in the separate Android Keystore instrumentation step at report time. No overall-run success is claimed. T0204's own Android JVM acceptance step did pass with the .NET fixture available, so T0204 is `done`.

## Scope limitation

This CI acceptance uses authenticated .NET peer processes over loopback on one CI runner. It does not establish physical Android-to-Windows LAN connectivity or real model inference.

## Graph validation correction

The graph guard's accepted successful history values are `succeeded` and `passed`. The V 3.0.938 record had `T0204-A003.result = "success"`, causing `DONE_WITHOUT_SUCCESSFUL_ATTEMPT` even though the exact-head Android JVM evidence was successful. The value is corrected to `succeeded`; no test result is being fabricated or changed.
