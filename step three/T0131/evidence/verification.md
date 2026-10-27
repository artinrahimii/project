# T0131 verification evidence — attempt A001

- **Baseline:** 9262a9b6fa6bdb68195d10face77bea1f5b422cb (V 3.0.438), branch main.
- **Graph source:** step three/project-graph.md, Blob 46d4427b8294fab6d4dd2ae028e928dc17d61cb7; complete original bytes reproduced the Git Blob ID and fenced JSON parsed as 229 nodes / 426 edges.
- **Implementation:** src/Shared.Security/PeerTrustStore.cs; mandatory trust store in src/Host.Windows/Networking/Libp2pPeerTransport.cs; portable trust tests; direct transport trust-gate tests; Windows DPAPI tests; product workflow registration; ADR-0018.
- **Safety properties:** new peers are Unknown by default; explicit approval is required to store a canonical PeerId; corrupt/unsupported/unprotectable data fails closed; a mismatched /p2p address identity is rejected; unknown identities are rejected before dial/listen; trust decisions use the OS-backed secret-protector interface.
- **Test status:** exact-commit CI pending. Do not mark the new acceptance tests passed before inspecting the workflow logs for the resulting version.
- **Dependency correction:** T0128 → T0131 retained as sequencing_only; T0131 → T0128 added as validated contract/interface dependency because the final multi-node test consumes the trust store. Execution order places T0131 immediately after T0130.
- **Boundary:** no WPF trust prompt or real independent-node E2E is claimed. AC-P2P-01 remains NOT RUN.


## Attempt A001 exact CI failure and A002 repair

- **V 3.0.439 quality:** failed with `DEPENDENCY_CYCLE` in [run 40582042880](https://github.com/artinrahimii/project/actions/runs/40582042880), caused by the three active sequencing-only edges T0128 → T0129/T0130/T0131 alongside the actual contract chain T0129 → T0130 → T0131 → T0128.
- **V 3.0.439 product:** HostLifecycle and Windows build failed with `CS9135` at `PeerTrustStore.cs:222` in [run 40582042752](https://github.com/artinrahimii/project/actions/runs/40582042752). The peer-trust tests did not execute.
- **A002:** fixed the bounds check with ordinary comparisons and removed the three obsolete sequencing edges from active adjacency. Their review/evidence is retained in the append-only log. The true T0131 → T0128 interface prerequisite remains.
- **Verification:** exact V 3.0.440 quality/product CI pending. No pass is claimed yet.
