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
