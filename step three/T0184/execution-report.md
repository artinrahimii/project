# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ پیاده‌سازی SQLCipher و ادغام Room موجود است، اما آزمون‌های runtime اختصاصی T0184 هنوز اجرا نشده‌اند.
- **commit پیاده‌سازی:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **آخرین commit CI بررسی‌شده:** `361ac9d1468602486b09256fc4c0c3419a30d046` — `V 3.0.857`.
- **تلاش‌ها:** A001 پیاده‌سازی؛ A002 شکست سه قرارداد قدیمی؛ A003 اصلاح قراردادها؛ A004 شکست KtLint در V855؛ A005 اصلاح اولیه؛ A006 شکست KtLint در V856؛ A007 اصلاح chainها و باقی‌ماندن یک import؛ A008 اصلاح ترتیب همان importها، در انتظار CI.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`؛ T0184 کلید پایگاه را با `AndroidKeyStoreProtectedBlobProtector` wrap می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`؛ selector و attempt budget در Room/SQLCipher مصرف نمی‌شوند. نوع اصلی یال `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- `T0184 → T0185` هنوز `unreviewed` است.

## پیاده‌سازی واقعی موجود

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ Room 2 از `SupportOpenHelperFactory` استفاده می‌کند.
- `ClientDatabase.getInstance` از `EncryptedClientDatabaseFactory` باز می‌شود و migrationهای Room 1→2، 2→3 و 3→4 حفظ شده‌اند.
- passphrase تصادفی ۲۵۶ بیتی per-database؛ فقط envelope محافظت‌شده با Android Keystore در SharedPreferences خصوصی ذخیره می‌شود. alias کلید database از aliasهای identity و trust مستقل است.
- مهاجرت پایگاه plaintext قدیمی: WAL checkpoint، export از طریق `sqlcipher_export()` به فایل رمز‌شدهٔ موقت، حفظ `user_version` و `application_id`، integrity check و سپس rename اتمی.
- اگر پایگاه رمز‌شده وجود داشته باشد اما envelope/کلید قابل بازیابی نباشد، factory fail-closed می‌شود و کلید تازه یا فایل جایگزین تولید نمی‌کند.
- چهار instrumentation test برای پایگاه تازه/reopen، مهاجرت plaintext، envelope کلید و fail-closed با کلید مفقود نوشته شده‌اند.

## تاریخچهٔ آزمون‌ها

- **V3.0.854 / A002:** quality run `41686907108`، ۳۵۳ آزمون؛ ۳۵۰ موفق و سه شکست در assertionهای قدیمی محل ثبت migration و افشای baseline ADR. A003 قراردادها را اصلاح کرد.
- **V3.0.855 / A004:** quality run `41687478161` با ۳۵۳ آزمون موفق؛ container run `41687478388` موفق؛ product run `41687478163` به‌علت import alias در KtLint متوقف شد.
- **V3.0.856 / A006:** quality run `41688070305` با ۳۵۳ آزمون موفق؛ container run `41688070068` موفق؛ Windows WPF و HostLifecycle موفق؛ Android KtLint با یک خطای import و هشت خطای chain continuation شکست خورد.
- **V3.0.857 / A007:** quality run `41688934109` با ۳۵۳ آزمون موفق؛ container run `41688934150` موفق؛ Windows WPF و HostLifecycle موفق. KtLint اکنون همهٔ خطاهای chain را پاک‌شده گزارش کرد، ولی یک خطای import-order در خط ۳ باقی ماند. Gradle ساخت APK، JVM tests و instrumentation را اجرا نکرد.
- **A008 — در انتظار CI:** `kotlinx.coroutines` پیش از `org.junit` منتقل شد. علت این ترتیب، تنظیم `ktlint_code_style = ktlint_official` در `.editorconfig` است: گروه عمومی باید الفبایی باشد؛ سپس گروه‌های `java` و `kotlin` و در پایان importهای alias می‌آیند. تغییر A008 فقط import order است.

## تغییر فایل این تلاش

- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`: انتقال دو import مربوط به `kotlinx.coroutines` به گروه عمومی الفبایی پیش از `org.junit`. تغییر runtime یا منطق دیتابیس ندارد.
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`
- `step three/project-graph.md`

## مرز پذیرش

چهار آزمون instrumentation T0184 هنوز هیچ اجرای موفقی ندارند. تا اجرای موفق exact-head، رمزگذاری واقعی Room، مهاجرت دادهٔ plaintext و رفتار fail-closed پذیرفته‌شده محسوب نمی‌شوند. موفقیت Keystore tests مربوط به T0183 جایگزین این آزمون‌ها نیست.

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های فلش و دستگاه روت‌شده خارج از دامنه‌اند.

**اقدام بعدی:** CI دقیق V3.0.858 باید KtLint، ساخت APK، JVM tests و `connectedDebugAndroidTest` چهار سناریوی SQLCipher را اجرا کند. فقط در صورت موفقیت، T0184 به `done` تبدیل می‌شود.
