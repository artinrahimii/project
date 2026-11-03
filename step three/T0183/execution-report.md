# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ مسیر instrumentation اصلاح شده، اما یک guard آزمون قراردادی قدیمی مانع سبزشدن quality gate شده است.
- **commit مبنا:** `88639fc553295c9b8a10b71b5eade12e1143ded8` — `V 3.0.852`
- **تلاش‌ها:** A001 پیاده‌سازی، A002 lint main source، A003 lint test source، A004 اصلاح importها، A005 اصلاح Builder chain، A006 اصلاح مسیر instrumentation، A007 اصلاح قرارداد آزمون workflow (CI در انتظار)

## پیش‌نیازها و قرارداد
T0182 و T0177 در گراف `done` و یال‌های ورودی T0183 از قبل `sequencing_only` / `validated` هستند؛ نوع `blocks` و ترتیب حفظ شده است. یال خروجی T0183→T0184 از نوع `blocks` و طبقه‌بندی `contract_or_interface_dependency` با وضعیت `validated` باقی می‌ماند؛ T0184 باید `ProtectedBlobProtector` را برای wrap کلید دیتابیس Room مصرف کند.

## پیاده‌سازی واقعی
helper مشترک AES-256-GCM در Android Keystore، کلیدهای non-exportable و aliasهای مستقل برای peer identity و trust، envelope نسخه‌بندی‌شده با nonce ۹۶ بیتی و tag ۱۲۸ بیتی، alias-bound AAD، محدودیت اندازه و fail-closed ایجاد شده‌اند. پنج سناریوی instrumentation برای provider واقعی AndroidKeyStore روی emulator API 36 در workflow تعریف شده‌اند.

## نتایج CI و اصلاح‌ها
- V848 / run `41673696991`: هشت خطای KtLint در فایل main helper.
- V849 / run `41675040018`: دو خطای import در test source.
- V850 / run `41675745738`: دو خطای قالب‌بندی در زنجیرهٔ Builder.
- V851 / run `41676304428`: KtLint، APK و JVM tests موفق؛ instrumentation به Gradle task نرسید، زیرا android-emulator-runner هر خط script را در shell جدا اجرا می‌کند و `cd` بین خطوط حفظ نشد. خطای واقعی Gradle این بود: `Directory '/home/runner/work/project/project' does not contain a Gradle build.`
- A006 / V852: فرمان instrumentation به یک خط با `--project-dir src/Client.Android` تغییر کرد تا به cwd shell متکی نباشد.
- Step Three quality gate run `41677809950` پس از A006، 350 آزمون را اجرا کرد: 349 موفق، 1 شکست. شکست در `tests/test_android_keystore_local_secrets.py:79` بود؛ test contract هنوز رشتهٔ قدیمی `gradle --no-daemon connectedDebugAndroidTest` را انتظار داشت. A007 assertion را به فرمان جدید `gradle --no-daemon --project-dir src/Client.Android connectedDebugAndroidTest` تغییر می‌دهد.
- Container run `41677809985` موفق شد. در زمان ثبت این گزارش، product run `41677809961` هنوز در حال اجرا بود و نتیجهٔ نهایی Android/WPF/HostLifecycle تثبیت نشده بود.

## وضعیت پذیرش
A006 هنوز در CI محصول به‌صورت قطعی تأیید نشده است و A007 guard مرتبط را با workflow همگام می‌کند. تا سبزشدن quality gate و اجرای موفق پنج instrumentation scenario، task `in_progress` باقی می‌ماند. Room encryption در T0184 است؛ emulator نیز TEE/StrongBox واقعی دستگاه را اثبات نمی‌کند.