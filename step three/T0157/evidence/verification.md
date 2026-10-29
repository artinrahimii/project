# T0157 verification evidence

- Base commit: `b4d4d5d36e2a3e074ec68f8fe177430e7eec5903` (`V 3.0.575`), branch `main`.
- Graph source path: `step three/project-graph.md`, blob `9ba603acbd0e37b2fdcbfc201af4ebc1a4fe1b61`; the graph was read from the exact base revision (229 nodes / 417 edges).
- Edge evidence:
  - `T0156 → T0157` validated as `contract_or_interface_dependency` against `ConversationContextCapsuleViewModel.kt`, `ConversationContextCapsuleSummarizer.kt`, and the existing T0156 acceptance test/report.
  - `T0150 → T0157` validated as `sequencing_only` against `ConversationWorkflowAcceptanceTest.kt` and T0150's accepted local Room history workflow. Both original `blocks` edges remain.
  - `T0157 → T0158` intentionally remains unreviewed.
- Implementation files: `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModel.kt`, `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`, `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModelTest.kt`, and `tests/test_android_context_capsule_conflict_review_contract.py`.
- Acceptance coverage added: local test keeps the saved goal and existing key fact unchanged until review; each conflict requires an explicit `KEEP_CURRENT` or `USE_SUGGESTION` choice before Save.
- Heuristic boundary: checks explicit negation and a bounded lexicon of opposite terms; changing a nonblank goal always prompts. It does not claim to identify every natural-language contradiction.
- Tests were not executed locally through the connected tool. Exact-head CI must establish outcomes before acceptance.


## V 3.0.576 failed acceptance and corrective A003

- Exact head: `b7b492a902a989dfeaeb10547af996ed7085bd0a` (`V 3.0.576`).
- Quality run [40920502076](https://github.com/artinrahimii/project/actions/runs/40920502076): 253 tests ran; one failure because the source-contract test looked for `KEEP_CURRENT` in the ViewModel instead of the Compose UI.
- Product run [40920502283](https://github.com/artinrahimii/project/actions/runs/40920502283): Android ktlint failed on formatting in the new conflict helper/resolver and two chained test expressions; APK/JVM tests did not execute. Windows host and HostLifecycle jobs passed.
- The failure is retained as `T0157-A002`. A003 corrects the assertion location and formatting. No successful task acceptance is claimed until exact-head CI passes.


## V 3.0.577 failed acceptance and corrective A004

- Quality [run 40921532799](https://github.com/artinrahimii/project/actions/runs/40921532799): pass.
- Product [run 40921532777](https://github.com/artinrahimii/project/actions/runs/40921532777): Android ktlint failed one remaining format finding at `ConversationContextCapsuleViewModel.kt:464:21`; APK/JVM were skipped. HostLifecycle passed; the Windows job was still pending at the time of recording.
- A004 joins the closing brace and `.toMutableList()`. The task remains in progress pending exact-head build and test evidence.


## Exact-head V 3.0.578 final verification — passed

- Online `main` tested implementation SHA: `6379a8510316be3c2cfd9faadf14898a604355db`; commit message: `V 3.0.578`.
- Graph source at that exact commit: `step three/project-graph.md`, tree Blob `b65cdbdf1dfcae918640e4032da5795e4c0442b7`, declared size 2,809,857 bytes. Git Blob API returned 2,809,857 UTF-8 bytes (2,687,553 Unicode characters); recomputed Git Blob SHA-1 matched. The original fenced JSON parsed completely as 229 nodes / 417 edges. Graph formatting fences were retained for hashing; only a separate in-memory copy was parsed.
- Incoming dependency evidence remains unchanged: `T0156 → T0157` is validated `contract_or_interface_dependency`; `T0150 → T0157` is validated `sequencing_only`. Both `blocks` edges are preserved. The outgoing `T0157 → T0158` edge remains `unreviewed`.
- Step Three automated test, Python syntax, graph integrity and recorded-output/evidence checks: PASS ([workflow](https://github.com/artinrahimii/project/actions/runs/40922097585), [job](https://github.com/artinrahimii/project/actions/runs/40922097585/job/123617769867)).
- Android Kotlin lint, debug APK build, JVM unit tests, generated Protobuf checks and APK upload: PASS ([workflow](https://github.com/artinrahimii/project/actions/runs/40922097593), [job](https://github.com/artinrahimii/project/actions/runs/40922097593/job/123617770490)).
- Windows WPF formatting/build/Protobuf/startup-smoke/artifact upload: PASS ([job](https://github.com/artinrahimii/project/actions/runs/40922097593/job/123617770138)). HostLifecycle unit/integration tests: PASS ([job](https://github.com/artinrahimii/project/actions/runs/40922097593/job/123617770564)).
- No local Android/Gradle/Python test run is claimed; acceptance is based on the exact-head GitHub Actions outcomes above.
