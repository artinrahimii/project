# گزارش اجرای UPD-S1-016 — انتقال امن P2P و خطای صریح اتصال مستقیم

- مخزن: `artinrahimii/project`
- شاخه: `main`
- مبنای کد: `4c09319b29d0b1fcc57a78851d552e01f8ac9d74` — `V 3.1.177`
- وضعیت تلاش A001: **partial / in_progress**
- شروع تلاش: `2026-11-10T20:23:30Z`
- نسخهٔ هدف این اصلاح: `V 3.1.178` (تا زمان commit، نسخهٔ بعدی از تاریخچه استخراج شده بود)

## ممیزی پیاده‌سازی موجود

قابلیت سالم بازنویسی نشد. موارد زیر در commit مبنا بررسی شدند:

- Android: `Libp2pInferenceRequestTransport`، `AndroidPeerIdentityStore`، `AndroidPeerTrustStore`، `RendezvousPeerDirectoryClient` و `AndroidInferenceRequestRuntime`.
- Windows: `IPeerTransport`، `Libp2pPeerTransport`، `PeerTransportFallbackConnector` و `PeerAnnouncementHostLifecycle`.
- Trust: اعتماد صریح PeerId در Android و DPAPI-backed trust store و تأیید owner در Windows.
- قرارداد: `/white-p/peer-message/1.0.0`، Protobuf مشترک، unsigned-varint frame در stream libp2p و validation هم‌بستگی request/attempt/model/host.
- relay: ADR-0004 صریحاً relay data path را برای first release غیرفعال می‌کند؛ Windows در transport از `.WithRelay()` استفاده نمی‌کند و Android آدرس `/p2p-circuit` را رد می‌کند.

## شکاف احرازشده و اصلاح

مسیر Android هنگام شکست اتصال مستقیم، خطا را با مقدار `InferenceTransportSubmission.REJECTED` برمی‌گرداند. dispatcher آن را به `SUBMISSION_REJECTED` و collector آن را به `VALIDATION_FAILED` تبدیل می‌کرد. بنابراین socket refusal/timeout یا نبود peer در directory به‌جای خطای قابل‌فهم شبکه، به‌صورت خطای اعتبارسنجی دیده می‌شد.

اصلاحات این تلاش:

1. `InferenceTransportSubmission.FAILED` از `REJECTED` جدا شد.
2. dispatcher مقدار FAILED را به `InferenceAttemptSubmissionStatus.FAILED` نگاشت می‌کند.
3. transport در نبود endpoint، انقضای lease، نبود direct TCP address و شکست اتصال مستقیم، outcome امن FAILED می‌دهد.
4. mismatch هویت endpoint، PeerId غیرcanonical، آدرس نامعتبر، trust ردشده و خطاهای authentication/validation همچنان REJECTED می‌مانند.
5. collector، `SUBMISSION_FAILED` را به `RequestErrorCode.NETWORK_UNAVAILABLE` تبدیل می‌کند؛ متن کاربرمحور موجود می‌گوید اتصال به میزبان یا directory برقرار نشده و پیشنهاد می‌کند اتصال بررسی و دوباره تلاش شود.
6. diagnostic فقط stage و نوع exception را ثبت می‌کند؛ متن prompt، response، IP/آدرس یا کلید ذخیره نمی‌شود.
7. آزمون واقعی loopback به یک TCP پورت بسته اضافه شد تا failure طبقه‌بندی‌شده و عدم افشای prompt/نشانی بررسی شود.
8. آزمون dispatcher و collector نیز تفاوت REJECTED و FAILED را تثبیت می‌کنند.

## تصمیم معماری و محدوده

- Android: direct TCP + Noise از کتابخانهٔ libp2p نگهداری‌شده؛ relay multiaddress رد می‌شود.
- Windows: direct authenticated libp2p TCP/QUIC برای یک PeerId موردانتظار؛ fallback فقط بین آدرس‌های مستقیم همان هویت و با بررسی identity انجام می‌شود.
- Rendezvous: فقط metadata discovery/control plane؛ prompt/response/history به آن ارسال نمی‌شود.
- Relay first release: تصویب نشده و فعال نمی‌شود؛ آزمون سه‌peer relay در این وظیفه applicable نیست و ادعای اجرای آن وجود ندارد.
- رمزنگاری اختصاصی اضافه نشده است؛ Noise/secure channel و PeerId handling به کتابخانه‌های نگهداری‌شده واگذار شده‌اند.
- ADR-0024 همچنان قرارداد listener قدیمی loopback با prefix چهار‌بایتی را توصیف می‌کند؛ مسیر P2P Android/.NET نسخهٔ جداگانهٔ ADR-0021 با unsigned-varint framing دارد.
- ADR-0021، ADR-0016، ADR-0005، ADR-0024 و جدول تصمیم‌ها به‌روزرسانی شدند تا وضعیت واقعی UI اعتماد و CI fixture-to-Windows .NET interop را منعکس کنند؛ محدودیت آزمون فیزیکی همچنان برقرار است.

