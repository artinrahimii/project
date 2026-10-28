# T0131 verification evidence — append-only record

- **Baseline:** 9262a9b6fa6bdb68195d10face77bea1f5b422cb (V 3.0.438), branch main.
- **Graph source:** step three/project-graph.md, Blob 46d4427b8294fab6d4dd2ae028e928dc17d61cb7; complete original bytes reproduced the Git Blob ID and fenced JSON parsed as 229 nodes / 426 edges.
- **Implementation:** src/Shared.Security/PeerTrustStore.cs; mandatory trust store in src/Host.Windows/Networking/Libp2pPeerTransport.cs; portable trust tests; direct transport trust-gate tests; Windows DPAPI tests; product workflow registration; ADR-0018.
- **Safety properties:** new peers are Unknown by default; explicit approval is required to store a canonical PeerId; corrupt/unsupported/unprotectable data fails closed; a mismatched /p2p address identity is rejected; unknown identities are rejected before dial/listen; trust decisions use the OS-backed secret-protector interface.
- **Test status:** exact-commit CI pending. Do not mark the new acceptance tests passed before inspecting the workflow logs for the resulting version.
- **Current dependency state:** T0130 → T0131 is a validated contract/interface dependency and T0131 → T0128 remains the validated contract/interface dependency. The earlier T0128 → T0131 sequencing-only edge was removed from active dependencies in A002 to prevent a cycle; history is preserved in the graph's edge_change_log.
- **Boundary:** no WPF trust prompt or real independent-node E2E is claimed. AC-P2P-01 remains NOT RUN.


## Attempt A001 exact CI failure and A002 repair

- **V 3.0.439 quality:** failed with `DEPENDENCY_CYCLE` in [run 40582042880](https://github.com/artinrahimii/project/actions/runs/40582042880), caused by the three active sequencing-only edges T0128 → T0129/T0130/T0131 alongside the actual contract chain T0129 → T0130 → T0131 → T0128.
- **V 3.0.439 product:** HostLifecycle and Windows build failed with `CS9135` at `PeerTrustStore.cs:222` in [run 40582042752](https://github.com/artinrahimii/project/actions/runs/40582042752). The peer-trust tests did not execute.
- **A002:** fixed the bounds check with ordinary comparisons and removed the three obsolete sequencing edges from active adjacency. Their review/evidence is retained in the append-only log. The true T0131 → T0128 interface prerequisite remains.
- **Verification:** exact V 3.0.440 quality/product CI pending. No pass is claimed yet.


## Attempt A003 — exact V 3.0.443 verification (2026-10-28)

- **Verified source commit:** `db6b7f1bf87747bf4d10f4f8e1f227df4264cde3` (`V 3.0.443`).
- **Quality gate:** PASS, [run 40638504799](https://github.com/artinrahimii/project/actions/runs/40638504799): 190 Python tests; syntax, 229-node / 424-edge graph validation, and recorded-output/evidence checks passed. The pre-existing T0001 unmapped-dependency warning remains preserved.
- **HostLifecycle:** PASS, [job 122650936156](https://github.com/artinrahimii/project/actions/runs/40638504787/job/122650936156). The log confirms explicit persisted trust, canonical PeerId binding, fail-closed unknown/corrupt identities and direct libp2p transport acceptance.
- **Windows WPF and DPAPI:** PASS, [job 122650936490](https://github.com/artinrahimii/project/actions/runs/40638504787/job/122650936490), including protected trust persistence across store instances and tamper rejection.
- **Android:** PASS, [job 122650936473](https://github.com/artinrahimii/project/actions/runs/40638504787/job/122650936473), including APK, lint and Protobuf checks.
- **Task boundary:** the reusable trust store and transport identity gate meet T0131's acceptance. No WPF enrollment prompt or independent-node E2E is claimed; AC-P2P-01 remains NOT RUN. The V 3.0.439 failures and A002 repair record above are retained.
