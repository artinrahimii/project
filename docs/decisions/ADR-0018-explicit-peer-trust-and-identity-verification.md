# ADR-0018: Explicit peer trust and identity verification

- **Status:** Accepted. Android peer-trust disclosure and Windows host-participation consent are integrated. Physical Android-to-Windows product acceptance remains unverified.
- **Original decision date:** 2026-10-27
- **Current implementation review:** 2026-11-08 — UPD-S1-007
- **Related task:** T0131.
- **Related update task:** UPD-S1-007.

## Decision

Unknown libp2p PeerIds are not trusted automatically. A caller must explicitly confirm the exact canonical PeerId before adding it to a persistent trust registry. Reconnection is permitted only for a stored decision; a different PeerId remains unknown rather than replacing a prior identity.

## Persistence and key protection

`PeerTrustStore` stores a bounded, versioned document protected by the injected `IUserSecretProtector`. Windows production callers use current-user DPAPI. Cross-instance access is serialized by a path-scoped named mutex; updates use same-directory atomic replacement. Corrupt, unsupported, unreadable or unprotectable data fails closed and is not reset automatically.

Android keeps peer identity and peer trust as purpose-separated records protected by non-exportable Android Keystore AES keys. Its versioned AES-GCM envelope uses a random nonce and authenticated associated data. Missing, corrupt or tampered existing records fail closed rather than silently rotating identity or granting trust. Identity and trust use separate Keystore aliases.

## Transport gate and authenticated identity

`Libp2pPeerTransport` requires a `PeerTrustStore`. Listening requires the expected remote identity to be trusted. Dialing requires a canonical PeerId, the `/p2p` identity in the multiaddress to match the expected identity, and a persisted explicit trust decision before dialing. The maintained libp2p Noise/TLS implementation remains responsible for secure-channel cryptography and authenticated session identity; this project adds no cryptographic algorithms.

The host request path verifies the authenticated remote identity against the configured expected PeerId before dispatch. Replay fencing is bound to the authenticated peer plus the exact request/attempt tuple and request deadline. Responses are checked against request, attempt, round, model, host identity and deadline before acceptance.

## User consent boundaries — current implementation

### Android requesting peer

When a discovered peer is not yet trusted, `MainActivity` displays **“Before you trust this host”** for the exact PeerId before persisting trust. The notice states that the processing host can read the prompt and context capsule after transport decryption, that transport encryption does not hide content from the selected host/operator, and that the host may inspect or retain content outside this app's control.

Confirming this dialog stores trust only; it does not send a prompt or context capsule. Sending remains a separate user action through **“Send to trusted hosts”**. The user can revoke trust later. Existing persisted trust is reused on subsequent requests; this disclosure is shown when a new peer is enrolled, not as a per-request modal.

### Windows processing host

Before constructing or starting `PeerAnnouncementHostLifecycle`, `MainWindow.ChangeNetworkParticipationAsync` displays the shared `PeerInferencePrivacyDisclosure` warning for the exact expected remote PeerId. It explains that the host process and computer operator can read prompt/context after transport decryption and that transport encryption protects transit, not the receiving endpoint.

The default answer is No. Declining resets the participation toggle and exits before the peer lifecycle/listener/lease is created. After the privacy notice, a separate confirmation verifies that the exact remote PeerId is trusted; a new trust decision is persisted only after explicit approval.

## Data-flow and retention boundary

- Android sends prompt and context-capsule data over the authenticated direct libp2p peer channel. The receiving Windows host can read those contents after transport decryption.
- Rendezvous is a metadata-only discovery/control-plane component. It receives peer identity/address/capability/expiry metadata, not inference prompt, response or conversation history in the intended protocol.
- First-release relay is disabled; the design must fail closed rather than silently sending inference payloads through rendezvous or another relay.
- Windows host conversation content is `ProcessSessionOnly` by default. Durable host history is optional, default-off and requires separate owner consent; when enabled, the local file is protected with current-user DPAPI.
- Android conversation/message/request-state history is intentionally local to Room and protected at rest by SQLCipher; this is separate from Windows host retention and rendezvous metadata.
- Regular product diagnostics use fixed allowlisted numeric event/outcome codes and elapsed duration, not prompt, response, context capsule, peer identifiers, keys or exception messages. App-owned wire/request buffers are zeroed at documented ownership boundaries.

## Verification — current implementation review

Exact V 3.1.40 evidence on commit `380c89043d7e2cfce7bc50926419a1a682aaf0cb`:

