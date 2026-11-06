# T0007 verification evidence — A083 baseline audit

- **Repository/branch:** `artinrahimii/project`, `main`.
- **Exact baseline commit:** `2927754d34c547b90dbc9a974b63febb8a7cae09` — `V 3.0.445`.
- **Graph source:** `step three/project-graph.md`, Blob `ce2fcdc126903e7458755d6485cc0e3a04a54c8d`. Blob content was fetched in full, its Git blob SHA was recomputed over the original UTF-8 bytes and matched, and the fenced JSON parsed successfully. Length: 2,045,208 Unicode characters; 2,167,061 UTF-8 bytes. It contains 229 task nodes and 424 edges.
- **Baseline quality:** [GitHub Actions run 40648190821](https://github.com/artinrahimii/project/actions/runs/40648190821) passed all 190 Python tests, syntax checks, graph structure and recorded-output/evidence validation; graph guard reported zero errors and one preserved T0001 unmapped-dependency warning.
- **Baseline product validation:** [run 40648190753](https://github.com/artinrahimii/project/actions/runs/40648190753) passed HostLifecycle integration, Windows WPF Release build/startup and DPAPI peer-trust checks, and Android APK/lint/Protobuf checks.
- **Decision-register audit:** 19 registered ADR rows/documents (two separate ADR-0003 decisions); current tests verify all ADR documents are registered, linked source/configuration/test/workflow paths exist, evidence boundaries are stated, and decision-specific source guards cover ADR-0001 through ADR-0018.
- **Mismatch identified:** graph metadata for T0007 had not yet listed ADR-0016, ADR-0017 and ADR-0018 under both `actual_output_paths` and `evidence`, despite their traceability regression tests and CI records already existing.
- **New guard:** `tests/test_decision_traceability.py::test_t0007_graph_tracks_current_adr_outputs_and_verification` asserts that those three ADR documents plus this evidence file are present in T0007's current output/evidence arrays and actually exist. This new test is **pending exact PR-head CI** at the time of this evidence draft; no pass is claimed until the workflow result is inspected.
- **Scope boundary:** T0007 is still `in_progress` pending the new guard's result. This verification is about decision/source/configuration/test traceability; it does not prove independent-node Android-to-Windows P2P. `T0121` remains blocked and `AC-P2P-01` remains **NOT RUN**.


## A084 — verification and completion boundary (2026-10-28)

- **Exact PR candidate checked:** `b19d533fbd190ac930359cf56992b553245911e6`.
- **Step Three quality:** PASS, [run 40650078853](https://github.com/artinrahimii/project/actions/runs/40650078853). All **191 Python tests** passed, including the new T0007 graph-output/evidence consistency regression. Python syntax, 229-node / 424-edge graph validation, and output/evidence checks passed with zero errors and one preserved T0001 unmapped-dependency warning.
- **Product baseline:** exact current-main V 3.0.445 product workflow [40648190753](https://github.com/artinrahimii/project/actions/runs/40648190753) passed HostLifecycle, Windows WPF/DPAPI, and Android APK/lint/Protobuf. The T0007 increment changes decision-traceability tests and metadata only, not runtime product code.
- **Conclusion:** T0007 is marked done only for decision/source/configuration/test traceability. No live Android-to-Windows P2P or external peer-announcement success is claimed; T0121 remains blocked and AC-P2P-01 remains NOT RUN. The updated PR head's full CI must also pass before merge.
