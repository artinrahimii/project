# T0133 verification evidence

- **Repo:** `artinrahimii/project` / `main`.
- **Reviewed current head:** `V 3.0.457` (`f7a708a47d07e2b5790d7e720874cf6cc7bfb8d1`). Product-code acceptance was completed on exact commit `V 3.0.456` (`13b6b91380e4d29054a03b0190284a4cbe699365`); V 3.0.457 did not change the relevant source/test files.
- **Session key and secure channel:** `src/Host.Windows/Networking/Libp2pPeerTransport.cs`; session-key establishment is delegated to the maintained `Nethermind.Libp2p` stack (ADR-0016), not custom cryptography.
- **Persistent identity/trust:** `src/Shared.Security/HostIdentityStore.cs`, `src/Shared.Security/PeerTrustStore.cs`, `src/Host.Windows/Security/WindowsHostIdentityProvider.cs`, `src/Host.Windows/Security/WindowsDpapiSecretProtector.cs`.
- **Executable test sources:** `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`, `HostIdentityStoreAcceptanceTests.cs`, `WindowsHostIdentityStoreAcceptanceTests.cs`, `PeerTrustStoreAcceptanceTests.cs`, and `WindowsPeerTrustStoreAcceptanceTests.cs`.
- **Exact-head CI evidence (V 3.0.456):**
  - [HostLifecycle integration](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254752) passed secure exchange/fallback, trusted-peer gating, identity parsing and persistence tests.
  - [Windows WPF identity/DPAPI checks](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254631) passed private-key and peer-trust protection at rest and tamper rejection.
  - [Android APK](https://github.com/artinrahimii/project/actions/runs/40713345711/job/122904254265) and [Step Three quality](https://github.com/artinrahimii/project/actions/runs/40713345604) completed successfully.
- **Boundary:** this verifies managed secure-session setup and peer-trust/identity handling in local acceptance tests; it does not prove public reachability or complete Android-to-Windows independent-network inference. `T0128` remains pending.
- **Reviewed edges:** `T0132 → T0133` validated as contract/interface dependency. `T0128 → T0133` reviewed as sequencing-only and removed from active edges with the prior rationale/evidence kept in `edge_change_log`.
