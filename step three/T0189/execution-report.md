# T0189 — کد خطای پایدار و قابل‌نمایش

- **وضعیت:** در حال اجرا؛ نگاشت خطا در Windows و Android، parse پیام‌های خطای پروتکل، نمایش امن در UI و آزمون‌های رگرسیون اضافه شده‌اند؛ CI دقیق این تغییرات هنوز اجرا نشده است.
- **مبنای بررسی:** `artinrahimii/project`، شاخهٔ `main`، commit `42c872b6620abcd81a553213fa957f6846c4fbdd`، نسخهٔ `V 3.0.884`.
- **پیش‌نیاز:** T0188 در گراف همین commit `done` است و معیارهای مستندات/آزمون منبعی‌اش در CI دقیق نسخهٔ پیاده‌سازی تأیید شده‌اند.
- **بازبینی یال `T0188 → T0189`:** نوع اصلی `blocks` حفظ شد؛ طبقه‌بندی به `sequencing_only` با وضعیت `validated` تغییر می‌کند. T0188 مدل تهدید و محدودیت‌های محرمانگی را مستند می‌کند، اما T0189 هیچ رابط اجرایی از آن مصرف نمی‌کند. دلیل و شواهد در `edge_change_log` ثبت شده‌اند و ترتیب محافظه‌کارانهٔ گراف عوض نشده است.
- **قرارداد wire:** enum موجود `ProtocolErrorCode` در `protocol/whitep/v1/messages.proto` تغییر نکرده است. کدهای `INVALID_MESSAGE`، `UNSUPPORTED_VERSION`، `UNSUPPORTED_MODEL`، `HOST_BUSY`، `REQUEST_EXPIRED`، `RESOURCE_LIMIT` و `INTERNAL` به دسته‌های محصول نگاشت می‌شوند. قالب‌های موجود `ProtocolError`، `HostBusy` و `UnsupportedModel` در Android parse می‌شوند.
- **Windows:** `HostErrorCode` و `HostErrorPresentation.ToErrorCode` خطاهای شبکه، مدل، منابع، اعتبارسنجی، انقضا، مشغول‌بودن میزبان، لغو و خطای داخلی را به دسته‌های پایدار تبدیل می‌کنند. `FromProtocolCode` همان دسته‌بندی را برای کدهای پروتکل فراهم می‌کند. `ToUserMessage` فقط پیام ثابت و امن برمی‌گرداند؛ متن exception، URL و مسیر محلی را افشا نمی‌کند.
- **Android:** `RequestErrorCode` شامل نام wire و پیام ثابت هر دسته است؛ `RequestExecutionException` کد را بدون انتقال متن exception حفظ می‌کند. `RemoteRequestErrorParser` متن آزاد server/peer را نادیده می‌گیرد و فقط کد enum و correlation metadata را برمی‌گرداند. Transport پاسخ خطا را فقط برای request و peer احراز‌شده و در صورت وجود model مطابق attempt فعال می‌پذیرد.
- **رابط کاربری:** نتیجهٔ هر attempt کد پایدار اختیاری دارد؛ کارت نتیجه و نمایش وضعیت درخواست ذخیره‌شده، نام کد و پیام ثابت متناظر را نشان می‌دهند. خطاهای محلی Android نیز با همان mapper دسته‌بندی می‌شوند، نه با نمایش خام `exception.message`.
- **ذخیره‌سازی:** ستون موجود `RequestStateEntity.errorCode` بدون تغییر schema استفاده می‌شود. نام قدیمی `HOST_UNAVAILABLE` به `NETWORK_UNAVAILABLE` و `TIMEOUT/TIMED_OUT` به `REQUEST_EXPIRED` نگاشت می‌شوند؛ migration پایگاه لازم نیست.
- **آزمون‌های افزوده‌شده:** آزمون Windows برای نگاشت exception و عدم افشای جزئیات؛ آزمون Android برای همهٔ enumهای wire، دسته‌بندی exceptionها، parse سه پیام خطای واقعی، عدم اشتباه‌گرفتن InferenceResponse موفق با خطا و حفظ کد خطای typed در collector؛ آزمون منبع Python برای یکپارچگی قراردادها.
- **نتیجهٔ آزمون:** در محیط این نوبت checkout محلی و SDK اجرایی برای اجرای تست‌ها در دسترس نیست؛ نتایج محلی ادعا نمی‌شود. workflowهای exact-head در انتظار اجرا هستند.
- **گام بعدی:** workflowهای دقیق commit را بررسی کن؛ شکست‌های KtLint، C# build، Android compile/unit tests و graph/evidence guard را بر اساس log واقعی اصلاح کن. T0189 فقط پس از موفقیت گیت‌های مربوطه `done` می‌شود.


## بازبینی CI دقیق commit و تلاش اصلاحی A002

