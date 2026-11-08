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
