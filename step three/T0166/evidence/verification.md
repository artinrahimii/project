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


## Fourth exact-head CI run — V 3.0.696

- Step Three quality passed: [41199928844](https://github.com/artinrahimii/project/actions/runs/41199928844).
- Windows WPF and HostLifecycle jobs passed in [product run 41199929005](https://github.com/artinrahimii/project/actions/runs/41199929005).
- Android KtLint passed.
- Android debug APK compilation failed at `MainActivity.kt:233:9`: `Unresolved reference 'hostResponseCards'`. The A004 cleanup removed the old import but omitted the lowercase function import after the Composable rename. [Android job log](https://github.com/artinrahimii/project/actions/runs/41199929005/job/124533545646).
- Android JVM tests, Protobuf verification and APK upload were skipped; no Android acceptance pass is claimed for this commit.

## Fifth attempt — A005 correction

- Add `import com.whitep.client.ui.request.hostResponseCards` to `MainActivity.kt`.
- Add a Python regression assertion for the correct import in `test_android_host_response_cards_contract.py`.
- No transport/runtime behavior or protocol contract changes. Exact-head V 3.0.697 CI is pending.


## Fifth exact-head CI run — V 3.0.697

- Step Three quality passed: [41202877855](https://github.com/artinrahimii/project/actions/runs/41202877855).
- Windows WPF and HostLifecycle jobs passed in [product run 41202877958](https://github.com/artinrahimii/project/actions/runs/41202877958).
- Android .NET peer interoperability fixture preparation passed.
- Android KtLint failed at `MainActivity.kt:3:1` because import order is lexicographic. The lowercase `hostResponseCards` import was placed before uppercase `RequestViewModel`; the required order is the reverse. [Android job](https://github.com/artinrahimii/project/actions/runs/41202877958/job/124542018744).
- Android APK build, JVM tests, generated Protobuf verification and artifact upload were not run because lint failed. No Android acceptance pass is claimed for V 3.0.697.

## Sixth attempt — A006 correction

- Reorder the two imports in `MainActivity.kt` as `RequestViewModel` followed by `hostResponseCards`.
- Add a Python regression assertion for this exact relative ordering.
- No production behavior or protocol change. Exact-head V 3.0.698 CI is pending.
