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
