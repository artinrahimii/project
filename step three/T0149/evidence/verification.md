# T0149 evidence ledger

- **Implementation/test revision:** [`V 3.0.541`](https://github.com/artinrahimii/project/commit/c619d3b1269357563ab2f2047ad6a7a2bf126d5b).
- **Actual database output:** `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt` contains the on-disk database close/reopen test for message ordering and request-state values/metadata.
- **Python contract test:** `tests/test_android_conversation_detail_contract.py` includes a source check for the file-backed regression.
- **Observed failure:** [quality run 40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336) ran 230 tests and failed one assertion because the test expected `Room.databaseBuilder(` contiguously, whereas the real Kotlin source has a line break. This is a test-source formatting mismatch; the database runtime test was not executed by the Python suite.
- **Correction:** A003 separates checks for `Room` and `.databaseBuilder(`. The corrected code has not yet passed exact-head CI.
- **Not proven yet:** actual Room file-backed reopen test, Android lint/build/JVM tests, new commit quality gate and graph/evidence checks.
