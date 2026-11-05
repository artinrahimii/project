# شواهد راستی‌آزمایی T0209

- شاخهٔ هدف: main
- commit مبنا: 5596ba37a03542277e980ba8a8397f310c263be5 (V 3.0.952)
- پیش‌نیاز T0208: گزارش و شواهد CI همان commit بررسی شد؛ Windows، Android و HostLifecycle موفق بوده‌اند.
- یال T0208 به T0209: sequencing_only و validated حفظ شد؛ هیچ یالی تغییر نکرد.

فایل‌های پیاده‌سازی:
- tools/installer/WhiteP.iss
- tools/build_windows_installer.ps1
- tools/generate_dependency_licenses.py
- tests/test_generate_dependency_licenses.py
- .github/workflows/product-build.yml
- docs/windows-installer.md

موارد پذیرش که CI باید اثبات کند:
1. publish واقعی self-contained برای win-x64 موفق شود.
2. نسخه و ناشر executable و installer به‌ترتیب 0.1.0 و Artin Rahimi باشند.
3. تمام بسته‌های NuGet restore شده inventory مجوز داشته باشند و اطلاعات مفقود باعث fail شود.
4. installer واقعی ساخته شود و metadata نسخه/ناشر داشته باشد.
5. نصب بی‌صدا، وجود executable و ThirdPartyNotices، smoke launch و uninstall موفق شوند.
6. artifact با نام whitep-windows-installer در Product Build بارگذاری شود.

در این checkpoint موارد فوق روی runner ویندوز اجرا نشده‌اند؛ وضعیت done یا انتشار Release ثبت نمی‌شود. installer فاقد امضای Authenticode است.
