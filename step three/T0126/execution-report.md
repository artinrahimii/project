# T0126 execution report — shortage when fewer than three compatible hosts exist

- **Base:** exact online `main` commit `bd2a3deecfaa0798709177fa04db4606f62c4755` (`V 3.0.766`).
- **Task state:** `in_progress`; implementation and focused acceptance tests have been added, but exact-head CI is still pending and the current Android request screen remains a prepare-only preview.
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

## Acceptance tests added (pending exact-head execution)

- `HostSelectionFeedbackTest`: zero, one, two and three available candidates; exact message and continue/no-continue decisions; reject counts above the requested ceiling.
- `InferenceAttemptDispatcherTest`: one host dispatches, two hosts dispatch, zero hosts returns `NO_SELECTED_HOSTS` and performs no transport submit.
- `RequestViewModelTest`: the two-of-three shortage message propagates into UI state without storing host identity data; one and zero host cases produce the intended policy messages.
- Exact-head Android KtLint, APK, JVM, Protobuf and Step Three checks have not yet run. No test is marked passed.

## Remaining integration boundary

The actual `MainActivity.requestScreen` still describes itself as a prepare-only preview; it calls `prepareRequest`, and there is no production caller that obtains a complete `HostAnnouncement` collection and then invokes `selectReachableHostsWithFeedback` / dispatch. The Connections screen also explicitly says peer discovery is not available. The message-rendering state is wired to Compose, but the runtime producer is not called by that preview screen. This is tracked by the separate blocked request-screen/discovery integration work (including T0174); do not mark T0126 complete or claim a live user notification until a real production flow supplies this feedback.

## Next

Run exact-head Android lint/APK/JVM/Protobuf and Step Three quality CI. Fix any failures. Then revalidate the caller wiring against the actual request screen/discovery integration before setting T0126 to `done`.
