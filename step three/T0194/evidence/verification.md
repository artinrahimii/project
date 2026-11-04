# T0194 evidence — A001

## Dependency evidence

- `T0193 → T0194`: reviewed as `sequencing_only / validated`. The Windows lifecycle implementation in `PeerAnnouncementHostLifecycle` exposes no shared runtime API or schema used by Android `RequestViewModel`. Preserve the original `blocks` edge and order.
- `T0188 → T0194`: reviewed as `sequencing_only / validated`. T0188 is a threat-model/privacy constraint; Android consumes the documented storage boundary but no executable API or generated contract. Preserve the original `blocks` edge and order.
- `T0194 → T0195`: left `unreviewed`; it was not needed for this task.

## Baseline observations

- The previous `RequestViewModel` kept `RetryableRequestContext`, active cancellation and `RequestRunStatus` in memory.
- `request_states` was only connected to `ConversationDetailScreen` and has a mandatory conversation foreign key; the New request screen has no conversation ID.
- No `requestStateDao.upsert` call existed in the live request ViewModel.
- `ClientDatabase` was schema version 4 and was opened through the SQLCipher-backed `EncryptedClientDatabaseFactory`.

## New implementation

- `InferenceRequestStateEntity` and `InferenceRequestStateDao` store only request ID, lifecycle status, timestamps, attempt count and fixed error code.
- The separate table has no conversation foreign key, prompt, capsule, peer identifier, payload or response field.
- `RoomInferenceRequestLifecycleRepository` transactionally expires in-flight rows on recovery and retains at most 20 recent records.
- Production `requestScreen` creates `RequestViewModel` with this repository, which opens the existing SQLCipher Room database.
- The ViewModel records `QUEUED` / `CONTACTING` before dispatch, `WAITING` after dispatch, and terminal state after the round result. On startup, stale active status is restored as `EXPIRED`.
- The UI explains that the host may have completed but the client did not recover a result; no remote cancellation is claimed.

## Acceptance tests

- File-backed Room close/reopen test verifies WAITING becomes EXPIRED and retains request ID, timestamp and attempt count.
- Version-1 Room migration test runs all migrations through version 5 and verifies the new table is empty while legacy records remain.
- ViewModel test seeds an in-memory WAITING state and verifies startup recovery shows EXPIRED.
- Status presentation test verifies EXPIRED is explicit and not mislabeled as remote cancellation.
- SQLCipher instrumentation scenario stores an in-flight record in the encrypted database, reopens the encrypted database, and verifies it is recovered as EXPIRED.
- Existing conversation/capsule database tests now register migration 4→5.
- Python source contracts verify startup reconciliation, production Room wiring, migration registration and absence of request-content fields in the new entity.

## CI

- Exact-head Quality and Android product-build results will be appended after the workflow finishes. A001 remains partial until the relevant CI gates pass.


## CI feedback

- Run `41875864372` / job `126734953437` — **failed**, 403 Python tests executed, 402 passed and one failed. Failure: the pre-existing `test_android_request_architecture` assertion still required `viewModel()`; it was stale after the request screen began injecting the Room repository through `RequestViewModelFactory`.
- A002 updates that source-contract assertion to the new production factory/repository pattern. This fixes the known test mismatch; it is not itself evidence of passing CI.
- At inspection, product jobs in run `41875864365` were still running (Windows, Android APK/interoperability and HostLifecycle); no result is claimed for them.


## Graph ledger validation correction

- A001 Quality failure was already counted in `failed_attempt_count=1`, but the corresponding history record still used `result=partial`.
- A002 Quality run 41876248248 had 403 Python tests pass, then graph guard failed with `FAILED_ATTEMPT_COUNT_MISMATCH` (summary=1, history=0).
- The history now marks A001 `failed`, matching the summary. A002 remains partial; a new exact-head graph/quality run is required.


## Android formatting gate

- Product run `41876248274`, Android job `126736315663`: failed at `:app:ktlintTestSourceSetCheck`, before APK/JVM/instrumentation.
- Repo `.editorconfig` sets Kotlin `max_line_length=120`.
- Local source inspection found a 146-character Room `.addMigrations(...)` invocation and two `RequestViewModel` lines at 121/122 characters. These were wrapped in A003. Exact-head KtLint rerun remains pending.
- Same product run Windows WPF job `126736316030` and HostLifecycle job `126736316108` passed.

## T0194-A004 source formatting correction

