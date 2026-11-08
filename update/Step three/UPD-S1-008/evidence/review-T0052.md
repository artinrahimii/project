# بازبینی محتوایی T0052 — مدل دامنهٔ HostCapabilities

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Domain/HostCapabilities.cs` — Blob SHA: `25a1454fd81d1529d5b96ae928640ea5e2100e5f`
- `src/Host.Application/HostReadinessEvaluator.cs` — Blob SHA: `049e6be160eb712602e4a56cf22b52ade916b302`
- `tests/HostLifecycle.Tests/Program.cs` — Blob SHA: `e63132c55538e289e5f49840943e35349e758052`

## یافته‌های کد و آزمون
- HostCapabilities اندازه‌گیری RAM و system-drive را nullable نگه می‌دارد و بررسی‌های memory/disk/backend را جدا ثبت می‌کند.
- ظرفیت Unknown می‌ماند تا اندازه‌گیری‌ها معتبر و همهٔ بررسی‌ها مثبت باشند؛ fail شناخته‌شده state را Insufficient می‌کند و CanRunInference فقط برای Ready است.
- HostLifecycle CI روی HEAD دقیق، رفتار Unknown و fail-closed برای measurement ناقص/نامعتبر را PASS ثبت کرده است.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- UPD-S1-013 باید این مدل را با محدودیت‌های قابل‌اندازه‌گیری مالک و admission واقعی یکپارچه کند؛ شروط idle/power غیرقابل‌سنجش نباید حدس زده شوند.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.
