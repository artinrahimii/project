# T0187 — آزمون اسکن log و فایل‌های موقت برای prompt، capsule و اسرار

- **وضعیت:** در حال اجرا؛ تست‌های runtime و source-contract افزوده شده‌اند، اما CI دقیق commit هنوز اجرا/تأیید نشده است.
- **مبنای بررسی:** مخزن `artinrahimii/project`، شاخه `main`، commit `e528d6d9e9daeb5a4c9540dfefe7d136770b90af`، نسخهٔ `V 3.0.876`.
- **پیش‌نیازها:** `T0186` و `T0177` هر دو `done` بودند و خروجی‌های واقعی و آزمون‌هایشان بررسی شدند.
- **بازبینی یال‌ها:**
  - `T0186 → T0187`: `sequencing_only` / `validated`; type اصلی `blocks` و ترتیب حفظ شد. T0186 رابط رضایت می‌سازد، اما T0187 از API/خروجی آن استفاده نمی‌کند.
  - `T0177 → T0187`: `sequencing_only` / `validated`; type اصلی `blocks` و ترتیب حفظ شد. آزمون‌های log و فایل موقت هیچ قرارداد selector/attempt-budget را مصرف نمی‌کنند.
  - یال خروجی `T0187 → T0188` بدون بررسی باقی ماند.
- **Windows temp/history runtime:** آزمون یک پوشهٔ موقت اختصاصی می‌سازد؛ ابتدا وجود canaryهای ساختگی prompt/capsule/secret را با scanner تأیید می‌کند، سپس `WindowsHostConversationStore` فایل stale را پاک می‌کند و نبود marker در همهٔ فایل‌های باقی‌مانده را بررسی می‌کند. تست DPAPI نیز تاریخچهٔ دارای همان markerها را ذخیره و reload می‌کند و نبود marker روشن در فایل‌های کنترل‌شده و نبود `*.tmp` پس از atomic move را می‌سنجد.
- **Windows logs:** آزمون EventSource payload عددی را همچنان بررسی می‌کند و خروجی message/payload را برای سه canary حساس جست‌وجو می‌کند.
- **Android database temp:** آزمون instrumentation مهاجرت Room/SQLCipher، markerهای ساختگی prompt/capsule/secret را در دادهٔ legacy قرار می‌دهد؛ پس از مهاجرت، فایل DB و sidecarهای WAL/SHM و فایل `.sqlcipher-migrating` در صورت وجود اسکن می‌شوند؛ فایل موقت مهاجرت نیز باید حذف شده باشد.
- **Android logs:** آزمون JVM خروجی formatter را با canaryهای حساس مقایسه می‌کند و حفظ schema عددی را می‌سنجد.
- **Static regression:** فایل Python جدید ترتیب Protect-before-temp و SQLCipher-export-before-rename را به کد اجرایی واقعی متصل می‌کند؛ آزمون موجود `test_network_payload_logging.py` همچنان log sinkهای شناخته‌شده را اسکن می‌کند.
- **محدودیت:** اسکن فقط روی فایل‌های برنامه و پوشه‌های آزمون کنترل‌شده است. pagefile/swap، crash dump، backup، screenshot، Event Viewer خارج از EventSource تعریف‌شده و logهای نرم‌افزار ثالث خارج از این تست‌اند.
- **CI:** هیچ نتیجهٔ pass برای commit جدید ادعا نمی‌شود؛ Windows HostLifecycle، Android KtLint/JVM/instrumentation و Python quality/graph guard باید روی exact-head اجرا شوند.
- **گام بعدی:** بر اساس نتیجهٔ واقعی CI اصلاح کن؛ تنها پس از پاس‌شدن همهٔ گیت‌های مرتبط، T0187 را done کن.


## A001 — نتیجهٔ CI نسخهٔ V 3.0.877 و اصلاح آزمون