The exact-head KtLint log for run 41876620048 identified remaining main-source issues in `MainActivity.kt` and `RequestViewModel.kt`. A004:
- sorts the imports in both source files using the configured official KtLint ordering (including JVM imports at the end);
- normalizes the requestScreen function signature;
- adds the trailing comma before the arrow in the multi-label when entry;
- removes the excess blank line before the factory declaration.

A004 is formatting-only. CI has not yet verified these changes; APK/JVM/instrumentation acceptance remains pending.

## A005 — case-sensitive KtLint import-order correction

- Re-read `.editorconfig` from the A004 base: `ktlint_code_style=ktlint_official`, Kotlin `max_line_length=120`.
- The official KtLint reference says imports in a group are sorted alphabetically with capital letters before lower-case letters, and the configured official layout places `java.`, `javax.`, and `kotlin.` imports at the end. The previous case-insensitive sort did not satisfy that rule for names sharing the same package prefix.
- A005 re-sorts the full import blocks in `MainActivity.kt` and `RequestViewModel.kt` with case-sensitive lexical order and configured trailing groups.
- Local source check: both import lists equal the comparator's expected order; neither file contains a line longer than 120 characters. No KtLint execution is claimed.
- A004's CI runs on `698308ad00a5d2f3768f09c9a1fb67653ae1dc64` were queued when checked; A005 must be validated on its exact new commit.

## A005 commit and CI state

- A005 is committed in `V 3.0.914` (`c942502c6e1514767f8bc957810feb6bb32fb256`); this identifier is now stored in the graph attempt record.
- Exact commit workflows:
  - Quality Gate run 41879452329 — queued.
  - Android/Windows/HostLifecycle Product Build run 41879452513 — queued.
  - Rendezvous run 41879452384 — queued.
- Earlier commit `698308ad00a5d2f3768f09c9a1fb67653ae1dc64` workflows also remained queued at the time of inspection.
- Code review: `ClientDatabase.getInstance` uses `EncryptedClientDatabaseFactory`; the factory opens Room with SQLCipher and registers migrations 1→2→3→4→5. The new app-level lifecycle entity contains request ID, enum status, timestamps, attempt count and fixed error code only. The Room DAO marks all prior QUEUED/CONTACTING/WAITING rows EXPIRED transactionally on recovery. The request ViewModel awaits recovery and persists QUEUED/CONTACTING before dispatch.
- Tests present: file-backed Room reopen/recovery, migration from v1 through v5, ViewModel process-restart recovery, exhaustive EXPIRED presentation, and SQLCipher instrumentation reopen. Presence of tests is not evidence that they passed.
- Local tests could not be run because Gradle and dotnet are unavailable in this environment and Git clone was blocked by DNS. The graph remains in_progress pending exact-head CI.

## A005 exact-head CI outcome

- Quality Gate run 41879924836: passed. Its log confirms 403 Python tests, syntax compilation, graph structure validation (230 tasks/422 edges), and recorded-output/evidence validation. One non-blocking unmapped dependency warning in T0001 remains preserved as metadata.
- Rendezvous container run 41879924251: passed.
- Product Build run 41879926235, Android job 126749441659: failed at KtLint test-source check:
  - ClientDatabaseTest.kt — four diagnostics across two Room builder chains.
  - ConversationContextCapsulePersistenceTest.kt — one chain-continuation diagnostic.
  - RequestViewModelTest.kt — two chained-call continuation diagnostics.
- APK build, permission verification, JVM tests and Keystore instrumentation were skipped.
- A006 corrects all seven diagnostics while leaving request lifecycle behavior, database schema and security boundaries unchanged. Targeted source and 120-column checks pass; KtLint itself is not claimed as passed.
- T0194 remains in_progress pending exact-head validation.

## A006 commit and A007 formatting follow-up

- A006 was committed in `V 3.0.916` (`85dc1ae10028b144b794f413ea5b0c299c644d42`).
- Its exact-head workflows were created: Product Build `41881677064` (queued), Quality Gate `41881677125` (in progress), Rendezvous `41881677074` (in progress).
- Review against the existing SQLCipher production builder in `EncryptedClientDatabaseFactory.kt` found the newly wrapped Room builder calls in `ClientDatabaseTest.kt` did not yet have the same continuation indentation for the method arguments and closing `addMigrations` call.
- A007 aligns both test builders to the existing production chain layout. The change is formatting-only; no schema or behavior change is intended.
- Exact-head KtLint and all Android build/test steps remain pending.

