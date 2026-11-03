# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ پیاده‌سازی SQLCipher و چهار instrumentation scenario وجود دارند، اما آزمون‌های runtime اختصاصی T0184 هنوز اجرا نشده‌اند.
- **پیاده‌سازی اولیه:** `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **اصلاح قراردادهای Python/ADR:** `7678df818d5911701a2ecb41e3aa87b0a2ef6d47` — `V 3.0.855`.
- **اصلاح KtLint:** در حال ثبت در `V 3.0.856`.
- **تلاش‌ها:** A001 پیاده‌سازی؛ A002 شکست سه قرارداد قدیمی؛ A003 اصلاح آن‌ها؛ A004 شکست KtLint؛ A005 اصلاح KtLint، در انتظار CI.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`.
- `T0177 → T0184`: `sequencing_only` / `validated`. یال اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- `T0184 → T0185` همچنان `unreviewed` است.

## پیاده‌سازی واقعی

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ اتصال Room 2 با `SupportOpenHelperFactory`.
- `ClientDatabase.getInstance` از factory رمز‌شده استفاده می‌کند و migrationهای Room 1→2، 2→3 و 3→4 حفظ شده‌اند.
- passphrase تصادفی ۲۵۶ بیتی؛ ذخیرهٔ فقط envelope محافظت‌شده توسط Android Keystore در preferences خصوصی و alias مستقل برای کلید database.
- مهاجرت SQLite قدیمی با WAL checkpoint، `sqlcipher_export()`، حفظ `user_version` و `application_id`، integrity check و rename اتمی.
- کلید مفقود برای فایل رمز‌شده fail-closed است و کلید تازه یا پایگاه reset تولید نمی‌شود.
- `allowBackup=false` باقی مانده است.

## نتایج CI و خطاهای اصلاح‌شده

- V3.0.854 / quality run **41686907108**: ۳۵۳ آزمون؛ ۳۵۰ موفق، سه assertion شکست‌خورده. دو مورد فرض می‌کردند `addMigrations` در ClientDatabase.kt باشد و یکی عبارت تاریخی `not encrypted at rest` را در ADR می‌خواست.
- V3.0.854 / product run **41686906946**: Windows WPF و HostLifecycle موفق؛ Android job **126094946982** به علت KtLint شکست خورد و build/APK/JVM/instrumentation بعد از آن skipped شدند.
- KtLint چهار ایراد در `EncryptedClientDatabaseAcceptanceTest.kt` گزارش کرد: ترتیب imports/alias، دو newline در زنجیرهٔ SharedPreferences و قالب تابع expression در `sha256`.
- V3.0.855: دو قرارداد Python به محل جدید migration در factory منتقل شدند و ADR عبارت تاریخی را با توضیح دقیق baseline حفظ کرد.
- V3.0.855 / quality run **41687478161**: موفق.
- V3.0.855 / container run **41687478388**: موفق.
- V3.0.855 / product run **41687478163** هنگام ثبت اصلاح KtLint در حال اجرا بود.
- A005 ترتیب alias import، chain style و function expression را مطابق log اصلاح کرد. اجرای مجدد آن هنوز در CI انجام نشده است.

## آزمون‌های اختصاصی T0184

چهار instrumentation scenario افزوده شده‌اند: پایگاه تازه و reopen، مهاجرت plaintext و حفظ رکوردها، envelope کلید Keystore و fail-closed هنگام فقدان کلید. تا زمان ثبت، هیچ‌یک از این چهار سناریو در CI با موفقیت اجرا نشده‌اند؛ `tests_passed` اختصاصی T0184 خالی می‌ماند.

## محدودیت‌ها

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. ادعا فقط رمزگذاری فایل پایگاه در حالت سکون است؛ plaintext داخل حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک فلش و دستگاه روت‌شده خارج از دامنه هستند.

**اقدام بعدی:** بررسی V3.0.856 quality، KtLint، build/APK/JVM و اجرای instrumentation؛ فقط پس از موفقیت این آزمون‌ها T0184 به `done` تبدیل می‌شود.
