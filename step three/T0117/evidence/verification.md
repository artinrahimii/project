# T0117 verification evidence — exact-SHA CI pending

## Source integrity
- Repository `artinrahimii/project`, branch `main`.
- Original implementation commit: `V 3.0.351`, SHA `d056c1c1c77903165576078b0d7c3fbe35b60676`.
- Graph at that commit: Blob `d006eb634df7008f2ffd95b447e636309cf44c4e`, 1,805,090 bytes; Git SHA-1 matched the original content; parsed 229 tasks / 426 edges.

## Implementation
- Package: [Nethermind.Libp2p 1.0.3](https://www.nuget.org/packages/Nethermind.Libp2p), MIT, .NET 10.
- Uses libp2p's default TCP stack with Noise/TLS negotiation; no forced-plaintext mode enabled.
- `Libp2pPeerTransport` validates the target multiaddress PeerId, matches it against the expected identity and then verifies the peer identity obtained from the session address. Listening is explicit and requires an expected peer identity.
- `Libp2pPeerMessageProtocol` sends and receives bounded varint-prefixed binary messages and uses bounded channel backpressure.

## Exact commit outcomes
- **V 3.0.351:** compile failed; fixed ambiguous channel type, missing published `ISession.RemotePeerId`, and protocol instance dialing API mismatch.
- **V 3.0.352:** compiled and the real loopback integration reached the first disconnect call after preceding assertions executed: authenticated peer identities, client-to-server binary round-trip, server-to-client binary round-trip, and oversized message rejection. Failed because the library's long-running `DialAsync<TProtocol>` task ends with `TaskCanceledException` when the session is deliberately disconnected, which the adapter did not yet treat as expected teardown.
- **V 3.0.353:** adds a narrowly filtered handling path for that expected cancellation, conditioned on the caller's cancellation token not being cancelled. Exact-head tests pending.
- Step Three quality gate on V 3.0.352 passed: https://github.com/artinrahimii/project/actions/runs/40444570058

## Dependency review
- `T0116 → T0117`: `contract_or_interface_dependency`, validated.
- `T0115 → T0117`: `sequencing_only`, validated; TCP-listener tests are not consumed by the libp2p adapter. Both `blocks` edges are retained.

## Remaining boundaries
No claim of hole punching, circuit relay, Android compatibility, app-level Protobuf inference request/response transport, peer discovery/rendezvous, or AC-P2P-01 is made. The library roadmap itself marks hole punching and circuit relay as work in progress.
