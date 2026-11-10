# شواهد ممیزی انتقال امن P2P — UPD-S1-016 A001

## Commit مرجع
- `4c09319b29d0b1fcc57a78851d552e01f8ac9d74` — `V 3.1.177`
- گراف به‌روزرسانی Blob: `14e3150e5ca4d1392629e3e1682bf9a79e6b0dcf`
- نیازمندی‌ها: `update/Step one/Output/V1.10.json` و acceptance پنج‌بندی UPD-S1-016

## مسیرهای موجود و کد احراز هویت
- Android direct transport: `Libp2pInferenceRequestTransport.kt`; stable peer identity and trust: `AndroidPeerIdentityStore.kt`, `AndroidPeerTrustStore.kt`; runtime factory: `AndroidInferenceRequestRuntime.kt`.
- Android checks exact expected PeerId before dial, verifies the authenticated `remotePeerId` after Noise stream establishment, refuses `/p2p-circuit`, and checks correlated response fields before accepting a result.
- Windows transport: `Libp2pPeerTransport.cs`; requires trust store approval and exact peer identity for the authenticated session; `PeerAnnouncementHostLifecycle` constructs it with explicit expected peer identity.
- Windows fallback: `PeerTransportFallbackConnector.cs`; tries ordered direct TCP/QUIC addresses for one pinned peer identity, checks authenticated `RemotePeerIdentity`, disposes failed transports, stores only address + exception type, and throws `PeerConnectionUnavailableException` after direct candidates are exhausted.
- Relay: ADR-0004 status is accepted first-release policy with no relay data path enabled. Windows transport guard forbids `.WithRelay()`; Android rejects `/p2p-circuit`.
- Crypto: Android jvm-libp2p 1.3.7-RELEASE and Windows Nethermind.Libp2p 1.1.0 provide maintained secure-channel/identity handling; no app-authored key exchange, cipher, signature or hash algorithm is added.

## Source revision SHAs before this task's change
- Android transport: `d60aaa45aa2e3566eb2d6313f8a6bcb3941ad390`
- Android dispatcher: `99f69fbadc0270aec5ed6464c3f4cbb383d2417f`
- Android trust store: reviewed in ADR-0018 and code at commit `4c09319b29d0b1fcc57a78851d552e01f8ac9d74`.
- Windows libp2p transport: `c8ac8f5c9785eac4fbb06ce23c05b7f9dc1b3a20`
- Windows direct connector: `c8f6fabe1813dc9741d09a9ec8e3ff8dc6781def`
- Windows transport acceptance tests: `c42306e027a8254fa80a04d8c7e15c32556a420b`
- Android .NET peer fixture: `c5b53d6d880207c79dcf5442f213e7dac96bc3ea`
- Android transport tests: `1e33584601e8f13f196f24a65c196e46ebeb3bfe`

## Regression identified
- Prior direct failure was collapsed into REJECTED -> SUBMISSION_REJECTED -> VALIDATION_FAILED.
- Added explicit FAILED path for connection/directory unavailability while preserving REJECTED for invalid request, trust and identity checks.
- Added a closed-TCP-port regression with a prompt sentinel and assertions that safe diagnostics contain neither sentinel nor IP address.
- Added dispatcher and collector tests to preserve the distinct UI/error mapping.

## CI state
- No tests for A001 have run at this point. This evidence file deliberately does not claim success.
- Prior V 3.1.176 Android suite had 234 passed, zero failures and one skipped LAN-on-emulator scenario; it included the Windows .NET peer fixture build. It is baseline evidence only.
- Exact-head CI after the A001 implementation commit is mandatory before marking this task done.

## Explicit exclusions
- No relay data path, relay reservation, or three-peer relay byte-transfer test is enabled because relay is not approved.
- No physical Android handset or user-owned Windows host was involved.
- No public NAT reachability is inferred from loopback or CI fixtures.
