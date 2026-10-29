# T0147 evidence ledger

- Implementation: [`V 3.0.532`](https://github.com/artinrahimii/project/commit/90f542e2500e1af5a99d8f0f11336d3a29f05a1a); source-contract correction: [`V 3.0.533`](https://github.com/artinrahimii/project/commit/95e1af212d790aba0e9f920b30090b53d402323f).
- V 3.0.532 quality [40868131396](https://github.com/artinrahimii/project/actions/runs/40868131396) failed seven Python subtests due to an incorrect assumption about Kotlin enum syntax; its Android job failed ktlint on the Room query chain.
- V 3.0.533 quality [40868507192](https://github.com/artinrahimii/project/actions/runs/40868507192) passed 228 Python tests, syntax and graph guards with zero errors and one preserved T0001 metadata warning.
- V 3.0.533 Android job in [40868507175](https://github.com/artinrahimii/project/actions/runs/40868507175) failed ktlint with four line-chain findings at ClientDatabaseTest.kt:124; APK and Android JVM tests did not run. HostLifecycle passed; Windows was still in progress when checked.
- V 3.0.534 reformats the Room query chain; its exact-head CI is pending.
- Both incoming edges are validated sequencing-only, with rationale/evidence captured in the graph. No `type=blocks` edge was deleted.
- **Not claimed:** live request transitions, host delivery, response generation or independent-network P2P connectivity.
