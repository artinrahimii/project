# T0142 verification evidence

## Scope

Revision under test: implementation commit to be resolved from the online main history after the tree update.

## Dependency review

- T0141 → T0142: validated as contract_or_interface_dependency. The migration must upgrade the concrete Room version-1 schema produced by T0141. Evidence: ClientDatabase.kt, RequestStateEntity.kt, ClientDatabaseMigrationTest.kt, and the checked-in v1-schema.sql fixture.
- T0138 → T0142: validated as sequencing_only. The processing-host privacy disclosure remains a requirement context, but it does not provide a Kotlin schema/API consumed by the migration. Evidence: ADR-0019, ADR-0020, and the version-1 schema fixture.

## Acceptance evidence planned for exact-head CI

- The runtime migration test must open a real version-1 database, preserve conversation/message/request-state/draft rows, and expose the version-2 composite index.
- The Android product workflow must pass ktlint, assembleDebug, testDebugUnitTest, Protobuf checks, and the APK artifact step.
- The Step Three quality gate must validate the graph and its evidence paths.

No test result is recorded as passed until a successful CI run for the implementation commit is confirmed.
