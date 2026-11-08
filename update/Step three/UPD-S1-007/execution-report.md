# گزارش اجرای UPD-S1-007 — بازنگری مدل امنیت و حریم خصوصی

- مبنای کد: `3378be2187e2aaa1cd38ca0483c9d518de9d4887` — `V 3.1.41`
- مبنای CI دقیق موجود: `380c89043d7e2cfce7bc50926419a1a682aaf0cb` — `V 3.1.40`
- عملیات گراف: `modify`؛ کد و کنترل‌های سالم حفظ شدند و یک مغایرت واقعی سندی اصلاح شد.
- وضعیت تلاش جاری: `in_progress` تا اجرای quality/product CI پس از اصلاح ADR.

## جریان داده و نقطهٔ اعتماد

1. Android درخواست `InferenceRequest` شامل prompt و بایت‌های context capsule می‌سازد.
2. پیام از طریق کانال مستقیم libp2p با secure channel احرازشدهٔ Noise/TLS به PeerId انتخابی می‌رود.
3. Windows پس از رمزگشایی، پیام را parse و validate می‌کند. فرایند میزبان می‌تواند prompt و capsule را بخواند و درخواست را به backend inference محلی بدهد.
4. پاسخ `InferenceResponse` از همان peer path برمی‌گردد و با شناسه‌های request/attempt/round/model/host/deadline تطبیق داده می‌شود.
5. Rendezvous در معماری مصوب فقط metadata کشف همتا شامل PeerId، نشانی، capability و expiry را نگه می‌دارد؛ prompt، response و history در payload آن نیست.
6. relay برای نسخهٔ اول غیرفعال است و شکست مسیر مستقیم نباید موجب ارسال payload به rendezvous یا relay دیگر شود.

## هشدار و رضایت

- Android برای PeerId جدید، پیش از ذخیرهٔ trust، پنجرهٔ «Before you trust this host» را با شناسهٔ همان peer نمایش می‌دهد. متن هشدار، مشاهدهٔ prompt/context توسط میزبان و احتمال بررسی/نگهداری آن را توضیح می‌دهد.
- تأیید trust هیچ promptی ارسال نمی‌کند؛ ارسال به اقدام جداگانهٔ کاربر با دکمهٔ Send وابسته است. trust قابل revoke است.
- Windows پیش از ساخت/شروع peer host، برای PeerId مشخص پنجرهٔ shared privacy notice می‌گیرد؛ پاسخ پیش‌فرض No است. رد رضایت toggle را خاموش می‌کند و listener/lease ایجاد نمی‌شود. تأیید هویت PeerId مرحله‌ای جداگانه است.
- trust ذخیره‌شده در اجراهای بعدی قابل استفاده است؛ هشدار Android در enrollment همتای جدید نمایش داده می‌شود، نه برای هر درخواست.

## Log و نگهداری

- API تشخیصی Android و .NET فقط کدهای enum ثابت، outcome و duration عددی می‌پذیرد؛ prompt، response، capsule، شناسه‌های درخواست/همتا، کلید و متن exception در allowlist نیست.
- نگهداری متن در Windows به‌طور پیش‌فرض `ProcessSessionOnly` است. ذخیرهٔ پایدار خاموش و نیازمند رضایت جداگانهٔ مالک است؛ فایل اختیاری با DPAPI(CurrentUser) محافظت می‌شود.
- Android تاریخچه را عمداً در Room نگه می‌دارد و با SQLCipher رمزگذاری می‌کند؛ کلید پایگاه با Android Keystore محافظت می‌شود. این مسیر از history میزبان و rendezvous جدا است.
- frameها و bufferهای تحت مالکیت برنامه در lifecycleهای معین پاک می‌شوند؛ این ادعا شامل VM/GC، kernel، pagefile، swap یا crash dump نیست.

## نگاشت کنترل به آزمون

| حوزه | کنترل | آزمون |
|---|---|---|
| احراز هویت همتا | canonical PeerId، explicit trust، libp2p Noise/TLS | `PeerTransportContractAcceptanceTests`, `PeerSecurityPolicyAcceptanceTests`, `WindowsPeerTrustStoreAcceptanceTests` |
| تمامیت/دستکاری | bounded parser و response correlation برای request/attempt/round/model/host/deadline | `PeerMessageSecurityAcceptanceTests`, `test_host_content_visibility.py` |
| replay | کلید authenticated-peer + requestId + attemptId با expiry؛ fail-closed در ظرفیت | `ReplayGuardRejectsRepeatedAuthenticatedRequestAttempts` |
| کلیدهای Windows | DPAPI CurrentUser و رد tamper/corruption | `WindowsHostIdentityStoreAcceptanceTests`, `WindowsPeerTrustStoreAcceptanceTests` |
| کلیدهای Android | alias مجزا، Keystore non-exportable AES-GCM | `AndroidKeyStorePersistenceTest` |
| عدم ثبت محتوای حساس | allowlist عددی و اسکن sinkهای log | `test_network_payload_logging.py`, `SafeDiagnosticLogAcceptanceTests`, `SafeDiagnosticLogTest.kt` |
| بافر و فایل موقت | zeroization و cleanup | `test_sensitive_buffer_cleanup.py`, `test_sensitive_artifact_scanning.py`, `ChatConversationAcceptanceTests` |
| افشای حریم خصوصی | هشدار Android و consent میزبان Windows | `test_peer_inference_privacy_disclosure.py`, `HostLifecycle.Tests/Program.cs` |

## CI دقیق مبنا

- Quality [42463288660](https://github.com/artinrahimii/project/actions/runs/42463288660): ۴۷۲ آزمون Python، همگی موفق؛ شامل privacy disclosure، content visibility، log scans، retention، threat model، artifact scanning و buffer cleanup.
- Product [42463288704](https://github.com/artinrahimii/project/actions/runs/42463288704): build/testهای Android، Windows و HostLifecycle موفق.
- HostLifecycle: ۶۷ critical PASS، یک skip، ۸۵٫۷۶٪ line coverage؛ شامل secure-channel mismatch، جلوگیری از plaintext downgrade، tamper، replay، response correlation، trust، DPAPI، disclosure و retention.
- Android JUnit: ۲۲۴ passed، صفر failed، یک skipped؛ Keystore instrumentation artifact `13610099035`.
- این نتایج runner/emulator هستند و آزمون فیزیکی Android-to-Windows محسوب نمی‌شوند.

## مغایرت سندی و اصلاح

ADR-0018 هنوز می‌گفت WPF UI پنجرهٔ enrollment ندارد؛ کد فعلی رضایت privacy و سپس تأیید exact PeerId را پیش از ساخت lifecycle نمایش می‌دهد. ADR-0018 به رفتار واقعی به‌روز شد و وضعیت V 3.0.443 فقط به‌عنوان سابقهٔ تاریخی حفظ شد.

## محدودیت

`AC-P2P-01` و `AC-PRIV-01` همچنان NOT RUN هستند تا Android↔Windows فیزیکی، بررسی مسیر ترافیک و ممیزی مستقل نگهداری/log اجرا شوند. CI، emulator و source-contract جایگزین این پذیرش نیستند.
