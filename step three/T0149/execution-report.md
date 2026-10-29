# T0149 execution report

- **Task:** Restore messages and request states after the app is closed and reopened.
- **Status:** `in_progress`; exact acceptance is still pending.
- **Database-reopen test:** `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt`, `messagesAndRequestStatusesSurviveClosingAndReopeningFileBackedDatabase`.
- **V 3.0.541 quality:** [run 40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336) ran 230 Python tests but failed one brittle assertion: it expected `Room.databaseBuilder(` as contiguous text when the Kotlin source formats it across lines.
- **V 3.0.542 quality:** [run 40874779599](https://github.com/artinrahimii/project/actions/runs/40874779599) passed 230 Python tests, syntax and graph/evidence checks (229 tasks/417 edges; zero errors, one preserved T0001 metadata warning).
- **V 3.0.542 product:** [run 40874779588](https://github.com/artinrahimii/project/actions/runs/40874779588) failed Android ktlint on two chain-wrapping rules at `ClientDatabaseTest.kt:257` and `:270`; APK and JVM tests were skipped. HostLifecycle passed; Windows WPF was still running at the last read.
- **V 3.0.543** changes only multiline chain formatting so the receiver is on its own line before `.messageDao()` and `.requestStateDao()`. Its CI results are pending.

## Scope and dependency review

The test seeds a file-backed Room database with a conversation, ordered messages, and WAITING/FAILED request-state rows, closes the first instance, opens the same file again, and verifies the recovered title, message IDs/content/order, states, attempt numbers, and error code. It closes both instances and deletes the temporary database in a `finally` block.

- **T0148 → T0149:** `sequencing_only`; T0148 persists the composer draft in SavedStateHandle while this task verifies Room-persisted message/status rows. Original `blocks` edge preserved.
- **T0138 → T0149:** `sequencing_only`; privacy disclosure supplies no database API consumed by the test. Original `blocks` edge preserved.
- Rationale/evidence is stored in graph `edge_change_log`; outgoing T0149 → T0150 remains unreviewed.

## Attempt history

- **A001:** adds the file-backed regression and Python source check.
- **A002:** records the quality test failure from V 3.0.541.
- **A003:** corrects the Python string assertion; V 3.0.542 quality passes.
- **A004:** records V 3.0.542 Android ktlint failure; no APK/JVM run.
- **A005:** corrects the two Kotlin chain-wrapping findings; CI pending.

No successful Room close/reopen runtime test is claimed until the exact-head Android JVM test passes.
