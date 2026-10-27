# T0118 verification evidence — accepted on exact V 3.0.357

## Source integrity
- Repository: `artinrahimii/project`, branch `main`.
- Tested code commit: `2ed9620edf1b1035ba17ae5b5f4d4e3414717cdb` (`V 3.0.357`).
- The graph at the prior commit was retrieved by Git Blob API (ordinary file fetch returned empty content); complete content matched its Git Blob SHA-1 and parsed/reconstructed exactly to 229 tasks and 426 edges.

## Runtime and product acceptance
- **HostLifecycle/P2P fallback — PASS:** https://github.com/artinrahimii/project/actions/runs/40448655571/job/122005897811. The job log explicitly reports `PASS: IPeerTransport contract, direct libp2p TCP exchange, authenticated TCP-to-QUIC fallback, bounded framing and disconnect semantics.`
- **Windows WPF build/policy checks — PASS:** https://github.com/artinrahimii/project/actions/runs/40448655571/job/122005898090.
- **Android APK/Protobuf checks — PASS:** https://github.com/artinrahimii/project/actions/runs/40448655571/job/122005898087.
- **Step Three quality gate — PASS:** https://github.com/artinrahimii/project/actions/runs/40448655603.

The regression scenario first attempts a refused TCP address, then connects to the same expected PeerId via its advertised QUIC/UDP address, transmits binary request bytes and returns binary response bytes over the selected peer transport. It also tests that a candidate list containing different expected identities is rejected before any transport factory call.

## Failed attempt retained
- **V 3.0.356 / T0118-A001 — FAIL:** `PeerConnectionException` wrapped a socket-refusal inner exception and was not recognized as retryable, preventing QUIC fallback. Exact log: https://github.com/artinrahimii/project/actions/runs/40448409783/job/122005094326.
- **V 3.0.357 / T0118-A002 — PASS:** nested exception classification corrected the retry behavior; exact-head tests passed. The graph retains the first failure and increments the successful attempt record.

## Scope limitation
This establishes an alternate direct route when TCP is unavailable but direct QUIC/UDP remains reachable. It does not demonstrate NAT hole punching, symmetric-NAT traversal, relay functionality, or connectivity in environments where TCP and QUIC are both blocked. Those limits are not represented as completed.
