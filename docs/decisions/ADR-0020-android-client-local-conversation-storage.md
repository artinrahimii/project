# ADR-0020: Android client-local conversation storage

- **Status:** Initial Room schema accepted; T0184 integrates SQLCipher-backed encryption and plaintext migration. Exact-commit acceptance remains pending.
- **Date:** 2026-10-29
- **Related tasks:** T0141 and T0142.
- **Related requirements:** REQ-009, REQ-011, REQ-019.

## Context

The Android client needs durable, device-local conversation, message, draft, and request-status data. The current `ClientRequestRepository` contract is a Flow/suspend boundary, and the existing in-memory implementation explicitly reports that host transport is unavailable. A local database must not be misrepresented as an implemented network transport or a central conversation store.

## Decision

1. Store Android conversation, message, request-state, and current-draft records in Room using the app's private application context and database file `whitep-client.db`.
2. Conversation messages and the current draft are intentional local persistence. This initial schema does not synchronize these rows to a central service or rendezvous server.
3. Request-state rows contain request identifiers, conversation relationship, lifecycle status, timestamps, attempt number, and an optional typed error code. They do not contain prompt text, context-capsule bytes, or response bodies.
4. Deleting a conversation cascades to its message and request-state rows. Clearing the current draft deletes its Room row.
5. The original Room-only storage baseline was not encrypted at rest. T0184 integrates SQLCipher Community Edition 4.20.0 with Room 2. A random 256-bit database passphrase is wrapped by the purpose-separated Android Keystore protector; only its authenticated envelope is stored in app-private preferences. A legacy plaintext SQLite file is exported into a separate encrypted file, integrity-checked, and atomically replaced only after success. Missing key material for an already encrypted database must fail closed rather than creating a replacement key. Exact-commit runtime acceptance is tracked in `step three/T0184/` and is not claimed until its instrumentation tests pass.
6. Schema version 1 is the initial database contract. T0142 must add checked-in schema/migration verification and upgrade tests; this task does not claim that migration behavior is implemented.
7. ADR-0019 remains in force: a processing host can read the prompt and context capsule it receives after transport decryption. This local-storage choice neither changes that endpoint trust boundary nor proves Android-to-Windows P2P delivery.

## Implementation and acceptance evidence

- Room schema and app database: `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`.
- Local entities and DAOs: `ConversationEntity.kt`, `MessageEntity.kt`, `RequestStateEntity.kt`, and `ClientRequestDraftEntity.kt` in the same package.
- Existing request repository contract adapter: `RoomClientRequestRepository.kt`.
- Runtime database acceptance tests: `ClientDatabaseTest.kt`.
- Repository/privacy boundary regression: `tests/test_android_local_storage_boundary.py`.
- Clean-build/test workflow: `.github/workflows/product-build.yml`.

The original Room acceptance tests are scoped to local persistence, deterministic message ordering, deletion cascades, and request-state restoration. They do not prove cross-device delivery or deployment-level privacy. SQLCipher at-rest encryption is covered separately by the T0184 instrumentation tests, whose exact-commit result is pending.


## T0184 — SQLCipher-backed Room storage

Room now opens through SQLCipher's `SupportOpenHelperFactory`. The per-database random passphrase is wrapped by `AndroidKeyStoreProtectedBlobProtector` using the separate alias `com.whitep.client.database_key.v1`; the raw passphrase is not stored in source or preferences.

For a legacy plaintext database, migration checkpoints the WAL, exports schema and records to a separately encrypted temporary database using `sqlcipher_export()`, preserves Room's `user_version` and the database `application_id`, checks integrity, and atomically renames the verified target over the original file. A missing or invalid key for an encrypted file fails closed.

The code and instrumentation scenarios are present, but runtime acceptance is pending the exact-head Android CI run. The design does not claim forensic erasure of flash blocks, protection of plaintext while it is in process memory, or hardware-backed TEE/StrongBox on every device.


## T0194 — App-level inference lifecycle metadata

The Android request screen now uses a separate Room table, `client_inference_request_states`, because the request screen is not necessarily attached to a local conversation. It stores only an opaque request ID, lifecycle enum, timestamps, attempt count and a fixed error code. It has no prompt, context-capsule, peer identity, payload or response columns.

The database is migrated from schema version 4 to 5 through the same SQLCipher-backed Room factory; no destructive migration fallback is used. On creation of a new request ViewModel, the repository transactionally marks stale `QUEUED`, `CONTACTING` and `WAITING` rows as `EXPIRED` and exposes the latest terminal state. `APP_CLOSED_BEFORE_RESULT` means the Android client did not record a response before the app/request screen lifecycle ended. It does **not** mean a remote host was cancelled or did not finish inference. Only the latest 20 app-level lifecycle records are retained.

Conversation-linked `request_states` remain unchanged in purpose and keep their conversation foreign key/cascade behavior. The new app-level table is local to the encrypted Room database and is not synchronized to Rendezvous or a central service.
