# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ اصلاحات KtLint مرحله‌ای ثبت شده‌اند و پذیرش نهایی هنوز به CI کامل Android وابسته است.
- **آخرین commit بررسی‌شده:** `6af58b498c536db0ae6788099e44195ff1ce7809` — `V 3.0.849`
- **تلاش‌ها:** A001 پیاده‌سازی، A002 شکست lint در main source، A003 شکست lint در test source، A004 اصلاح importهای آزمون (CI در انتظار)

## پیش‌نیازها و قرارداد

- T0182 و T0177 در گراف `done` هستند؛ دو یال ورودی T0183 از قبل `sequencing_only` / `validated` هستند و نوع `blocks` و ترتیب اجرایی حفظ شده است.
- یال `T0183 → T0184` از نوع `blocks` و کلاس `contract_or_interface_dependency` با وضعیت `validated` است. T0184 باید از `ProtectedBlobProtector` برای wrap کلید رمزگذاری دیتابیس Room استفاده کند. رمزگذاری خود Room در T0183 انجام نشده است.

## پیاده‌سازی موجود

- helper مشترک `ProtectedBlobProtector` و `AndroidKeyStoreProtectedBlobProtector` با AES-256-GCM، کلید non-exportable، nonce تصادفی ۹۶ بیتی، tag ۱۲۸ بیتی، AAD وابسته به alias و محدودیت اندازه ایجاد شده‌اند.
- ذخیره‌ساز هویت و trust از دو alias جدا استفاده می‌کنند و در خطای کلید/رکورد به‌صورت fail-closed رفتار می‌کنند.
- پنج instrumentation سناریو برای provider واقعی AndroidKeyStore روی emulator API 36 اضافه شده‌اند.
- آزمون‌های JVM و قراردادهای CI حفظ شده‌اند.

## CI و شکست‌های ثبت‌شده

- Snapshot `V 3.0.848`، run `41673696991`: product build شکست خورد. KtLint در فایل اصلی Keystore هشت مورد گزارش کرد؛ APK و instrumentation این revision اجرا نشدند.
- اصلاح main source در `V 3.0.849` ثبت شد. run `41675040018` از `ktlintMainSourceSetCheck` عبور کرد، اما `ktlintTestSourceSetCheck` در دو فایل آزمون JVM متوقف شد: import گروه `com` قبل از `android` بود و میان importها خط خالی وجود داشت.
- فایل‌های `AndroidPeerIdentityStoreTest.kt` و `AndroidPeerTrustStoreTest.kt` اکنون در تلاش A004 با ترتیب گروه‌های import اصلاح می‌شوند. هیچ منطق آزمون یا رفتار رمزنگاری تغییر نمی‌کند.
- run `41675040020`، Step Three quality gate، موفق است. HostLifecycle job `126053271139` نیز موفق شده است. Windows WPF و container باید از نتیجهٔ کامل همین run تأیید شوند.
- تا زمان اجرای مجدد KtLint test source، ساخت APK، JVM unit tests، connected instrumentation، Protobuf checks و artifact upload، وضعیت T0183 `in_progress` است.

## محدودیت

emulator API 36، provider integration و عدم export کلید از API را آزمایش می‌کند، اما TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. حفاظت کلید Room و رمزگذاری پایگاه داده همچنان متعلق به T0184 است.
