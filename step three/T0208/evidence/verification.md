# شواهد و راستی‌آزمایی T0208

## وابستگی‌ها
- T0207 موفقیت CI پاک را فراهم کرد؛ این رابطه صرفاً ترتیبی است و یال blocks حفظ شده است.
- T0195 فایل‌های تست واقعی برای failure/resilience را فراهم می‌کند که در گزارش بحرانی و پوشش لحاظ می‌شوند؛ وابستگی فنی ضروری است و یال blocks حفظ شده است.

## خروجی‌های ایجادشده
- .github/workflows/product-build.yml: تولید و بارگذاری گزارش‌های پوشش Android و HostLifecycle و log/نتایج بحرانی.
- src/Client.Android/app/build.gradle.kts: فعال‌سازی پوشش واحد و instrumentation در variant دیباگ.
- tests/validate_coverage_artifacts.py: اعتبارسنجی ساختار گزارش Cobertura/HTML/JUnit و ساخت خلاصه JSON.
- tests/test_validate_coverage_artifacts.py: آزمون‌های parser/validator.

## وضعیت آزمون
آزمون‌های CI برای commit این تغییرات هنوز اجرا نشده‌اند. هیچ نرخ پوشش یا تعداد آزمون تولیدشده‌ای پیش از خواندن گزارش‌های واقعی ادعا نمی‌شود. در صورت نبود گزارش، صفر بودن آزمون یا شکست parser، job باید شکست بخورد و artifact ناقص به‌عنوان موفق منتشر نشود.

## CI checkpoint و اصلاح A002

- Quality Gate run 41928891108 روی commit V 3.0.949 به علت assertion تعداد artifactها شکست خورد: انتظار قدیمی ۴، مقدار جدید ۶. از ۴۱۴ آزمون، ۴۱۳ موفق بودند.
- شش آزمون جدید validator گزارش پوشش موفق بودند.
- اصلاح tests/test_reproducible_build_contract.py انتظارهای artifact/count/path را با workflow جدید هماهنگ می‌کند و صراحتاً وجود Android coverage tasks و HostLifecycle collector را بررسی می‌کند.
- Product Build run 41928891017 هنگام ثبت checkpoint هنوز کامل نشده بود. هیچ نرخ پوشش یا قبولی آزمون‌های محصول از روی این وضعیت اعلام نمی‌شود.
- T0208 پس از ثبت A002 هنوز تا اجرای موفق Quality Gate و Product Build، تولید واقعی گزارش‌های پوشش و بارگذاری artifactها در وضعیت in_progress باقی می‌ماند.

## شواهد CI روی V 3.0.950

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/41929223701 — success، هر ۴۱۴ آزمون و بررسی‌های گراف/evidence موفق.
- Product Build: https://github.com/artinrahimii/project/actions/runs/41929223424
- HostLifecycle job 126921549109 — ۱۳/۱۳ موفق؛ artifact 13369539243؛ Cobertura: 8,849/10,325 lines (85.70%) و 2,485/3,789 branches (65.58%)؛ log شامل ۶۷ PASS و یک SKIP.
- Windows WPF job 126921549182 — ۱۶/۱۶ موفق؛ artifact 13370841360.
- Android job 126921548656 در آخرین checkpoint ۱۵/۲۹ گام موفق داشت؛ clean APK build و حداقل مجوزها موفق بودند و JVM unit coverage در حال اجرا بود. Android coverage metrics در این checkpoint موجود نیست.
- T0208 در وضعیت in_progress حفظ شده؛ هیچ پوشش یا قبولی Android پیش از دریافت artifact واقعی ادعا نمی‌شود.


## A003 — راستی‌آزمایی روی commit V 3.0.951

- Commit/branch: `34eaca4b3a0ba5055705241cca14894b896078d2` / `main`.
- Quality Gate run [41930272372](https://github.com/artinrahimii/project/actions/runs/41930272372): success؛ ۴۱۴ آزمون Python، compileall، graph validation و recorded-output/evidence validation همگی موفق.
- Product Build run [41930272240](https://github.com/artinrahimii/project/actions/runs/41930272240): success؛ هر سه job ویندوز، اندروید و HostLifecycle موفق.
- Windows WPF job `126925093390`: build پاک و smoke test راه‌اندازی موفق؛ artifact `13370524858` با اندازهٔ ۱۴٬۲۸۱٬۶۵۳ بایت.
- Android job `126925092909`: ۲۲۴ تست JUnit از ۳۴ فایل؛ ۲۲۳ passed، یک skipped، صفر failure/error. دو HTML coverage report به‌ترتیب ۱۱٬۶۵۱ و ۱۱٬۴۶۹ بایت توسط validator پذیرفته شدند. artifactهای APK، instrumentation، JUnit و coverage با شناسه‌های `13371462924`، `13371163313`، `13370878545` و `13371418035` منتشر شدند.
- تست skipشدهٔ Android: `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan`؛ پارامترهای peer/IP/port به workflow عادی داده نشده بود. این مورد اجرای LAN واقعی نیست و همچنان به T0202 مربوط است.
- HostLifecycle job `126925093261`: Cobertura معتبر، ۸٬۸۴۹/۱۰٬۳۲۵ خط (۸۵٫۷۰٪)، ۶۷ PASS و یک SKIP؛ artifact `13371976216` با اندازهٔ ۱۱۰٬۹۲۰ بایت شامل Cobertura، JSON summary و log کامل.
- مورد skipشدهٔ HostLifecycle مربوط به real-model integration opt-in است؛ اجرای مدل واقعی در این CI صورت نگرفته است.
- Windows build artifact و گزارش‌های coverage/critical در GitHub Actions با retention هفت‌روزه موجود بودند؛ اندازه‌ها و IDها در گزارش اجرای T0208 ثبت شده‌اند.
- Rendezvous run [41930272348](https://github.com/artinrahimii/project/actions/runs/41930272348): success؛ اجرای non-root container smoke test برای health، announcement، discovery و رد ورودی نامعتبر.

**نتیجه:** معیار پذیرش خود T0208 پاس است. آزمون LAN مستقل و آزمون مدل واقعی همچنان انجام‌نشده/skip هستند و با این نتیجه تکمیل‌شده محسوب نمی‌شوند. clone و اجرای محلی در این محیط به‌علت خطای DNS ممکن نشد؛ CI روی commit دقیق بالا مبنای شواهد این پذیرش است.
