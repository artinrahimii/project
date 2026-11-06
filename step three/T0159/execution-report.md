# T0159 execution report

- **Task:** Add capsule acceptance tests for the byte ceiling, corrupt stored data, user edits, deletion, and conflicting local-history suggestions.
- **Base revision:** `b82208a9e613792e3facb52f92d77c7af1facad7` on `main` (`V 3.0.590`).
- **Dependency review:** `T0158 → T0159` is a validated `contract_or_interface_dependency`; the Room repository/DAO persist source, time and changed-field metadata transactionally and cascade-delete history rows with the capsule. `T0150 → T0159` is validated as `sequencing_only`; T0159 consumes capsule-specific codec/repository/ViewModel contracts rather than a T0150-exclusive artifact. Both `blocks` edges remain in the graph.
- **Gaps confirmed before edits:** Existing tests covered a configured limit below the ceiling, edit/delete, provenance, and explicit conflict review, but did not directly assert the exact 4,096-byte hard boundary and next byte, malformed/invalid-UTF-8 bytes already persisted in Room, or that refused unresolved-conflict saves do not append a history event.
- **Implementation:** Added two Room-backed JVM acceptance tests. One calculates a JSON goal that encodes to exactly `MAX_SERIALIZED_BYTES`, then verifies the next byte is rejected without overwriting the accepted capsule or adding provenance. The other injects malformed JSON and invalid UTF-8 through the Room DAO, verifies `InvalidStoredCapsule` with the respective error code, and proves a later valid explicit save repairs the row. Extended ViewModel assertions for USER_EDIT provenance, edits, history cascade cleanup, and conflict rejection leaving history unchanged. Python source-contract coverage now requires both new runtime test names.
- **Local execution:** No local Android/Gradle runtime is available through the connected GitHub tools. Results will be claimed only from exact-head GitHub Actions.
- **Status:** `done`; exact-head V 3.0.592 passed Step Three quality (257 Python tests and graph/evidence validation), Android KtLint, debug APK build, JVM unit tests, generated Protobuf verification and APK upload, Windows WPF build/startup/policy/security checks, and HostLifecycle integration tests. No local Gradle execution is claimed.
- **Next action:** Review the incoming edges to T0160 and implement the request-payload acceptance test without claiming a live network transport that does not exist yet.


## Exact-head V 3.0.591 test outcomes

- Step Three quality gate: PASS, 257 Python tests and graph/evidence validation, [workflow 40954025969](https://github.com/artinrahimii/project/actions/runs/40954025969).
- Android product build: FAIL at Kotlin lint with `Needless blank line(s)` at `ConversationContextCapsulePersistenceTest.kt:249`; APK/JVM/Protobuf were skipped, [workflow 40954025988](https://github.com/artinrahimii/project/actions/runs/40954025988).
- Windows WPF formatting/build/startup/policy/security checks: PASS. HostLifecycle tests: PASS, [workflow 40954025988](https://github.com/artinrahimii/project/actions/runs/40954025988).
- A002 removes the extra empty line before the first new persistence test; it does not alter test behavior. Exact-head V 3.0.592 CI is pending.


## Exact-head V 3.0.592 acceptance results

- Step Three quality gate: PASS, 257 Python tests, syntax checks, graph structure and evidence path validation, [workflow 40954472497](https://github.com/artinrahimii/project/actions/runs/40954472497).
- Android product: PASS. Kotlin lint, debug APK build, `testDebugUnitTest`, generated Protobuf Java/Kotlin verification and artifact upload all succeeded, [workflow 40954472513](https://github.com/artinrahimii/project/actions/runs/40954472513). The JVM test task completed successfully after the added byte-boundary, corrupt-data and ViewModel provenance/conflict tests were committed.
- Windows WPF host: PASS (formatting, Release build, Protobuf, startup smoke, process restrictions, policy rejection, model-capability gating, and DPAPI identity/peer-trust checks). HostLifecycle unit/integration: PASS, [workflow 40954472513](https://github.com/artinrahimii/project/actions/runs/40954472513).
- These results apply to exact-head commit `8983e1b20bf2becb01c70618bb4f49c69d503e79` (`V 3.0.592`). No local Android/Gradle execution is claimed.
