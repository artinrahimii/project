# ADR-0013: Temporary cryptographic request identifiers

- **Status:** Accepted for the existing generator, protocol validator, response mapping and rendezvous metadata contract.
- **Date:** 2026-10-27
- **Related task:** T0007 (technical-decision traceability).
- **Related requirement:** REQ-012 (temporary request identity).
- **Scope:** Request correlation across the versioned inference protocol without requiring a permanent user/device account identifier.

## Context

Requests and typed responses need a correlation value, but the protocol requirement does not call for an account identifier. The host already generates request IDs locally; the wire contract validates identifier fields, and response mapping returns the same request ID so clients can correlate an outcome.

## Decision

1. **Generate identifiers with a cryptographic RNG.** `SecureRequestIdGenerator` uses 16 random bytes (128 bits) and formats them as `req_` followed by 32 lowercase hexadecimal characters. The identifier contains no encoded account, device, prompt, or host data.
2. **Treat uniqueness probabilistically, not as a global guarantee.** The executable test generates 4,096 identifiers, validates their format, and checks for repetition in that sample. This is a useful regression check, not a proof that collisions are impossible. Under the birthday approximation, for (n) independently uniform 128-bit IDs the collision probability is approximately (n(n-1)/2^{129}) when the probability is small.
3. **Validate identifiers at the protocol boundary.** `ProtocolMessageValidator` bounds request/attempt identifier length and rejects invalid values before they are accepted as valid protocol input. `NetworkInferenceRequestResponseMapper` requires an identifier before mapping typed failure outcomes and preserves it only for the corresponding response.
4. **Keep rendezvous state metadata-only and ephemeral.** `PeerAnnouncement` exposes peer ID, direct addresses, capabilities and expiry. `PeerAnnouncementDirectory` retains only the current announcement per peer in memory and removes expired entries; it has no request-ID field or request/history store. The rendezvous control plane is not an inference request/response history service.
5. **Do not promote a correlation ID into a stable identity.** The request-ID format is not an account/device identity and should not be used to link requests into a stable user profile. Any future persistence, analytics, or logging of request IDs must be evaluated as a separate privacy decision.

## Implementation and verification links

- Requirement: `docs/requirements.md` (REQ-012).
- Cryptographic generation: `src/Host.Windows/Requests/SecureRequestIdGenerator.cs`.
- Protocol field and versioned wire contract: `protocol/whitep/v1/messages.proto`.
- Identifier validation: `src/Shared.Contracts/ProtocolMessageValidator.cs`.
- Response correlation: `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`.
- Rendezvous storage boundary: `src/Shared.Contracts/Networking/PeerAnnouncement.cs` and `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`.
- Executable generation regression: `tests/HostLifecycle.Tests/SecureRequestIdGeneratorTests.cs`, registered in `tests/HostLifecycle.Tests/Program.cs`.
- Metadata-only rendezvous regression: `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`.
- Decision/source traceability: `docs/decisions/README.md` and `tests/test_decision_traceability.py`.
- CI entrypoint: `.github/workflows/product-build.yml`.

## Evidence boundary

The generator test establishes format and no repeat within a finite sample, not mathematically guaranteed global uniqueness. Source and loopback acceptance tests show the current rendezvous contract does not store request IDs or inference contents, but this does not substitute for a deployment-level log/retention audit or prove the separate Android-to-Windows runtime gate `AC-P2P-01`; that gate remains **NOT RUN**.
