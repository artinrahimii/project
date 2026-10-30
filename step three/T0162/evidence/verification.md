# T0162 verification evidence — exact V 3.0.612

- **Repository / branch:** `artinrahimii/project` / `main`.
- **Acceptance commit:** `e52b563e310d0e7115b0f612ee77ef819985462b` (`V 3.0.612`).
- **Graph path:** `step three/project-graph.md`.
- **Graph Blob:** `8566966ac26b6cb9c7dbbbf6eb90657f9bc84c7f`; exact-tree size 2,979,601 UTF-8 bytes; fetched complete content measured 2,979,601 UTF-8 bytes; recomputed Git Blob SHA-1 matched; full fenced JSON parsed successfully (229 nodes, 417 edges, 229 unique IDs, zero dangling edges).
- **Incoming dependency review:** T0160 → T0162 = `contract_or_interface_dependency`, validated; T0161 → T0162 = `sequencing_only`, validated. Original `blocks` edge types and execution order are preserved.
- **Production output:** `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`.
- **JVM behavior tests:** `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt` (eight named cases).
- **Repository-level source contract:** `tests/test_android_host_candidate_selector_contract.py`.
- **Step Three quality gate — PASS:** https://github.com/artinrahimii/project/actions/runs/41040959242 (Python tests, syntax, graph and output/evidence validation).
- **Android — PASS:** https://github.com/artinrahimii/project/actions/runs/41040959393 (KtLint, debug APK, JVM tests, generated Protobuf Java/Kotlin and artifact upload).
- **Windows WPF — PASS:** https://github.com/artinrahimii/project/actions/runs/41040959393 (formatting, Release build, Protobuf, startup, agent-tool process/policy/capability, DPAPI identity/trust and artifact upload).
- **HostLifecycle — PASS:** https://github.com/artinrahimii/project/actions/runs/41040959393.
- **Attempt history:** A001 and A002 failed at KtLint; A003 failed at a stale Python assertion and missing Kotlin test import; A004 corrected the harness and all exact-head checks passed.
- **Accepted scope:** deterministic filtering and selecting at most three distinct READY peers compatible with a supplied request and supplied host snapshots.
- **Not claimed:** remote discovery/status freshness, reachability, transport connection, inference dispatch, retry/deadline orchestration or full Android-to-Windows request/response.
