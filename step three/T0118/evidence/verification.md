# T0118 verification evidence — fallback failure found; correction pending CI

## Baseline integrity
- Repository `artinrahimii/project`, branch `main`.
- Implementation under test: `V 3.0.356`, `27099fc451a383811823e3232ac49fce02fb4b8a`.
- Graph path `step three/project-graph.md`, Blob `2d66e97a2480905f26a2085471c0eae7d307cee0`; retrieved in full and original Git Blob SHA-1 matched. Fenced JSON parsed/reconstructed exactly: 229 tasks, 426 edges.

## V 3.0.356 exact-head results
- Step Three quality gate: **PASS**, https://github.com/artinrahimii/project/actions/runs/40448409838.
- HostLifecycle: **FAIL** at `VerifyQuicFallbackAfterUnreachableTcpAddressAsync`, https://github.com/artinrahimii/project/actions/runs/40448409783/job/122005094326.
- Linux MsQuic installation: **PASS** within the HostLifecycle job.
- Failure: `PeerConnectionException` wrapped `SocketException(Connection refused)`; the direct-address fallback did not recognize the wrapper as retryable and stopped before trying QUIC.

## Correction in T0118-A002
- Retry detection inspects nested `InnerException` causes and `AggregateException` children for connection errors.
- The test still requires the same expected PeerId across candidates and successful binary request/response exchange over direct QUIC.
- Exact V 3.0.357 HostLifecycle, Windows WPF, Android APK/Protobuf, and Step Three quality results are pending; no acceptance is inferred from source presence.

## Scope boundary
This fallback can use QUIC when TCP is unavailable but direct UDP/QUIC is reachable. Hole punching, symmetric-NAT traversal, relay availability and app-level Android↔Windows end-to-end routing remain unproven.
