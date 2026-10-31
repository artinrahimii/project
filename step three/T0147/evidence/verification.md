# T0147 evidence ledger

- **Code-tested revision:** [`V 3.0.534`](https://github.com/artinrahimii/project/commit/a10aebc13e8a5caf7ce27552fd3c970f49c6c820).
- **Product CI:** [run 40868883480](https://github.com/artinrahimii/project/actions/runs/40868883480) passed Android ktlint/build/JVM tests/Protobuf/APK upload, Windows WPF build/security/Protobuf/artifact upload and HostLifecycle tests.
- **Quality CI:** [run 40868883509](https://github.com/artinrahimii/project/actions/runs/40868883509) passed 228 Python tests, syntax and graph checks on V 3.0.534.
- **V 3.0.535 closeout issue:** [run 40869475088](https://github.com/artinrahimii/project/actions/runs/40869475088) passed all 228 Python tests but graph guard found `DONE_WITHOUT_TEST_FILE_EVIDENCE`: the test source path was not directly present in `T0147.evidence`.
- The source files are real: `tests/test_android_request_status_contract.py`, `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestStatusPresentationTest.kt`, and `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt`.
- V 3.0.536 now adds the test source paths to `evidence`. Its exact-head graph/evidence validation is pending.
- The first two implementation attempts and the later graph-evidence correction remain in append-only attempt history.
- T0146 → T0147 and T0138 → T0147 remain present and were validated as sequencing-only with rationale.
- **Not claimed:** request lifecycle transitions, host delivery, response generation or independent-network P2P reachability.