- Step Three quality gate [run 42463288660](https://github.com/artinrahimii/project/actions/runs/42463288660): all 472 Python tests passed, including Android/Windows disclosure, host-content visibility, diagnostic log scans, threat-model documentation, sensitive artifact scanning and buffer cleanup.
- Product build [run 42463288704](https://github.com/artinrahimii/project/actions/runs/42463288704): Android build/instrumentation, Windows WPF/installer and HostLifecycle jobs succeeded.
- Windows HostLifecycle emitted 67 critical pass markers with one skip and 85.76% line coverage. Its log includes secure-channel identity rejection, plaintext/no-downgrade rejection, tampered request rejection, replay fencing, forged response correlation rejection, peer trust, DPAPI host identity, privacy disclosure, safe diagnostics and conversation-retention/canary checks.
- Android JUnit validation recorded 224 passed, 0 failures and 1 skipped. Android Keystore instrumentation artifact: `13610099035`; Android reports: `13610895903` and `13610288694`.
- These are runner/emulator results, not physical Android-to-Windows acceptance or independent packet/storage forensic verification.

### Historical verification at the original ADR baseline

At V 3.0.443 (commit `db6b7f1bf87747bf4d10f4f8e1f227df4264cde3`), the original caller-boundary wording stated that WPF **“does not yet present a peer-enrollment prompt”**. That phrase is retained here solely as a historical statement about that earlier baseline; it is superseded by the current Windows privacy-consent and explicit PeerId-enrollment UI described above. The earlier verification also stated that **full independent-node E2E remain separate** from the trust-store/transport checks; that remains true for current operational acceptance.


## Limits and outstanding operational acceptance

- A trusted processing host is an endpoint that can inspect the decrypted prompt/context capsule; this design is not end-to-end encryption against that host.
- The application cannot control retention by the host operator, OS, pagefile/swap, crash dumps, backup, screenshots or third-party tools.
- Source/CI checks do not prove every external rendezvous provider or deployment reverse proxy has no metadata logs.
- `AC-P2P-01` and `AC-PRIV-01` remain **NOT RUN** until physical Android-to-Windows operation, runtime routing, and independent deployment storage/log review are completed. Emulator, CI, loopback tests or source contracts are not substitutes.

## Traceability

- Core store, transport and protector: `src/Shared.Security/PeerTrustStore.cs`, `src/Host.Windows/Networking/Libp2pPeerTransport.cs`, `src/Host.Windows/Security/WindowsDpapiSecretProtector.cs`.
- Historical and current executable trust tests: `tests/HostLifecycle.Tests/PeerTrustStoreAcceptanceTests.cs`, `tests/HostLifecycle.Tests/WindowsPeerTrustStoreAcceptanceTests.cs`, `tests/HostLifecycle.Tests/PeerTransportContractAcceptanceTests.cs`, `tests/HostLifecycle.Tests/Program.cs`.
- CI and regression traceability: `.github/workflows/product-build.yml`, `tests/test_decision_traceability.py`.

- Trust and validation: `src/Shared.Security/PeerTrustStore.cs`, `src/Shared.Security/PeerSecurityPolicy.cs`.
- Authenticated transport: `src/Host.Windows/Networking/Libp2pPeerTransport.cs`, `PeerAnnouncementHostLifecycle.cs`, `Libp2pPeerMessageProtocol.cs`.
- Android identity/trust: `AndroidPeerIdentityStore.kt`, `AndroidPeerTrustStore.kt`, `AndroidKeyStoreProtectedBlobProtector.kt`.
- Windows key protection: `src/Shared.Security/HostIdentityStore.cs`, `src/Host.Windows/Security/WindowsDpapiSecretProtector.cs`.
- Privacy UI: `PeerInferencePrivacyDisclosure.cs`, `src/Host.Windows/MainWindow.xaml.cs`, `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`.
- Replay/integrity: `AuthenticatedRequestReplayGuard.cs`, `ProtocolMessageValidator.cs`, `PeerResponseCorrelationValidator.cs`.
- Tests: `tests/test_peer_inference_privacy_disclosure.py`, `test_host_content_visibility.py`, `test_network_payload_logging.py`, `test_host_conversation_retention_policy.py`, `test_threat_model_documentation.py`, `test_sensitive_artifact_scanning.py`, `test_sensitive_buffer_cleanup.py`, `PeerMessageSecurityAcceptanceTests.cs`, `WindowsPeerTrustStoreAcceptanceTests.cs`, `WindowsHostIdentityStoreAcceptanceTests.cs`, `SafeDiagnosticLogAcceptanceTests.cs`, `AndroidKeyStorePersistenceTest.kt`.
