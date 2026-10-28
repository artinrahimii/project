# ADR-0019: Processing-host visibility of inference request content

- **Status:** Accepted privacy disclosure and endpoint-trust boundary; product-level Android-to-Windows request delivery remains unverified.
- **Date:** 2026-10-28
- **Related task:** T0138.
- **Related requirements:** REQ-010, REQ-011, and REQ-019 in `docs/requirements.md`.
- **Related decisions:** [ADR-0002 — Direct P2P data path](ADR-0002-direct-p2p-data-path.md), [ADR-0016 — Maintained libp2p cryptographic transport](ADR-0016-maintained-libp2p-cryptographic-transport.md), and [ADR-0018 — Explicit peer trust and identity verification](ADR-0018-explicit-peer-trust-and-identity-verification.md).

## Context

The shared `InferenceRequest` contract carries the user-supplied `prompt` as a string and `context_capsule` as bytes. On the host-side path, `PeerAnnouncementHostLifecycle` receives and validates the request, and the WPF handler parses the message and passes the request object to `HostInferenceSession.GenerateAsync`. The libp2p application framing protocol is carried over the maintained Noise/TLS transport; framing itself does not provide encryption.

## Decision

1. **The processing host can read the request content.** After transport decryption at the destination, the host process can inspect `InferenceRequest.prompt` and the bytes of `InferenceRequest.context_capsule`. The capsule is part of the received protocol message even where a particular local inference adapter may not consume every capsule field.
2. **Transport encryption is not endpoint invisibility.** Noise/TLS protects the peer-to-peer channel in transit against intermediate observers, but the recipient endpoint must receive the plaintext application message to parse and process it. Do not call this end-to-end encryption against the processing host.
3. **The host is a trust boundary.** A caller should send only content it is willing to disclose to the chosen host. This repository does not promise that a recipient host, its operator, or another process with sufficient local access can never inspect or retain that content.
4. **Keep the control plane distinct.** The intended rendezvous service handles discovery/connection metadata only and must not receive inference prompts, responses, or conversation history as a relay. The design rule does not replace a runtime path/packet trace.
5. **Do not overstate product completion.** This ADR documents the protocol and host source boundary; it is not evidence that Android-to-Windows request delivery is complete, that no payload crosses an intermediary in every deployment, or that the processing host has no persistence path. `AC-P2P-01` and `AC-PRIV-01` remain **NOT RUN** until their acceptance criteria are tested.

## Implementation and repeatable source-contract evidence

- Canonical message fields: `protocol/whitep/v1/messages.proto`.
- Secure transport boundary: `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs`.
- Host request reception: `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`.
- WPF processing handler: `src/Host.Windows/MainWindow.xaml.cs`.
- Repeatable source/documentation checks: `tests/test_host_content_visibility.py` and `tests/test_claims_audit.py`.
- Product runtime gates: `docs/acceptance.md` (`AC-P2P-01`, `AC-PRIV-01`).

These source-contract checks make the disclosure auditable and protect against contradictory docs; they do not substitute for independent-node Android-to-Windows testing or a deployment storage/log audit.