- **CI دقیق `V 3.0.885` / commit `2daa4f30826bdb547a5ae92d2b8957f9b212a1ef`:** Step Three quality gate run `41829441636` موفق شد؛ Rendezvous container run `41829441657` موفق شد؛ Windows WPF job `126572504180` موفق شد.
- **شکست Windows HostLifecycle:** job `126572504604` در آزمون `ParticipationActivationRecoveryTests.FailedRollbackAddsSafeRecoveryGuidanceAsync` متوقف شد. علت واقعی: نگاشت عمومی `IOException` به `RESOURCE_LIMIT` پیام عمومی امن fallback را به پیام منابع تبدیل کرده بود.
- **شکست Android:** job `126572504616` در KtLint، فایل `RequestErrorCodeTest.kt:3` را به‌دلیل ترتیب importها رد کرد؛ مرحله‌های APK، JVM و instrumentation در این job اجرا نشدند.
- **Real local inference integration:** run `41829441646` در همان آزمون HostLifecycle متوقف شد؛ سناریوی اجرای واقعی llama.cpp به مرحلهٔ acceptance خودش نرسید و بنابراین نتیجهٔ مستقلی دربارهٔ مدل به دست نیامد.
- **T0189-A002:** نگاشت عمومی `IOException` حذف شد تا خطای I/O ناشناخته به `INTERNAL` برگردد؛ آزمون صریح generic IOException اضافه شد. importهای Kotlin دست‌خورده مرتب شدند تا importهای `java.*` در انتهای بلوک قرار گیرند.
- **وضعیت پس از اصلاح:** `in_progress`. آزمون‌های اصلاحی هنوز روی commit جدید اجرا نشده‌اند؛ موفقیت T0189 ادعا نمی‌شود.


## بازبینی CI دقیق commit و تلاش اصلاحی A003

- **CI دقیق `V 3.0.886` / commit `29b468456290b5cecf672df4533f445d893ae216`:** Step Three quality gate run `41831155750` موفق؛ Rendezvous container run `41831155720` موفق؛ Windows WPF job `126578385121` موفق.
- **شکست HostLifecycle:** job `126578385038` از آزمون قدیمی T0047 با پیام `Invalid settings must map to safe guidance` شکست خورد. دسته‌بندی `ArgumentException` همچنان `VALIDATION_FAILED` می‌ماند، اما پیام کاربر باید راهنمای مشخص بررسی server URL و model ID را حفظ کند.
- **شکست Android:** job `126578384349` در KtLint، ترتیب importهای `ConversationDetailScreen.kt` را رد کرد؛ مراحل APK، JVM و instrumentation اجرا نشدند.
- **Real local inference integration:** run `41831155932` باینری pin‌شدهٔ llama.cpp و فایل GGUF را دریافت و اعتبارسنجی کرد، اما پیش از اجرای acceptance واقعی مدل، در همان آزمون T0047 متوقف شد. بنابراین موفقیت یا شکست inference واقعی هنوز اثبات نشده است.
- **T0189-A003:** پیام امن اختصاصی `ArgumentException` بازگردانده شد، در حالی‌که کد پایدار `VALIDATION_FAILED` حفظ می‌شود؛ آزمون پذیرش برای راهنمای تنظیمات و redaction افزوده شد و importهای `ConversationDetailScreen.kt` مرتب شدند.
- **وضعیت:** `in_progress` تا اجرای کامل CI دقیق این اصلاحات.


## بازبینی CI دقیق commit و تلاش قالب‌بندی A004

- **CI دقیق `V 3.0.887` / commit `9460217afc6bac4b0c0b253ef3947f85e0451c3b`:** Step Three quality gate run `41831823959` موفق؛ Rendezvous container run `41831823935` موفق؛ Windows WPF job `126580651445` موفق؛ HostLifecycle job `126580651477` موفق؛ Real local inference job `126580651317` موفق.
- **شاهد inference واقعی:** log job `126580651317` صریحاً `real local llama.cpp inference succeeded` را ثبت کرد؛ پاسخ واقعی دریافت شد، cancellation حین generation مشاهده شد و process تحت مالکیت برنامه تمیز متوقف شد.
- **شکست Android:** job `126580651061` تنها در `ktlintMainSourceSetCheck` شکست خورد. سه مورد دقیق: `RequestErrorCode.kt:61` ویرگول پایانی پیش از `;`، `Libp2pInferenceRequestTransport.kt:612` شروع expression چندخطی روی خط بعد، و `RequestViewModel.kt:880` قراردادن expression کوتاه در همان خط signature. APK، JVM tests و instrumentation به‌علت این gate اجرا نشدند.
- **T0189-A004:** همین سه مورد قالب‌بندی اصلاح شدند؛ هیچ منطق خطا، قرارداد یا assertion تغییر نکرد.
- **وضعیت:** `in_progress` تا اجرای lint و تمام مراحل build/test اندروید روی commit بعدی.
