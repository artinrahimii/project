# T0173 — verification evidence

## Base and graph integrity

- Branch: `main`
- Reviewed base: `9da6438b085bc133d4b34311d80932118d6b7fcf` (`V 3.0.730`)
- Graph path: `step three/project-graph.md`
- Tree Blob ID: `a3bb3ba74ad33e16699853773a9a02f7d9299564`
- Git Blob SHA-1 recomputed from the original UTF-8 bytes: `a3bb3ba74ad33e16699853773a9a02f7d9299564` — matched.
- Graph content size: 3,485,452 bytes; text length is 3,361,849 Unicode characters.
- Parsed structure: 229 tasks, 417 edges; T0172 and T0160 were both `done`.

## Code evidence

- `RequestViewModel.recordValidatedResponse` validates the expected request/attempt/host/model and response round/outcome before accepting the response.
- The synchronized critical section rejects an already stored `(requestId, attemptId)` pair and does not replace its original card.
- `RequestViewModelTest.keepsEachCorrelatedHostAttemptAsAnIndependentResponseCard` verifies two unique attempts remain separate and replay of the first attempt is rejected without increasing the card count.
- `HostResponseCards.kt` renders the current accepted response-card list independently.
- `InferenceAttemptDispatcher.kt` rejects terminal request statuses `COMPLETED`, `FAILED` and `CANCELLED` before any further host submission.

## Provenance and test evidence

- T0166 implementation introduced at `89f9badabc5a5f7d11739a53f578b47f5f561154`.
- T0166 exact-head acceptance on V 3.0.698: Android KtLint, APK, JVM unit tests (including the response-card duplicate case), generated Protobuf checks, Windows WPF and HostLifecycle passed — [Android job](https://github.com/artinrahimii/project/actions/runs/41203195523/job/124542949303).
- Current response ViewModel and regression test remained unchanged through V 3.0.729. Exact-head V 3.0.729 Android APK/JVM/Protobuf job passed — [Android job](https://github.com/artinrahimii/project/actions/runs/41232429429/job/124629217032).
- Exact-head V 3.0.729 Step Three quality gate passed — [run](https://github.com/artinrahimii/project/actions/runs/41232429421).

## Dependency-edge validation

1. `T0172 → T0173` — `contract_or_interface_dependency`, `validated`.
   Rationale: T0172's request/attempt identity validation is the response-correlation boundary consumed by the idempotent response-card path.
   Evidence: `src/Client.Android/app/src/main/java/com/whitep/client/network/InferenceResponseCorrelationValidator.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`, `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`, `step three/T0172/execution-report.md`.

2. `T0160 → T0173` — `contract_or_interface_dependency`, `validated`.
   Rationale: the canonical protocol request ID and attempt ID produced for each host are the exact identity components used by the de-duplication key.
   Evidence: `src/Client.Android/app/src/main/java/com/whitep/client/protocol/InferenceRequestPayloadBuilder.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`, `protocol/whitep/v1/messages.proto`, `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`, `step three/T0160/execution-report.md`.

The original `type=blocks` values were preserved. The edge `T0173 → T0174` was left unreviewed.

## Result

T0173's acceptance criteria were already met by real source code and a JVM regression test delivered under T0166. The exact-head Android CI evidence confirms the duplicate is rejected and not displayed again. T0173 is therefore closed as an already-implemented requirement, without adding redundant runtime logic or claiming new product functionality.
