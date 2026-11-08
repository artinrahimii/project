# بازبینی محتوایی T0051 — تخمین سازگاری حافظهٔ مدل

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Windows/ModelMemoryCompatibilityEstimator.cs` — Blob SHA: `9b528770c1eeecf7c7f1220812f47ddb0301cd58`
- `src/Host.Windows/WindowsHostHardwareDiscovery.cs` — Blob SHA: `a3ee29c8d112453ab58a53d5029f153f535ff648`
- `src/Host.Windows/MainWindow.xaml` — Blob SHA: `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/MainWindow.xaml.cs` — Blob SHA: `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/test_windows_hardware_discovery.py` — Blob SHA: `b6c7de34baf195f3df96e8d789b2a181fe2ce360`

## یافته‌های کد و آزمون
- Estimator مقدار مثبت requiredMemoryBytes را با AvailablePhysicalMemoryBytes مقایسه و ورودی غیرمثبت را رد می‌کند.
- پیام نتیجه صریحاً اعلام می‌کند GPU/VRAM، context length، فرمت مدل و overhead backend ارزیابی نشده‌اند؛ این برآورد محدود RAM است، نه تأیید قطعی سازگاری مدل.
- تست‌ها استفاده از RAM آزاد واقعی، رد ورودی نامعتبر و نمایش محدودیت‌ها در UI را بررسی می‌کنند.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- سیاست انتخاب مدل و محدودیت خروجی هنوز نیازمند اجرای UPD-S1-023 است؛ estimator به‌تنهایی تأیید کامل مدل/backend را فراهم نمی‌کند.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.
