# UPD-S1-022 — شواهد ممیزی اعتماد و ابطال همتا

## خط مبنا و شواهد commit

- Repository: `artinrahimii/project`
- Branch: `main`
- Exact commit under review: `0eeb9a4ba0ebabb4e689c8a07442558f80b8c675` — `V 3.1.147`
- Update graph blob at the same commit: `41a64cbb6a5500e37e40736548430e4c92df4f8f`
- V1.10 requirements source: `update/Step one/Output/V1.10.json`
- Current task prerequisities were `UPD-S1-001`, `UPD-S1-007`, `UPD-S1-024`; all three were already `done` with actual outputs and successful evidence.

## شواهد کد

### هویت محلی
- Android `AndroidPeerIdentityStore.loadOrCreate()` creates a stable Ed25519 libp2p identity, protects private-key bytes using a separate non-exportable Android Keystore AES key, clears transient byte arrays and fails closed if an existing identity envelope cannot be authenticated or parsed.
- Windows `WindowsHostIdentityProvider` uses the user-scoped DPAPI-protected `HostIdentityStore`; the implementation explicitly fails closed for corrupt existing files rather than silently rotating the PeerId.
- Identity tests verify stability across store/provider instances and reject corrupt or tampered persisted identity material without replacing it.

### اعتماد به همتا
- Android `AndroidPeerTrustStore` canonicalizes the PeerId, refuses trust without `explicitlyConfirmed=true`, persists a purpose-separated authenticated record protected by Android Keystore, bounds the record/list and fails closed on malformed or tampered storage.
- Android `revokeTrust` removes the exact PeerId and deletes the persisted envelope when the final entry is removed. The request runtime and view model pass through the trust-store operation and update the peer's visible trust state.
- Windows `PeerTrustStore` enforces canonical identity and explicit approval. The WPF `Revoke saved trust` action refuses to run while hosting or a participation transition is active, verifies the exact stored PeerId, uses a second confirmation with default No, then revokes it. The last trust entry deletes the protected file.
- The Windows libp2p transport checks the stored trust decision before dialing and checks the expected PeerId against the /p2p identity in the multiaddress. The session's authenticated identity is checked before the peer is accepted.
- Android request discovery does not auto-trust discovered entries. The UI uses a dedicated onboarding alert for the exact new PeerId, and a separate user action sends the request. Trust persists across subsequent requests.
- The ordinary Room conversation detail screen remains a local-message surface and does not display peer identity; the PeerId disclosure is in the distinct New Request / host-trust decision flow.

## نگاشت آزمون به معیار

| معیار | آزمون/منبع شواهد |
|---|---|
| هویت Android پایدار و fail-closed | `AndroidPeerIdentityStoreTest.kt` |
| enrollment، ذخیره و revoke در Android؛ رد tamper | `AndroidPeerTrustStoreTest.kt` |
| discovery بدون auto-trust و عدم ارسال تا تأیید | `RequestViewModelTest.kt` |
| trust gate، هویت احرازشده و رفتار revoke در transport | `Libp2pInferenceRequestTransportTest.kt` |
| هویت Windows با DPAPI و رد tamper | `WindowsHostIdentityStoreAcceptanceTests.cs` |
| ذخیره، رد ناشناخته/خراب، revoke دقیق و حذف آخرین رکورد | `PeerTrustStoreAcceptanceTests.cs` و `WindowsPeerTrustStoreAcceptanceTests.cs` |
| mismatch در multiaddress و رد reconnect پس از revoke | `PeerTransportContractAcceptanceTests.cs` |
| رضایت و توقف امن listener در میزبان | `test_network_listener_exposure.py` |
| مرز metadata-only rendezvous و جدایی گفت‌وگوی Room از request network | `test_update_transport_trust_boundary.py` و `ConversationDetailScreen.kt` |

## CI شواهد

- Quality gate: [42790619781](https://github.com/artinrahimii/project/actions/runs/42790619781) — تمام مراحل job از جمله Python unittest، compileall و چهار اجرای graph_guard موفق بودند.
- Product build: [42790619479](https://github.com/artinrahimii/project/actions/runs/42790619479)
  - HostLifecycle clean build، acceptance suite، coverage validation و upload موفق.
  - Android lint، debug/release، JVM tests، Keystore instrumentation، permission check، generated Protobuf و coverage validation موفق.
  - Windows WPF build/publish/startup smoke، DPAPI host identity/peer trust، installer build/acceptance موفق.
- Real local inference job: [42790619433](https://github.com/artinrahimii/project/actions/runs/42790619433) — یک آزمون مستقل inference با llama.cpp و GGUF واقعی موفق؛ این مورد جایگزین Android-to-Windows physical E2E نیست.
- Rendezvous container: [42790619836](https://github.com/artinrahimii/project/actions/runs/42790619836) — build و HTTP smoke موفق.

## محدودیت صریح

هیچ دستگاه فیزیکی Android و نصب واقعی Windows Host در این اجرای ممیزی کنترل نشده است؛ job مربوط به AndroidWindowsLanIntegrationTest یک fixture پاسخ deterministic دارد و طبق معیارهای پروژه مدرک real-model acceptance نیست. بنابراین هیچ ادعایی دربارهٔ end-to-end واقعی Android-to-Windows، پاسخ واقعی در همان گفت‌وگوی Room یا ارتقای درجا ارائه نمی‌شود. این موارد به‌ترتیب در `UPD-S1-020` و `UPD-S1-021` باقی می‌مانند.
