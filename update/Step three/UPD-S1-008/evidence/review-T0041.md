# بازبینی T0041 — نقطهٔ ورود واقعی WPF

- commit بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`.
- وضعیت پایهٔ T0041: `done`؛ پیش‌نیاز T0040 نیز `done` است.
- رابطهٔ `T0040 → T0041` در گزارش وظیفه به‌صورت `sequencing_only` ثبت شده و حفظ می‌شود؛ WPF entry point از مسیر `StartupUri/MainWindow` استفاده می‌کند و مستقیماً به `ProtocolMessageParser` وابسته نیست.
- نگاشت: `UPD-S1-001`، `UPD-S1-012` و `UPD-S1-024`. هر سه مرجع بررسی شدند؛ `UPD-S1-024` هنوز `not_started` است و مستقیماً icon، palette و سطوح UI هر دو برنامه را تغییر می‌دهد.
- تصمیم reconciliation: `re_review_required`؛ وضعیت پایهٔ `done` تغییر نمی‌کند.

## بررسی کد فعلی و شواهد اجرایی
- `App.xaml` مسیر واقعی `StartupUri="MainWindow.xaml"` را تعریف می‌کند.
- `App.xaml.cs` حالت `--smoke-test` را در process واقعی WPF اجرا می‌کند، تا dispatcher idle منتظر می‌ماند، نمایش پنجره و کنترل‌های کلیدی/وضعیت اولیه را بررسی می‌کند و در پایان تمیز خارج می‌شود؛ خطا یا نبود کنترل باید exit code غیرصفر بدهد.
- `tests/test_wpf_entrypoint.py` قرارداد نقطهٔ ورود، کنترل‌ها، cleanup و اجرای process-level smoke را نگهبانی می‌کند.
- Blob SHA این فایل‌ها در V3.1.70 با commit کامل Product CI برابر است:
  - `App.xaml`: `d527701f91d3d97833163003c70393af3fe8c0b6`
  - `App.xaml.cs`: `69a15e938db4d291485d94270deb37a92d3b3d08`
  - `MainWindow.xaml`: `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
  - `MainWindow.xaml.cs`: `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
  - `tests/test_wpf_entrypoint.py`: `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`
  - `product-build.yml`: `64fd1fb9695ae0aeb593cdb5b1499a9786c16f2a`
- Product CI روی commit `a3fa1f54d99db809cc670ece1cfc244239f6a727`، run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544)، با نتیجهٔ success پایان یافت؛ هر سه job Windows WPF، Android APK و HostLifecycle موفق بودند.
- شواهد اختصاصی وظیفه نیز Step Three quality run [42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) و Product smoke run [39862296386](https://github.com/artinrahimii/project/actions/runs/39862296386) را موفق ثبت کرده است. Product workflow فعلی V3.1.70 در زمان این بازبینی هنوز in_progress بوده و موفقیت آن ادعا نمی‌شود.

## نتیجه و محدودیت
- T0041 در baseline خود `done` می‌ماند؛ تصمیم فعلی `re_review_required` و وضعیت بازبینی `reviewed` است.
- پس از اجرای UPD-S1-024 باید آیکون/تم و layoutهای واقعی صفحهٔ اصلی Windows دوباره بررسی شوند.
- Smoke test فقط startup و بارگذاری پنجره را اثبات می‌کند؛ نصب روی سیستم فیزیکی، بارگذاری مدل محلی، inference زنده و Android-to-Windows/P2P در این بازبینی آزموده نشده‌اند.
