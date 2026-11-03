# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ SQLCipher و ادغام Room پیاده‌سازی شده‌اند، اما پذیرش runtime اختصاصی T0184 هنوز انجام نشده است.
- **پیاده‌سازی اولیه:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **آخرین CI بررسی‌شده:** `d740c10032d488c6d698aa4d43e78f6e1458d8b2` — `V 3.0.862`.
- **تلاش‌ها:** A001 تا A013؛ پنج تلاش ناموفق فنی قبلی ثبت شده‌اند. A012 خطای overload کامپایل را اصلاح کرد و build/JVM را در V 3.0.862 پاس کرد؛ A013 تنظیم runner شبیه‌ساز را برای رفع کمبود فضای پارتیشن اصلاح می‌کند.

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
- **V3.0.860 / A010:** quality run `41693397321` با ۳۵۳ آزمون Python موفق؛ graph guard با ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطا و یک هشدار قدیمی T0001؛ container run `41693397637` موفق. در product run `41693397531`، Windows WPF و HostLifecycle موفق و test-source KtLint نیز موفق بود؛ اما main-source KtLint چهار خطای تورفتگی در `EncryptedClientDatabaseFactory.kt:194-197` گزارش کرد. پس از آن APK، JVM unit tests و چهار connected instrumentation tests اجرا نشدند.
- **A011 — اصلاح فعلی:** بدنهٔ lambda مربوط به `source.rawQuery(...).use` و آکولاد بسته‌شدن آن چهار فاصله به داخل منتقل شدند تا با مقادیر دقیق مورد انتظار KtLint (به‌ترتیب ۲۴، ۲۸، ۲۴ و ۲۰ فاصله) مطابقت داشته باشند. منطق SQLCipher، export، integrity، key handling و Room تغییر نکرده است. CI دقیق هنوز اجرا نشده است.

## فایل‌های تغییر این نوبت

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`: اصلاح تورفتگی چهار خط در cursor lambda پس از شکست واقعی KtLint در V3.0.860.
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`
- `step three/project-graph.md`

## مرز پذیرش و محدودیت‌ها

چهار آزمون instrumentation مخصوص T0184 تا این لحظه هیچ اجرای موفقی ندارند. تا اجرای موفق exact-head، رمزگذاری runtime فایل Room، مهاجرت دادهٔ plaintext و رفتار fail-closed تأییدشده محسوب نمی‌شوند. موفقیت آزمون‌های Keystore در T0183 جایگزین این چهار آزمون نیست.

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های فلش و دستگاه روت‌شده خارج از دامنه‌اند.

**اقدام بعدی:** CI دقیق V3.0.861 باید KtLint کامل، ساخت APK، JVM tests و چهار سناریوی `connectedDebugAndroidTest` را اجرا کند. فقط پس از موفقیت معیارهای واقعی، T0184 به `done` تغییر می‌کند.

## A012 — اصلاح امضای SQLCipher و نتیجهٔ CI دقیق V 3.0.861

- commit بررسی‌شده: `6325ae0097bf262c02aa298a389481dc12db1625` (`V 3.0.861`).
- workflow محصول: run `41695659213`؛ job اندروید `126125365784`.
- KtLint کامل موفق شد. کامپایل Kotlin در `EncryptedClientDatabaseFactory.kt:248-250` شکست خورد: overload چهارتایی انتخاب‌شده، آرگومان `ByteArray` را به‌عنوان `CursorFactory` و آرگومان‌های بعدی را با انواع نامتناسب تفسیر می‌کرد. در نتیجه APK، JVM unit tests و چهار instrumentation test اجرا نشدند.
- Windows WPF و HostLifecycle موفق شدند؛ Step Three quality run `41695659074` و Rendezvous container run `41695659104` نیز موفق شدند. این نتایج جایگزین آزمون‌های اندروید نیستند.
- بازبینی منبع رسمی SQLCipher Android در tag `v4.20.0`، overload پنج‌آرگومانی را تأیید کرد: `openDatabase(String, byte[], CursorFactory, int, SQLiteDatabaseHook)`. منبع: `sqlcipher/src/main/java/net/zetetic/database/sqlcipher/SQLiteDatabase.java` در tag `v4.20.0`.
- **اصلاح A012:** آرگومان پنجم `null` به فراخوانی اضافه شد تا overload دارای `byte[] password` و `SQLiteDatabaseHook` صریحاً انتخاب شود. تغییر فقط در امضای فراخوانی بازکردن پایگاه مهاجرت‌شده است؛ الگوریتم رمزگذاری، ساخت passphrase، export، integrity check، Room migration و ذخیرهٔ Keystore تغییر نکردند.
- نتیجهٔ آزمون A012 هنوز pending است؛ تا CI دقیق commit بعدی، T0184 همچنان `in_progress` می‌ماند و هیچ‌یک از چهار سناریوی runtime به‌عنوان موفق ثبت نمی‌شوند.

