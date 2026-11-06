# ADR-0005: Authenticated direct peer transport fallback

- **Status:** Accepted for the host-side transport connection boundary.
- **Date:** 2026-10-27
- **Related task:** T0007 (decision-to-source/test traceability).
- **Scope:** Ordered fallback across direct TCP and QUIC candidate addresses for one pinned peer identity. This decision does not claim Android-to-Windows application-level routing is delivered.

## Context

A peer may advertise more than one direct address. A TCP path can be unreachable while a QUIC path for the same peer is reachable. Trying a second address is safe only if fallback cannot silently switch to a different peer, leave abandoned transports undisposed, or capture exception text or application payloads in diagnostic evidence.

## Decision

1. `PeerTransportFallbackConnector.ConnectAsync` consumes candidate endpoint metadata and one expected peer identity. It rejects an empty candidate list and rejects any endpoint whose pinned identity differs from that expected identity before creating or dialing a transport; duplicate addresses are ignored while the original order is preserved.
2. Each candidate is connected through `IPeerTransport.ConnectSecureAsync`. The result is accepted only when the transport reports connected and its authenticated `RemotePeerIdentity` exactly matches the pinned identity.
3. A failed candidate's transport is disposed on the abandonment path. Caller-requested cancellation is propagated after best-effort cleanup rather than being turned into a successful connection.
4. Retryable connection failures may advance to the next candidate. The failure record stores only the address and exception type, not exception messages or inference payload bytes. If no direct candidate succeeds, `PeerConnectionUnavailableException` exposes a safe summary and the structured attempt list.
5. On success, the caller receives the authenticated transport, selected endpoint, and count of earlier failures. The connector itself handles connection setup only; application messages are sent directly through the returned transport. This is not a relay and does not make the rendezvous service an inference-data path.

## Implementation and executable evidence

- Production implementation: `src/Host.Windows/Networking/PeerTransportFallbackConnector.cs`.
- Runtime acceptance suite: `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`, registered in `tests/HostLifecycle.Tests/Program.cs` and run by `.github/workflows/product-build.yml`.
- `VerifyQuicFallbackAfterUnreachableTcpAddressAsync` exercises an unreachable TCP candidate followed by a reachable QUIC address, checks the failed-attempt count and selected endpoint, checks authenticated identities, exchanges binary request/response bytes directly, and disconnects the selected transport.
- `VerifyFallbackRejectsMixedPeerIdentitiesBeforeConnectingAsync` asserts candidate lists pinned to different identities are rejected before the transport factory is invoked.
- `tests/test_decision_traceability.py` checks that this ADR, its source paths, and these executable tests stay connected in the decision register.

## Evidence boundary and non-goals

The host-side transport tests do not prove Android interoperability, NAT traversal in arbitrary networks, app-level inference request routing from Android to Windows, or the full product acceptance gate. `AC-P2P-01` remains **NOT RUN** until its end-to-end criteria are exercised. This ADR records the implemented host connector contract; it does not mark the overall P2P product task complete.
