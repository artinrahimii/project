# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ پیاده‌سازی واقعی و چهار سناریوی instrumentation اضافه شده‌اند، اما پذیرش CI دقیق هنوز کامل نیست.
- **پیاده‌سازی اولیه:** commit `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- **تلاش‌ها:** A001 پیاده‌سازی؛ A002 اجرای quality gate ناموفق به علت سه قرارداد قدیمی؛ A003 اصلاح آن قراردادها، در انتظار CI.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`. T0184 از `AndroidKeyStoreProtectedBlobProtector` برای wrap کلید دیتابیس استفاده می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`. selector و attempt budget در local Room/SQLCipher مصرف نمی‌شوند. یال اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شده است.
- `T0184 → T0185` همچنان `unreviewed` است؛ قبل از اجرای T0185 باید بررسی شود.

## پیاده‌سازی واقعی

- SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0` به Android app افزوده شده‌اند؛ Room 2 از `SupportOpenHelperFactory` استفاده می‌کند.
- `ClientDatabase.getInstance` اکنون از `EncryptedClientDatabaseFactory` استفاده می‌کند و migrationهای Room 1→2، 2→3 و 3→4 را حفظ می‌کند.
- passphrase تصادفی ۲۵۶ بیتی ساخته می‌شود؛ فقط envelope رمز‌شده در SharedPreferences خصوصی ذخیره می‌شود. alias کلید پایگاه از aliasهای peer identity و trust مستقل است.
- پایگاه‌های plaintext قدیمی: WAL checkpoint، export با `sqlcipher_export()` به فایل موقت، حفظ `user_version` و `application_id`، integrity check و جایگزینی اتمی با `Os.rename`.
- اگر فایل رمز‌شده وجود داشته باشد اما کلید قابل بازیابی نباشد، برنامه fail-closed می‌شود؛ کلید تازه نمی‌سازد و فایل را reset نمی‌کند.
- `allowBackup=false` در manifest حفظ شده است.

## آزمون‌ها و نتایج واقعی

چهار instrumentation scenario برای Android API 36 افزوده شده‌اند:
1. ساخت پایگاه تازهٔ رمز‌شده و بازیابی مکالمه/پیام پس از reopen.
2. تبدیل پایگاه plaintext قدیمی و حفظ conversation/message/draft.
3. بررسی envelope محافظت‌شدهٔ کلید در preferences و بازکردن آن با Keystore.
4. فقدان envelope و اثبات عدم تغییر hash فایل رمز‌شده.

- Quality run **41686907108** روی V 3.0.854: ۳۵۳ آزمون اجرا شد؛ ۳۵۰ موفق و ۳ ناموفق.
- شکست‌ها از آزمون‌های قدیمی قرارداد بودند: دو آزمون انتظار داشتند `addMigrations(...)` داخل `ClientDatabase.kt` باشد، درحالی‌که فراخوانی اکنون در factory رمز‌شده قرار دارد؛ آزمون سوم عبارت تاریخی `not encrypted at rest` را پس از به‌روزرسانی ADR پیدا نمی‌کرد.
- هیچ‌یک از چهار instrumentation test مربوط به T0184 در این quality run اجرا نمی‌شود؛ بنابراین `tests_passed` مربوط به T0184 خالی باقی می‌ماند.
- اصلاح A003، دو تست موجود را با مسیر factory و عبارت تاریخی ADR هماهنگ می‌کند. اجرای مجدد CI هنوز در انتظار است.
- Container run **41686907102** موفق شد.
- Product run **41686906946** در زمان ثبت این گزارش هنوز در حال اجرا بود.

## محدودیت‌ها

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. این قابلیت رمزگذاری فایل پایگاه در حالت سکون را هدف دارد؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های قدیمی فلش و دستگاه روت‌شده خارج از ادعا هستند.

**اقدام بعدی:** اجرای مجدد quality و بررسی نتیجهٔ product CI روی V 3.0.855؛ فقط پس از موفقیت آزمون‌های SQLCipher instrumentation و همهٔ کنترل‌های مرتبط، وضعیت به `done` تغییر می‌کند.
