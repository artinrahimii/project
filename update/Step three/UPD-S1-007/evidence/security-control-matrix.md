# ماتریس شواهد UPD-S1-007

مبنای ممیزی کد: `3378be2187e2aaa1cd38ca0483c9d518de9d4887`. CI تأییدشده روی commit `380c89043d7e2cfce7bc50926419a1a682aaf0cb`.

## جریان داده

- Android request: `messages.proto`, `AndroidInferenceRequestRuntime.kt`, `Libp2pInferenceRequestTransport.kt`.
- Windows receive/parse/inference: `PeerAnnouncementHostLifecycle.cs`, `ProtocolMessageParser.cs`, `MainWindow.xaml.cs`, `HostInferenceSession`.
- مرز مشاهدهٔ میزبان: `ADR-0019-host-visible-inference-content.md`, `ADR-0002-direct-p2p-data-path.md`, `docs/security.md`.
- Rendezvous metadata-only؛ relay نسخهٔ اول غیرفعال. آزمون‌های `test_host_content_visibility.py` و `test_update_transport_trust_boundary.py` در quality gate پاس شدند.

## رضایت UI

- Android: پنجرهٔ privacy مختص PeerId قبل از `trustPeerAfterExplicitConfirmation`; trust بدون ارسال prompt؛ ارسال فقط پس از action جداگانهٔ Send.
- Windows: privacy warning با default No پیش از `new PeerAnnouncementHostLifecycle`; رد رضایت toggle را خاموش و lifecycle/listener/lease را ایجاد نمی‌کند.
- تمام چهار آزمون `test_peer_inference_privacy_disclosure.py` در run `42463288660` پاس شدند؛ HostLifecycle نیز marker رضایت میزبان را ثبت کرد.

## نگهداری و log

- Windows: `ProcessSessionOnly` پیش‌فرض؛ persistence اختیاری با رضایت مالک و DPAPI.
- Android: Room + SQLCipher برای history محلی؛ کلید با Android Keystore.
- Diagnostic API فقط allowlist عددی؛ prompt/response/capsule/secret در schema نیست.
- آزمون‌های `test_host_conversation_retention_policy.py`, `test_network_payload_logging.py`, `test_sensitive_artifact_scanning.py`, `test_sensitive_buffer_cleanup.py`, `ChatConversationAcceptanceTests` و `SafeDiagnosticLogAcceptanceTests` در CI موفق شدند.

## اعتماد، integrity، replay و کلید

- peer authenticity: canonical PeerId، explicit trust، libp2p Noise/TLS.
- replay: authenticated peer + requestId + attemptId تا expiry.
- integrity/correlation: request/attempt/round/model/host/deadline.
- Windows identity/trust: DPAPI CurrentUser و fail-closed tamper.
- Android identity/trust: aliasهای جدا و non-exportable Keystore AES-GCM.
- آزمون‌ها: `PeerMessageSecurityAcceptanceTests`, `WindowsHostIdentityStoreAcceptanceTests`, `WindowsPeerTrustStoreAcceptanceTests`, `AndroidKeyStorePersistenceTest`.

## CI

- Quality [42463288660](https://github.com/artinrahimii/project/actions/runs/42463288660): ۴۷۲ آزمون Python موفق.
- Product [42463288704](https://github.com/artinrahimii/project/actions/runs/42463288704): Android/Windows/HostLifecycle موفق.
- Android JUnit: ۲۲۴ موفق، صفر شکست، یک skipped؛ instrumentation artifact `13610099035`.
- HostLifecycle artifact `13610527167`: ۶۷ critical PASS و ۸۵٫۷۶٪ line coverage، یک skip.

## عدم ادعا

- آزمون گوشی فیزیکی Android با میزبان Windows انجام نشده است.
- packet capture، بررسی provider logs و ممیزی forensic سیستم‌عامل انجام نشده است.
- pagefile/swap، crash dump، backup، screenshot و ابزار ثالث خارج از تضمین برنامه باقی می‌مانند.
- `AC-P2P-01` و `AC-PRIV-01` همچنان NOT RUN هستند.


## CI failure and correction

- Exact V 3.1.42 quality run: [42465040801](https://github.com/artinrahimii/project/actions/runs/42465040801) — 472 tests ran; 8 subtest assertions failed inside `test_explicit_peer_trust_decision_maps_to_persistent_store_transport_gate_and_acceptance`.
- The source and runtime controls themselves were not the cause. The regression test requires the historical ADR to keep explicit paths to the portable trust test, Windows trust test, peer-transport test, test Program, product workflow and traceability test, plus two phrases documenting the original V 3.0.443 caller boundary.
- V 3.1.43 restores those references and phrases as historical evidence while explicitly stating that the former “no WPF prompt” claim is superseded by current code. Exact-head CI remains pending.


## Final exact-commit verification — V 3.1.43

- Quality [42465146045](https://github.com/artinrahimii/project/actions/runs/42465146045): **success**, 472 Python tests passed including ADR-0018 traceability.
- Product [42465146051](https://github.com/artinrahimii/project/actions/runs/42465146051): **success**, Windows, Android and HostLifecycle jobs all passed.
- Android JUnit summary: `tests=225, passed=224, failures=0, skipped=1`. The skipped test was `AndroidWindowsLanIntegrationTest`; no physical Android-to-Windows result is claimed.
- Android Keystore instrumentation job succeeded; the debug APK artifact and reports were uploaded.
- Windows installer install/smoke/uninstall and WPF startup smoke passed; 66 publish files were produced.
- HostLifecycle summary: 67 critical PASS markers, 1 skip, 85.75% line coverage.
- Rendezvous [42465146137](https://github.com/artinrahimii/project/actions/runs/42465146137): **success**.
