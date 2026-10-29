

## First exact-head CI result — V 3.0.553

- Quality [run 40887316120](https://github.com/artinrahimii/project/actions/runs/40887316120) ran **239 Python tests** and failed three existing contract assertions that hard-coded database schema v2 and a single 1→2 migration. The four newly added T0152 source-contract tests passed. The graph guard and subsequent quality checks were skipped after the suite failed.
- Product [run 40887316116](https://github.com/artinrahimii/project/actions/runs/40887316116) passed HostLifecycle but Android `ktlintTestSourceSetCheck` failed two format findings: `ClientDatabaseMigrationTest.kt:108` and `ConversationContextCapsulePersistenceTest.kt:147`. APK/JVM test stages were skipped; Windows WPF was still running at report time.
- **T0152-A001 is recorded as failed** rather than accepted. **T0152-A002** updates the two legacy Python guard files to expect schema v3 and both migrations, reformats both Kotlin expressions, and awaits a new exact-head CI run.
# T0152 execution report

- **Task:** Persist each versioned conversation context capsule in Room as a conversation-scoped record.
- **Status:** `in_progress`; implementation and evidence are committed with exact-head CI pending.
- **Baseline:** `main` at [`V 3.0.552`](https://github.com/artinrahimii/project/commit/354bed16594c5c344acb78a77d40ddbd4f31f055).
- **Attempt:** `T0152-A001`.
- **Scope boundary:** local Room persistence only. This task does not add a network request path, expose the capsule in UI, or claim encryption at rest.

## Dependency review

- **T0151 → T0152:** the existing `contract_or_interface_dependency` remains validated. The Room repository stores the output of the versioned v1 capsule codec and decodes it using that same contract.
- **T0150 → T0152:** reviewed against the actual accepted T0150 workflow test and DAO/ViewModel contracts. T0150 tests conversation/message/request-state creation and history restoration; it does not create or consume a capsule API. Classification is now `sequencing_only`, validation `validated`; the original `blocks` edge remains as conservative order. Rationale/evidence is appended to `edge_change_log`.

## Implementation

- Added `ConversationContextCapsuleEntity` with the conversation ID as its primary key, serialized v1 bytes in a BLOB, and an update timestamp. A foreign key to `conversations.conversationId` cascades when its parent conversation is deleted.
- Added a Room DAO for one-record-per-conversation lookup, observation, upsert, deletion and count.
- Added `RoomConversationContextCapsuleRepository`; saves encode the capsule and reject invalid payloads before touching Room, while reads decode and represent missing/corrupt values explicitly.
- Raised Room schema version from 2 to 3 with a sequential `MIGRATION_2_3`; the database builder registers both migrations. Extended the v1 migration acceptance test so a version-1 database traverses both migrations to version 3 without losing legacy records.
- Added Robolectric persistence tests for independent capsules across two conversations, database close/reopen, replacement of one capsule without changing the other, cascade deletion, rejection of invalid capsules, and foreign-key rejection of orphan records.
- Added Python source-contract checks for schema, migration, repository and acceptance-test coverage. No new dependencies were added.

## Verification status

The source changes and graph record are committed as `V 3.0.553`. Exact-head CI is pending; no local test execution is claimed. Keep T0152 `in_progress` until Android lint, APK build, Android JVM tests, generated Protobuf verification, Windows WPF/HostLifecycle jobs and graph/evidence guards pass.
