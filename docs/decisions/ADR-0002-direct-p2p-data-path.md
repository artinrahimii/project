# ADR-0002: Direct P2P is the inference data path

- **Status:** Accepted for the first-release architecture and the metadata-only rendezvous/direct-peer boundaries. Normal-chat integration is wired to the existing Android request runtime; physical Android-to-Windows product acceptance remains unverified.
- **Original decision date:** 2026-10-22
- **Implementation review:** 2026-11-10
- **Related tasks:** T0019; update tasks UPD-S1-002 and UPD-S1-010
- **Requirements:** REQ-010, REQ-011, REQ-019 in `docs/requirements.md`
- **Scope:** Select the first-release request/response data path, define what intermediaries may observe and retain, and distinguish implemented source/loopback behavior from unverified release acceptance.

## Context

The Android client requests inference from a selected Windows host that runs its own local model. A rendezvous service is useful for peer discovery and exchanging connection metadata, but it must not become a central inference endpoint or a store for prompts, responses, context capsules or conversation history. A relay is not acceptable merely because the relay hop is encrypted: the endpoint security channel must remain end-to-end, and the relay must not be able to decrypt application content.

The original product policy and REQ-010/REQ-011 require direct peer-to-peer transfer as the first-release data path. The initial proposal to route inference through an intermediary is therefore not accepted as the default.

## Decision

Direct peer-to-peer transfer is the default data path. The rendezvous/coordination component must not relay inference prompts or responses, generate inference output, or maintain conversation history.


1. **Primary data path — authenticated direct P2P.** The Android request runtime opens a direct libp2p connection to the selected Windows host and transfers the request/response over that peer connection. The transport authenticates the remote PeerId and uses the maintained libp2p secure-channel stack; the application must not invent a cryptographic algorithm.
2. **Control plane — metadata-only rendezvous.** The rendezvous service may accept a peer announcement and answer a capability-discovery query. It may exchange PeerId, direct-address and capability metadata needed to find a candidate. It must not accept inference prompts, context capsules, responses, request history, or model output, and must not generate inference.
3. **Direct-address alternatives only.** A caller may try another direct address for the **same pinned PeerId**. The Windows host-side connector can attempt its ordered TCP/QUIC candidates and returns an authenticated connection only when the remote PeerId matches. The current Android directory client deliberately selects only direct TCP addresses; although QUIC addresses can appear in announcement metadata, Android does not currently select them. This ADR therefore does not claim Android QUIC fallback or general NAT traversal.
4. **Fail closed when direct paths fail.** If no supported direct peer connection can be established, the caller must fail with an explicit connection-unavailable outcome; it must not silently fall back to a server-relayed inference payload. The request returns a visible connection failure and no prompt or response is sent through rendezvous, a server relay or a central inference API.
5. **Relay policy.** No relay inference-data path is enabled in the first release. Any future relay requires a separate explicit product/privacy decision and a working three-peer runtime test proving opaque bidirectional byte forwarding under endpoint-to-endpoint authentication and encryption. The relay must not terminate the endpoint security layer, parse or transform application frames, or log/persist payloads. The metadata that such a relay can observe is documented in ADR-0004.
6. **Request identity.** Temporary request and attempt identifiers are used for correlation. Rendezvous is not a permanent user identity service and must not persist request identifiers or conversation history.

## Implemented boundary at the 2026-11-10 code baseline

The following statements are source-backed descriptions of the current tree, not a claim that the complete release acceptance gate has passed:

