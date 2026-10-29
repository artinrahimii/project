# T0149 execution report

- **Task:** Restore messages and request states after the app is closed and reopened.
- **State:** `in_progress`; no acceptance claim yet.
- **Baseline implementation:** [`V 3.0.541`](https://github.com/artinrahimii/project/commit/c619d3b1269357563ab2f2047ad6a7a2bf126d5b).
- **Quality run on V 3.0.541:** [40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336) failed one newly added source-contract assertion after running 230 Python tests. The failed assertion expected `Room.databaseBuilder(` as contiguous text; the real Kotlin source formats this call across lines as `Room` then `.databaseBuilder(`.
- **Product run on V 3.0.541:** [40874466234](https://github.com/artinrahimii/project/actions/runs/40874466234) was still in progress at this report revision; its final result must be checked separately.

## Dependency review

- **T0148 → T0149:** reviewed as `sequencing_only`. T0148 restores the unsent composer draft through SavedStateHandle; T0149 verifies persistent message/request rows in Room and consumes no API unique to draft restoration. The original `blocks` edge is preserved.
- **T0138 → T0149:** reviewed as `sequencing_only`. The host-visibility/privacy disclosure is not an API used by the local database-reopen test; the original `blocks` edge is preserved.
- Reasons/evidence are appended to graph `edge_change_log`. T0149 → T0150 remains unreviewed for the next task.

## Implementation

- Added `messagesAndRequestStatusesSurviveClosingAndReopeningFileBackedDatabase` in `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt`. It writes a conversation, ordered user/assistant messages, and WAITING/FAILED request-state rows to a named on-disk Room database, closes that instance, opens the same file again, then checks recovered IDs/content/order/status/attempt/error metadata. Temporary database cleanup is in `finally`.
- Added a Python source-contract check in `tests/test_android_conversation_detail_contract.py`.
- The new test itself remains unchanged. The Python assertion has been adjusted to separately check `Room` and `.databaseBuilder(`, matching the existing source formatting.

## Attempt history and verification

- **A001 / V 3.0.541:** implementation and test added; CI quality suite then ran 230 tests but failed the new formatting-sensitive Python assertion. The graph gate did not run because the test suite failed.
- **A002:** records the observed quality-run failure; preserved as failed history.
- **A003:** corrects the assertion only. Exact-head quality and product CI are pending and have not been claimed successful.

Keep T0149 `in_progress` until the Python suite and graph/evidence guards pass and Android ktlint, APK build, and the file-backed Room reopen JVM test pass on an exact commit.
