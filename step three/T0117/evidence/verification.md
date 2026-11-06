# T0117 verification evidence — accepted

## Graph/source integrity
- Repository: `artinrahimii/project`, branch `main`.
- Baseline for the task: `d4b2c7ee8e28729b4d22179810614c516aee7d0f` (`V 3.0.350`).
- Baseline graph Blob `d006eb634df7008f2ffd95b447e636309cf44c4e`, 1,805,090 bytes; exact Git object SHA-1 matched and the JSON parsed to 229 tasks / 426 edges.

## Library/implementation
- `Nethermind.Libp2p` `1.0.3`, MIT, .NET 10: [NuGet](https://www.nuget.org/packages/Nethermind.Libp2p), [source](https://github.com/NethermindEth/dotnet-libp2p).
- `Libp2pPeerTransport` implements `IPeerTransport`, verifies target address PeerId against `PeerTransportEndpoint` and then verifies the authenticated session PeerId.
- `Libp2pPeerMessageProtocol` provides bounded length-prefixed binary messages with backpressure and I/O errors on incomplete/disconnected streams.
- The library default TCP stack negotiates Noise or TLS; the code does not enable the plaintext enforcement option.
- Two-peer runtime tests exercise mismatched-identity rejection, bidirectional binary transfer, size limits and disconnect state.

## Exact-SHA verification on V 3.0.354
- HostLifecycle/P2P integration: **PASS** — https://github.com/artinrahimii/project/actions/runs/40445103348/job/121994383982
- Windows WPF product build and policy checks: **PASS** — https://github.com/artinrahimii/project/actions/runs/40445103348/job/121994383755
- Android client build/Protobuf tests: **PASS** — https://github.com/artinrahimii/project/actions/runs/40445103348/job/121994384052
- Step Three quality gate: **PASS** — https://github.com/artinrahimii/project/actions/runs/40445103235

## Failure and retry record
- A001 / V 3.0.351: compile-time API mismatches; failed.
- A002 / V 3.0.352: expected libp2p protocol-task cancellation escaped during disconnect; failed.
- A003 / V 3.0.353: `IsConnected` stayed true briefly after disconnect; failed.
- A004 / V 3.0.354: all runtime and product/quality acceptance passed; succeeded.
All three failures remain in the graph's execution history; they have not been removed from the record.

## Reviewed next dependencies
- `T0117 → T0118`: `contract_or_interface_dependency`; fallback connection logic directly consumes the secure peer transport / expected PeerId contract.
- `T0115 → T0118`: `sequencing_only`; the older TCP listener tests are not a direct API input to the libp2p fallback path.
Both original `blocks` edge types remain.
