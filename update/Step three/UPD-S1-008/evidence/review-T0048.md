# بازبینی محتوایی T0048 — راه‌اندازی، خاموش‌کردن و بازیابی میزبان

## خط مبنا و تصمیم
- commit کد بررسی‌شده: `c933559b3be973f1cdf21e094b786c84742ca7ab` — `V 3.1.74`.
- وضعیت پایهٔ T0048: `done`.
- وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-012`.
- تصمیم: `re_review_required`؛ وضعیت قدیمی و تاریخچه تغییر نمی‌کند.

## پیاده‌سازی فعلی
- `HostStartup.Start()` میزبان را ابتدا Starting می‌کند؛ backend در دسترس به Ready می‌رسد، backend ناموجود یا exception به Error می‌رود و جزئیات exception در status افشا نمی‌شوند.
- آزمون‌های `tests/HostLifecycle.Tests/Program.cs` مسیر backend ناموجود، healthy، exception، reason خالی و recovery را اجرا می‌کنند: پس از failure وضعیت Ready نمی‌شود؛ با آماده‌شدن backend، retry همان lifecycle را به Ready می‌رساند.
- `HostInferenceSession` چرخهٔ اتصال، generation، cancellation، disconnect و dispose را مدیریت می‌کند.
- `HostInferenceSessionConcurrencyTests` قطع ارتباط و disposal را تا پایان generation جاری هماهنگ می‌کند؛ cancellation و drain عملیات فعال، disposal تکراری و توقف وضعیت idle را بررسی می‌کند.
- `MainWindow_Closing` کار UI را فوراً غیرفعال می‌کند، tokenهای participation/inference/model-update را cancel می‌کند، task شبکه را await می‌کند و سپس سرویس peer را dispose می‌کند. Process engine متعلق به برنامه نیز متوقف می‌شود؛ process backend خارجی kill نمی‌شود.
- Blobهای بررسی‌شده: `HostStartup.cs` = `fc062d2e717c1af9cdd94c2e8e6fa6f72c7768ab`؛ `HostInferenceSession.cs` = `2ccb69e748c5b40e89252925d1da80c1e259fab5`؛ `HostInferenceSessionConcurrencyTests.cs` = `0cfca8cca8e3c820a57505d6c090f2528767b833`؛ `Program.cs` = `e63132c55538e289e5f49840943e35349e758052`.

## آزمون و CI
- harness واقعی C# در Product CI run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) موفق شد؛ jobهای HostLifecycle، WPF build و WPF startup/close smoke با موفقیت پایان یافتند.
- Quality Gate روی HEAD دقیق V3.1.74، run [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109)، موفق شد.
- Product run دقیق V3.1.74، [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134)، در آخرین مشاهده `in_progress` بود؛ بنابراین موفقیت نهایی آن ادعا نمی‌شود.
- Blobهای source/test در V3.1.74 با فایل‌های همان مسیر در source snapshot موفق Product CI یکسان‌اند.

## وابستگی‌ها
- `T0047 → T0048`: `sequencing_only/validated`؛ آزمون startup recovery و disposal از HostStartup/HostInferenceSession استفاده می‌کند و تابع presentation خطا را به‌عنوان ورودی فنی مصرف نمی‌کند.
- `T0040 → T0048`: `sequencing_only/validated`؛ این آزمون‌ها مستقیماً parser/validator پروتکل را مصرف نمی‌کنند.
- هر دو یال `blocks` و ترتیب اصلی حفظ شده‌اند.

## چرا بازبینی مجدد لازم است؟
`UPD-S1-012` lifecycle را با opt-in، شروع/توقف listener، رد admission در حالت خاموش و توقف فوری مشارکت یکپارچه می‌کند. پس از تغییر باید shutdown race، cancellation، drain، retry و انتقال‌های جدید با سرویس P2P واقعی بازآزمایی شوند.

## خارج از محدودهٔ شواهد
آزمون‌های fake backend برای آزمون state/recovery مجازند اما اثبات اجرای مدل محلی واقعی نیستند. نصب روی دستگاه Windows فیزیکی، مدل GGUF واقعی، ارتباط Android-to-Windows، E2E محصول، clean install و in-place upgrade در این بازبینی اجرا نشده‌اند.
