# T0131 verification evidence — attempt A001

- **Baseline:** 9262a9b6fa6bdb68195d10face77bea1f5b422cb (V 3.0.438), branch main.
- **Graph source:** step three/project-graph.md, Blob 46d4427b8294fab6d4dd2ae028e928dc17d61cb7; complete original bytes reproduced the Git Blob ID and fenced JSON parsed as 229 nodes / 426 edges.
- **Implementation:** src/Shared.Security/PeerTrustStore.cs; mandatory trust store in src/Host.Windows/Networking/Libp2pPeerTransport.cs; portable trust tests; direct transport trust-gate tests; Windows DPAPI tests; product workflow registration; ADR-0018.
- **Safety properties:** new peers are Unknown by default; explicit approval is required to store a canonical PeerId; corrupt/unsupported/unprotectable data fails closed; a mismatched /p2p address identity is rejected; unknown identities are rejected before dial/listen; trust decisions use the OS-backed secret-protector interface.
- **Test status:** exact-commit CI pending. Do not mark the new acceptance tests passed before inspecting the workflow logs for the resulting version.
- **Dependency correction:** T0128 → T0131 retained as sequencing_only; T0131 → T0128 added as validated contract/interface dependency because the final multi-node test consumes the trust store. Execution order places T0131 immediately after T0130.
- **Boundary:** no WPF trust prompt or real independent-node E2E is claimed. AC-P2P-01 remains NOT RUN.
