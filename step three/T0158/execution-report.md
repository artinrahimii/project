# T0158 execution report

- **Base revision:** `e4293fb9f5fce5bbb3aa346383ef41cc1055cf90` on `main` (`V 3.0.579`).
- **Graph source:** `step three/project-graph.md`, Git Blob `8a5041a47a5870815090982dfe049b0dce40c105` from the exact base tree. The original fenced Markdown/JSON was checked against its declared byte size and recomputed Git Blob SHA-1 before parsing; it contains 229 tasks and 417 edges.
- **Task:** Record local capsule changes with source and time without adding a permanent conversation identifier to network requests.
- **Dependency review:**
  - `T0157 → T0158`: kept as `blocks`; validated `contract_or_interface_dependency`. T0158 records a change only when the accepted ViewModel save operation succeeds, after the T0157 conflict-review gate. Evidence includes the ViewModel, repository/DAO, tests and T0157 report.
  - `T0150 → T0158`: kept as `blocks`; validated `contract_or_interface_dependency`. T0150 provides the local Room conversation identity and persistence lifecycle used to scope and clean up provenance. That local identifier is not used for network routing. Evidence includes ConversationEntity, the Room migration, conversation workflow tests, T0150 report, requirements and Protobuf.
  - `T0158 → T0159` remains `unreviewed`; it was not reclassified to permit execution.
- **Implementation:** Added local Room change records with sources `USER_EDIT`, `LOCAL_HISTORY_SUMMARY` and `LOCAL_HISTORY_SUMMARY_REVIEWED`. Each event contains a timestamp, capsule schema version, serialized byte count and JSON array of changed field names only. It does not copy capsule field values or messages. The capsule row and change record are written through one Room transaction. Deleting the capsule cascades to its local change records.
- **Protocol boundary:** `protocol/whitep/v1/messages.proto` is unchanged. `InferenceRequest` retains its existing temporary `request_id`/`attempt_id` and bounded capsule bytes; no permanent `conversationId` and no complete history were added.
- **Migration:** Room database schema moves from v3 to v4 using non-destructive `MIGRATION_3_4`; old capsule/request/message records are not intentionally rewritten.
- **Tests added/updated:** Room persistence tests cover source/time/field-name metadata, not duplicating content values, delete cascade and migration. A ViewModel test covers source attribution after an explicitly reviewed Save. Python source contracts guard migration and network-boundary constraints.
- **Local execution:** The connected GitHub tools do not provide a local Gradle/Android/Python runtime. No local test run is claimed.
- **Current status:** `in_progress`; the Python NameError and subsequent KtLint issues were recorded and corrected stepwise. V 3.0.583 quality and Windows/HostLifecycle checks passed, but KtLint reported `Unexpected newline before '.'` at two test builder chains. A005 attaches the following call to the closing parenthesis; V 3.0.584 product checks are pending. No acceptance claim is made.
- **Next action:** Inspect exact-head quality/product CI. Fix any compile, schema migration, lint or test failures before marking T0158 complete.


## T0158-A001 CI result and T0158-A002 correction

- Exact candidate commit V 3.0.580: 9673db6f13e46c05b289dd2e096eeeb0e4a29eb7.
- Step Three workflow [40945024330](https://github.com/artinrahimii/project/actions/runs/40945024330) failed in test_runtime_upgrade_test_checks_version_data_and_index: NameError: name 'database' is not defined. The run completed 257 tests; this was the only error, with 256 tests passing. The assertion was accidentally placed in the test method whose local variable is test_source, not database.
- Corrected by moving the conversation_context_capsule_changes assertion into test_room_version_and_migration_are_registered, where database is loaded.
- The V 3.0.580 Android product workflow was still running when this correction was recorded; it is not counted as passed or failed here.
- T0158 stays in_progress and A001 stays in the history as a failed attempt. The follow-up commit is V 3.0.581; its exact-head workflows must pass before acceptance.


## T0158-A002 CI result and T0158-A003 correction

- Exact candidate commit `V 3.0.581`: `09c6636a866cc12242a02b699001edf568779c7c`.
- Step Three quality workflow [40945445001](https://github.com/artinrahimii/project/actions/runs/40945445001) succeeded; the log confirms `Ran 257 tests` and graph/evidence gates passed.
- Android product workflow [40945444883](https://github.com/artinrahimii/project/actions/runs/40945444883) failed in Kotlin lint: line 52 of `ConversationContextCapsuleViewModelTest.kt` had a long `.addMigrations(...)` call (120-column limit and separate-argument formatting). The APK build and JVM tests were not reached on that run.
- Windows WPF build and HostLifecycle unit/integration jobs both passed on the same head.
- A003 changes only the migration-registration formatting in the test source; T0158 remains `in_progress` until exact-head V 3.0.582 checks pass.


## T0158-A003 CI result and T0158-A004 correction

- Exact candidate commit `V 3.0.582`: `43d636c0b325be15cfb1931bf6c07f1f71076a6e`.
- Step Three quality workflow [40945957102](https://github.com/artinrahimii/project/actions/runs/40945957102) succeeded; its 257-test Python suite and graph/evidence checks passed.
- Android product workflow [40945957438](https://github.com/artinrahimii/project/actions/runs/40945957438) failed at KtLint, line 91 of `ClientDatabaseMigrationTest.kt`, because the three `addMigrations` arguments had to be put on separate lines. APK build and Android JVM tests were not reached.
- Windows WPF build/startup smoke and HostLifecycle unit/integration jobs passed on the same head.
- A004 formats the migration call and updates `test_android_database_migration_contract.py` to verify the multi-line registration safely. T0158 remains `in_progress` until exact-head V 3.0.583 gates pass.


## T0158-A004 CI result and T0158-A005 correction

- Exact candidate commit `V 3.0.583`: `378de686fc2bcb0bb8c4c54bd0728de5a4d6833f`.
- Step Three quality workflow [40946486989](https://github.com/artinrahimii/project/actions/runs/40946486989) passed its 257 Python tests and graph/evidence checks.
- Android product workflow [40946486860](https://github.com/artinrahimii/project/actions/runs/40946486860) failed at KtLint with `Unexpected newline before '.'` at `ClientDatabaseMigrationTest.kt:96` and `ConversationContextCapsuleViewModelTest.kt:57`. APK/JVM test steps were not reached.
- Windows WPF build/startup/policy checks and HostLifecycle unit/integration both passed on the same head.
- A005 attaches the next chained call to the closing parenthesis for both builders; exact-head V 3.0.584 CI is pending.
