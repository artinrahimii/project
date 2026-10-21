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

## اصلاح در V 3.0.28

- آخرین quality gate روی commit `8e1a4431fd7a83989ad571d20d78070a4aafafde` شکست خورد؛ آزمون‌های واحد (۲۹) و syntax موفق بودند، اما graph guard خطاهای `T0005` فاقد شاهد فایل آزمون و شمارنده‌های تلاش ناهماهنگ برای `T0014` و `T0016` را گزارش کرد.
- فایل `tests/test_product_build_contract.py` اضافه شد تا تنظیمات واقعی build ویندوز، Android SDK و فرمان‌های CI را بررسی کند. این آزمون قراردادی است، نه اثبات اجرای برنامه روی دستگاه.
- `T0005.evidence` و `actual_output_paths` به فایل آزمون اشاره می‌کنند؛ شمارنده‌های تلاش `T0014` و `T0016` با طول تاریخچهٔ واقعی برابر شدند (۵).
- اجرای CI برای همین تغییر هنوز در انتظار است؛ نتیجهٔ آزمون جدید تا دریافت نتیجهٔ workflow موفق اعلام نمی‌شود.


## اصلاح تکمیلی در V 3.0.29

- پس از افزودن فایل آزمون، quality gate در run [39646046000](https://github.com/artinrahimii/project/actions/runs/39646046000) آزمون‌ها، syntax و اعتبارسنجی ساختار گراف را گذراند، اما اعتبارسنجی خروجی‌ها شکست خورد چون فیلد `evidence` نشانی وبِ اجراهای CI را به‌عنوان مسیر فایل تفسیر می‌کند.
- دو URL از فیلد `T0005.evidence` حذف شدند؛ نشانی‌های CI همچنان در این گزارش و `execution_summary.tests_passed` حفظ می‌شوند. این اصلاح با قرارداد graph guard سازگار است و شواهد را حذف نمی‌کند.
- آزمون قراردادی `tests/test_product_build_contract.py` در گراف به‌عنوان فایل واقعی و شواهد آزمون باقی می‌ماند. نتیجهٔ CI روی commit تازه باید دوباره بررسی شود.
