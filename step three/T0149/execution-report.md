# T0149 execution report

- **Task:** Restore messages and request states after the app is closed and reopened.
- **Baseline:** `main` at `acc40b5a1d53e6a5d41d8366b473022ffdb777ac` (`V 3.0.540`).
- **Attempt:** `T0149-A001`; result `partial`; exact-head CI pending.
- **Status:** `in_progress`.

## Dependency review

- **T0148 → T0149:** reviewed as `sequencing_only`. T0148 stores the unsent editor draft in SavedStateHandle; T0149 validates durable message and request-state rows held by Room. The latter consumes no API/output unique to draft restoration. The original `blocks` edge is retained.
- **T0138 → T0149:** reviewed as `sequencing_only`. T0138 establishes the receiving-host visibility/privacy disclosure; the local persistence check consumes no API from it and does not assert network delivery or encryption at rest. The original `blocks` edge is retained.
- Both classification reviews, reasons and evidence paths were appended to `edge_change_log`. The outgoing `T0149 → T0150` edge remains unreviewed for the next task.

## Implementation/test output

- Added `messagesAndRequestStatusesSurviveClosingAndReopeningFileBackedDatabase` to `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt`.
- The test uses a named on-disk Room database, writes one conversation, two chronologically ordered messages, and WAITING/FAILED request-state rows, closes the first database instance, then opens a second instance on the same file and checks that titles, message IDs/content/order, request statuses, attempt numbers, and error code survive.
- The test always closes each database instance and deletes its temporary database file in a `finally` block.
- Added repository-root source-contract coverage to `tests/test_android_conversation_detail_contract.py`.
- Existing production behavior is local Room storage via `whitep-client.db`; this task adds regression evidence for reading those persisted rows after database reopen. It does not add network transport and does not claim at-rest encryption.

## Verification status

No local Gradle/Kotlin or Python execution is claimed. Exact-head CI is pending for:
- the file-backed Room close/reopen JVM/Robolectric test;
- Android `ktlintCheck`, `assembleDebug`, and `testDebugUnitTest`;
- Step Three quality, graph-structure, and output/evidence validation.

Keep the task `in_progress` until those results are inspected. If any test fails, append a failed/partial attempt and fix it before marking the task done.
