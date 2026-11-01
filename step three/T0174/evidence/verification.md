# T0174 — verification evidence

## Base revision and graph integrity

- Branch: `main`
- Base commit: `b7d47212a6f52f2c24a7bba591ba957e7072dbe6` (`V 3.0.731`)
- Base tree: `677fbf853ee71b2f2d7205e21d5f22cb0b852b20`
- Graph path: `step three/project-graph.md`
- Tree Blob ID: `b482a2e5a338ea9bcf7e7583bb64e72074fb048c`
- Git Blob SHA-1 recomputed from the complete original UTF-8 content: `b482a2e5a338ea9bcf7e7583bb64e72074fb048c` — matched.
- Content length: 3,495,851 bytes; 3,372,239 Unicode characters.
- Parsed structure: 229 tasks and 417 edges; T0160 and T0173 were both `done`.

## Protocol and source evidence

- Existing shared contract: `protocol/whitep/v1/messages.proto` defines `CancelRequest { request_id, attempt_id, reason }` and `InferenceOutcome.INFERENCE_OUTCOME_CANCELLED`. No schema change was made.
- `src/Host.Infrastructure/ProtocolMessageParser.cs`: new bounded `ParseCancelRequest` parser.
- `src/Shared.Contracts/ProtocolMessageValidator.cs`: new exact request/attempt/reason semantic checks; reason allowlist is `user_cancelled`.
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`: sends cancellation on the original authenticated peer stream and preserves the pending response until a correlated terminal response arrives.
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`: cancellation API consumes exact dispatch attempts and returns per-attempt sent/not-sent results.
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`: concurrent bounded request handlers keep the receive loop free to read CancelRequest; cancellation lookup uses exact `(requestId, attemptId)`; only the matching token is signalled; CANCELLED response remains correlated and deadline-bounded.
- Existing `HostRequestCoordinator` already passes a linked CancellationToken to local inference, so no duplicate cancellation subsystem was added there.

## Tests authored; CI pending

- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`: real trusted loopback peer test sends a second active request, then sends CancelRequest while inference is awaiting its token, and checks the correlated CANCELLED response.
- `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs`: parser tests valid request, malformed bytes, absent attempt, and unsupported reason.
- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`: two independent authenticated libp2p peers receive and confirm separate CancelRequest messages, then each returns a distinct correlated CANCELLED result.
- These new tests have not yet run on an exact commit; pending CI is not a pass.

## Incoming dependency-edge validation

1. `T0173 → T0174`: `contract_or_interface_dependency`, `validated`.
   Reason: response cards and terminal results are identified/de-duplicated by exact request/attempt identity. Cancellation must preserve that identity so one host's terminal state cannot affect another host's attempt.
   Evidence: `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`, `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/network/InferenceResponseCorrelationValidator.kt`, `protocol/whitep/v1/messages.proto`, `step three/T0173/execution-report.md`.

2. `T0160 → T0174`: `contract_or_interface_dependency`, `validated`.
   Reason: cancellation targets the canonical request ID and per-host attempt IDs generated for a dispatch round; those fields are the identity contract shared by InferenceRequest, InferenceHostAttempt and CancelRequest.
   Evidence: `src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt`, `protocol/whitep/v1/messages.proto`, `step three/T0160/execution-report.md`.

The original `type=blocks` values are preserved. The outgoing `T0174 → T0175` edge remains unreviewed.

## UI integration limitation

The current `MainActivity.requestScreen` is a preparation-only preview and explicitly states it does not send requests to hosts. It has no real dispatch result or active request lifecycle. The current Connections screen reports discovery unavailable. Therefore no user-facing Cancel button was wired in this attempt; an inert button or mock list of peers would be misleading. T0174 remains `in_progress` until real dispatch UI integration is possible and tested.

## Attempt result

`T0174-A001` is `partial`: real transport/runtime/host cancellation paths and integration tests were added; exact-head CI and user-facing control integration remain outstanding.


## Android KtLint failure and formatting correction (append-only)

- Exact V 3.0.732 Android product job: [41271523252](https://github.com/artinrahimii/project/actions/runs/41271523252), job [124745099617](https://github.com/artinrahimii/project/actions/runs/41271523252/job/124745099617).
- KtLint findings: imports were not in lexicographic order (generated CancelRequest import / java imports); two chained calls needed the newline before the dot at the request builder and CANCELLED response builder.
- The APK build, JVM tests and generated Protobuf validation were skipped because lint failed first.
- T0174-A005 reorders the full import list and adjusts both call-chain continuation layouts. No runtime behavior changed. The exact V 3.0.733 Android workflow will verify the correction.
- Independent checks on V 3.0.732 succeeded: Windows WPF [job 124745099788](https://github.com/artinrahimii/project/actions/runs/41271523252/job/124745099788), HostLifecycle [job 124745100081](https://github.com/artinrahimii/project/actions/runs/41271523252/job/124745100081), real local inference [run 41271523283](https://github.com/artinrahimii/project/actions/runs/41271523283).


## Android production-source KtLint correction (append-only)

- Exact V 3.0.733 Android product job: [124746076534](https://github.com/artinrahimii/project/actions/runs/41271856082/job/124746076534).
- Findings: `Libp2pInferenceRequestTransport.kt:317` required the CancelRequest builder expression and leading-dot chain on continuation lines. `AndroidInferenceRequestRuntime.kt:271, 274, 284` required multiline expression formatting for the queued-attempt filter, transport cancellation invocation and per-attempt boolean.
- Android APK/JVM/Protobuf validation was skipped because main-source-set KtLint failed first.
- T0174-A007 adjusts exactly these expression layouts without changing cancellation behavior. The new exact-head Android workflow must still confirm formatting, build, JVM regressions and generated-source checks.
- On exact V 3.0.733, Step Three quality, Windows WPF build/startup/security and HostLifecycle integration passed: [quality](https://github.com/artinrahimii/project/actions/runs/41271856045), [Windows](https://github.com/artinrahimii/project/actions/runs/41271856082/job/124746076851), [HostLifecycle](https://github.com/artinrahimii/project/actions/runs/41271856082/job/124746076793).


## Final import-order correction (append-only)

- Exact V 3.0.734 Android job: [124746942803](https://github.com/artinrahimii/project/actions/runs/41272151054/job/124746942803).
- Production Kotlin KtLint passed. Test-source-set KtLint failed one import-order check because the `java.*` block was placed before `org.whitep.shared.contracts.v1.hostAnnouncement/hostCapabilities/hostStatus/modelCapability`.
- T0174-A009 moves `java.*` after all `org.whitep` imports, keeps imports contiguous and leaves the cancellation generated import alphabetically placed.
- No runtime changes were made by this formatting correction.
- Step Three, Windows WPF, and HostLifecycle checks succeeded on exact V 3.0.734; Android APK/JVM was skipped at KtLint and remains unverified.


## Exact V 3.0.735 CI and A010 failure

- Step Three quality gate passed on exact head `0391c10da301d876b549d6d23eb753e0b00c5dc4`: [run 41272466910](https://github.com/artinrahimii/project/actions/runs/41272466910).
- Windows WPF build/startup/security passed: [job 124747874779](https://github.com/artinrahimii/project/actions/runs/41272466918/job/124747874779).
- HostLifecycle unit/integration passed, including the trusted loopback CancelRequest-to-CANCELLED response case: [job 124747874652](https://github.com/artinrahimii/project/actions/runs/41272466918/job/124747874652).
- Android KtLint passed and `assembleDebug` passed. The JVM test source did not compile:
  `Libp2pInferenceRequestTransportTest.kt:72:61`: `ProtocolBinding<LoopbackCancellationResponder>` was not assignable to the test helper's `ProtocolBinding<LoopbackInferenceResponder>`.
- Therefore the Android JVM test method was not executed, and no Android cancellation-test pass is claimed for V 3.0.735.

## A011 source correction

- Changed test helper `createHost` to accept `ProtocolBinding<T>` for `T : SimpleClientHandler`.
- This removes the responder-type mismatch while preserving the common test-host builder and protocol binding cast.
- No production code or protocol behavior changed.
- Exact-head Android test-source compilation, the cancellation JVM regression and generated Protobuf validation remain pending.
- T0174 remains `in_progress`: the Android request UI is still a prepare-only preview without real dispatch/selected-attempt state, so there is no genuine click path to attach a Cancel button to.


## Exact V 3.0.736 CI evidence — T0174-A012

- Exact commit `339b1efb54449fe0a3d5d4dad3d26c096ad3d36d` (`V 3.0.736`).
- Step Three quality gate: PASS — [run 41274355328](https://github.com/artinrahimii/project/actions/runs/41274355328).
- Product workflow: SUCCESS — [run 41274355277](https://github.com/artinrahimii/project/actions/runs/41274355277).
- Android job [124753390180](https://github.com/artinrahimii/project/actions/runs/41274355277/job/124753390180): KtLint, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin all succeeded.
- Windows WPF [job 124753390376](https://github.com/artinrahimii/project/actions/runs/41274355277/job/124753390376): PASS.
- HostLifecycle [job 124753390338](https://github.com/artinrahimii/project/actions/runs/41274355277/job/124753390338): PASS, including the Windows loopback CancelRequest/CANCELLED integration acceptance.
- Android test report artifact `13070711776`: 150 tests, zero failures, zero ignored. All 4 `Libp2pInferenceRequestTransportTest` tests passed; `cancelAttemptsSendsCorrelatedCancelRequestsToEverySelectedAuthenticatedHost` passed in 6.055 seconds.
- Android-JVM test results cover two authenticated JVM peers, not native .NET peers. Do not describe this as cross-runtime cancellation acceptance.

## T0174-A013 — native Android/.NET cancellation interop test (pending)

- The .NET test fixture can now run in `cancel` mode, validate the exact CancelRequest pair/reason and send a correlated CANCELLED response.
- The new Android JVM test starts three separate native .NET peer processes, submits the shared request to each, waits for all three requests to arrive, sends cancellation for the selected attempts and validates the three returned CANCELLED outcomes.
- Test source is committed as part of the next execution step, but has not yet run in CI. No pass is claimed for this new test until exact-head product CI succeeds.

## Outstanding production UI integration

The existing Android request screen is a prepare-only preview. It creates neither a live dispatch nor a retained list of selected `InferenceHostAttempt` values. No production call site invokes `cancelSelectedHosts`. A real Cancel button is therefore not yet implemented. T0174 remains `in_progress` despite the passing backend/JVM/Windows acceptance tests.


## V 3.0.737 — exact-head result

- Quality gate: PASS — [run 41275179422](https://github.com/artinrahimii/project/actions/runs/41275179422).
- Windows WPF build/startup/security: PASS — [job 124755825167](https://github.com/artinrahimii/project/actions/runs/41275179435/job/124755825167).
- HostLifecycle integration: PASS — [job 124755825154](https://github.com/artinrahimii/project/actions/runs/41275179435/job/124755825154).
- Real local inference integration: PASS — [run 41275179495](https://github.com/artinrahimii/project/actions/runs/41275179495).
- Android KtLint failed one import-order check at `Libp2pInferenceRequestTransportTest.kt:3:1`. The new `kotlinx.coroutines.delay` import was placed before `Dispatchers`; the sorted order is `CompletableDeferred`, `Dispatchers`, `delay`, `runBlocking`, etc.
- Android APK, JVM and Protobuf steps were skipped, so the A013 Android-to-.NET cancellation test did not execute.

## A015 source correction

- Moved `import kotlinx.coroutines.delay` after `import kotlinx.coroutines.Dispatchers`.
- Only the test-source import order changed. Production code, C# fixture and protocol behavior are unchanged.
- Exact-head CI for A015 remains pending. T0174 stays `in_progress`.


## T0174-A016 — exact V 3.0.738 acceptance evidence

- Exact tested `main` commit: `34db50f1e11de7ee034fb1082b7f901ffc1134d9` (`V 3.0.738`).
- Product workflow SUCCESS: [41275610857](https://github.com/artinrahimii/project/actions/runs/41275610857).
- Step Three graph/Python quality workflow SUCCESS: [41275610846](https://github.com/artinrahimii/project/actions/runs/41275610846).
- Android CI job SUCCESS: [124757067197](https://github.com/artinrahimii/project/actions/runs/41275610857/job/124757067197); KtLint, `assembleDebug`, `testDebugUnitTest` and generated Protobuf Java/Kotlin verification passed.
- Android test report artifact: [whitep-android-unit-test-reports]( https://github.com/artinrahimii/project/actions/runs/41275610857/artifacts/13071167147 ). The extracted Gradle report shows 151 tests, 0 failures, 0 ignored. The transport test class contains 5 tests, all passing:
  - `cancelAttemptsSendsCorrelatedCancelRequestsToEverySelectedAuthenticatedHost` — PASS, 6.053 seconds.
  - `sendsCancellationToThreeNativeDotNetLibp2pHostsAndReceivesCancelledResponses` — PASS, 3.808 seconds.
  - The existing Android request/response, three-host dispatch and response-correlation tests also passed.
- Windows HostLifecycle job SUCCESS: [124757066927](https://github.com/artinrahimii/project/actions/runs/41275610857/job/124757066927).
- Windows WPF Release build/startup/security job SUCCESS: [124757067131](https://github.com/artinrahimii/project/actions/runs/41275610857/job/124757067131).
- Product CI proves the Android cancellation message interoperates with three independent native .NET peer processes and yields three correctly correlated `CANCELLED` responses.
- Graph Blob from the exact V 3.0.738 tree: `e2a9429213736a69786d886739f92e2700049128`; full UTF-8 source content was 3,538,870 bytes and the locally recomputed Git Blob SHA-1 matched. The original graph structure parsed as 229 tasks and 417 edges.

## User-facing UI still not accepted

- `MainActivity.requestScreen` is still a preparation-only preview: no live host discovery, no real dispatch call, and no active selected-attempt state.
- `MainActivity.connectionsScreen` explicitly reports discovery unavailable and performs no connection attempt.
- `AndroidInferenceRequestRuntime` requires caller-provided `HostCandidate` snapshots from a separately validated discovery/status source; it deliberately does not invent readiness from metadata-only rendezvous announcements.
- No production Android call site invokes `dispatchRound` or `cancelSelectedHosts`.
- Therefore the visible Cancel button acceptance criterion is still unmet; T0174 remains `in_progress`, despite passing Android↔native-.NET cancellation interoperability.
