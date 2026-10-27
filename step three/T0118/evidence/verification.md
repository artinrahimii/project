# T0118 verification evidence — pending exact-head CI

## Baseline integrity
- Repository: `artinrahimii/project`, branch `main`.
- Base commit: `V 3.0.355`, `af032ec123eaa65dd77c835ac0b1f43c4970e501`.
- Graph path: `step three/project-graph.md`, Blob `18784bac6e9e353f0137531fe17fdc61e2672fe3`. The complete original source measured 1828192 bytes and 1706476 returned text characters; the Git Blob SHA-1 matched. The fenced JSON was parsed and reconstructed exactly.
- Graph shape: 229 task nodes, 426 edges, 229 execution-order entries.

## Implementation
- `Libp2pPeerTransport` registers the library QUIC transport and exposes TCP and QUIC/UDP multiaddresses for one PeerId.
- `PeerTransportFallbackConnector` tries direct address candidates sequentially with one pinned expected PeerId, disposes failed transports and returns the selected verified transport. Duplicate address candidates are ignored.
- Attempt diagnostics retain only candidate address and exception type; no exception message or application content is captured.
- The connector accepts metadata only and does not forward inference content to rendezvous. Application data is sent over the selected peer connection.

## Test added
- Keep the real TCP loopback acceptance.
- Add a second actual two-peer test: a refused TCP address is tried first; QUIC/UDP for the same pinned peer must then connect and preserve bidirectional binary payloads.
- Reject mixed expected-peer identities before the transport factory runs.
- Linux workflow installs `libmsquic`, the documented native dependency for .NET QUIC on Linux.

## Pending verification and boundaries
- Exact-head HostLifecycle, Windows WPF, Android APK/Protobuf, and Step Three quality workflows are pending.
- This verifies a direct alternate transport only if CI succeeds; it does not establish hole punching, symmetric-NAT traversal, relay availability, or cross-platform Android-to-Windows integration.
