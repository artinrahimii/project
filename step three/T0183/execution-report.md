# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ اصلاح KtLint ثبت می‌شود و تأیید نهایی به CI دقیق نسخهٔ اصلاحی وابسته است.
- **شاخه:** `main`
- **آخرین snapshot بررسی‌شده:** `0be00fb2f36f1a69fff8af31acc4339a76c7eb8e` — `V 3.0.848`
- **تلاش‌ها:** `T0183-A001` (پیاده‌سازی اولیه)، `T0183-A002` (شکست KtLint)، `T0183-A003` (اصلاح قالب‌بندی؛ CI در انتظار)

## پیش‌نیازها و قراردادها

- `T0182` و `T0177` در گراف `done` هستند. خروجی‌های واقعی آن‌ها بررسی شدند؛ پاک‌سازی فایل‌های موقت میزبان Windows و منطق انتخاب میزبان/بودجهٔ تلاش Android توسط Android Keystore key wrapping مصرف نمی‌شوند. دو یال ورودی از قبل `sequencing_only` و `validated` هستند؛ نوع اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شده است.
- یال خروجی `T0183 → T0184` از نوع `blocks` و طبقه‌بندی `contract_or_interface_dependency` با وضعیت `validated` است. T0184 باید از رابط `ProtectedBlobProtector` برای حفاظت کلید پایگاه Room استفاده کند؛ کد دیتابیس رمز‌شده در خود T0183 ادعا نمی‌شود.

## پیاده‌سازی موجود

- پیاده‌سازی مشترک AES-256-GCM در `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt` قرار گرفته است.
- هویت Peer و trust از دو alias مستقل و non-exportable در Android Keystore استفاده می‌کنند.
- envelope نسخه‌بندی‌شده، nonce تصادفی ۹۶ بیتی، tag احراز اصالت ۱۲۸ بیتی و AAD وابسته به alias دارد.
- اعتبارسنجی اندازهٔ payload، نوع کلید و exportability وجود دارد؛ envelope خراب یا کلید مفقود fail-closed است.
- آزمون‌های provider واقعی Android در instrumentation test اضافه شده‌اند و workflow محصول آن‌ها را روی emulator API 36 اجرا می‌کند.
- هیچ تغییر رفتاری یا معماری در این اصلاح قالب‌بندی اعمال نشده است.

## نتایج CI در snapshot فعلی

برای commit `0be00fb2f36f1a69fff8af31acc4339a76c7eb8e`:

- Step Three quality gate — run `41673697033`: موفق.
- Rendezvous container build and smoke — run `41673696995`: موفق.
- White-P product build — run `41673696991`: ناموفق.
- Windows WPF و HostLifecycle در product run موفق بودند.
- job ساخت Android، `126048506024`، در `ktlintMainSourceSetCheck` متوقف شد؛ هشت مورد KtLint در فایل پیاده‌سازی Keystore گزارش شد. به‌دلیل توقف در lint، آزمون JVM، ساخت APK و instrumentation این revision را موفق ثبت نمی‌کنیم.

## اصلاح T0183-A003

فقط فایل `AndroidKeyStoreProtectedBlobProtector.kt` از نظر قالب Kotlin اصلاح شد: فاصلهٔ بین اعلان‌ها، سازندهٔ چندخطی، chain سازندهٔ KeyGenParameterSpec، عبارت‌های چندخطی و expression body تابع `additionalData`. پس از ثبت، workflow کامل محصول باید دوباره اجرا شود.

## محدودیت امنیتی و پذیرش

- emulator API 36 می‌تواند provider integration و non-exportability قابل مشاهده از API را آزمایش کند؛ TEE/StrongBox واقعی دستگاه فیزیکی را اثبات نمی‌کند.
- رمزگذاری پایگاه Room و migration کلید آن در T0184 باقی می‌ماند.
- تا موفقیت KtLint، ساخت APK، JVM unit tests، connected instrumentation، Protobuf checks و سایر jobهای product build روی commit اصلاحی، وضعیت T0183 `in_progress` باقی می‌ماند.
