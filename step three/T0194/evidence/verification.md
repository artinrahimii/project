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
