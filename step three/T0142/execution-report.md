# T0142 execution report — third attempt pending CI

- **Task:** Add a Room database version migration and regression test for upgrades.
- **Current status:** in_progress; the migration has not passed its runtime acceptance test yet.
- **Implementation commit:** V 3.0.507 (`55dbf0ac0a7429ecf5acf56bdd508a2de92a2af9`).
- **Attempt A001 — failed:** Step Three quality gate run 40842236474 failed because the existing storage-boundary test still expected Room schema version 1; Android product run 40842236492 failed ktlint. Windows WPF and host lifecycle eventually passed, but this does not verify Android migration behavior.
- **Attempt A002 — failed:** Step Three quality gate run 40843247078 passed, including automated tests, syntax, graph structure and output/evidence checks. Android product run 40843247158 failed ktlint with two chained-call formatting violations in ClientDatabaseMigrationTest.kt (lines 86 and 91) and a >120-character SQL literal line in ClientDatabase.kt (line 53). Android APK assembly/unit tests were skipped. Host lifecycle tests passed.
- **Attempt A003 — pending exact-head CI:** Corrected the method-chain wrapping and split the long SQL index name over concatenated string literals. The existing version-boundary test now requires database version 2 and verifies migration registration.
- **Runtime test plan implemented:** Build a version-1 database with a conversation, message, request state and draft; open it with Room migration 1→2; assert that existing values survive and the composite request-state index exists.
- **Data safety:** Migration adds an index only; it does not recreate/drop tables or delete rows. The request-state schema still excludes prompt, context-capsule and response content. Existing app-private conversation/draft storage remains not encrypted at rest, as explicitly documented in ADR-0020.
- **Dependency validation:** T0141 → T0142 is a validated schema contract dependency. T0138 → T0142 is sequencing-only. Their recorded rationale/evidence remain in the graph edge change log.
- **Acceptance status:** Do not mark done until the new exact-head CI passes ktlint, builds the Android APK, runs Android unit tests (including ClientDatabaseMigrationTest), checks generated Protobuf sources, and the Step Three quality gate passes.
