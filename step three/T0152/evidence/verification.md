# T0152 evidence ledger

- **Entity:** `src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationContextCapsuleEntity.kt` stores one BLOB per `conversationId` and declares a Room foreign key with cascade deletion.
- **DAO/repository:** `RoomConversationContextCapsuleRepository.kt` uses `ConversationContextCapsule.encode/decode`; rejected encodings do not reach the database, and malformed stored bytes are exposed as a distinct result.
- **Schema migration:** `ClientDatabase.kt` registers version 3 and `MIGRATION_2_3`; `ClientDatabaseMigrationTest.kt` now verifies migration from v1 through v2 into v3 and preservation of existing conversation/message/request/draft rows.
- **Acceptance test:** `ConversationContextCapsulePersistenceTest.kt` exercises separate conversation scopes, file-backed close/reopen, upsert isolation, cascade deletion, invalid-capsule rejection and foreign-key enforcement.
- **Regression guard:** `tests/test_android_context_capsule_persistence_contract.py` asserts critical schema, migration, repository and test contracts.
- **Dependency evidence:** T0150 → T0152 validated as `sequencing_only`; T0151 → T0152 remains `contract_or_interface_dependency`. The `blocks` relation is retained.
- **Not yet proven:** exact-head Android ktlint/build/JVM test results and post-change CI. This ledger will be amended with those results before T0152 can be marked done.
