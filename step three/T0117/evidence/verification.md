# T0117 verification evidence — awaiting exact-head CI

## Baseline integrity
- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `d4b2c7ee8e28729b4d22179810614c516aee7d0f`; base tree: `350e508da8513337c84c3cf8c2df02e123fa41ae`.
- Graph path: `step three/project-graph.md`, Blob `d006eb634df7008f2ffd95b447e636309cf44c4e`, 1,805,090 bytes. The full Blob was retrieved through Git Blob API after normal file fetch was known to return empty content. The Git SHA-1 of the entire original content matched `d006eb634df7008f2ffd95b447e636309cf44c4e`; JSON parsed to 229 tasks / 426 edges.

## Library review
- Package: [Nethermind.Libp2p 1.0.3](https://www.nuget.org/packages/Nethermind.Libp2p), license MIT and target .NET 10.
- Source: https://github.com/NethermindEth/dotnet-libp2p.
- Default stack uses TCP and negotiates Noise or TLS; the code does not call `WithPlaintextEnforced`.
- The source project's own roadmap marks hole punching and circuit relay as work in progress. They are explicitly not treated as available features.

## Implementation and test surface
- `src/Host.Windows/Networking/Libp2pPeerTransport.cs`: concrete `IPeerTransport`; validates target multiaddress PeerId and checks the established session's authenticated PeerId; opt-in listening requires a specific bind address and expected remote identity.
- `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs`: message-level length-prefixed binary transfer, maximum length bound, inbound queue capacity/backpressure, safe error on incomplete frame and no payload logging.
- Host and HostLifecycle test project reference `Nethermind.Libp2p` `1.0.3`.
- `PeerTransportContractAcceptanceTests` now exercises a real pair of local libp2p peers and checks identity mismatch rejection, two-way binary round-trip, over-limit message rejection and idempotent disconnect.

## Reviewed dependencies
- `T0116 → T0117`: `contract_or_interface_dependency`, validated; implementation conforms to the secure connect/send/receive/disconnect interface and expected-peer endpoint.
- `T0115 → T0117`: `sequencing_only`, validated; existing TCP listener tests are not a direct API/data input to the new libp2p adapter. The original `blocks` order is retained.

## Verification status
- V 3.0.351: HostLifecycle compile **FAILED** before runtime acceptance with CS0104 (ambiguous `Channel`), CS1061 (`ISession.RemotePeerId` absent in the published API), and CS0411 (unsupported instance `DialAsync` overload). These errors were observed in workflow job logs, then corrected in attempt A002.
- Exact-head HostLifecycle/libp2p exchange after correction: pending.
- Exact-head Windows WPF Release build and package restore: pending.
- Exact-head Step Three quality/graph/output/evidence gate: pending.
- No end-to-end Android↔Windows claim is made in this task.