**گام بعدی:** اجرای KtLint، `assembleDebug`، `testDebugUnitTest` و چهار سناریوی `connectedDebugAndroidTest` روی commit دقیق A012؛ سپس اصلاح خطاهای واقعی بعدی، در صورت وجود.

## A013 — رفع کمبود فضای دیسک Android Emulator

- commit مبنا: `d740c10032d488c6d698aa4d43e78f6e1458d8b2` (`V 3.0.862`).
- Product workflow: run `41697319573`؛ Android job `126131159871`.
- KtLint، `assembleDebug` و `testDebugUnitTest` موفق شدند. گزارش Gradle بارگذاری‌شده (`whitep-android-unit-test-reports`, artifact `13263131610`, اندازه `51,122` بایت) تعداد **۱۹۶ تست، صفر شکست و صفر ignored** را نشان می‌دهد.
- Windows WPF، HostLifecycle، Step Three quality gate (۳۵۳ تست Python و graph guard با ۲۳۰ task/۴۲۲ edge، صفر خطا و یک هشدار metadata قدیمی T0001) و Rendezvous container build/smoke موفق شدند.
- اجرای `reactivecircus/android-emulator-runner` پیش از اجرای `connectedDebugAndroidTest` شکست خورد: emulator خطای `Not enough space to create userdata partition. Available: 5861.69 MB, need 7372.80 MB` داد؛ دستگاه `emulator-5554` در ADB ظاهر نشد و runner با `Timeout waiting for emulator to boot` متوقف شد. بنابراین **هیچ‌یک از چهار تست instrumentation مربوط به T0184 اجرا نشدند** و پوشهٔ گزارش connected نیز ساخته نشد؛ عدم وجود artifact گزارش به معنی pass نیست.
- **اصلاح A013 در workflow:** گزینهٔ رسمی `-partition-size 4096` به emulator اضافه شد تا پارتیشن داده در اندازهٔ مناسب برای runner ایجاد شود. خروجی‌های موقت `tests/HostLifecycle.Tests/bin` و `obj` نیز پس از پایان JVM tests حذف می‌شوند، چون fixture .NET در همان job دیگر مصرفی ندارد؛ فضای آزاد با `df -h` ثبت خواهد شد.
- این اصلاح هنوز در commit جدید CI نشده است. T0184 همچنان `in_progress` می‌ماند و پذیرش runtime SQLCipher هنوز اثبات نشده است.

**گام بعدی:** اجرای exact-head V 3.0.863؛ باید emulator boot شود و چهار تست `connectedDebugAndroidTest` واقعاً اجرا و نتیجهٔ report/runner بررسی شود.

## A014 — اصلاح تورفتگی YAML در workflow

- commit A013: `d9a2889743f26571fa80fea9b0d4d7be54519575` (`V 3.0.863`).
- workflow run `41700157377` برای `.github/workflows/product-build.yml` پیش از ایجاد jobها ناموفق شد. diff همان commit نشان می‌دهد بلوک cleanup جدید با تورفتگی سطح ریشه وارد `jobs.android.steps` شده بود؛ `shell` و `run` در سطح نادرست قرار گرفتند.
- **اصلاح A014:** تورفتگی cleanup و step بعدی با ساختار sibling در `steps` هماهنگ شد؛ دستورهای cleanup و گزینهٔ `-partition-size 4096` تغییر منطقی نکردند.
- این اصلاح هنوز روی commit جدید CI نشده است. T0184 همچنان `in_progress` است و نتیجهٔ جدیدی برای instrumentation ثبت نمی‌شود.

**گام بعدی:** exact-head V 3.0.864 باید workflow را parse کند و build، JVM و emulator/instrumentation را اجرا کند. پذیرش runtime فقط پس از موفقیت چهار تست واقعی ثبت می‌شود.

## A015 — تنظیم صحیح ظرفیت userdata در Android Emulator