- Quality run `41818883061`، job `126536652274`: از ۳۷۷ آزمون Python، یک شکست داشت. آزمون `test_windows_eventsource_runtime_check_covers_sensitive_canaries` عبارت کوتاه `payload is not { Count: 3 }` را جست‌وجو می‌کرد؛ متن واقعی C# برابر `record.Payload is not { Count: 3 } payload` است. این شکست mismatch در assertion منبع بود و نه افشای canary در اجرای EventSource.
- HostLifecycle job `126536651616` در product run `41818883046` موفق شد؛ بنابراین آزمون runtime مربوط به Windows temp/DPAPI و EventSource اجرا و پاس شده است.
- Rendezvous container run `41818882949` موفق شد. WPF و Android jobهای product run هنگام ثبت شکست هنوز در حال اجرا بودند؛ نتیجهٔ آن‌ها را موفق فرض نمی‌کنیم.
- A002 فقط متن مورد انتظار source-contract را با expression دقیق C# هم‌راستا می‌کند؛ هیچ کنترل runtime یا معیار امنیتی حذف/تضعیف نمی‌شود. کیفیت exact-head نسخهٔ بعد باید دوباره پاس شود.
- وضعیت T0187: `in_progress`؛ یک تلاش failed و یک تلاش اصلاحی partial ثبت شده است.

 
## A003 — اصلاح capsule واقعی و گسترش ممیزی فایل موقت/log

- marker مربوط به capsule به‌جای عنوان گفتگو، اکنون در `ConversationContextCapsule.keyFacts` قرار می‌گیرد؛ با `encode()` به JSON معتبر UTF-8 تبدیل و از طریق `ConversationContextCapsuleEntity.serializedCapsule` در Room ذخیره می‌شود. پس از export با SQLCipher، آزمون ردیف capsule را از DAO می‌خواند، decode می‌کند و marker را از همان فیلد تأیید می‌کند.
- آزمون منبعی برای writerهای فایل موقت HostIdentityStore، PeerTrustStore، LocalHostSettingsStore و VerifiedModelInstaller افزوده شد. دو writer امنیتی envelope محافظت‌شده را می‌نویسند؛ تنظیمات سند غیرمکالمه‌ای است؛ فایل دانلود مدل فقط staging محتوای مدل است و hash/format-check می‌شود.
- اسکن sinkهای log به کل `src/Host.Windows` گسترش یافت؛ آزمون اکنون وجود sink قابل‌اسکن را الزام می‌کند و `SerializedCapsule` را در فیلدهای حساس می‌شناسد.
- نتایج exact-head نسخهٔ `V 3.0.878`: quality gate، Windows WPF، HostLifecycle، Android client (شامل JVM و Keystore instrumentation)، و Rendezvous همگی موفق شدند.
- کدهای A003 هنوز در CI اجرا نشده‌اند؛ وضعیت وظیفه تا تأیید workflowهای exact-head بعدی `in_progress` می‌ماند.
- زمان ثبت: 2026-11-04T11:22:07.732Z


## A004 — اصلاح KtLint در تست instrumentation اندروید

- CI دقیق commit `9b646018e4ec4d11b898aac5ade2c3ce9fa09af1` (`V 3.0.879`) quality gate، Rendezvous و jobهای Windows WPF و HostLifecycle را با موفقیت گذراند.
- Android job `126541644002` در step `Verify Kotlin formatting and lint` شکست خورد. خروجی KtLint دو مورد `A multiline expression should start on a new line` را در خطوط 111 و 184 فایل `EncryptedClientDatabaseAcceptanceTest.kt` گزارش کرد. به همین دلیل build APK، JVM و instrumentation در همین run اجرا نشدند.
- A004 فقط assignmentهای `capsule` و `migratedCapsule` را مطابق قالب چندخطی Kotlin تنظیم می‌کند؛ هیچ assertion یا شرط امنیتی حذف نشده است.
- نتیجهٔ A004 تا CI exact-head جدید `partial` است.
- زمان ثبت: 2026-11-04T11:26:21.387Z


## A005 — اصلاح دقیق تورفتگی‌های KtLint

- CI دقیق `V 3.0.880` در job Android `126543645782` دوباره در `ktlintAndroidTestSourceSetCheck` شکست خورد؛ این بار پنج گزارش تورفتگی برای پارامترهای دو expression چندخطی ثبت شد (خطوط 113 تا 115 و 187 تا 188).
- KtLint انتظار داشت پارامترهای فراخوانی چهار فاصله از خط فراخوانی جلوتر و پرانتز بسته با خط فراخوانی هم‌تراز باشد. تورفتگی همین خطوط اصلاح شد؛ کد و منطق تست تغییری نکرد.
- KtLint شکست قبلی را به‌درستی ثبت کرده؛ A005 تا اجرای مجدد دقیق CI `partial` باقی می‌ماند.
- زمان ثبت: 2026-11-04T11:29:25.911Z
