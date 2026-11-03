# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ اصلاح workflow برای اجرای واقعی instrumentation ثبت می‌شود و پذیرش نهایی به CI تازه وابسته است.
- **آخرین commit بررسی‌شده:** `c964c25c34c0b0321a369fee8d126ebeb78f686c` — `V 3.0.851`
- **تلاش‌ها:** A001 پیاده‌سازی، A002 شکست lint در main source، A003 شکست lint در test source، A004 اصلاح importهای آزمون، A005 اصلاح زنجیرهٔ Builder، A006 اصلاح working directory instrumentation (CI در انتظار)

## پیش‌نیازها و قرارداد
- T0182 و T0177 در گراف `done` هستند؛ یال‌های ورودی T0183 از قبل `sequencing_only` و `validated` هستند و نوع اصلی `blocks` و ترتیب اجرایی حفظ شده‌اند.
- یال `T0183 → T0184` از نوع `blocks` و طبقه‌بندی `contract_or_interface_dependency` با وضعیت `validated` است. T0184 باید رابط `ProtectedBlobProtector` را برای wrap کردن کلید دیتابیس Room مصرف کند؛ رمزگذاری خود Room در T0183 انجام نشده است.

## پیاده‌سازی
- helper مشترک `ProtectedBlobProtector` و `AndroidKeyStoreProtectedBlobProtector` با AES-256-GCM، کلید non-exportable، nonce تصادفی ۹۶ بیتی، tag ۱۲۸ بیتی، AAD وابسته به alias و محدودیت اندازه ایجاد شده‌اند.
- ذخیره‌ساز هویت و trust از دو alias مجزا استفاده می‌کنند و در رکورد خراب یا کلید مفقود fail-closed هستند.
- پنج آزمون instrumentation برای provider واقعی AndroidKeyStore روی emulator API 36 اضافه شده‌اند.
- اصلاح‌های A003 تا A005، قالب Kotlin و ترتیب importها را با CI هماهنگ کردند؛ رفتار رمزنگاری تغییر نکرد.

## نتایج CI و شکست‌ها
- `V 3.0.848`, run `41673696991`: KtLint در فایل اصلی Keystore هشت violation گزارش کرد.
- `V 3.0.849`, run `41675040018`: lint به test source رسید و دو فایل آزمون JVM به دلیل ترتیب importها/blank line رد شدند.
- `V 3.0.850`, run `41675745738`: main source هنوز دو violation در زنجیرهٔ `KeyGenParameterSpec.Builder` داشت.
- `V 3.0.851`, run `41676304428`:
  - Step Three quality gate، container، Windows WPF و HostLifecycle موفق شدند.
  - KtLint main/test، ساخت debug APK و JVM unit tests موفق شدند.
  - emulator API 36 boot شد؛ اما instrumentation اصلاً به taskهای Gradle نرسید. action هر خط script را در shell جدا اجرا کرد: دستور `cd src/Client.Android` در یک shell اجرا شد و فرمان بعدی `gradle --no-daemon connectedDebugAndroidTest` از ریشهٔ مخزن اجرا شد. Gradle با پیام `Directory '/home/runner/work/project/project' does not contain a Gradle build.` متوقف شد. این failure اجرای آزمون/provider نیست؛ پیکربندی مسیر اجرای workflow است. پس از آن مراحل Protobuf و APK upload به‌صورت skipped ثبت شدند.

## اصلاح A006
گام instrumentation اکنون یک فرمان واحد با `--project-dir src/Client.Android` اجرا می‌کند تا وابسته به ماندگاری `cd` در shell نباشد. هیچ کد محصول یا قرارداد رمزنگاری در این اصلاح تغییر نکرده است. CI دقیق commit A006 باید پنج instrumentation scenario را واقعاً اجرا کند.

## محدودیت پذیرش
تا اجرای موفق connected instrumentation، گزارش test artifacts و Protobuf verification و APK artifact روی commit اصلاحی، وضعیت T0183 `in_progress` می‌ماند. emulator provider integration و عدم export کلید از API را می‌سنجد، اما TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند. حفاظت کلید Room و رمزگذاری دیتابیس در T0184 هستند.
