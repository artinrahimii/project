# T0134 verification evidence (interim)

- Reviewed source commit: `c49edacbc89498ce35a3ba906b463ce305c5136a` (V 3.0.458).
- Graph Blob for the reviewed base: `4e7891ec268b973e7aaba74c270043427ed7c599`; fetched via Git Trees API then Git Blob API. Exact Git SHA-1 matched the tree entry; the 2,259,671-byte graph parsed as a complete fenced JSON document with 229 nodes and 422 active edges.
- Existing behavior inspected: `PeerSecurityPolicy.ValidatePeerIdentity` rejects malformed/non-normalized PeerIds; `PeerTrustStore.EnsureTrusted` fails closed for unknown IDs; `HostIdentityStore` rejects unsupported identity-envelope versions and malformed key encodings; `Libp2pPeerTransport` requires an authenticated identity matching the expected peer; `ProtocolVersionPolicy` / `Program.cs` reject malformed and unsupported application protocol versions. Cryptographic negotiation remains the maintained libp2p Noise/TLS stack; no custom crypto-version negotiation is introduced.
- Added acceptance cases: invalid private-key protobuf data and an unsupported identity-envelope version; both preserve the original file on rejection.
- Dependency validation: `T0133 → T0134` contract/interface dependency (validated); `T0128 → T0134` sequencing-only (validated, removed from active edges). T0128 remains not_started.
- Exact commit CI: pending; no test result asserted in this interim evidence.
