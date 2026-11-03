# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ SQLCipher و ادغام Room پیاده‌سازی شده‌اند، اما پذیرش runtime اختصاصی T0184 هنوز انجام نشده است.
- **پیاده‌سازی اولیه:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **آخرین CI بررسی‌شده:** `1e14ea896eaff3d8c24399156b8c79c7ba53308a` — `V 3.0.859`.
- **تلاش‌ها:** A001 تا A010؛ چهار تلاش CI ناموفق ثبت‌شده‌اند. A010 تنها یک اصلاح formatter است و هنوز CI نشده است.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`؛ T0184 از `AndroidKeyStoreProtectedBlobProtector` برای محافظت از کلید پایگاه استفاده می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`؛ selector و attempt budget در Room/SQLCipher مصرف نمی‌شوند. یال اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- `T0184 → T0185` همچنان `unreviewed` است؛ پیش از شروع T0185 باید بازبینی شود.

## پیاده‌سازی واقعی موجود

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ Room از `SupportOpenHelperFactory` استفاده می‌کند.
- `ClientDatabase.getInstance` از `EncryptedClientDatabaseFactory` باز می‌شود و migrationهای Room 1→2، 2→3 و 3→4 حفظ شده‌اند.
- برای هر نام پایگاه، passphrase تصادفی ۲۵۶ بیتی ساخته می‌شود؛ فقط envelope محافظت‌شده با Android Keystore در SharedPreferences خصوصی ذخیره می‌شود. alias کلید database مستقل از aliasهای peer identity/trust است.
- مهاجرت SQLite plaintext: WAL checkpoint، export با `sqlcipher_export()` به فایل موقت رمز‌شده، حفظ `user_version` و `application_id`، integrity check و سپس جایگزینی اتمی.
- اگر فایل رمز‌شده موجود باشد ولی envelope/کلید قابل بازیابی نباشد، factory fail-closed می‌شود و کلید تازه یا پایگاه جایگزین تولید نمی‌کند.
- چهار instrumentation test برای پایگاه تازه/reopen، مهاجرت plaintext و حفظ رکوردها، ذخیرهٔ envelope کلید، و fail-closed هنگام فقدان کلید نوشته شده‌اند.

## تاریخچهٔ آزمون و اصلاح

- **V3.0.854 / A002:** quality run `41686907108`؛ ۳۵۰ از ۳۵۳ آزمون موفق، سه assertion قدیمی محل ثبت migration/ADR ناموفق؛ در A003 اصلاح شدند.
- **V3.0.855 / A004:** quality run `41687478161` با ۳۵۳ آزمون موفق؛ container `41687478388` موفق؛ product `41687478163` به علت lint فایل instrumentation متوقف شد.
- **V3.0.856 / A006:** quality `41688070305` با ۳۵۳ آزمون موفق؛ container `41688070068` موفق؛ Android KtLint یک خطای import و هشت خطای chain در test source گزارش کرد.
- **V3.0.857 / A007:** quality `41688934109` موفق با ۳۵۳ آزمون؛ container `41688934150` موفق؛ Android KtLint یک خطای import در test source داشت.
- **V3.0.858 / A008:** quality `41689552284` و container `41689552377` موفق؛ Windows WPF و HostLifecycle موفق. test-source lint موفق شد، اما main-source KtLint سه ایراد در factory را گزارش کرد؛ APK/JVM/instrumentation اجرا نشدند.
- **V3.0.859 / A009:** quality `41691220553` با ۳۵۳ آزمون موفق؛ container `41691220755` موفق؛ Windows WPF و HostLifecycle موفق. test-source و بیشتر main-source lint موفق شدند؛ تنها خطای باقی‌مانده `EncryptedClientDatabaseFactory.kt:70:21 Unexpected newline before '.'` در زنجیرهٔ Room بود. بنابراین ساخت APK، JVM tests و چهار instrumentation test اجرا نشدند.
- **A010 — اصلاح فعلی:** `.openHelperFactory(openHelperFactory)` به خط بسته‌شدن `Room.databaseBuilder(...)` منتقل شد. این تغییر فقط formatting است و هیچ منطق runtime را تغییر نمی‌دهد. CI دقیق هنوز اجرا نشده است.

## فایل‌های تغییر این نوبت

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`: اصلاح newline در زنجیرهٔ Room.
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`
- `step three/project-graph.md`

## مرز پذیرش و محدودیت‌ها

چهار آزمون instrumentation مخصوص T0184 تا این لحظه هیچ اجرای موفقی ندارند. تا اجرای موفق exact-head، رمزگذاری runtime فایل Room، مهاجرت دادهٔ plaintext و رفتار fail-closed تأییدشده محسوب نمی‌شوند. موفقیت آزمون‌های Keystore در T0183 جایگزین این چهار آزمون نیست.

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های فلش و دستگاه روت‌شده خارج از دامنه‌اند.

**اقدام بعدی:** CI دقیق V3.0.860 باید KtLint کامل، ساخت APK، JVM tests و چهار سناریوی `connectedDebugAndroidTest` را اجرا کند. فقط پس از موفقیت معیارهای واقعی، T0184 به `done` تغییر می‌کند.
