# T0179 — لاگ ساختاریافته با allowlist فیلدهای مجاز

- **وضعیت:** `in_progress`؛ پیاده‌سازی .NET و Android انجام شده، اما lint اندروید در دو تلاش متوالی مانع پذیرش APK و آزمون JVM شده است.
- **A001:** commit `583dccc5decebccb78d6599c082e12e516bb80c9` — `V 3.0.816`.
- **A002:** commit `4bfb38ab6238b3c4b8168352715704241c739b82` — `V 3.0.817`.
- **A003:** اصلاح دوم constructorهای enum؛ commit در حال ثبت با پیام `V 3.0.818`؛ CI هنوز اجرا نشده است.

## نتیجهٔ CI در A001

- Step Three quality gate: ۳۳۳ آزمون Python و compileall موفق؛ graph_guard روی ۲۳۰ وظیفه و ۴۲۲ یال با صفر خطا و یک هشدار metadata قدیمی T0001.
- HostLifecycle و آزمون EventSource: موفق؛ EventSource تنها سه مقدار عددی مجاز را منتشر کرد.
- Windows WPF: format، build، Protobuf، smoke test و کنترل‌های policy/identity موفق.
- Rendezvous container و Real local inference integration: موفق.
- Android ktlintCheck شکست خورد: خط ۳۵ طول بیش از ۱۲۰ و خط ۴۳ قالب‌بندی expression؛ APK و JVM tests اجرا نشدند.

## نتیجهٔ CI در A002

- Step Three quality gate: ۳۳۳ آزمون Python، compileall و graph_guard موفق؛ صفر خطای گراف و همان هشدار metadata قدیمی T0001.
- HostLifecycle و Rendezvous container: موفق.
- Android ktlintCheck دوباره شکست خورد؛ خطاهای A001 مربوط به JSON و expression دیگر گزارش نشدند. این بار فقط دو enum constructor در خطوط ۷ و ۱۲ باید چندخطی باشند.
- به دلیل lint failure، assembleDebug و testDebugUnitTest اجرا نشدند.
- Windows WPF در زمان تهیهٔ این گزارش هنوز در حال اجرای کنترل‌های انتهایی CI بود؛ نتیجهٔ نهایی آن را نباید زودتر از گزارش workflow اعلام کرد.

## اصلاح A003

- هر دو enum (`SafeDiagnosticEventCode` و `SafeDiagnosticOutcomeCode`) اکنون constructor parameter list چندخطی دارند.
- schema و مقادیر عددی allowlist، روش ساخت JSON و سقف ۸۰ رکورد تغییر نکرده‌اند.
- ktlint، APK build و JVM unit tests برای A003 هنوز اجرا نشده‌اند.

## وابستگی‌ها و ادامه

- یال‌های `T0177 → T0179` و `T0178 → T0179` همچنان نوع `blocks` با طبقه‌بندی اعتبارسنجی‌شدهٔ `sequencing_only` دارند؛ هیچ یالی حذف و ترتیب اجرا تغییر نکرده است.
- وضعیت T0179 تا پاس‌شدن `ktlintCheck`، `assembleDebug` و `testDebugUnitTest` در CI دقیق commit، `in_progress` می‌ماند.
- کنترل این پیاده‌سازی به‌تنهایی logهای سیستم‌عامل، crash dump، فرایندهای خارجی یا کتابخانه‌های ثالث را نفی نمی‌کند.
