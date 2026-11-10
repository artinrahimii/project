# گزارش اجرای UPD-S1-010 — اتصال چت به چرخهٔ ارسال و دریافت

## وضعیت
**در حال اجرا (in_progress)** — تلاش A001 اصلاح نخست کد و آزمون Room را اعمال می‌کند؛ نتیجهٔ CI دقیق این commit هنوز دریافت نشده است.

## مبنای Git
- مخزن: `artinrahimii/project`
- شاخهٔ مقصد: `main`
- مبنای کد ممیزی‌شده: `cf380af27d462eba973574bac6c2aec467901a28` (`V 3.1.164`)
- پیام commit مبنا: `V 3.1.164`

## شکاف احرازشده
صفحهٔ گفت‌وگوی فعلی فقط پیام کاربر را در Room ثبت می‌کرد و صریحاً اعلام می‌کرد ارسال به میزبان در دسترس نیست. RequestViewModel و runtime واقعی موجود بودند، اما در مسیر گفت‌وگوی عادی فراخوانی نمی‌شدند. برای جزئیات به `evidence/chat-runtime-gap-audit.md` مراجعه شود.

## تغییرات کد این تلاش
- `MainActivity.kt`: URL دایرکتوری و رضایت حریم خصوصی در تنظیمات برنامه نگهداری می‌شوند و به گفت‌وگوی موجود منتقل می‌شوند؛ مسیر ناوبری جدید یا چت بازنویسی نشده است.
- `ConversationDetailScreen.kt`: RequestViewModel مستقل برای همان conversation با Room lifecycle repository ساخته می‌شود؛ دایرکتوری در صورت تنظیم‌شدن کشف می‌شود؛ همتاها در رابط عادی با عنوان عمومی نمایش داده می‌شوند؛ trust و رضایت حریم خصوصی پیش از ارسال لازم‌اند؛ پیام از مسیر `RequestViewModel.sendRequest(prompt)` به runtime واقعی فرستاده می‌شود.
- وضعیت انتخاب مدل، کشف میزبان، اتصال/پردازش، شکست، لغو و retry در همان صفحه نمایش داده می‌شود.
- پاسخ موفق و خطای typed با request/attempt correlation در همان جدول Room مکالمه ذخیره می‌شوند.
- `ConversationMessagesViewModel.kt`: ذخیرهٔ پاسخ assistant و اعلان خطای system با شناسهٔ deterministic و جلوگیری از duplicate اضافه شد.
- `ConversationMessagesViewModelTest.kt`: آزمون Room/Robolectric برای ترتیب پیام کاربر، پاسخ و خطا و idempotency اضافه شد.

