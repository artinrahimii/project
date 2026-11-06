# T0182 — پاک‌سازی فایل‌های موقت یتیم پس از راه‌اندازی مجدد

<badge>Completed</badge>

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پیاده‌سازی و آزمون دقیق: `f21dc02702f654f51e8cae699aa592f9a5399fac` — `V 3.0.842`
- commit ثبت وضعیت نهایی گراف و گزارش: `V 3.0.843`
- تلاش‌ها: `T0182-A001` (پیاده‌سازی)، `T0182-A002` (اعتبارسنجی کامل CI)

## بررسی پیش‌نیازها و یال‌ها

- `T0181 → T0182`: `hard_dependency` و `validated`. T0181 ذخیره‌ساز Windows و قرارداد نام‌گذاری فایل موقت `.<history-file>.<GUID-N>.tmp` را ایجاد می‌کند؛ T0182 همان artifact را پاک‌سازی می‌کند. نوع اولیهٔ `blocks` حفظ شد.
- `T0177 → T0182`: `sequencing_only` و `validated`. selector و retry budget اندروید در پاک‌سازی فایل موقت میزبان Windows مصرف نمی‌شوند. نوع اولیهٔ `blocks` و ترتیب محافظه‌کارانه حفظ شد.

هر دو دلیل و شواهد در `edge_change_log` ثبت شده‌اند. سایر یال‌ها تغییر نکرده‌اند.

## پیاده‌سازی واقعی

در `WindowsHostConversationStore`، پاک‌سازی در constructor و هنگام آغاز store اجرا می‌شود. حذف فقط روی فایل‌های هم‌مسیر با تاریخچهٔ تنظیم‌شده انجام می‌شود که نام دقیق زیر را داشته باشند:

`.<history-file>.<32-hex-GUID>.tmp`

قواعد حفاظتی:
- فایل‌هایی که کمتر از ۳۰ ثانیه عمر دارند حذف نمی‌شوند تا فاصلهٔ کوتاه بستن فایل و rename نهایی با عملیات Save تداخل نکند.
- پیش از حذف، فایل به‌شکل انحصاری با `FileShare.None` باز می‌شود؛ فایل قفل‌شده حذف نمی‌شود.
- پوشه‌ها، reparse pointها، نام GUID نامعتبر، فایل‌های متعلق به store دیگر و فایل‌های غیرقابل‌دسترسی حذف نمی‌شوند.
- خطای دسترسی یا فهرست‌کردن پوشه مانع راه‌اندازی میزبان نمی‌شود؛ نامزدهای باقی‌مانده در startup بعدی دوباره بررسی می‌شوند.

## فایل‌های واقعی تغییر‌یافته

- `src/Host.Windows/Chat/WindowsHostConversationStore.cs`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `docs/security.md`
- `step three/T0182/execution-report.md`
- `step three/T0182/evidence/verification.md`
- `step three/project-graph.md`

## آزمون‌های CI دقیق V 3.0.842

- Step Three quality gate — run `41636799068`: **۳۴۴ آزمون Python موفق**؛ `compileall` موفق؛ graph guard برای ۲۳۰ وظیفه و ۴۲۲ یال با صفر خطا و یک هشدار metadata قدیمی T0001.
- Rendezvous container — run `41636799141`: ساخت container و smoke با کاربر non-root، health، announcement، discovery و ورودی نامعتبر موفق.
- HostLifecycle — run `41636799070` / job `125919924364`: **موفق**. log دقیقاً ثبت کرده است: `PASS: in-memory conversation, explicit retention consent, Windows DPAPI history and orphaned temporary-file cleanup.`
- Windows WPF — run `41636799070` / job `125919923959`: `dotnet format` و Release build موفق با صفر warning و صفر error؛ startup smoke و DPAPI identity/trust checks موفق. artifact میزبان `13231511016`، ۱۴٬۲۷۵٬۴۳۳ بایت.
- Android — run `41636799070` / job `125919924294`: KtLint، APK debug، JVM unit tests، Protobuf Java/Kotlin و بارگذاری artifactها موفق. APK `13230882593`، ۱۹٬۵۳۱٬۹۸۲ بایت؛ گزارش JVM `13231970969`، ۵۱٬۲۲۰ بایت.
- Real local inference integration — run `41636799166`: موفق؛ شاهد تکمیلی محصول روی همین commit.

## دامنه و محدودیت

این cleanup فقط فایل‌های موقت متعلق به `WindowsHostConversationStore` را هدف می‌گیرد. پاک‌سازی pagefile، crash dump، backup سیستم‌عامل، logها یا فایل‌های موقت سایر قسمت‌های برنامه در دامنهٔ T0182 نیست.

**وضعیت نهایی:** همهٔ معیارهای T0182 با کد واقعی و آزمون‌های CI قابل‌بررسی احراز شده‌اند؛ وظیفه در گراف `done` است.
