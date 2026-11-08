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


## تلاش بعدی — اصلاح خطای traceability در V 3.1.42

- [Quality gate run 42465040801](https://github.com/artinrahimii/project/actions/runs/42465040801) روی SHA `0056553382a43fc21c0c74fe17341df571c0b80e` پس از اجرای ۴۷۲ آزمون با ۸ شکست زیرمجموعه‌ای در یک تست traceability پایان یافت.
- آزمون `test_explicit_peer_trust_decision_maps_to_persistent_store_transport_gate_and_acceptance` مسیرهای تاریخی `PeerTrustStoreAcceptanceTests.cs`, `WindowsPeerTrustStoreAcceptanceTests.cs`, `PeerTransportContractAcceptanceTests.cs`, `Program.cs`, `product-build.yml`, `test_decision_traceability.py` و دو عبارت baseline قدیمی را در ADR-0018 جست‌وجو می‌کند.
- نسخهٔ جدید ADR-0018 هر شش مسیر را در بخش traceability ثبت می‌کند و دو عبارت را فقط در بخش تاریخی V 3.0.443 و با تصریح منسوخ‌بودن ادعای نبود UI نگه می‌دارد. رفتار اجرایی Android/Windows تغییر نکرده است.
- نتیجهٔ CI اصلاحیهٔ V 3.1.43 هنوز pending است؛ UPD-S1-007 همچنان `in_progress` می‌ماند تا quality gate دقیق و product CI همان commit بررسی شوند.

## نتیجهٔ فعلی

- شکست quality در V 3.1.42: هشت assertion در یک آزمون traceability؛ هیچ شکست محصولی در این آزمون گزارش نشده است.
- Rendezvous container job V 3.1.42 موفق شد.
- Windows/Android/HostLifecycle product jobهای V 3.1.42 در زمان ثبت گزارش هنوز در حال اجرا بودند؛ تا مشاهدهٔ نتیجه، موفق یا ناموفق محسوب نمی‌شوند.


## نتیجهٔ نهایی تلاش V 3.1.43 — موفق

- [Step Three quality gate — run 42465146045](https://github.com/artinrahimii/project/actions/runs/42465146045): **success**؛ ۴۷۲ آزمون Python پاس شدند؛ آزمون traceability مربوط به ADR-0018 نیز پس از بازگردانی مسیرها و عبارات تاریخی پاس شد.
- [Product build — run 42465146051](https://github.com/artinrahimii/project/actions/runs/42465146051): **success**؛ jobهای Android، Windows WPF و HostLifecycle هر سه موفق شدند.
- [Rendezvous container — run 42465146137](https://github.com/artinrahimii/project/actions/runs/42465146137): **success**؛ build و smoke موفق شدند.

### Android

- debug و release variant با موفقیت ساخته شدند.
- کلید امضای موقت CI پس از ساخت release پاک شد.
- بررسی حداقل مجوزها موفق شد.
- Keystore instrumentation job کامل و موفق شد.
- JUnit validator گزارش داد: ۲۲۵ تست، ۲۲۴ موفق، صفر شکست، یک skipped.
- `AndroidWindowsLanIntegrationTest` skipped شد؛ بنابراین این نتیجه، آزمون فیزیکی Android-to-Windows نیست.
- Artifact IDs: APK `13611960371`، Android JUnit `13611179090`، Keystore instrumentation `13611148973`، Android coverage `13611148982`.

### Windows

- WPF build و startup smoke موفق شدند.
- ۶۶ فایل میزبان publish شدند.
- Installer با موفقیت compile، install، smoke-launch و uninstall شد و notices وابستگی/مدل را شامل شد.
- هشدار metadata مربوط به padding مقدار PE CompanyName ثبت شد؛ ثبت AppPublisher نصب‌شده جداگانه بررسی و تأیید شد.
- Artifact IDs: installer `13611675667`، host `13611052409`، publish `13610957706`.

### HostLifecycle و امنیت

- ۶۷ critical PASS، یک skip و ۸۵٫۷۵٪ line coverage.
- Secure-channel identity، رد plaintext downgrade، tamper/replay، correlation پاسخ، trust، disclosure حریم خصوصی، log امن، retention و cleanup آزموده شدند.
- Artifact: `13611013614`.

### checksum بسته‌های artifact

| Artifact | اندازهٔ ZIP | SHA-256 |
|---|---:|---|
| Android debug APK | 28,972,716 bytes | `6c8568797d0e2847ae9aa58820f7a307ea638f86d4957d4c83a887f0465ec960` |
| Windows installer | 60,986,640 bytes | `af32da4b84cc08a4247c932ab288c55176971ff4665e12e10c8b3d0cb456fe64` |
| Windows host | 101,568,007 bytes | `2383e46d36544229226a14970e9aabb745ade55c8444ec7e1ff9f007756038a4` |
| Windows publish | 10,020,703 bytes | `c350581e5793868b7dd0bb5ca70c4f4907b2824931effffe0827ce851b697852` |
| Android JUnit | 91,061 bytes | `f6d0de18195a8f2c6be5c346b9382f0a708e84e872ca7f89084ab30f0574abb3` |
| Android Keystore instrumentation | 83,657 bytes | `52bbca827b056a4b3308efe3f3605e64620bcdc30f55109518b23abd783d8df5` |
| Android coverage/critical | 5,377 bytes | `c9e0c53026eff0868accdd1e5e96c50d174b0d9012761db8d86affd2d05452fa` |
| HostLifecycle coverage/critical | 111,395 bytes | `5a0d708c0f1b6929a496cda6440888a158860bfb64aca4c73e87c34059027cbe` |

## مرز پذیرش باز

CI و emulator جایگزین Android فیزیکی و Windows فیزیکی نیستند. `AC-P2P-01` و `AC-PRIV-01` همچنان **NOT RUN** هستند؛ اتصال Chat عادی Room به runtime درخواست، بازگشت پاسخ به همان Conversation، clean install روی دستگاه هدف و in-place upgrade نیز در این تلاش اجرا نشدند.
