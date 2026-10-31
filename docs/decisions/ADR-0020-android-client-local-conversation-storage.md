# ADR-0020: Android client-local conversation storage

- **Status:** Accepted for the initial Room schema; encryption-at-rest and migration acceptance remain separate tasks.
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
5. **The database is not encrypted at rest by this task.** Android app-private storage is not a substitute for at-rest encryption. T0184 remains responsible for selecting and testing an encrypted-storage approach before the product claims that stored conversation content is encrypted at rest.
6. Schema version 1 is the initial database contract. T0142 must add checked-in schema/migration verification and upgrade tests; this task does not claim that migration behavior is implemented.
7. ADR-0019 remains in force: a processing host can read the prompt and context capsule it receives after transport decryption. This local-storage choice neither changes that endpoint trust boundary nor proves Android-to-Windows P2P delivery.

## Implementation and acceptance evidence

- Room schema and app database: `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`.
- Local entities and DAOs: `ConversationEntity.kt`, `MessageEntity.kt`, `RequestStateEntity.kt`, and `ClientRequestDraftEntity.kt` in the same package.
- Existing request repository contract adapter: `RoomClientRequestRepository.kt`.
- Runtime database acceptance tests: `ClientDatabaseTest.kt`.
- Repository/privacy boundary regression: `tests/test_android_local_storage_boundary.py`.
- Clean-build/test workflow: `.github/workflows/product-build.yml`.

The database acceptance tests are scoped to local persistence, deterministic message ordering, deletion cascades, and request-state restoration. They do not prove cross-device delivery, encryption at rest, or deployment-level privacy.
