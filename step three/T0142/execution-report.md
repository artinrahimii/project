# T0142 execution report — retry after CI findings

- **Task:** Add a Room database version migration and migration regression test.
- **Status:** in_progress. Exact-head validation of V 3.0.507 failed, so task completion is not claimed.
- **Base revision for original implementation:** eef2e2a83da3ba23ec88e92f76ae5c7b0452b5ce (V 3.0.506).
- **Implementation commit:** 55dbf0ac0a7429ecf5acf56bdd508a2de92a2af9 (V 3.0.507).
- **Implementation:** Room database version changed from 1 to 2. Migration 1→2 adds a composite index on request_states(conversationId, createdAtEpochMillis, requestId); it does not drop/recreate tables or copy/delete user data. Entity metadata declares the same index.
- **Historical fixture:** src/Client.Android/app/src/test/resources/db/v1-schema.sql records the previous schema and indexes.
- **Runtime acceptance test:** ClientDatabaseMigrationTest.kt creates a version-1 SQLite database with a conversation, message, request state, and draft, opens it through Room, and verifies version 2, row preservation, status/error fields, draft preservation, and index existence.
- **Failed attempt A001 — exact revision V 3.0.507:**
  - Step Three quality run 40842236474 failed after 211 tests because the existing tests/test_android_local_storage_boundary.py still expected version = 1. The graph validation steps were skipped after the Python test failure. [Run](https://github.com/artinrahimii/project/actions/runs/40842236474)
  - Android product run 40842236492 failed at ktlintCheck: argument wrapping and chaining format in ClientDatabaseMigrationTest.kt. Android APK and JUnit tests were skipped. [Run](https://github.com/artinrahimii/project/actions/runs/40842236492)
  - Windows WPF build and host lifecycle tests passed on the same commit, but do not substitute for Android acceptance.
- **Attempt A002:** Updated the stale version assertion to require version 2 and the registered migration; corrected the two exact ktlint locations. These corrections are pending validation on the next exact-head CI run.
- **Privacy boundary:** Migration changes only the request-state index. It adds no prompt, context-capsule, or response-body columns to request_states; existing local conversation/message/draft storage semantics remain unchanged. Storage is still not claimed to be encrypted at rest.
- **Dependency review:** T0141 → T0142 is validated as a schema contract dependency. T0138 → T0142 is validated as sequencing-only; both classification changes and evidence are in edge_change_log.
- **Tests on the current revision:** No new test result is recorded as passed yet. Do not mark T0142 done until the Step Three quality gate and the Android product workflow both pass on the corrected commit.
