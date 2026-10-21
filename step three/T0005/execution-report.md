# T0005 — گزارش اجرای واقعی

- تاریخ بازبینی: 2026-10-21.
- شاخهٔ هدف: `main`; مبنای این تلاش: `90d48be3129cc1d721e0bc5311ec611ba5ccf7b7` (`V 3.0.26`).
- نتیجهٔ تلاش دوم: `succeeded` برای معیار build واقعی Windows و Android.
- پیش‌نیاز T0004: گراف آن را `done` ثبت کرده؛ وضعیت یال تغییر نکرده و طبقه‌بندی `unreviewed` حفظ شده است.
- شواهد build واقعی:
  - [White-P product build — run 39641644577](https://github.com/artinrahimii/project/actions/runs/39641644577): نتیجهٔ workflow برابر `success`.
  - job `Build Windows WPF host` (ID `119366422112`): موفق؛ گام `Build host` موفق.
  - job `Build Android client APK` (ID `119366421861`): موفق؛ پذیرش مجوزها، نصب SDK و `Build debug APK` همگی موفق.
- مسیرهای واقعی ساخت: `src/Host.Windows/WhiteP.Host.Windows.csproj` و `src/Client.Android/app/build.gradle.kts`؛ workflow: `.github/workflows/product-build.yml`.
- نتیجهٔ پذیرش: مسیر build واقعی وجود دارد و build آن در CI موفق است. این فقط کامپایل/تولید APK را اثبات می‌کند؛ اجرای تعاملی، inference محلی، سخت‌افزار یا P2P را اثبات نمی‌کند.
- آزمون‌ها: build ویندوز و APK در run فوق موفق؛ quality gate گراف در [run 39641644748](https://github.com/artinrahimii/project/actions/runs/39641644748) نیز موفق.
- اقدام بعدی: T0006 بر اساس قابلیت‌های واقعاً پیاده‌سازی‌شده بررسی شود؛ قابلیت سخت‌افزاری/انتقالی که وجود ندارد، آزمون‌شده معرفی نشود.