## فایل‌های تغییر‌یافته

- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundResponseCollectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`
- `tests/test_android_libp2p_transport_contract.py`
- `tests/test_decision_traceability.py`
- `docs/decisions/ADR-0005-authenticated-peer-transport-fallback.md`
- `docs/decisions/ADR-0016-maintained-libp2p-cryptographic-transport.md`
- `docs/decisions/ADR-0021-android-libp2p-peer-transport.md`
- `docs/decisions/ADR-0024-tcp-request-framing.md`
- `docs/decisions/README.md`

## آزمون‌ها

این تلاش هنوز روی commit جدید ساخته/اجرا نشده است؛ بنابراین هیچ تست جدیدی PASS اعلام نمی‌شود. در محیط محلی نیز checkout شبکه‌ای موجود نیست و build محلی Android/.NET انجام نشده است. تست‌های exact-head از GitHub Actions پس از ثبت commit بررسی خواهند شد.

شواهد CI قبلی Android در V 3.1.176 شامل 234 تست موفق، صفر failure و یک تست LAN non-loopback skip شده بود؛ این فقط baseline interop است و جایگزین CI اصلاح جدید نیست.

## موارد خارج از معیار این وظیفه

- Android گوشی فیزیکی به Windows نصب‌شدهٔ واقعی کاربر: در این تلاش انجام نشده.
- شبکهٔ عمومی/NAT traversal: اثبات نشده.
- relay byte path: تصویب نشده و اجرا نشده.
- نصب پاک/in-place upgrade: متعلق به UPD-S1-021 است.
- آزمون Android-to-Windows روی دستگاه هدف و persistence در همان Room: متعلق به UPD-S1-020 است.

تا CI دقیق commit اصلاحی، UPD-S1-016 در وضعیت `in_progress` می‌ماند.


## تلاش A002 — اصلاح فایل آزمون پیش از CI

- مبنا: `fb47c4c8287bc231fbc809e3aad614da3a05e173` — `V 3.1.178`.
- در بازخوانی blob دقیق commit، خط پایان assertion قبلی و فراخوانی بعدی `assertEquals` به‌علت newline مفقود به یک خط متصل بودند.
- اصلاح: جداکردن دو statement در `InferenceRoundResponseCollectorTest.kt`.
- این کشف حاصل بررسی source بود، نه نتیجهٔ شکست تست؛ job Android هنگام این کشف هنوز به JVM tests نرسیده بود.
- تا اجرای مجدد CI روی commit A002، هیچ نتیجهٔ آزمون جدیدی PASS اعلام نمی‌شود و وظیفه `in_progress` می‌ماند.


## نتیجهٔ CI تلاش A002 — V 3.1.179

- Python quality: **۴۹۶ تست اجرا شد؛ ۶ مورد شکست خورد**. یک assertion هنوز نام قدیمی آزمون dispatcher را می‌خواست؛ سه assertion نام فایل‌های تست Android/Keystore را در فهرست تصمیم‌ها پیدا نکردند؛ یک assertion نام فایل پروژهٔ Shared.Security را در فهرست پیدا نکرد؛ و یک assertion retry-budget نیز نام قدیمی آزمون را انتظار داشت.
- Android ktlint: شکست با دو تخلف `spacing-between-declarations-with-comments` در اطراف دو comment enum جدید. به‌علت این شکست، unit test و Keystore instrumentation در این job اجرا نشدند.
- Windows WPF/build/installer: موفق.
- HostLifecycle: موفق.
- Rendezvous: موفق.
- هیچ نتیجهٔ شکست‌خورده حذف نشده و A002 در تاریخچه با نتیجهٔ `failed` حفظ شده است.

## تلاش A003 — اصلاح شکست‌های دقیق CI

- دو خط خالی لازم قبل از commentهای مستندکنندهٔ enum اضافه شد.
- نام آزمون در دو Python contract به نام واقعی تغییر یافتهٔ `keepsSubmittingOtherHostsAfterTransportRejectsOrFails` همگام شد.
- README تصمیم‌ها به‌روزرسانی شد تا تمام basenameهای مورد انتظار ردیابی شامل `AndroidKeyStorePersistenceTest.kt`، `AndroidPeerIdentityStoreTest.kt`، `AndroidPeerTrustStoreTest.kt` و `WhiteP.Shared.Security.csproj` را ذکر کند.
- وضعیت A003 فعلاً partial است؛ CI روی commit این اصلاح هنوز اجرا نشده است.


## نتیجهٔ CI تلاش A003 — V 3.1.180

- Python quality: ۴۹۶ تست اجرا شد و فقط یک assertion باقی‌مانده شکست خورد: `test_one_round_dispatch_times_out_instead_of_replaying_a_host_submission` هنوز `assertEquals(3, transport.requests.size)` را انتظار داشت، در حالی که آزمون dispatcher پس از اضافه‌شدن حالت FAILED اکنون چهار candidate را بررسی می‌کند.
- Android lint و ساخت fixture ویندوز .NET در job Android موفق شدند؛ JVM/Keystore/APK در لحظهٔ ثبت این گزارش هنوز ادامه داشت.
- HostLifecycle و Rendezvous موفق شدند.
- Windows product job در لحظهٔ ثبت هنوز در حال اجرا بود.

## تلاش A004 — اصلاح آخرین assertion کیفیت

- تعداد درخواست موردانتظار در `tests/test_request_retry_budget_boundary.py` از ۳ به ۴ اصلاح شد تا با آزمون dispatcher به‌روز همسان شود.
- این تلاش فقط اصلاح assertion قراردادی است؛ نتیجهٔ آزمون exact-head هنوز pending است.


## نتیجهٔ CI دقیق V 3.1.181

- Commit آزموده‌شده: `7c95d5d8c817ac5d82c4aa63d65b3df436a8b0e7`.
- Step Three quality gate: **PASS**، run [42896931063](https://github.com/artinrahimii/project/actions/runs/42896931063).
- Rendezvous container build/smoke: **PASS**، run [42896931302](https://github.com/artinrahimii/project/actions/runs/42896931302).
- Windows WPF: **PASS**؛ clean build، publish، startup smoke، هویت/اعتماد DPAPI، بررسی agent-tool و ساخت و پذیرش installer؛ job [130112373952](https://github.com/artinrahimii/project/actions/runs/42896931040/job/130112373952).
- HostLifecycle unit/integration/coverage: **PASS**؛ job [130112373643](https://github.com/artinrahimii/project/actions/runs/42896931040/job/130112373643).
- Android ktlint، ساخت clean debug/release APK و بررسی permissionها: **PASS**.
- Android JVM: **FAIL**؛ ۲۲۵ آزمون اجرا شد و یک آزمون شکست خورد. JUnit artifact شمارهٔ `13802330493` با SHA-256 برابر `546dea024dd25a107c56a5445a446a3a5d003d7cc23978069418c0cbedf4faf8` دریافت و خوانده شد.
- علت: آزمون `keepsSubmittingOtherHostsAfterTransportRejectsOrFails` چهار host را در یک round می‌فرستاد؛ dispatcher سقف سه host در هر round دارد و پیش از dispatch نتیجهٔ `TOO_MANY_HOSTS` برمی‌گرداند. شکست در assertion موفق‌بودن dispatch رخ داد؛ این آزمون از سقف compatibility عبور کرده بود.
- Keystore instrumentation، اجرای emulator، آپلود APK و coverage Android به‌علت توقف در JVM **اجرا نشدند**.

## تلاش A005 — اصلاح آزمون در محدودهٔ قرارداد سه‌میزبانه

- شروع: `2026-11-10T21:18:09.123Z`.
- تصمیم: سقف production یک میزبان در هر round و سقف generic compatibility سه میزبان بدون تغییر می‌ماند؛ پیاده‌سازی dispatcher دست‌کاری نمی‌شود.
- آزمون اصلی اکنون حداکثر سه host دارد: rejection، exception/failure و سپس queued؛ بنابراین ادامهٔ dispatch در سقف مجاز بررسی می‌شود.
- آزمون مستقل `continuesAfterExplicitTransportFailureAndQueuesNextHost` رفتار outcome صریح `FAILED` و ارسال candidate بعدی را بررسی می‌کند.
- دو Python source-contract نیز با سقف واقعی سه‌تایی و نام آزمون جدید هماهنگ شدند.
- نتیجهٔ CI دقیق commit اصلاحی هنوز **اجرا نشده** است؛ A005 تا اجرای مجدد کیفیت و Product Build در وضعیت `in_progress` می‌ماند.