- `PeerAnnouncement` contains only `PeerId`, direct `Addresses`, `Capabilities` and `ExpiresAtUtc`. It validates the peer identity, direct addresses, capability values, field counts, message size and announcement lifetime. Unknown JSON fields are rejected by the rendezvous HTTP host.
- `PeerAnnouncementServiceHost` exposes `POST /v1/peers/announcements` and `GET /v1/peers?capability=...` (plus a health endpoint). It does not expose a central inference or conversation-history route. The announcement directory is an in-memory current-record dictionary; a later announcement replaces the earlier record for the same PeerId.
- The Windows peer host publishes an expiring metadata lease. Defaults are a five-minute TTL and a one-minute renewal interval. Stopping renewal does not synchronously revoke the remote record: the record is omitted after expiry when the directory next performs cleanup.
- Non-loopback rendezvous URLs require HTTPS. The Android directory client issues a metadata-only GET, requests `Cache-Control: no-store`, keeps an in-memory cache for five seconds by default (maximum one minute), and does not return stale cache entries after a failed refresh.
- The rendezvous contract permits an announcement lifetime no longer than 24 hours and limits the serialized control message to 16 KiB. The directory holds only the latest current announcement per PeerId, not an application-level announcement history.
- Windows advertises direct TCP and QUIC addresses and has an ordered same-PeerId TCP/QUIC fallback connector. The Android client filters for direct TCP addresses and uses the TCP + Noise path; QUIC metadata is not currently selected by Android.
- The transport endpoints are not privacy-blind. The processing host necessarily receives and can read the prompt/context after transport decryption in order to perform local inference. The user must approve/trust the intended peer; transport encryption protects data in transit, not from the selected host.

## Data visibility and retention

| Component | Data visible to that component | Application-level retention / limit |
|---|---|---|
| Android rendezvous client | PeerId, advertised direct TCP addresses, expiry and the capability needed for discovery | In-memory cache; five-second default, one-minute maximum; expiry-aware and no stale fallback |
| Rendezvous application | PeerId; announced direct IP/port addresses; capability tokens; expiry timestamp; and ordinary connection metadata available at the HTTPS service edge | One current in-memory record per PeerId; announcement expiry is at most 24 hours (five minutes by default, refreshed each minute by the Windows publisher). Expired records are pruned on the next announce/discovery operation, not by a dedicated timer. No durable application database or conversation-history store is configured in this service |
| Windows host | Its own PeerId, announced addresses/capabilities, authenticated client PeerId, and the inference request after the secure peer channel is decrypted | Peer announcement template and lease publisher state live in the host process; request lifecycle is bounded by protocol deadlines/cancellation. This is not a claim that the host cannot read content |
| Direct peer endpoints | Each endpoint sees its authenticated peer identity, remote network address/port and transferred application messages; the selected Windows host reads the prompt/context to infer and returns the model response | No inference payload is sent to rendezvous by the application protocol. Endpoint-local chat persistence is governed by the platform app's own storage policy, not by the rendezvous directory |
| Optional relay (not enabled) | If approved in a future release: PeerIds, network addresses/ports, connect/reservation outcomes, start/end time and duration, byte counts, payload sizes, timing/rate patterns and likely communicating peers | No first-release relay exists. A future relay would need an explicit retention/deletion policy; end-to-end encryption would not hide this connection metadata or provide anonymity |
| Hosting provider / reverse proxy / network logs | Potentially source IP, endpoint path, timing, connection outcomes and other operational metadata depending on deployment configuration | The repository does not configure a production reverse proxy, provider logging policy or retention period. Their behavior and deletion schedules are **not verified** here; do not claim zero metadata logs or instantaneous deletion at this infrastructure layer |

The application-level directory has no persistent store in the reviewed service code. This statement does **not** establish that an external hosting provider, reverse proxy, firewall or network operator retains no connection metadata. Production deployment must separately set and review access-log policy, log retention and deletion controls.

## Rejected alternatives

