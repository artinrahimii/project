# T0194 execution report

- **Task:** هنگام بسته‌شدن اپ اندروید، وضعیت درخواست‌های جاری را بازیابی‌پذیر یا صریحاً منقضی کن.
- **Base:** `main` / `21e31e097a8818c2806f9e8611dc280897ae70e6` (`V 3.0.908`).
- **Attempt:** `T0194-A001`.
- **Status:** `in_progress`; implementation committed for exact-head CI validation. No acceptance success is claimed until Quality, Android lint/build/JVM tests and relevant Room migration/restart tests pass.
- **Dependency review:** both incoming edges were validated as `sequencing_only` while preserving their original `type=blocks` and conservative execution order. T0193 supplies no Windows runtime interface consumed by Android T0194; T0188 supplies privacy/threat-model guardrails rather than an executable artifact. The outgoing edge to T0195 remains `unreviewed`.
- **Finding:** the previous `RequestViewModel` retained request lifecycle state only in memory. The existing `request_states` Room table is conversation-bound and the request screen is independent; its UI displayed persisted conversation rows but did not persist live inference lifecycle transitions.
- **Implementation:** added an independent Room metadata table `client_inference_request_states`, an encrypted Room repository, schema migration 4→5, startup recovery, and lifecycle writes before dispatch / while awaiting / after terminal outcomes. Stale `QUEUED`, `CONTACTING`, and `WAITING` states become `EXPIRED` with fixed error code `APP_CLOSED_BEFORE_RESULT`.
- **Privacy boundary:** only opaque request ID, state enum, timestamps, attempt number and fixed error code are stored. Prompt, context capsule, peer identifiers, payload and response bodies remain in memory and are not part of this table.
- **Remote-work boundary:** `EXPIRED` means the Android client did not record the result before its process/request owner ended. It does not assert that a remote host was stopped or did not finish.
- **Retention:** only the 20 most recent app-level lifecycle rows are retained.
- **Tests added/updated:** Room file-reopen recovery, migration v1→v5, ViewModel startup recovery, expired-status presentation, SQLCipher restart recovery and Python source/privacy contracts. Existing capsule persistence tests were updated to register migration 4→5.
- **Verification boundary:** source/contract structure was inspected, but no local Android SDK/build is available in this execution environment. Exact-head GitHub Actions results are pending; no test success is yet claimed for A001.
- **Next:** inspect exact-head Step Three quality and Android lint/build/JVM/instrumentation results. Fix failures and only then accept T0194.


## T0194-A001 CI finding and T0194-A002 correction

- Exact-head Quality run [41875864372](https://github.com/artinrahimii/project/actions/runs/41875864372), job `126734953437`, ran **403 tests** and failed one source-contract test: `test_android_request_architecture.test_compose_screen_uses_viewmodel_and_production_peer_gateway` expected the old `viewModel()` no-argument syntax. The production code now correctly injects `RequestViewModelFactory` and `RoomInferenceRequestLifecycleRepository`; this test had not been updated for that intended wiring. The other 402 tests passed.
- Corrected `tests/test_android_request_architecture.py` to assert the Compose `viewModel(` factory call, `RequestViewModelFactory`, and `RoomInferenceRequestLifecycleRepository`.
- This correction is attempt `T0194-A002`. No Android build or runtime success is claimed yet. The product workflow was still running Windows, Android and host lifecycle jobs at inspection. Rerun exact-head CI is required.


## T0194-A002 graph validation correction

- Exact-head Quality run [41876248248](https://github.com/artinrahimii/project/actions/runs/41876248248), job `126736315881`: **403 Python tests passed** (`OK`). The graph guard then reported one error: `FAILED_ATTEMPT_COUNT_MISMATCH: T0194: summary=1, history=0`.
- Cause: A001's CI gate had failed the stale architecture assertion, and the summary incremented `failed_attempt_count`, but its history item still said `partial`.
- Correction: A001 history is now marked `failed`, matching the recorded counter. A002 remains `partial`; this corrects the execution ledger and does not assert Android product acceptance.
- A fresh Quality run is required before T0194 can proceed to acceptance. Product workflow results remain separately pending.


## T0194-A002 product gate results

- Product run [41876248274](https://github.com/artinrahimii/project/actions/runs/41876248274):
  - Android job `126736315663` — **failed** at `:app:ktlintTestSourceSetCheck`. The job stopped before APK compilation, JVM unit tests or instrumentation; none of those are claimed.
  - Windows job `126736316030` — **passed**.
  - HostLifecycle job `126736316108` — **passed**.
- KtLint's configured `max_line_length=120`. Source inspection found a 146-character `.addMigrations(...)` call in `ConversationContextCapsulePersistenceTest.kt`, plus two RequestViewModel lines at 121/122 characters. All three were reformatted in attempt `T0194-A003`. No behavioral change was intended.
- A002 is recorded as failed because its exact-head Android gate did not complete; A003 is partial pending the next exact-head test run.

## T0194-A004 Android KtLint follow-up

- Based on the exact-head log for product run [41876620048](https://github.com/artinrahimii/project/actions/runs/41876620048), applied the remaining reported main-source formatting corrections:
  - sorted imports in `MainActivity.kt` and `RequestViewModel.kt`;
  - put the `requestScreen` parameter on the function declaration line;
  - added the required trailing comma/newline in the multi-label `when` branch;
  - removed the extra blank line before `RequestViewModelFactory`.
- The preceding A003 commit [f1a43e416b85b5994eec14184ec7f55bfc935746](https://github.com/artinrahimii/project/commit/f1a43e416b85b5994eec14184ec7f55bfc935746) had already wrapped the Room migration invocation and two long ViewModel expressions. Its exact-head workflows are queued as of this report update.
- A004 is a formatting-only correction; it does not change the lifecycle state model, Room schema, encryption boundary, network behavior or dependency edges.
- **Verification state:** no KtLint, APK, Android JVM or instrumentation pass is claimed for A004. Exact-head workflows are required before T0194 can move to `done`.

## T0194-A004 commit identification and A005 import-order correction

- The A004 formatting changes were committed in [`V 3.0.913`](https://github.com/artinrahimii/project/commit/698308ad00a5d2f3768f09c9a1fb67653ae1dc64); the graph attempt entry is reconciled to this commit SHA.
- An independent review against the repository's `.editorconfig` setting `ktlint_code_style=ktlint_official` found that A004's import sorter was case-insensitive. The official KtLint ordering is lexicographic within each import group, with capital letters before lower-case letters; therefore some same-package imports still had the wrong order.
- A005 reorders imports in `MainActivity.kt` and `RequestViewModel.kt` using a case-sensitive lexicographic comparison, with the configured `java.`, `javax.`, and `kotlin.` groups at the end and aliases last. Source-level order checks and the 120-character limit check passed in the preparation script; this is not a substitute for KtLint itself.
- A005 changes imports only. It does not change runtime statements, request lifecycle behavior, database schema, protocols, or privacy boundaries.
- GitHub Actions for A004 commit `698308ad00a5d2f3768f09c9a1fb67653ae1dc64` (Quality `41877738825`, Product `41877739005`, Rendezvous `41877738930`) remained `queued` on inspection; no test pass is inferred.
- **Verification state:** A005 remains partial until exact-head Quality, Android KtLint, APK build, JVM and SQLCipher/Room instrumentation results are available.
