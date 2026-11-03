# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ SQLCipher و Room ادغام شده‌اند، اما آزمون‌های runtime اختصاصی T0184 هنوز اجرا نشده‌اند.
- **پیاده‌سازی اولیه:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **آخرین commit CI بررسی‌شده:** `e7affffd1c05736338c2caf0f1149baaec38b207` — `V 3.0.858`.
- **تلاش‌ها:** A001 پیاده‌سازی؛ A002 شکست سه قرارداد قدیمی؛ A003 اصلاح قراردادها؛ A004 شکست test-source KtLint؛ A005 اصلاح اولیه؛ A006 شکست KtLint با chainها؛ A007 اصلاح chainها و import؛ A008 قبولی test-source lint و شکست main-source lint؛ A009 اصلاح factory، در انتظار CI.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`؛ T0184 از `AndroidKeyStoreProtectedBlobProtector` برای wrap کلید پایگاه استفاده می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`؛ selector و attempt budget در Room/SQLCipher مصرف نمی‌شوند. نوع اصلی یال `blocks` و ترتیب محافظه‌کارانه حفظ شده است.
- `T0184 → T0185` همچنان `unreviewed` است؛ این یال پیش از شروع T0185 باید بررسی شود.

## پیاده‌سازی واقعی موجود

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ Room 2 از `SupportOpenHelperFactory` استفاده می‌کند.
- `ClientDatabase.getInstance` از `EncryptedClientDatabaseFactory` باز می‌شود و migrationهای Room 1→2، 2→3 و 3→4 حفظ شده‌اند.
- passphrase تصادفی ۲۵۶ بیتی per-database ساخته می‌شود؛ فقط envelope محافظت‌شده با Android Keystore در SharedPreferences خصوصی ذخیره می‌شود. alias کلید database از aliasهای peer identity و trust مستقل است.
- مهاجرت plaintext: WAL checkpoint، export از طریق `sqlcipher_export()` به فایل رمز‌شدهٔ موقت، حفظ `user_version` و `application_id`، integrity check و سپس rename اتمی.
- در صورت وجود پایگاه رمز‌شده و فقدان envelope/کلید قابل بازیابی، factory fail-closed می‌شود و کلید تازه یا فایل جایگزین تولید نمی‌کند.
- چهار سناریوی instrumentation برای ساخت/reopen پایگاه، مهاجرت plaintext، envelope کلید و fail-closed با کلید مفقود نوشته شده‌اند.

## تاریخچهٔ اجرا و CI

- **V3.0.854 / A002:** quality run `41686907108`؛ ۳۵۰ از ۳۵۳ آزمون موفق، سه assertion قدیمی محل migration/ADR ناموفق؛ در A003 اصلاح شدند.
- **V3.0.855 / A004:** quality `41687478161` موفق؛ container `41687478388` موفق؛ product `41687478163` در test-source KtLint شکست خورد.
- **V3.0.856 / A006:** quality `41688070305` موفق با ۳۵۳ آزمون؛ container `41688070068` موفق؛ Android KtLint هشت chain diagnostic و یک import diagnostic گزارش کرد.
- **V3.0.857 / A007:** quality `41688934109` موفق با ۳۵۳ آزمون؛ container `41688934150` موفق؛ Windows WPF و HostLifecycle موفق؛ test-source KtLint فقط یک import diagnostic داشت و build/testهای Android اجرا نشدند.
- **V3.0.858 / A008:** quality run `41689552284` موفق؛ ۳۵۳ آزمون Python، graph guard با ۲۳۰ task/۴۲۲ edge و صفر خطا (یک هشدار metadata قدیمی T0001 حفظ شد). Container run `41689552377` موفق. Windows WPF و HostLifecycle موفق. فایل instrumentation test و test source set از KtLint عبور کردند؛ اما main-source KtLint در factory شکست خورد.
- **خطاهای دقیق V858:** در `EncryptedClientDatabaseFactory.kt` سه diagnostic ثبت شد: (۱) import دارای alias باید بعد از importهای عادی قرار بگیرد؛ (۲) `Room.databaseBuilder` باید به خط continuation منتقل شود؛ (۳) `source.rawQuery` باید به خط continuation منتقل شود. در نتیجه `assembleDebug`، JVM tests و چهار `connectedDebugAndroidTest` اجرا نشدند.
- **A009 — V3.0.859 در انتظار CI:** import دارای alias به انتهای فهرست منتقل شد؛ `Room.databaseBuilder` و `source.rawQuery` روی خط continuation قرار گرفتند و chain مربوط به Room به صورت یک فراخوانی در هر خط درآمد. این تغییر فقط قالب‌بندی است و منطق پایگاه، مهاجرت و کلید را تغییر نمی‌دهد.

## فایل‌های تغییر یافته در A009

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`
- `step three/project-graph.md`

## مرز پذیرش و ادامه

هیچ‌یک از چهار instrumentation test اختصاصی T0184 هنوز اجرای موفقی ندارند. تا اجرای موفق exact-head، رمزگذاری runtime فایل Room، مهاجرت واقعی دادهٔ plaintext و رفتار fail-closed پذیرفته‌شده محسوب نمی‌شوند. موفقیت Keystore tests مربوط به T0183 جایگزین این آزمون‌ها نیست.

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های فلش و دستگاه روت‌شده خارج از دامنه‌اند.

**اقدام بعدی:** CI دقیق V3.0.859 باید KtLint کامل، ساخت APK، JVM tests و چهار سناریوی `connectedDebugAndroidTest` را اجرا کند. فقط پس از موفقیت معیارهای واقعی، T0184 به `done` تغییر می‌کند.
