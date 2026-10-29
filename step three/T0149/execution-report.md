# T0149 execution report

- **Task:** Restore messages and request states after the app is closed and reopened.
- **Status:** `in_progress`; exact acceptance pending.
- **File-backed regression:** `undefined`, `messagesAndRequestStatusesSurviveClosingAndReopeningFileBackedDatabase`.
- **V 3.0.541 quality** [40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336): 230 tests ran; a Python assertion expected `Room.databaseBuilder(` contiguously despite a Kotlin line break.
- **V 3.0.542 quality** [40874779599](https://github.com/artinrahimii/project/actions/runs/40874779599): passed 230 Python tests and graph/evidence checks (229 tasks/417 edges, zero errors, one preserved T0001 warning).
- **V 3.0.542 product** [40874779588](https://github.com/artinrahimii/project/actions/runs/40874779588): Android ktlint failed two chain-wrapping findings; APK/JVM steps were skipped. HostLifecycle passed.
- **V 3.0.543 quality** [40875172297](https://github.com/artinrahimii/project/actions/runs/40875172297): 230 tests ran; a Python assertion expected `reopenedDatabase.messageDao()` as one line even though ktlint requires a multiline chain. Graph/syntax steps skipped.
- **V 3.0.544** makes the source-contract check assert the receiver and DAO selector names independently. Exact-head workflows are pending.

## Implementation and dependency review

The test opens a named on-disk Room database, writes a conversation, ordered messages, and WAITING/FAILED request-state rows, closes the first instance, reopens the same file, and verifies recovered details, message order/content, states, attempt numbers, and error code. It closes both instances and deletes the temporary DB in `finally`.

- **T0148 → T0149:** `sequencing_only`; SavedStateHandle draft restoration does not provide the Room-persistence API tested here. Original `blocks` edge retained.
- **T0138 → T0149:** `sequencing_only`; the host-visibility disclosure is not a database API. Original `blocks` edge retained.
- Both reasons/evidence are recorded; T0149 → T0150 remains unreviewed.

## Attempt history

A001 initial implementation; A002 quality assertion failure; A003 Python assertion correction; A004 ktlint failure; A005 Kotlin chain formatting correction; A006 repeat Python source assertion failure; A007 line-wrap-insensitive source assertion correction. All failed attempts remain in the history. The file-backed Room JVM test has not yet been observed passing; T0149 remains `in_progress`.
