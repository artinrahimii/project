# T0132 verification evidence

- **Repository / branch:** `artinrahimii/project` / `main`.
- **Exact code and CI baseline:** `V 3.0.456`, commit `13b6b91380e4d29054a03b0190284a4cbe699365`.
- **Secure transport implementation:** `src/Host.Windows/Networking/Libp2pPeerTransport.cs`; maintained library Noise/TLS/QUIC secure-channel stack; expected PeerId pinning and trust store checks.
- **Shared identity policy:** `src/Shared.Security/PeerSecurityPolicy.cs`.
- **Tests:** `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`, `tests/HostLifecycle.Tests/PeerSecurityPolicyAcceptanceTests.cs`, `tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs`; calls are registered in `tests/HostLifecycle.Tests/Program.cs`.
- **Exact CI test result:** [HostLifecycle job 122904254752](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254752) completed successfully; logs include the pass messages for direct libp2p exchange, authenticated TCP-to-QUIC fallback, malformed PeerId rejection, and explicit fail-closed peer trust.
- **Other exact-head CI:** [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40713345604), [Windows WPF](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254631), [Android APK](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254265) all completed successfully.
- **Limit:** local test execution was not performed in this environment. These results are actual completed GitHub Actions runs on the online current commit. This verifies the secure-channel implementation and its local integration acceptance; it does not prove public reachability or independent-network end-to-end inference. `T0128` remains pending.
- **Reviewed edges:** `T0131 → T0132` validated as contract/interface dependency. `T0128 → T0132` reviewed as sequencing-only and removed from active edges, with original edge and evidence preserved in `edge_change_log`.
