# T0176 — deterministic multi-response acceptance tests

- **Base revision:** `201cf977f09c6c0f01ec0ea284eed3d25b92ab39` on `main` (`V 3.0.807`).
- **Current attempt:** `T0176-A002-v3.0.808-exact-head-ci-pending`.
- **Status:** in progress; exact-head `V 3.0.808` CI is pending.
- **Previous attempt retained:** `T0176-A001-v3.0.807-exact-head-ci-pending` failed two independent acceptance gates. The Step Three workflow ran 324 Python tests and compileall successfully, and graph structure validation passed (230 nodes / 422 edges / zero errors / one existing T0001 metadata warning), but its repository-backed evidence stage rejected six GitHub Actions URLs in the pre-existing `T0175.evidence` file-path list. Android product CI failed KtLint on three newly added long list literals and one multiline assertion; Android APK/JVM steps were skipped. Windows WPF and HostLifecycle passed. Full evidence is recorded in the graph attempt history.
- **Corrections in A002:** formatted all three Kotlin three-host attempt lists; replaced a tautological assertion with exact counts of successful and failed response cards; moved the six T0175 Actions URLs out of the path-only `evidence` array while preserving them in execution summary/history test URL fields and the report.
- **Dependency review:** both incoming `blocks` edges remain validated as `contract_or_interface_dependency`; neither edge's `blocks` type was changed.
- **Scope:** deterministic tests and graph evidence metadata only. No production transport behavior is claimed as newly implemented.
- **Files changed:** `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundResponseCollectorTest.kt`, `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`, `tests/test_android_inference_round_response_collector_contract.py`, `tests/test_android_inference_response_correlation_contract.py`, `step three/T0176/execution-report.md`, `step three/T0176/evidence/verification.md`, `step three/project-graph.md`.
- **Next action:** inspect exact-head `V 3.0.808` Android KtLint/JVM and Step Three quality/evidence runs. Keep the task in progress until every required gate passes.
