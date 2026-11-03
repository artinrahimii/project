# T0184 — رمزگذاری ذخیره‌سازی حساس Android بدون کلید تعبیه‌شده در کد

- **وضعیت:** `in_progress`؛ پیاده‌سازی و چهار سناریوی instrumentation افزوده شده‌اند، اما CI دقیق این commit هنوز اجرا نشده است.
- **مبنای بررسی:** `9cf5e9b858f975b605c952ead3c4010fa78a9d11` — `V 3.0.853`.
- **تلاش:** `T0184-A001` — پیاده‌سازی اولیه؛ نتیجه `partial`.

## وابستگی‌ها

- `T0183 → T0184`: `contract_or_interface_dependency` / `validated`. T0184 از `AndroidKeyStoreProtectedBlobProtector` برای wrap کلید دیتابیس استفاده می‌کند.
- `T0177 → T0184`: `sequencing_only` / `validated`. selector و attempt budget در local Room/SQLCipher مصرف نمی‌شوند. یال `blocks` و ترتیب اجرای محافظه‌کارانه حفظ شده است.
- `T0184 → T0185` همچنان `unreviewed` است و قبل از اجرای T0185 باید بررسی شود.

## پیاده‌سازی واقعی

- وابستگی‌ها: SQLCipher Community Edition `4.20.0` و `androidx.sqlite:sqlite:2.7.0`؛ اتصال Room 2 با `SupportOpenHelperFactory`.
- `ClientDatabase.getInstance` اکنون از factory رمز‌شده استفاده می‌کند و migrationهای Room 1→2، 2→3 و 3→4 حفظ شده‌اند.
- passphrase تصادفی ۲۵۶ بیتی برای پایگاه ساخته می‌شود؛ فقط envelope رمز‌شده در SharedPreferences خصوصی ذخیره می‌شود. alias کلید پایگاه از aliasهای peer identity و trust مستقل است.
- پایگاه‌های plaintext قدیمی: WAL checkpoint، export با `sqlcipher_export()` به فایل موقت، حفظ `user_version` و `application_id`، integrity check و جایگزینی اتمی با `Os.rename`.
- اگر فایل رمز‌شده وجود داشته باشد اما کلید قابل بازیابی نباشد، برنامه fail-closed می‌شود؛ کلید تازه نمی‌سازد و فایل را reset نمی‌کند.
- `allowBackup=false` در manifest حفظ شده است.

## آزمون‌های افزوده‌شده — در انتظار اجرا

چهار instrumentation scenario برای Android API 36:
1. پایگاه تازه رمز‌شده و بازیابی conversation/message پس از reopen.
2. تبدیل پایگاه plaintext قدیمی و حفظ conversation/message/draft.
3. بررسی envelope محافظت‌شدهٔ کلید در preferences و بازکردن آن با Keystore.
4. فقدان envelope و اثبات عدم تغییر hash فایل رمز‌شده.

آزمون‌های قراردادی Python نیز مسیر integration، migration و چهار سناریوی instrumentation را بررسی می‌کنند. تا زمان اجرای واقعی CI هیچ‌یک از آزمون‌های T0184 موفق ثبت نشده‌اند.

## محدودیت‌ها

emulator، TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. این قابلیت رمزگذاری فایل پایگاه در حالت سکون را هدف دارد؛ plaintext در حافظهٔ فرایند، pagefile/swap، crash dump، پاک‌سازی فورنزیک بلوک‌های قدیمی فلش و دستگاه روت‌شده خارج از ادعا هستند.

**اقدام بعدی:** اجرای quality و product CI روی همین commit؛ اصلاح هر شکست و تنها پس از موفقیت همهٔ معیارها تغییر وضعیت به `done`.
