# بازبینی محتوایی T0056 — نمایش ظرفیت نامشخص به‌صورت Unknown

## خط مبنا و تصمیم
- شاخه: `main`
- commit کد بررسی‌شده: `a19595c475752832fdd3701e109dc0e1a088721f` — `V 3.1.77`
- وضعیت پایه: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-005`، `UPD-S1-013`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌های بررسی‌شده و Blob SHA
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `src/Host.Windows/MainWindow.xaml` — `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/App.xaml.cs` — `69a15e938db4d291485d94270deb37a92d3b3d08`
- `src/Host.Domain/HostCapabilities.cs` — `25a1454fd81d1529d5b96ae928640ea5e2100e5f`
- `tests/test_windows_hardware_discovery.py` — `b6c7de34baf195f3df96e8d789b2a181fe2ce360`
- `tests/test_wpf_entrypoint.py`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`

## یافته‌های کد و مصرف‌کنندگان
- HostCapabilities اندازه‌گیری ناقص یا بررسی انجام‌نشده را Unknown نگه می‌دارد و CanRunInference فقط برای Ready واقعی true می‌شود.
- MainWindow هنگام شکست اندازه‌گیری زنده، Capacity را Unknown و میزبان را not Ready نشان می‌دهد؛ جزئیات CPU/RAM ناموجود نیز با پیام امن نمایش داده می‌شوند.
- Python source contracts و HostLifecycle Program مسیر Unknown را پوشش می‌دهند؛ Quality CI دقیق V3.1.77 موفق شده است.

## محدودیت و trigger بازبینی مجدد
- UPD-S1-013 هنوز سیاست نهایی ظرفیت و admission را اعمال نکرده است؛ هر منبع یا قید جدید باید پس از آن در UI و domain دوباره بررسی شود.

## CI و مرز شواهد
- Quality V3.1.77: [run 42502195218](https://github.com/artinrahimii/project/actions/runs/42502195218) موفق.
- Product Build V3.1.77: [run 42502195216](https://github.com/artinrahimii/project/actions/runs/42502195216) هنگام ثبت بررسی هنوز در حال اجرا بود.
- آزمون‌های قبلی همان وظیفه در تاریخچه حفظ شده‌اند؛ CI Product جدید هنوز نتیجهٔ نهایی نداشت.
- دستگاه فیزیکی، inference زنده در همین attempt، Android-to-Windows E2E و upgrade فیزیکی اجرا نشده‌اند.
