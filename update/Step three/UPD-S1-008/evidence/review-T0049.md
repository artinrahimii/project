# بازبینی محتوایی T0049 — کشف سیستم‌عامل، RAM و فضای دیسک

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Windows/WindowsHostHardwareDiscovery.cs` — Blob SHA: `a3ee29c8d112453ab58a53d5029f153f535ff648`
- `src/Host.Windows/App.xaml.cs` — Blob SHA: `69a15e938db4d291485d94270deb37a92d3b3d08`
- `tests/test_windows_hardware_discovery.py` — Blob SHA: `b6c7de34baf195f3df96e8d789b2a181fe2ce360`

## یافته‌های کد و آزمون
- سرویس Windows-only اطلاعات OS/version را از RuntimeInformation/Environment می‌خواند؛ CPU name از Registry و تعداد پردازندهٔ منطقی از Environment.ProcessorCount دریافت می‌شود.
- RAM واقعی با GlobalMemoryStatusEx و ظرفیت/free space درایو سیستم با DriveInfo اندازه‌گیری و sanity-check می‌شوند.
- آزمون Windows startup در تاریخچهٔ وظیفه Discover() را روی runner ویندوز فراخوانی کرده و OS، RAM غیرصفر و ظرفیت دیسک را بررسی کرده است؛ source contract نیز API و سیم‌کشی startup را می‌سنجد.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- محدودیت‌های نهایی مالک و admission باید پس از اجرای UPD-S1-013 روی سرویس اندازه‌گیری اعمال و دوباره آزموده شوند.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.
