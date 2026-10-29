

## V 3.0.553 CI failure and A002 correction

- [Quality run 40887316120](https://github.com/artinrahimii/project/actions/runs/40887316120) ran 239 tests and failed three older assertions coupled to Room schema version 2 and the earlier single migration. The new T0152 contract checks themselves passed.
- [Product run 40887316116](https://github.com/artinrahimii/project/actions/runs/40887316116) failed Android ktlint on two Kotlin test formatting findings; APK and JVM tasks were skipped. HostLifecycle passed; Windows WPF was not yet complete when recorded.
- The fix corrects old tests to assert schema v3, both registered migrations and the v1→v3 path, and reformats the flagged Kotlin expressions. This attempt is not considered successful until exact-head CI passes.
# T0152 evidence ledger

- **Entity:** `src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationContextCapsuleEntity.kt` stores one BLOB per `conversationId` and declares a Room foreign key with cascade deletion.
- **DAO/repository:** `RoomConversationContextCapsuleRepository.kt` uses `ConversationContextCapsule.encode/decode`; rejected encodings do not reach the database, and malformed stored bytes are exposed as a distinct result.
- **Schema migration:** `ClientDatabase.kt` registers version 3 and `MIGRATION_2_3`; `ClientDatabaseMigrationTest.kt` now verifies migration from v1 through v2 into v3 and preservation of existing conversation/message/request/draft rows.
- **Acceptance test:** `ConversationContextCapsulePersistenceTest.kt` exercises separate conversation scopes, file-backed close/reopen, upsert isolation, cascade deletion, invalid-capsule rejection and foreign-key enforcement.
- **Regression guard:** `tests/test_android_context_capsule_persistence_contract.py` asserts critical schema, migration, repository and test contracts.
- **Dependency evidence:** T0150 → T0152 validated as `sequencing_only`; T0151 → T0152 remains `contract_or_interface_dependency`. The `blocks` relation is retained.
- **Not yet proven:** exact-head Android ktlint/build/JVM test results and post-change CI. This ledger will be amended with those results before T0152 can be marked done.


## V 3.0.554 CI result and A003

- [Quality run 40887843412](https://github.com/artinrahimii/project/actions/runs/40887843412) passed 239 Python tests and graph validation with zero errors.
- [Product run 40887843362](https://github.com/artinrahimii/project/actions/runs/40887843362) failed Android ktlint at `ConversationContextCapsulePersistenceTest.kt:149–152` due to argument indentation; build/JVM stages were skipped.
- A003 fixes the remaining indentation. No success is claimed until the exact-head product pipeline passes.
