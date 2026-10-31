# T0166 — response-card verification evidence

## Source contract reviewed

- `InferenceResponseCorrelationValidator.matches` checks request ID, attempt ID, round ID, authenticated host ID, model ID, host completion time and local receive time.
- `Libp2pInferenceRequestTransport` completes the pending response only after the correlation validator passes.
- `InferenceAttemptDispatcher` allocates a fresh attempt ID per selected host while reusing the distributed request ID for the round.
- `InferenceRequestPayloadBuilder` serializes request/attempt/model/deadline and the Protobuf schema defines those fields.

## Implemented state and UI

- ViewModel storage is an in-memory `StateFlow<List<HostResponseCardState>>`.
- Response identity is `(requestId, attemptId)`; two attempts do not overwrite one another, including retries from the same host.
- The ViewModel checks the expected request, attempt, host and model before accepting a response returned by the correlation-validating transport.
- The Compose UI iterates the response list and creates one independent card per entry. Output text is not joined or merged.
- The state is ephemeral and does not persist response bodies.

## Automated checks

- JVM tests added for two independent host responses, duplicate attempt rejection, correlation mismatch rejection and clearing ephemeral result state.
- Python source-contract checks added for correlation checks, unique attempt identity, UI card iteration and test coverage.
- CI result: pending exact-head workflow for this change. No test is marked passed before the workflow reports success.


## First exact-head CI run — V 3.0.693

- Step Three quality gate passed: [41199229262](https://github.com/artinrahimii/project/actions/runs/41199229262).
- Windows WPF host and HostLifecycle jobs passed: [41199229209](https://github.com/artinrahimii/project/actions/runs/41199229209).
- Android KtLint failed with `RequestViewModelTest.kt:77:57 Expected newline before '.'`. Android APK build, JVM tests and generated Protobuf verification were skipped: [41199229209](https://github.com/artinrahimii/project/actions/runs/41199229209).
- A002 extracts a local variable to correct that exact formatting diagnostic. CI for the correction is pending; no acceptance pass is claimed yet.


## Second exact-head CI run — V 3.0.694

- Step Three quality gate passed: [41199481762](https://github.com/artinrahimii/project/actions/runs/41199481762).
- HostLifecycle job passed: [41199481700](https://github.com/artinrahimii/project/actions/runs/41199481700).
- Android KtLint failed at `HostResponseCards.kt:16:14` (uppercase function name) and `RequestViewModel.kt:14:1` (unused import); APK/JVM/Protobuf skipped: [41199481700](https://github.com/artinrahimii/project/actions/runs/41199481700).
- A003 applies those two exact source corrections and updates the call site and source-contract test. New CI is pending.


## Third exact-head CI run — V 3.0.695

- Step Three quality gate passed: [41199685219](https://github.com/artinrahimii/project/actions/runs/41199685219).
- HostLifecycle job passed: [41199685198](https://github.com/artinrahimii/project/actions/runs/41199685198).
- Android main-source KtLint failed with one unused import at `MainActivity.kt:42:1` (`HostResponseCards`). Android APK, JVM tests and Protobuf verification were skipped: [41199685198](https://github.com/artinrahimii/project/actions/runs/41199685198).
- A004 removes that unused import. Exact-head CI is pending.
