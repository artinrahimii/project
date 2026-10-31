# T0150 evidence ledger

- **Accepted implementation:** [`V 3.0.546`](https://github.com/artinrahimii/project/commit/c3030cd33b9d0d8f78e22c3cb31bdb75e420b48b).
- **Workflow integration test:** `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationWorkflowAcceptanceTest.kt` uses production ViewModels and DAO APIs to create a conversation, send two messages, persist a request state, close/reopen a file-backed Room database and verify restored history/status.
- **Python source-contract coverage:** `tests/test_android_conversation_detail_contract.py` covers Compose list/detail wiring and the integration test's local-only boundary.
- **Quality:** [run 40877312783](https://github.com/artinrahimii/project/actions/runs/40877312783) passed 231 Python tests, syntax, graph structure and output/evidence checks (229 tasks/417 edges; zero errors; one preserved T0001 metadata warning).
- **Android product:** [run 40877312857](https://github.com/artinrahimii/project/actions/runs/40877312857) passed Kotlin lint, debug APK build, JVM suite, Protobuf generated-source checks and APK archive upload (11,875,219 bytes).
- **Windows/host product:** [run 40877312857](https://github.com/artinrahimii/project/actions/runs/40877312857) passed Windows WPF build/startup/security/Protobuf checks and artifact upload (14,254,980 bytes), plus HostLifecycle tests.
- **Dependency review:** T0149 → T0150 = `contract_or_interface_dependency`; T0138 → T0150 = `sequencing_only`. Original `blocks` edges retained and rationale/evidence recorded in graph history.
- **Boundary:** local Room + ViewModel flow only; no P2P delivery or at-rest encryption claim.
