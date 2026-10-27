# T0119 execution report — relay privacy design in progress

- **Task:** design the relay privacy boundary if relay-based encrypted byte forwarding is ever needed.
- **Attempt:** `T0119-A001`.
- **Base:** `main` at `fd86ea667ff5e13dc230d982f76b2b2a55a26993` (`V 3.0.358`). The graph Blob `1c95ae7879e578edf1df37c325df215ea9d50954` was retrieved in full; SHA-1 matched; fenced JSON was reconstructed exactly: 229 tasks, 426 edges.
- **Prerequisites:** `T0118` and `T0115` are done. `T0118 → T0119` and `T0115 → T0119` remain validated sequencing-only edges.
- **Decision artifact:** `docs/decisions/ADR-0004-encrypted-relay-boundary.md` records that first release does not enable relay fallback. If direct TCP/QUIC candidates all fail, the existing connector returns an explicit `PeerConnectionUnavailableException` instead of sending content through a central service.
- **Reason:** ADR-0002 forbids rendezvous from relaying prompts/responses. The upstream [.NET libp2p relay README](https://github.com/NethermindEth/dotnet-libp2p/blob/main/libp2p/Libp2p.Protocols.Relay/README.md) identifies a critical stream-bridging gap. A Hop/Stop control response alone does not prove that the application byte stream works.
- **Future path specified:** a later approved relay must forward opaque bytes below an end-to-end authenticated/encrypted channel. The relay may see PeerIds, endpoint IPs/ports, reservation and connection outcomes, timestamps/duration, byte counts and traffic-size/rate patterns. It must not terminate endpoint encryption, parse/transform application messages, or log/persist prompts/responses.
- **Guard added:** `tests/test_relay_privacy_boundary.py` checks that the host stack does not opt into `WithRelay()`, direct-path exhaustion remains explicit, metadata exposure is documented, and the decision is registered.
- **Dependency review:** `T0119 → T0120` was reviewed and validated as `contract_or_interface_dependency`: the rendezvous/announcement service must consume this metadata-only privacy contract. The existing `blocks` edge is preserved.
- **Limit:** no runtime relay byte stream has been implemented or tested. This round delivers a decision and source-contract test only. Exact-head CI is pending; T0119 remains in progress until the new guard passes.
