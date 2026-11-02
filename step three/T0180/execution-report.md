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

## تلاش T0180-A004 — رفع شکست واقعی CI و هشدار nullable در صف Windows

**زمان ثبت:** 2026-11-02T18:27:13+03:30  
**وضعیت:** partial؛ T0180 همچنان in_progress است.

### یافته‌های CI روی V 3.0.824

- Step Three quality gate، اجرا 41465625302: موفق؛ 338 آزمون Python، compileall و graph_guard با 230 وظیفه/422 یال، صفر خطا و یک هشدار metadata قدیمی T0001.
- Rendezvous container، اجرا 41465625258: build و smoke test موفق.
- White-P product build، اجرا 41465625430: شکست فقط در گام KtLint Android؛ Windows WPF build و smoke، fixture بین‌عملیاتی .NET و HostLifecycle tests موفق بودند. Android APK، JVM unit tests و generated Protobuf verification به‌دلیل توقف KtLint اجرا نشدند.
- خطای دقیق KtLint: Libp2pInferenceRequestTransport.kt:521:5 — Expected a blank line for this declaration. log کامل job 125345607621 این محل را مشخص کرد.
- build ویندوز یک هشدار CS8600 در حلقهٔ Dispose فایل Libp2pPeerMessageProtocol.cs، خط 197 گزارش کرد؛ build شکست نخورده بود.

### اصلاحات واقعی این تلاش

- خط خالی موردنیاز KtLint پیش از ensureHostStarted اضافه شد.
- فاصلهٔ خالی استاندارد پیش از guard اصلی فایل تست Python افزوده شد.
- حلقهٔ تخلیهٔ صف Windows اکنون nullable بودن خروجی TryRead را صریح اعلام می‌کند و فقط وقتی pendingMessage دارای Length مثبت است ZeroMemory را اجرا می‌کند؛ این اصلاح هشدار CS8600 را هدف می‌گیرد.
- آزمون قراردادی Python برای قرارداد nullable و guard صف به‌روز شد.

### اعتبارسنجی

- نتایج CI بالا روی commit مبنای V 3.0.824 هستند، نه روی اصلاحات A004. هیچ نتیجهٔ KtLint یا build برای این اصلاحات هنوز ادعا نمی‌شود.
- اجرای بعدی CI باید KtLint، Android assembleDebug، testDebugUnitTest، Protobuf verification، Windows build بدون هشدار nullable و HostLifecycle را روی SHA دقیق جدید تأیید کند.

## تلاش T0180-A005 — اعتبارسنجی دقیق V 3.0.825

- Step Three quality gate run 41467491212: 338 آزمون Python و compileall موفق؛ graph_guard ناموفق با `FAILED_ATTEMPT_COUNT_MISMATCH: T0180: summary=1, history=0`.
- Product workflow run 41467491153 در تلاش نهایی سوم موفق شد. Android KtLint، ساخت APK، JVM unit tests، بررسی generated Protobuf و upload artefactها در تلاش دوم موفق شدند. Windows WPF build، startup smoke، DPAPI/trust و upload artefactها نیز موفق شدند.
- HostLifecycle در تلاش اول و دوم با بسته‌شدن peer stream هنگام انتظار برای HostStatus/InferenceResponse شکست خورد؛ تلاش سوم همان job موفق شد و نتیجهٔ نهایی product workflow success بود.
- Rendezvous container run 41467491150: build و non-root HTTP smoke موفق.
- هیچ‌یک از این نتایج، شکست graph_guard را برطرف نمی‌کرد؛ A005 آن را در تاریخچهٔ افزایشی به‌عنوان یک تلاش ناموفق ثبت می‌کند.

## تلاش T0180-A006 — اصلاح سازگاری شمارنده و تاریخچه

- علت دقیق guard failure: execution_summary.failed_attempt_count برابر ۱ بود، در حالی که هیچ رکورد execution_history با result=failed وجود نداشت.
- رکورد A005 نتیجهٔ واقعی validation شکست‌خوردهٔ V 3.0.825 را ثبت می‌کند؛ A006 این مغایرت metadata را با حفظ سوابق قبلی اصلاح می‌کند.
- تعداد تلاش‌ها اکنون ۶ و تعداد تلاش‌های ناموفق ۱ است؛ فقط A005 نتیجهٔ failed دارد. وضعیت T0180 همچنان in_progress است.
- commit بعدی باید quality gate را روی SHA دقیق اجرا کند؛ این اصلاح به‌تنهایی اثبات قبولی graph_guard نیست.

## تلاش T0180-A007 — شکست‌های دقیق CI در V 3.0.826

- Step Three quality gate run 41469076873 موفق: 338 آزمون Python، compileall و graph_guard؛ 230 وظیفه/422 یال، صفر خطا و یک هشدار metadata قدیمی T0001.
- Rendezvous container run 41469076680 موفق.
- Windows WPF job 125357569545 موفق.
- Android KtLint و assembleDebug موفق؛ در `testDebugUnitTest` تعداد 196 آزمون اجرا شد و فقط یک آزمون شکست خورد: `sendsCancellationToThreeNativeDotNetLibp2pHostsAndReceivesCancelledResponses`. گزارش JUnit artifact 13153164724 نشان می‌دهد هر سه fixture میزبان مراحل CANCEL_RECEIVED، VALIDATED، SENT و PASS را ثبت کردند، ولی Android قبل از دریافت پاسخ با بسته‌شدن stream مواجه شد. fixture .NET بلافاصله پس از ارسال پاسخ خارج می‌شد.
- HostLifecycle job 125357569172 شکست خورد؛ پاسخ دریافت‌شده در assertion خط 293 با قرارداد request/attempt/round/peer/outcome/output تطبیق نداشت. پیام قبلی فیلد ناسازگار را مشخص نمی‌کرد.
- این تلاش به‌عنوان failed در تاریخچه افزوده شده؛ T0180 تکمیل‌شده نیست.

## تلاش T0180-A008 — اصلاح چرخهٔ عمر fixture و تشخیص فیلد پاسخ

- fixture سه‌میزبانهٔ .NET پس از ارسال پاسخ دیگر بلافاصله خارج نمی‌شود؛ تا بسته‌شدن stream از سوی Android منتظر می‌ماند. Android در `awaitResponse` پس از دریافت/پردازش پاسخ stream را می‌بندد؛ این handshake تست از race میان ارسال پاسخ و disposal میزبان جلوگیری می‌کند.
- assertion تست Windows HostLifecycle اکنون فقط booleanهای تطبیق request/attempt/round/peer/model/output و وضعیت outcome/deadline را گزارش می‌کند؛ شناسه‌ها یا متن واقعی prompt ثبت نمی‌شوند.
- این تغییرات فقط test fixture و diagnostics را اصلاح می‌کنند؛ هنوز هیچ موفقیت CI برای commit جدید ادعا نمی‌شود.
