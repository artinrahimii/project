# T0149 evidence ledger

- `undefined` contains a disk-backed Room close/reopen test covering conversation details, ordered messages, persisted request statuses, attempt counts and error metadata.
- V 3.0.542 quality [40874779599](https://github.com/artinrahimii/project/actions/runs/40874779599) passed 230 Python tests and graph/evidence validation.
- V 3.0.541 quality [40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336) failed a brittle multiline `Room.databaseBuilder(` assertion.
- V 3.0.542 product [40874779588](https://github.com/artinrahimii/project/actions/runs/40874779588) failed Android ktlint on two wrapped DAO chains; APK/JVM tests skipped.
- V 3.0.543 quality [40875172297](https://github.com/artinrahimii/project/actions/runs/40875172297) failed another source assertion that assumed `reopenedDatabase.messageDao()` was on one line.
- V 3.0.544 updates the assertion to be line-wrap-insensitive; exact-head CI pending.
- Runtime persistence is not yet proven until the Android JVM/Robolectric test succeeds.
