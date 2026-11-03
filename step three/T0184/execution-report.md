# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ پیاده‌سازی SQLCipher و ادغام Room موجود است، اما آزمون‌های runtime اختصاصی T0184 هنوز اجرا نشده‌اند.
- **پیاده‌سازی اولیه:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **آخرین commit بررسی‌شده:** `5a0e424c391dfd2f3665bf8d84ae462188287e58` — `V 3.0.856`.
- **تلاش‌ها:** A001 پیاده‌سازی، A002 شکست سه قرارداد قدیمی، A003 اصلاح قراردادها، A004 شکست KtLint در V855، A005 اصلاح ناقص KtLint، A006 شکست دوبارهٔ KtLint در V856، A007 اصلاح کامل‌تر formatter در انتظار CI.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`. T0184 کلید دیتابیس را با `AndroidKeyStoreProtectedBlobProtector` wrap می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`. selector و attempt budget در Room/SQLCipher مصرف نمی‌شوند؛ یال اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- `T0184 → T0185` همچنان `unreviewed` است.

## پیاده‌سازی موجود

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ Room 2 از `SupportOpenHelperFactory` استفاده می‌کند.
- `ClientDatabase.getInstance` از `EncryptedClientDatabaseFactory` باز می‌شود و migrationهای Room 1→2، 2→3 و 3→4 حفظ می‌شوند.
- passphrase تصادفی ۲۵۶ بیتی per-database؛ فقط envelope محافظت‌شده با Android Keystore در SharedPreferences خصوصی ذخیره می‌شود. alias کلید database از aliasهای identity و trust مستقل است.
- مهاجرت پایگاه plaintext قدیمی: WAL checkpoint، export از طریق `sqlcipher_export()` به فایل رمز‌شدهٔ موقت، حفظ `user_version` و `application_id`، integrity check و سپس rename اتمی.
- اگر پایگاه رمز‌شده وجود داشته باشد اما envelope/کلید قابل بازیابی نباشد، factory fail-closed می‌شود و کلید تازه یا فایل جایگزین تولید نمی‌کند.
- چهار instrumentation test برای پایگاه تازه/reopen، مهاجرت plaintext، envelope کلید و fail-closed با کلید مفقود نوشته شده‌اند.

## تاریخچهٔ آزمون‌ها و اصلاح‌ها

- **A002 — V3.0.854:** quality run `41686907108`، ۳۵۳ آزمون؛ ۳۵۰ موفق و سه شکست در assertionهای قدیمی محل ثبت migration و افشای baseline ADR. A003 این قراردادها را اصلاح کرد.
- **A004 — V3.0.855:** quality run `41687478161` موفق شد؛ ۳۵۳ آزمون Python، graph guard، compileall و شواهد موفق. container run `41687478388` نیز موفق بود. Product run `41687478163` در Android KtLint متوقف شد، زیرا alias import انتهای فهرست نبود؛ APK، JVM و instrumentation اجرا نشدند.
- **A005 — V3.0.856:** اصلاح اولیهٔ import و بخشی از chain formatting در فایل آزمون ثبت شد.
- **A006 — V3.0.856:** quality run `41688070305` با ۳۵۳ آزمون موفق، container run `41688070068` موفق، Windows WPF و HostLifecycle موفق؛ اما Product run `41688070251` در Android KtLint شکست خورد. لاگ job `126099017248` یک خطای import-order و هشت خطای chain-method-continuation را در `EncryptedClientDatabaseAcceptanceTest.kt` نشان داد. پس از lint، `assembleDebug`، JVM tests و چهار instrumentation tests اجرا نشدند.
- **A007 — V3.0.857 در انتظار CI:** importها طبق `ktlint_official` مرتب شده‌اند: imports عمومی با ترتیب الفبایی، سپس `java`، `kotlin` و در پایان alias. زنجیره‌های SharedPreferences، Room و DAO بازچینی شده‌اند تا dotهای multiline هم‌تراز باشند. این تغییر فقط قالب‌بندی test source است و کد runtime را تغییر نمی‌دهد.

## تغییر فایل این تلاش

- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`: اصلاح import layout و تراز chain continuation.
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`
- `step three/project-graph.md`

## مرز پذیرش

چهار آزمون instrumentation مخصوص T0184 هنوز حتی یک اجرای موفق ثبت‌شده ندارند. تا اجرای موفق exact-head، نه رمزگذاری واقعی Room، نه مهاجرت دادهٔ plaintext و نه رفتار کلید مفقود پذیرفته‌شده محسوب نمی‌شود. موفقیت آزمون‌های T0183 جایگزین آن‌ها نیست.

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های فلش و دستگاه روت‌شده خارج از دامنه‌اند.

**اقدام بعدی:** CI دقیق V3.0.857 باید KtLint، ساخت APK، JVM tests و `connectedDebugAndroidTest` چهار سناریوی SQLCipher را اجرا کند. فقط در صورت موفقیت، T0184 به `done` تبدیل می‌شود.
