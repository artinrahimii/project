# T0177 — per-round host and per-request attempt budget acceptance

<badge>done</badge>

- **Accepted status:** `done`; exact-head product and quality CI passed.
- **Implementation/test revision:** `main`, `8345a037602b9cdf6f562f98cde18f6be8954da6`, message `V 3.0.811`.
- **Final ledger revision:** this report and graph status are recorded in `V 3.0.812`.
- **Attempt A001 — failed:** V 3.0.810's Android `ktlintTestSourceSetCheck` identified one needless blank line at `InferenceAttemptDispatcherTest.kt:510`. APK and Android JVM tests were skipped; the failure is preserved in the graph history.
- **Attempt A002 — succeeded:** removed the redundant blank line and reran the exact-head workflows. No production runtime source changed in this testing task.
- **Implemented Android JVM coverage:** drives the real HostCandidateSelector and InferenceAttemptDispatcher through three rounds with twelve available hosts; verifies at most three different hosts per round, no ordinary reuse across rounds, and the nine-attempt request maximum. A second scenario sets a four-attempt total budget and explicitly retries a previously attempted host, proving that the retry consumes the final slot and the dispatcher rejects any fifth send.
- **Python contract coverage:** verifies the centralized 3-round / 3-host / 9-attempt limits and that selector/dispatcher guard paths remain present alongside the Android JVM acceptance scenarios.
- **Dependency validation:** `T0160 → T0177` is `contract_or_interface_dependency`; `T0176 → T0177` is `sequencing_only`. Both original `blocks` edges remain intact, with reasons and evidence in `edge_change_log`. The conservative execution order was not changed.
- **Exact-head V 3.0.811 results:** Android KtLint, `assembleDebug`, `testDebugUnitTest` and generated Java/Kotlin Protobuf checks passed; Windows WPF build/startup/security/DPAPI checks passed; HostLifecycle passed; 328 Python tests, `compileall`, graph/evidence validation and the Rendezvous container build/non-root smoke passed.
- **Artifacts:** Android unit-test reports artifact `13137490934` (49,961 bytes); Android debug APK artifact `13137520910` (19,524,952 bytes).
- **Next task:** `T0178`, following the graph's existing order. Its incoming dependencies must be reviewed before its implementation begins.
