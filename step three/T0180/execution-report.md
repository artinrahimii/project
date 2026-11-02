# T0180 — پاک‌سازی payloadها و بافرهای موقت

**وضعیت:** `in_progress`؛ پیاده‌سازی A001 آمادهٔ اعتبارسنجی CI است.  
**مخزن/شاخه:** `artinrahimii/project` / `main`  
**نسخهٔ مبنا:** `1c329bd92f403ee380962dc7a237a3fce19bf84e` — `V 3.0.821`  
**تلاش:** `T0180-A001`  
**زمان شروع:** 2026-11-02T17:03:47+03:30

## بررسی پیش‌نیاز و یال‌ها

- `T0177` و `T0179` در گراف مبنا `done` بودند و خروجی/شواهد ثبت‌شدهٔ آن‌ها بررسی شد.
- یال‌های ورودی این وظیفه به `sequencing_only` اعتبارسنجی شدند؛ `type=blocks` و ترتیب اجرایی حفظ شدند.
- یال `T0180 → T0181` هنوز `unreviewed` است.

## تغییرات واقعی

- Windows: صفرکردن frame ارسالی، پیام ورودی در صورت شکست انتقال مالکیت به صف، پیام‌های باقی‌مانده در صف هنگام disposal، و بافرهای request/response در میزبان پس از پایان مصرف.
- Windows: افزودن helper برای پاک‌سازی آرایه‌های موقت پیام Protobuf مربوط به status/rejection/busy.
- Android: صفرکردن بافر قبلی، بافر ترکیبی و frameهای کامل decoder؛ پاک‌سازی وضعیت ناقص در پایان/بسته‌شدن stream و پاک‌سازی تمام frameها حتی اگر callback یکی را با خطا متوقف کند.
- افزودن آزمون JVM برای مشاهدهٔ واقعی backing array و آزمون‌های قراردادی Python.
- ثبت مرز تضمین و محدودیت‌ها در `docs/security.md`.

## فایل‌های تغییر‌یافته

- `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/PeerMessageFrameCodecTest.kt`
- `docs/security.md`
- `tests/test_sensitive_buffer_cleanup.py`
- `step three/project-graph.md`

## آزمون و پذیرش

آزمون‌های جدید اضافه شده‌اند، اما CI دقیق commit هنوز اجرا/بررسی نشده است. به همین دلیل T0180 در وضعیت `in_progress` می‌ماند و هیچ نتیجهٔ موفق آزمون ادعا نمی‌شود. معیار پذیرش نهایی، موفقیت Step Three quality gate، Android lint/compile/APK/JVM، Windows WPF/HostLifecycle و container checks روی همان commit است.

## محدودیت

صفرشدن کپی‌های بایتی تحت مالکیت مستقیم برنامه ادعا می‌شود؛ صفرشدن objectهای immutable، کپی‌های runtime، بافرهای داخلی کتابخانه/سیستم‌عامل، pagefile/swap یا crash dump اثبات نمی‌شود. frame خروجی Android تا تکمیل/شکست عملیات نوشتن حفظ می‌شود تا دادهٔ درحال‌ارسال تغییر نکند.


## تلاش T0180-A002 — اصلاح قالب آزمون و بازبینی اجرا

**بازهٔ اجرا:** 2026-11-02T17:23:00+03:30 تا 2026-11-02T17:29:27+03:30  
**وضعیت:** `partial`؛ `T0180` همچنان `in_progress` است.

- یک خط خالی تکراری پیش از آزمون `zeroesThePreviousPendingBuffer` در فایل آزمون Android حذف شد تا قالب با سبک رسمی KtLint مخزن هماهنگ بماند.
- فایل‌های منبع و آزمون در commit مبنای `b14a8c311cd98422c5b15ddb9089d24f3170edc8` بازبینی شدند؛ هیچ تغییر رفتاری در این تلاش ادعا نمی‌شود.
- آزمون Python، KtLint، Android build/JVM، Windows build/HostLifecycle یا CI دقیق این commit در این محیط اجرا نشد. clone مخزن به‌علت خطای DNS انجام نشد؛ .NET SDK و Gradle/Android SDK نیز در محیط محلی موجود نیستند.
- GitHub برای SHA مبنا status check برنگرداند؛ ابزار موجود برای فهرست workflow runها فقط اجراهای PR را برمی‌گرداند و نتیجه‌ای نشان نداد. بنابراین هیچ آزمونی موفق یا ناموفق اعلام نمی‌شود.

**اقدام بعدی:** اجرای quality gate و workflowهای واقعی Windows/Android/HostLifecycle روی commit جدید؛ تنها پس از موفقیت شواهد، وضعیت به `done` تغییر کند.

## تلاش T0180-A003 — بستن stream در timeout/cancellation نوشتن Android

**زمان ثبت:** 2026-11-02T18:14:10+03:30  
**وضعیت:** partial؛ وظیفه همچنان in_progress است.

### یافته و اصلاح واقعی

- بازبینی دقیق نشان داد CompletableFuture.get(timeout) در cancelAttempts صرفاً انتظار فراخوان را timeout می‌کند و لزوماً Promise نوشتن Netty را لغو نمی‌کند. مسیر قبلی در TimeoutException یا interruption فقط false برمی‌گرداند؛ در نتیجه callback پاک‌سازی frame ممکن بود تا پایان نامعلوم Promise اجرا نشود.
- مسیر Android اکنون در CancellationException، TimeoutException و InterruptedException، stream مربوط را می‌بندد تا نوشتن معلق شکست بخورد و callback موجود sendRequest(...).whenComplete پاک‌سازی frame را اجرا کند. در interruption، وضعیت interrupt thread نیز برگردانده می‌شود.
- مستند تابع به‌روزرسانی شد: پاسخ CANCELLED/پاسخ مسابقه‌ای فقط بعد از write موفق مرجع می‌ماند؛ اگر خود write پیش از تکمیل لغو یا timeout شود، بسته‌شدن stream مسیر شکست را قطعی می‌کند.
- آزمون قراردادی Python برای حفاظت از سه شاخهٔ timeout/cancel/interruption و ارتباط آن‌ها با closeStream و whenComplete اضافه شد.

### آزمون‌های اجراشده

- کامپایل مستقل فایل تولیدی PeerMessageFrameCodec.kt با Kotlin/JVM 1.9.0 و JDK 21: موفق.
- harness اجرایی decoder: ۸ از ۸ سناریو موفق؛ شامل مرز طول‌های varint، frame تکه‌تکه، چند frame، پیام خالی، طول بیش‌ازحد، prefix غیر canonical/بیش‌ازحد بلند، EOF ناقص و صفرشدن بافر قبلی/ناقص.
- ممیزی predicateهای منبع روی فایل‌های دقیق commit مبنا: ۱۱ از ۱۱ شرط موجود.
- آزمون Gradle/JUnit برنامهٔ Android، کل Python unittest suite، build ویندوز و CI دقیق این commit در این محیط اجرا نشده‌اند. نتیجهٔ harness مستقل، جایگزین آن‌ها نیست.

### محدودیت باقی‌مانده

تأیید end-to-end بسته‌شدن stream و تکمیل Promise واقعی Netty در timeout هنوز به آزمون Android integration/CI نیاز دارد. آزمون‌های Windows، کل برنامه و workflowهای CI نیز هنوز لازم‌اند؛ بنابراین T0180 تکمیل‌شده اعلام نمی‌شود.
