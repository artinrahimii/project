# بازبینی محتوایی T0053 — آزمون ظرفیت ناکافی و backend پشتیبانی‌نشده

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs` — Blob SHA: `fd4635da2b59af198533dbc5438c241067685f6e`
- `tests/HostLifecycle.Tests/Program.cs` — Blob SHA: `e63132c55538e289e5f49840943e35349e758052`
- `tests/test_host_resource_settings.py` — Blob SHA: `ef72305a3b0f9bdb917beaf5cf744ea083ca30ee`

## یافته‌های کد و آزمون
- HostCapacityAcceptanceTests مسیرهای low RAM، low disk و unsupported backend را پوشش می‌دهد؛ harness اصلی Unknown capacity را نیز بررسی می‌کند.
- اجرای HostLifecycle روی HEAD جاری پیام PASS مشخص برای low-RAM، low-disk، unsupported-backend و unknown-capacity ثبت کرده است.
- tests/test_host_resource_settings.py دوام تنظیمات RAM/disk، migration schema و admission fail-closed/single-inference را بررسی می‌کند.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- پس از تغییر معیارهای admission یا افزودن منبع پشتیبانی‌شده در UPD-S1-013، ماتریس آزمون باید برای هر قید جدید توسعه و دوباره اجرا شود.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.
