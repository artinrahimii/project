# ADR-0004: Encrypted relay boundary and metadata visibility

- **Status:** Accepted first-release policy; no relay data path is enabled.
- **Related task:** T0119.
- **Related requirements:** REQ-010 and REQ-011 in `docs/requirements.md`.
- **Related decisions:** [ADR-0002 — Direct P2P data path](ADR-0002-direct-p2p-data-path.md).
- **Purpose:** define the privacy boundary before any production relay fallback is enabled. This decision is not a claim that relay transport is implemented.

## Context

T0118 now provides ordered direct TCP-to-QUIC fallback for the same authenticated PeerId. If every direct candidate fails, `PeerTransportFallbackConnector` returns an explicit `PeerConnectionUnavailableException`. ADR-0002 prohibits a central rendezvous service from relaying inference prompts or responses and requires a visible connection-unavailable result rather than a silent server-relayed fallback.

The upstream [.NET libp2p relay protocol README](https://github.com/NethermindEth/dotnet-libp2p/blob/main/libp2p/Libp2p.Protocols.Relay/README.md) describes the Hop/Stop control protocol but also calls out a critical stream-bridging gap. The upstream [RelayHopProtocol source](https://github.com/NethermindEth/dotnet-libp2p/blob/main/libp2p/Libp2p.Protocols.Relay/RelayHopProtocol.cs) must be reviewed with the exact package version before production relay use. A successful reservation/control handshake alone is not evidence that an application byte stream works.

## Decision

1. **First release does not enable a relay data path.** Direct TCP and QUIC candidate attempts remain the only automatically attempted connection paths. When no direct candidate succeeds, the caller gets an explicit connection-unavailable outcome. No prompt, response or conversation history is sent to a rendezvous or relay service.
2. **A future approved relay must be an opaque byte-forwarding path.** The endpoint peers must complete end-to-end authentication and encryption across the relay. The relay must forward the already protected byte stream, without terminating the endpoint security layer, parsing application messages, transforming the payload, or logging/persisting its content. Two independent client-to-relay and relay-to-peer encrypted connections are not sufficient if that arrangement lets the relay decrypt application bytes.
3. **Relay metadata exposure is accepted and must be disclosed.** A relay can observe peer IDs, network addresses and ports, reservation/connection outcomes, connection start/end times and duration, byte counts, payload sizes, timing/rate patterns, and likely which peers communicate. End-to-end encryption can protect prompt and response contents; it does not hide this connection metadata or provide anonymity.
4. **No silent fallback.** A future relay mode requires an explicit product decision, documented user-visible behavior, a working relay transport, and passing runtime evidence. Until then, direct-connection failure remains a real failure, not a cue to transmit content elsewhere.
5. **Acceptance gate for any future runtime relay:** a real three-peer integration test must establish a relayed application channel, transfer arbitrary binary bytes in both directions, verify end-to-end identity/encryption at the two endpoints, prove that the relay has no application-payload parsing/logging/storage path, and record the metadata that remains visible. A control-plane RESERVE/CONNECT status test by itself is insufficient.

## Current implementation boundary

- `Libp2pPeerTransport` uses direct TCP/QUIC and does not opt into `WithRelay()`.
- `PeerTransportFallbackConnector` accepts endpoint/identity metadata only and never accepts prompts or responses; exhausted direct paths throw `PeerConnectionUnavailableException`.
- This ADR and its source-contract test decide the first-release boundary and define the future relay requirements. They do not implement or verify a working circuit-relay stream.