- **Central inference API or hosted model proxy:** rejected for the first release because it would put prompt/response content and potentially conversation history at a central service, contrary to REQ-011 and the direct-P2P privacy boundary.
- **Plaintext or server-terminated relay:** rejected because an intermediary that terminates the application security layer can read inference content.
- **Two unrelated client-to-relay and relay-to-host encrypted connections:** rejected as insufficient for end-to-end privacy if the relay can decrypt or re-encode application data between the connections.
- **Automatic relay fallback after direct failure:** rejected. A failed direct connection remains a visible failure; it does not authorize disclosure of content to another service.
- **Custom cryptographic protocol:** rejected. Existing maintained libp2p Noise/TLS support and peer identity validation remain the cryptographic transport boundary.

## Existing verification and limits

- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs` runs a real loopback HTTP service and verifies metadata-only announcement/discovery, exact response fields, replacement rather than history, absence of an inference route, rejection of unknown `prompt`/`contextCapsule`/`capsule` fields, TTL refresh/expiry and bounded control messages.
- The same HostLifecycle acceptance suite exercises an authenticated encrypted peer connection and request/response/cancellation over loopback peer processes. These are executable loopback integration checks, not a physical Android handset talking to a separate Windows host with a loaded release model.
- `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs` covers the Windows-side same-PeerId direct TCP-to-QUIC fallback and rejects mixed peer identities.
- `tests/test_relay_privacy_boundary.py`, `tests/test_p2p_data_path_decision.py` and the new `tests/test_update_transport_trust_boundary.py` preserve the decision/source boundaries. Source-contract tests do not prove packet-level privacy by themselves. This architecture decision does not prove runtime routing; it records the approved boundary and current evidence only.
- Android retains a separate request runtime using the rendezvous directory and direct P2P transport. As of the 2026-11-10 code baseline, the normal conversation screen is wired to that existing runtime: it discovers the configured directory, requires explicit peer trust and first-request privacy consent, invokes automatic model resolution through `RequestViewModel.sendRequest(prompt)`, and persists correlated assistant responses and typed request notices into the active conversation's Room message table. The conversation message ViewModel owns Room persistence only; it does not create sockets or perform network transport itself. This source-level integration does not prove successful physical-device routing or release acceptance.
- `AC-P2P-01` remains **NOT RUN** until the required real Android-to-Windows product path, local model response and same-conversation persistence are exercised on the target devices. Public reachability, deployment-level metadata logging and production rendezvous configuration also remain separate acceptance checks.

## Legacy task alignment

- `T0009`: its bounded source-inventory task found no central prompt/response/history API at that snapshot; its own report expressly says that source inspection is not runtime routing evidence.
- `T0019`: direct P2P as the intended data path remains the accepted architecture decision; it did not claim transport implementation.
- `T0118`: same-PeerId ordered direct TCP/QUIC fallback is implemented on the Windows side; it does not establish arbitrary NAT traversal or Android QUIC selection.
- `T0119`: ADR-0004 defines future opaque encrypted relay requirements and metadata visibility; no relay byte path is enabled.
- `T0120`: metadata-only announcement/capability service and loopback HTTP contract are implemented and covered by HostLifecycle tests.
- `T0206`: tests reject sensitive prompt/context fields at the control-plane HTTP endpoint and verify Android discovery uses a bodyless GET; this is not a general packet-capture or external-infrastructure audit.
- `T0227`: remains **not_started** in the legacy graph; the release-level proof that no central component receives or stores any prompt, response or history still requires the end-to-end/network evidence specified by its task.

## Implementation evidence references

- `docs/decisions/ADR-0004-encrypted-relay-boundary.md`
- `docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md`
- `docs/decisions/ADR-0006-peer-announcement-leases.md`
- `src/Shared.Contracts/Networking/PeerAnnouncement.cs`
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`
- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`
- `src/Host.Windows/Networking/PeerAnnouncementClient.cs`
- `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs`
- `src/Host.Windows/Networking/PeerTransportFallbackConnector.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`

The original decision remains direct P2P. The 2026-11-10 implementation review records the normal-chat UI integration with the existing runtime; it does not enable relay, weaken privacy, or mark any physical end-to-end gate complete.
