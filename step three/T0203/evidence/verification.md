# T0203 — verification evidence

## Repository snapshot and source integrity

- Repository: `artinrahimii/project`
- Branch: `main`
- Current snapshot inspected: `6c5074ce39bc54b750f8030d4c55a0de9a8aa50d` (`V 3.0.932`)
- CI source revision: `fff8b813cc90bd853b83b01b87a42fc57e58657c` (`V 3.0.930`)
- Target test path: `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`
- Target test Blob SHA at both revisions: `e691c971cd893b0b50e003f86c74948030a42f90` (identical).
- The two commits between the CI source and the inspected main snapshot (`V 3.0.931`, `V 3.0.932`) update only `step three/T0202/execution-report.md`, `step three/T0202/evidence/verification.md`, and `step three/project-graph.md`. The tested code and workflow were not changed.

## Exact CI result

- Product Build run: https://github.com/artinrahimii/project/actions/runs/41902912019
- Run head SHA: `fff8b813cc90bd853b83b01b87a42fc57e58657c`
- Run attempt: `2`
- Android job ID: `126832565983` — `success`
- Step 14: `Run Android JVM unit tests` — `success`
- Command: `gradle --no-daemon testDebugUnitTest`
- JUnit artifact: `whitep-android-unit-test-reports`, ID `13359797798`, size 54,694 bytes.
- JUnit test class report: `com.whitep.client.network.Libp2pInferenceRequestTransportTest`
- Target test: `dispatchesRoundToThreeNativeDotNetLibp2pHosts`
- Target test status: `passed`
- Target test duration: `3.289 s`

The CI workflow builds the .NET interop fixture before the Android JVM test and sets `WHITEP_INTEROP_SERVER_DLL` for the test task. The test's guard therefore has its required fixture input; it was not skipped for a missing fixture.

## What the test verifies

1. Three independent .NET fixture processes are started.
2. Each process creates a separate libp2p identity and listening endpoint.
3. The production Android inference runtime dispatches the same request round to all three peer IDs.
4. Three submissions are queued, with three distinct host IDs and three distinct attempt IDs.
5. Each returned response is checked against its host identity and attempt ID, along with the shared request/model IDs and successful outcome.
6. All three peer fixture processes exit successfully.

Relevant source paths:

- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceRoundResponseCollector.kt`
- `tests/HostLifecycle.Tests/AndroidInteropPeerServer.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `.github/workflows/product-build.yml`

## CI retry note

On Android job attempt 1, 17 Robolectric class setups failed while fetching Maven dependencies at `MavenArtifactFetcher.java:130` with `HttpURLConnection` I/O errors. The Android job was retried. Attempt 2 passed the JVM unit-test step, and the JUnit artifact records the target three-peer test as passed. No target-test assertion failure is recorded.

## Dependency validation

- `T0202 → T0203`: validated as `sequencing_only`. T0202 requires a physical Android device and an independent Windows host on a shared non-loopback LAN. T0203 exercises three local .NET peer processes in CI and consumes no output from that physical-LAN acceptance. Evidence: T0202 execution report/evidence, the T0203 cross-runtime test, the .NET fixture, and Product Build workflow.
- `T0195 → T0203`: validated as `sequencing_only`. T0195's installer/failure/recovery outputs are not consumed by the three-peer dispatch/correlation test. Evidence: T0195 execution report/evidence and the T0203 cross-runtime test.
- The original `type: blocks` fields and task order are preserved; only the separately maintained dependency classification and validation fields were updated.

## Scope boundary

The test uses three separate .NET processes on one CI runner and the test fixture's loopback transport. It is not a physical three-computer LAN test and does not run three independent local inference models. Each fixture returns the deterministic text `dotnet-host-response`; the asserted independence is per-host identity and response/attempt correlation. T0202 remains blocked, and no physical-device LAN pass is claimed.

## Runtime limitations

The current authoring environment did not provide `dotnet` or `gradle`, so no local build or rerun is claimed. The evidence above comes from the completed GitHub Actions run and its downloaded JUnit artifact.