## فایل‌های تغییر‌یافته
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationMessagesViewModelTest.kt`
- `update/Step three/UPD-S1-010/execution-report.md`
- `update/Step three/UPD-S1-010/evidence/chat-runtime-gap-audit.md`

## آزمون‌ها
- آزمون جدید `persistsAssistantResponseAndTypedErrorInTheSameConversationExactlyOnce` اضافه شد؛ هنوز اجرا نشده است.
- Android JVM tests، Android build، CI و UI tests روی exact commit این تلاش هنوز اجرا نشده‌اند.
- آزمون فیزیکی Android-to-Windows اجرا نشده است.

## محدودیت‌ها و ادامه
- وضعیت این وظیفه done نیست.
- باید CI دقیق اجرا شود، خطاهای کامپایل/آزمون اصلاح شوند و مصرف‌کنندگان مستقیم و غیرمستقیم request/runtime و navigation دوباره بررسی شوند.
- بعد از آزمون‌های UI و integration مناسب، معیارهای پذیرش تک‌به‌تک ارزیابی خواهند شد.
- آزمون P2P فیزیکی و مسیر real inference Android-to-installed-Windows در UPD-S1-020 همچنان معیار مستقل است.


## تلاش A002 — اصلاح قراردادهای آزمون پس از تغییر مسیر چت

- مبنای A002: `0ec848f0573de18449bf01f1b80e0352ab692620` (V 3.1.165)
- نتیجهٔ A001: quality gate به‌دلیل ۷ assertion قدیمی که اتصال normal chat به runtime را ممنوع می‌کردند شکست خورد؛ Android job نیز در `ktlintMainSourceSetCheck` متوقف شد، بنابراین APK و آزمون‌های Android اجرا نشدند.
- آزمون‌های قرارداد منبع اصلاح شدند تا رفتار مصوب جدید را بسنجند: اتصال ConversationDetailScreen به RequestViewModel، ذخیرهٔ Room، پاسخ/خطای هم‌بسته، consent/trust، حفظ اعتبارسنجی پیام و مسیر مستقیم P2P. تست‌های context capsule همچنان تضمین می‌کنند که پیشنهاد خلاصه فقط پیش‌نویس محلی است و خود capsule ViewModel شبکه ایجاد نمی‌کند.
- ADR-0002 به‌روزرسانی شد تا اتصال منبعی چت به runtime موجود را ثبت کند، اما وضعیت physical Android-to-Windows را همچنان NOT RUN نگه دارد.
- خطوط تازه‌افزوده‌شدهٔ متن UI برای رفع شکست قالب‌بندی Kotlin بازآرایی شدند.
- A002 تا اجرای مجدد quality gate، ktlint، build اندروید و unit testها `partial` است. وضعیت وظیفه `in_progress` باقی می‌ماند.


## تلاش A003 — رفع آخرین assertion قدیمی وضعیت چت

- مبنا: `V 3.1.166`، commit `f87926d9906cf0c0e6b9718fa81a40d8faad9464`
- CI تلاش A002: ۴۹۲ آزمون Python اجرا شد؛ فقط یک assertion در `test_android_request_status_contract.py` به‌دلیل انتظار عنوان قدیمی `Request status` شکست خورد. آزمون‌های status اکنون باید وجود و نمایش ردیف‌های Room و وضعیت زندهٔ پردازش را بررسی کنند.
- آزمون HostLifecycle در همان build موفق شد.
- در A003 assertion عنوان قدیمی حذف شد و دو assertion مستقیم برای مشاهدهٔ ردیف‌های ذخیره‌شدهٔ Room و `requestStatusCard` جایگزین آن شد.
- نتیجهٔ CI دقیق A003 هنوز pending است؛ این وظیفه همچنان `in_progress` است.


## تلاش A004 — اصلاح آزمون وضعیت و ثبت گزارش تشخیصی KtLint

- مبنا: `V 3.1.167`، commit `66149a232d17363f08ac1269f07b22ced14fb5a6`.
- assertion قدیمی عنوان `Request status` حذف شد؛ آزمون‌های پیشین وجود جریان `requestStates` و تابع `requestStatusCard` را مستقیماً کنترل می‌کنند.
- workflow محصول یک مرحلهٔ `always()` اضافه دارد که گزارش `ktlintMainSourceSetCheck.txt` را در log چاپ می‌کند؛ شکست `ktlintCheck` همچنان build را متوقف می‌کند.
- CI دقیق این commit هنوز شروع نشده؛ A004 و UPD-S1-010 در وضعیت `partial/in_progress` باقی می‌مانند.


## تلاش A005 — اصلاح دقیق خطاهای KtLint

- مبنا: `V 3.1.168`، commit `084cdf7013a42a2f15c1acec4510590843285267`.
- log دقیق KtLint پنج مورد را گزارش کرد: import-ordering، دو argument-list-wrapping، max-line-length و chain-method-continuation.
- این پنج مورد در `ConversationDetailScreen.kt` و `ConversationMessagesViewModel.kt` اصلاح شدند؛ هیچ قاعدهٔ lint غیرفعال نشد و workflow همچنان روی `ktlintCheck` شکست می‌خورد تا تمام قواعد پاس شوند.
- نتیجهٔ lint، کامپایل و unit testهای این تلاش پس از اجرای CI دقیق ثبت خواهد شد؛ UPD-S1-010 هنوز `in_progress` است.


## تلاش A006 — ذخیرهٔ چرخهٔ درخواست با مالکیت همان گفت‌وگو

- مبنا: `V 3.1.169`، commit `4bcf7ce1d043f6894227ee5adf8b07dd2d5e7be1`.
- نقص جدید تأیید شد: repository موجود `RoomInferenceRequestLifecycleRepository` برای صفحهٔ مستقل New request از جدول app-level و فاقد `conversationId` استفاده می‌کرد؛ بنابراین نمی‌توانست وضعیت هر درخواست چت را به همان گفت‌وگو نسبت دهد و بازیابی یک مکالمه ممکن بود وضعیت مکالمهٔ دیگری را نشان دهد.
- کلاس `RoomConversationInferenceRequestLifecycleRepository` اضافه شد که همان قرارداد lifecycle را روی DAO موجود `request_states` و `conversationId` جاری اجرا می‌کند.
- DAO اکنون آخرین وضعیت را فقط در همان conversation بازیابی و فقط درخواست‌های درحال‌اجرای همان conversation را هنگام شروع مجدد به EXPIRED تبدیل می‌کند.
- صفحهٔ گفت‌وگو از repository جدید استفاده می‌کند؛ صفحهٔ مستقل New request همچنان repository قدیمی و global خودش را حفظ می‌کند.
- آزمون Robolectric جدید با دو گفت‌وگوی مستقل اثبات می‌کند recover/expire کردن Conversation A، وضعیت Conversation B را تغییر نمی‌دهد.
- در همین تلاش دو خطای chain-method-continuation آخرین گزارش KtLint نیز اصلاح شدند.
- نتیجهٔ CI دقیق A006 هنوز pending است؛ وظیفه `in_progress` باقی می‌ماند.


## تلاش A007 — اصلاح آزمون حساس به قالب‌بندی و خطای KtLint

- مبنا: `V 3.1.170`، commit `68cae864c567238fe831cbda12abb4182a9c3e23`.
- CI همین مبنا در run `42862592160` تعداد ۴۹۲ آزمون Python اجرا کرد و فقط یک آزمون شکست خورد: assertion آزمون چت به‌صورت literal انتظار `UUID.nameUUIDFromBytes(` داشت، درحالی‌که KtLint کد معتبر را چندخطی کرده بود.
- در run `42862592314`، KtLint فقط یک خط خالی غیرضروری در `InferenceRequestLifecycleRepository.kt:91` گزارش کرد؛ به همین دلیل APK build و آزمون‌های Android بعدی اجرا نشدند.
- jobهای Windows WPF و HostLifecycle در همان product run موفق بودند؛ smoke سرویس rendezvous نیز در run `42862592458` موفق شد.
- assertion UUID به regex مستقل از فاصله و شکست خط تبدیل شد؛ آزمون lifecycle که قبلاً خارج از کلاس و پس از `unittest.main()` قرار داشت، داخل کلاس قرار گرفت و خواندن فایل‌ها به `Path.read_text(encoding="utf-8")` تغییر کرد تا آزمون واقعاً کشف شود.
- خط خالی اضافی گزارش‌شده توسط KtLint حذف شد؛ هیچ قاعدهٔ lint غیرفعال و هیچ قرارداد runtime/protocol تغییر نکرد.
- شواهد تشخیصی: `update/Step three/UPD-S1-010/evidence/ci-failure-analysis-A007.md`.
- نتیجهٔ CI دقیق A007 تا زمان ثبت این commit pending است؛ UPD-S1-010 همچنان `in_progress` است و آزمون فیزیکی Android-to-Windows انجام نشده است.
