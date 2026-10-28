# ADR-0006: Expiring metadata-only peer announcement leases

- **Status:** Accepted for the rendezvous-directory, reusable lease publisher, and Windows peer-host lifecycle integration. Deployment-specific public reachability remains an unverified acceptance condition.
- **Date:** 2026-10-27
- **Related tasks:** T0007 (decision-to-source/test traceability), T0120 (metadata-only rendezvous), T0121 (renewable peer presence).
- **Related requirements:** REQ-010 and REQ-011 in `docs/requirements.md`.
- **Scope:** Define how the control plane represents an available peer and how stale metadata leaves discovery results. This decision does not claim Android-to-Windows inference routing is complete.

## Decision

1. The rendezvous control plane receives only peer ID, direct-address metadata, capability tokens, and an expiry timestamp through `PeerAnnouncement`. Prompts, responses, conversation history, and context capsules are not fields in this contract. The client exposes announcement and capability-discovery operations only.
2. The directory keeps the latest in-memory announcement for each peer ID. A replacement announcement replaces the previous current record; it does not create an announcement history. The directory removes expired records when announcements are accepted and when capability discovery is queried.
3. A reusable publisher sends an announcement immediately and renews the expiry timestamp periodically. Defaults are a five-minute TTL and a one-minute renewal interval; the renewal interval must be positive and shorter than the TTL. The shared contract caps an announcement's lifetime at 24 hours and the serialized control message at 16 KiB.
4. Stopping the publisher stops renewal; it does not immediately revoke or delete the remote directory entry. The entry is omitted after its last expiry is reached and the directory next performs expiry cleanup. This is TTL-based stale-record cleanup, not proof of instantaneous network-disconnect detection.
5. Non-loopback rendezvous URLs require HTTPS. The application must not advertise a loopback address as remotely reachable. Address advertisement must be based on a verified peer-reachable endpoint rather than an invented or assumed public address.

## Implementation and evidence links

- Metadata contract and size/lifetime bounds: `src/Shared.Contracts/Networking/PeerAnnouncement.cs`.
- HTTPS/loopback policy and metadata-only HTTP API: `src/Host.Windows/Networking/PeerAnnouncementClient.cs`.
- Renewal lifecycle and TTL refresh: `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs`.
- Expiry cleanup and current-record replacement: `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`.
- Executable loopback acceptance cases: `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`, including `PeriodicLeaseRenewalRefreshesTtlAndStopsAsync` and `PeerHostLifecyclePublishesAndServesTrustedPeerAsync`.
- Stable Windows host identity and explicit peer trust consumed by the WPF lifecycle: `src/Host.Windows/Security/WindowsHostIdentityProvider.cs` and `src/Shared.Security/PeerTrustStore.cs`.
- Source/configuration/test traceability regression: `tests/test_decision_traceability.py`.
- Decision register: `docs/decisions/README.md`.

## Evidence boundary and remaining work

The loopback acceptance test covers immediate publication, renewal to a later expiry, stopping renewal, and omission from discovery after the last TTL expires. A new loopback host-lifecycle test also exercises authenticated libp2p request/response after the lease is published and stops renewal after the peer disconnects. WPF participation enable/disable and shutdown are now wired to this lifecycle and exact-candidate CI passed. These tests still do not prove that a deployment's advertised address is reachable from the public Internet or that a production HTTPS rendezvous endpoint is configured.

**T0121 remains `blocked`** until deployment-specific rendezvous and advertised TCP/UDP addresses are verified from a peer on an independent network. The application requires explicit operator confirmation but does not itself perform a public reachability probe. **AC-P2P-01 remains `NOT RUN`** until Android-to-Windows end-to-end inference routing is exercised against its acceptance criteria.
