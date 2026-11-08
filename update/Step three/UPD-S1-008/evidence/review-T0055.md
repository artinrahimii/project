# بازبینی محتوایی T0055 — نمایش CPU، GPU inventory و RAM در رابط میزبان

## خط مبنا و تصمیم
- شاخه: `main`
- commit دقیق کد بررسی‌شده: `6c77fded48253e0b9ddbadd37033e788a43944a5` — `V 3.1.76`
- وضعیت پایهٔ وظیفهٔ قدیمی: `done`؛ بدون تغییر
- وظایف به‌روزرسانی مرتبط: `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-024`
- تصمیم: `re_review_required`
- وضعیت بازبینی رکورد: `reviewed`
- `status_changed=false`

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Windows/WindowsHostHardwareDiscovery.cs` — Blob SHA: `a3ee29c8d112453ab58a53d5029f153f535ff648`
- `src/Host.Windows/MainWindow.xaml` — Blob SHA: `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/MainWindow.xaml.cs` — Blob SHA: `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `src/Host.Windows/App.xaml.cs` — Blob SHA: `69a15e938db4d291485d94270deb37a92d3b3d08`
- `tests/test_windows_hardware_discovery.py` — Blob SHA: `b6c7de34baf195f3df96e8d789b2a181fe2ce360`
- `tests/test_wpf_entrypoint.py` — Blob SHA: `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`

## یافته‌های کد و آزمون
- MainWindow نام CPU، تعداد logical processor، RAM کل/آزاد و display adapterها را از WindowsHostHardwareDiscovery می‌گیرد؛ WPF startup smoke صفحه و کنترل‌های لازم را بازبینی می‌کند.
- رابط صریحاً display inventory را از GPU compute/VRAM capability تفکیک می‌کند و آن را آمادهٔ پردازش فرض نمی‌کند.
- tests/test_windows_hardware_discovery.py و tests/test_wpf_entrypoint.py source wiring را بررسی می‌کنند؛ Product CI جاری نیز MainWindow startup و clean shutdown را تأیید کرد.

## CI روی همان HEAD
- Quality Gate: [run 42486973958](https://github.com/artinrahimii/project/actions/runs/42486973958) — موفق؛ Python tests، compileall و هر دو graph guard.
- Product Build: [run 42486974256](https://github.com/artinrahimii/project/actions/runs/42486974256) — سه job Windows، Android و HostLifecycle موفق.
- Windows WPF job: build با صفر خطای کامپایل؛ startup smoke، نمایش MainWindow و کنترل‌ها، و installer smoke موفق.
- HostLifecycle: ۶۷ نشانگر PASS، یک آزمون اختیاری real-model SKIP، پوشش خطی ۸۵٫۸۸٪. این SKIP موفقیت inference واقعی نیست.

## تصمیم و محدودیت
- پس از UPD-S1-013 باید readiness و محدودیت‌های قابل اعمال در UI منعکس شوند؛ پس از UPD-S1-024 صفحات، رنگ و آیکون باید دوباره بررسی شوند.
- این بازبینی بر کد و CI مخزن متکی است؛ دستگاه Android فیزیکی، Android-to-Windows واقعی، clean install هدف و in-place upgrade در این بازبینی اجرا نشده‌اند.
