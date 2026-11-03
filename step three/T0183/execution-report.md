# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

<badge>Completed</badge>

- **وضعیت نهایی:** `done`؛ معیارهای T0183 با آزمون provider واقعی روی Android API 36 emulator و CI دقیق احراز شدند.
- **پیاده‌سازی:** `cf383f4e94cd41f6e63fa78dbbabfe2e4d0afdd8` — `V 3.0.844`.
- **CI تأییدکننده:** `9cf5e9b858f975b605c952ead3c4010fa78a9d11` — `V 3.0.853`.
- **تلاش‌ها:** A001 پیاده‌سازی و اصلاحات آغازین؛ A002 بررسی نهایی exact-head پس از رفع خطاهای KtLint، فرمان instrumentation و guard قراردادی.

## وابستگی‌ها و قرارداد

- `T0182 → T0183`: `sequencing_only` / `validated`.
- `T0177 → T0183`: `sequencing_only` / `validated`.
- یال‌های اصلی `blocks` و ترتیب محافظه‌کارانه حفظ شدند.
- قرارداد `ProtectedBlobProtector` اکنون برای wrap کلید پایگاه Room در T0184 مصرف می‌شود. رمزگذاری خود پایگاه در دامنهٔ T0183 نبود.

## پیاده‌سازی واقعی

- helper مشترک AES-256-GCM با کلید non-exportable در Android Keystore.
- envelope نسخه‌دار با nonce تصادفی ۹۶ بیتی، ciphertext، tag احرازاصالت ۱۲۸ بیتی و AAD مقید به alias.
- alias مستقل برای peer identity و peer trust؛ SharedPreferences فقط envelope رمز‌شده را نگه می‌دارد.
- خواندن envelope خراب یا کلید مفقود fail-closed است؛ identity بی‌صدا rotate نمی‌شود و trust خودکار ساخته نمی‌شود.
- پنج سناریوی instrumentation برای provider واقعی Android Keystore روی emulator API 36 در workflow محصول اجرا می‌شوند.

## شواهد CI دقیق

- Product run **41678108221** روی commit دقیق `9cf5e9b858f975b605c952ead3c4010fa78a9d11`: موفق؛ jobهای Windows WPF، Android و HostLifecycle همگی موفق.
- Android job **126064102869**: KtLint، ساخت APK، آزمون JVM، بررسی Protobuf و `connectedDebugAndroidTest` موفق.
- log instrumentation: **۵ آزمون** روی `emulator-5554 - API 36` شروع و تمام شدند؛ Gradle با `BUILD SUCCESSFUL` خاتمه یافت.
- Instrumentation artifact **13252670975** (۵٬۳۵۳ بایت)، JVM report **13252631231** (۵۱٬۲۱۸ بایت)، APK artifact **13252915718** (۱۹٬۵۳۲٬۹۲۱ بایت).
- Windows host artifact **13250877673** (۱۴٬۲۷۵٬۵۱۴ بایت).
- Step Three quality run **41678108248**: موفق.
- Rendezvous container run **41678108226**: موفق.

## محدودیت

این آزمون‌ها provider را در Android API 36 emulator تأیید می‌کنند؛ سخت‌افزاری بودن کلید با TEE/StrongBox روی دستگاه فیزیکی را اثبات نمی‌کنند. رمزگذاری Room، حفاظت plaintext در حافظه و پذیرش نهایی P2P جزو T0183 نیستند.

**نتیجه:** T0183 از `in_progress` به `done` تغییر کرد.
