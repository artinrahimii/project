# T0149 evidence ledger

- **File-backed test:** `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt` adds a named Room database persistence/reopen regression covering conversation metadata, ordered user/assistant messages, WAITING/FAILED statuses, attempts, and error code.
- **Quality CI:** [V 3.0.542 run 40874779599](https://github.com/artinrahimii/project/actions/runs/40874779599) passed 230 Python tests, syntax, graph and output/evidence checks (229 tasks / 417 edges; zero errors, one preserved warning).
- **First quality failure:** [V 3.0.541 run 40874466336](https://github.com/artinrahimii/project/actions/runs/40874466336) failed only a source assertion that assumed a contiguous Kotlin call string.
- **First product failure:** [V 3.0.542 run 40874779588](https://github.com/artinrahimii/project/actions/runs/40874779588) failed ktlint for expected newline-before-dot at two chains in `ClientDatabaseTest.kt`; APK and JVM test tasks did not run.
- **Correction:** V 3.0.543 changes chain formatting only; exact-head CI still pending.
- **Dependency review:** T0148 → T0149 and T0138 → T0149 were validated as sequencing-only with their original `blocks` edges preserved.
- **Not yet proven:** Kotlin lint/build/JVM test, including the file-backed close/reopen test, on the corrected revision.
