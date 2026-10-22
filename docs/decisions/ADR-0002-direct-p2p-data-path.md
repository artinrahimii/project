# ADR-0002: Direct P2P is the inference data path

- **Status:** Accepted as the first-release architecture decision; implementation remains pending.
- **Date:** 2026-10-22
- **Related task:** T0019
- **Requirements:** REQ-010 and REQ-011 in `docs/requirements.md`
- **Scope:** Select the expected request/response data path without claiming that peer discovery or transport already exists.

## Context

The Android client must be able to request inference from a Windows host running its own local model. A helper service may be needed to introduce peers or coordinate connection setup, but routing inference content through a central service would conflict with the product's privacy boundary and direct-P2P requirement.

## Decision

1. **Direct peer-to-peer transfer is the default data path.** The Android client sends an inference request directly to the selected Windows host, and the host returns the response over the direct peer connection.
2. An optional rendezvous or coordination service may support peer discovery, exchange connection-establishment metadata, and coordinate connection setup only.
3. The rendezvous/coordination component must not relay inference prompts or responses, generate inference output, or maintain conversation history.
4. If a direct data path cannot be established, the request must fail with an explicit connection-unavailable outcome. The first release must not silently fall back to a server-relayed inference payload.
5. Request identifiers remain temporary; this decision does not introduce a permanent user identity for inference routing.
6. Selection of a P2P library, NAT traversal strategy, wire-level authentication, encryption protocol, and detailed failure/retry behavior remains with the later implementation tasks and their acceptance tests.

## Consequences

- Transport code must keep the rendezvous/control plane distinct from the inference data plane.
- Tests must demonstrate that request/response payloads travel directly between peers and are absent from rendezvous traffic.
- Privacy, encryption, no-storage, and end-to-end behavior must be verified separately; this ADR is not runtime evidence for those properties.

## Implementation status and verification

This is an architecture decision, not a delivered P2P feature. The current Android request UI still discloses that transport is not implemented. Peer discovery, host-to-client transport, real direct-P2P request/response, and the product acceptance gate `AC-P2P-01` remain unimplemented / NOT RUN.

The source-contract test `tests/test_p2p_data_path_decision.py` checks consistency between this decision, the requirement, and the README. Passing that test does not prove runtime routing or network privacy.
