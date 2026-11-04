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

## T0194-A005 commit and exact-head workflow state

- A005's case-sensitive import correction and corresponding report/evidence/graph update are included in [`V 3.0.914`](https://github.com/artinrahimii/project/commit/c942502c6e1514767f8bc957810feb6bb32fb256); the A005 `commit_sha` is now reconciled to this commit.
- Exact-head workflows created for `c942502c6e1514767f8bc957810feb6bb32fb256`:
  - Quality Gate [41879452329](https://github.com/artinrahimii/project/actions/runs/41879452329) — `queued`.
  - Product Build [41879452513](https://github.com/artinrahimii/project/actions/runs/41879452513) — `queued`.
  - Rendezvous [41879452384](https://github.com/artinrahimii/project/actions/runs/41879452384) — `queued`.
- The predecessor commit `698308ad00a5d2f3768f09c9a1fb67653ae1dc64` workflows `41877738825`, `41877739005`, and `41877738930` also remained queued at inspection. No test pass/fail is inferred from a queued run.
- Source review confirms the production factory opens Room through SQLCipher and registers migration 4→5; startup recovery expires QUEUED/CONTACTING/WAITING rows; request content is not part of the lifecycle entity; and dispatch waits for successful local recovery and pre-dispatch persistence.
- Relevant Room migration, ViewModel recovery, status-presentation and SQLCipher instrumentation tests exist in the repository. They have not yet been executed successfully by the new exact-head CI runs.
- Local execution limitation: this environment has no Gradle or .NET SDK, and a shallow clone attempt could not resolve `github.com`; no local Kotlin/Android or .NET test result is claimed.
- T0194 remains `in_progress` until exact-head KtLint, APK, JVM and SQLCipher/Room instrumentation pass.

## T0194-A005 exact-head CI result

- Quality Gate [41879924836](https://github.com/artinrahimii/project/actions/runs/41879924836): **passed** on `1a427e7d24070be0dedbc88f6ef190eed21870ef`. The job log confirms 403 Python tests passed, Python syntax compilation passed, graph structure guard passed (230 tasks, 422 edges; one existing non-blocking metadata warning), and recorded-output/evidence validation passed.
- Rendezvous container [41879924251](https://github.com/artinrahimii/project/actions/runs/41879924251): **passed** on the same commit.
- Product Build [41879926235](https://github.com/artinrahimii/project/actions/runs/41879926235): **failed**. Android job `126749441659` stopped at `ktlintTestSourceSetCheck` with seven diagnostics:
  - `ClientDatabaseTest.kt`: lines 298:25, 307:47, 320:25, and 329:47.
  - `ConversationContextCapsulePersistenceTest.kt`: line 367:13.
  - `RequestViewModelTest.kt`: line 482:44 and 482:83.
- The failure is confined to Kotlin test-source chain formatting. Android APK build, merged-manifest permission verification, JVM unit tests and Keystore instrumentation were skipped.
- A006 corrects all seven reported locations. It does not change lifecycle behavior, Room schema, SQLCipher handling, network contracts, or user-data privacy.
- Targeted source-level checks confirmed the changed chain patterns and no line over 120 columns. This is not a substitute for KtLint itself.
- **Verification remains pending:** no APK/JVM/Instrumentation pass is claimed for A006.