- commit مبنا: `3f41808e3995110b746f8c538e1bc5fc585f195f` (`V 3.0.864`).
- اجرای workflow در نسخهٔ 864 این بار jobهای Android، Windows و HostLifecycle را ایجاد کرده است؛ پس خطای YAML نسخهٔ 863 برطرف شده است. در checkpoint بررسی، KtLint اندروید موفق، `assembleDebug` در حال اجرا، HostLifecycle موفق، و instrumentation هنوز شروع نشده بود. اجرای exact-head هنوز نهایی نشده است.
- بررسی سورس دقیق action استفاده‌شده در workflow (`reactivecircus/android-emulator-runner` commit `a421e43855164a8197daf9d8d40fe71c6996bb0d`) نشان می‌دهد ورودی `disk-size` پیکربندی `disk.dataPartition.size` را تنظیم می‌کند. این ورودی همان ظرفیت userdata مورد نیاز برای ساخت AVD است.
- `-partition-size` صرفاً پارتیشن سیستم emulator را تغییر می‌دهد و علت خطای userdata را برطرف نمی‌کند؛ از workflow حذف شد.
- **اصلاح A015:** ورودی `disk-size: 4096M` به action اضافه و گزینهٔ نامرتبط `-partition-size 4096` حذف شد. پاک‌سازی bin/obj تولیدشدهٔ fixture .NET بعد از JVM tests و ثبت `df -h "$HOME"` حفظ شد.
- این تغییر هنوز CI نشده است. T0184 همچنان `in_progress` است و هیچ‌یک از چهار سناریوی SQLCipher instrumentation به‌عنوان موفق ثبت نشده‌اند.

**گام بعدی:** exact-head V 3.0.865 باید workflow و build را پاس کند، AVD با userdata چهار گیگابایتی boot شود و چهار سناریوی `connectedDebugAndroidTest` واقعاً اجرا شوند.

## A016 — ایجاد صحیح فایل مقصد هنگام ATTACH در مهاجرت SQLCipher

- commit مبنا: `e2e13532785b7f02ada7864e0eed9cb84dfee97e` (`V 3.0.865`).
- Product workflow دقیق: run `41701047926`؛ Android job `126144093268`. اجرای emulator موفق شد و `sys.boot_completed=1` شد. KtLint، `assembleDebug` و `testDebugUnitTest` موفق شدند؛ Windows WPF، HostLifecycle، Step Three quality gate و Rendezvous container نیز موفق بودند.
- `connectedDebugAndroidTest` واقعاً شروع شد و ۹ اجرای instrumentation را گزارش کرد. یک مورد مشخص شکست خورد: `EncryptedClientDatabaseAcceptanceTest.legacyPlaintextRoomDatabaseIsExportedAndExistingRowsArePreserved`. خطا هنگام بازکردن فایل مقصد `t0184-legacy-plaintext.db.sqlcipher-migrating` در `ATTACH DATABASE` بود: `SQLiteCantOpenDatabaseException` / کد ۱۴ (`SQLITE_CANTOPEN`). این شکست runtime واقعی است؛ مهاجرت plaintext هنوز پذیرفته‌شده نیست.
- بررسی سورس دقیق SQLCipher Android tag `v4.20.0` نشان می‌دهد `CREATE_IF_NECESSARY` یک open flag پشتیبانی‌شده است. سابقهٔ upstream در issue #259 نیز توضیح می‌دهد که برای ایجاد فایل مقصدِ ATTACH باید این flag به‌جای `OPEN_READWRITE` صرف استفاده شود: https://github.com/sqlcipher/android-database-sqlcipher/issues/259 .
- **اصلاح A016:** فراخوانی بازکردن پایگاه plaintext اکنون `OPEN_READWRITE or CREATE_IF_NECESSARY` را درخواست می‌کند. این تغییر به همان نقطهٔ بازکردن منبع محدود است؛ رمزگذاری کلید، envelope، Room migrations، الگوریتم export، integrity check و جایگزینی اتمی تغییر نکرده‌اند.
- نتیجهٔ runtime اصلاح A016 هنوز pending است. `T0184` در `in_progress` باقی می‌ماند؛ هیچ‌کدام از معیارهای مهاجرت دادهٔ قدیمی را تا اجرای exact-head جدید pass نمی‌دانیم.

**گام بعدی:** اجرای exact-head `V 3.0.866`؛ بررسی موفقیت مهاجرت plaintext و حفظ conversation/message/draft، header رمز‌شده، بازیابی کلید Keystore و fail-closed در برابر فقدان کلید.
