# T0176 — deterministic multi-response acceptance tests

- **Status:** `done` after exact-head CI acceptance.
- **Final acceptance revision:** `V 3.0.808`, commit `062794153557c686538150b2f4af0abfccba9778`, branch `main`.
- **Task attempt:** `T0176-A002-v3.0.808-exact-head-ci-pending` (`succeeded`).
- **Earlier attempt:** `T0176-A001-v3.0.807-exact-head-ci-pending` (`failed`); its concrete failures and test results remain in the append-only graph history.
- **Dependency review:** `T0175 → T0176` and `T0160 → T0176` were verified against production source, protocol and test evidence. Both are `contract_or_interface_dependency`, validated; their original edge type `blocks` was retained.
- **Implemented test coverage:** zero, one, two and three responses; per-attempt timeouts when responses are absent; preserving responses after one sibling times out; dispatch-order collection; duplicate response suppression without replacing the first card; response completion and arrival after the request deadline; cancellation terminal behavior and disabled retry.
- **Determinism:** replaced the 20 ms wall-clock timeout race in the collector unit test with an explicit `TimeoutException`. The production round window continues to use the existing clock/deadline implementation.
- **Task code changes:** Android JVM test files and Python source-contract checks. No production runtime behavior was changed by this task.
- **A001 correction:** V 3.0.807 had 324 Python tests and compileall passing, but its quality gate failed the repository-backed evidence check because six old T0175 Actions URLs were placed in a path-only field; Android KtLint also reported formatting issues. The URL records were preserved in historical test details, the file-path list was corrected, and Kotlin formatting/assertions were fixed in A002.
- **Successful exact-head CI:** [Android / Windows product build](https://github.com/artinrahimii/project/actions/runs/41431021173) passed all jobs, including Android KtLint, `assembleDebug`, `testDebugUnitTest`, generated Protobuf checks, Windows WPF build/startup/security and HostLifecycle. The [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41431021155) passed 324 Python tests, compileall, graph validation and repository-backed evidence validation. The [Rendezvous container build/smoke test](https://github.com/artinrahimii/project/actions/runs/41431021244) passed.
- **Artifacts:** Android JVM report artifact `13133179900` (49,921 bytes); Android debug APK `13134591087` (19,524,950 bytes); Windows host artifact `13133473445` (14,266,183 bytes). They are accessible from the linked GitHub Actions run.
- **Next task:** `T0177`, after reviewing both incoming dependency edges `T0176 → T0177` and `T0160 → T0177`.
