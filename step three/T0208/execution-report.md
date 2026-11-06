# گزارش اجرای T0208

## هدف
انتشار گزارش پوشش کد و نتایج آزمون‌های بحرانی به‌عنوان artifactهای قابل دریافت از GitHub Actions.

## بازبینی وابستگی‌ها
- T0207 -> T0208: طبقه‌بندی sequencing_only و اعتبارسنجی شد. T0208 خط مبنای CI پاک و موفق را گسترش می‌دهد و API یا باینری اختصاصی از T0207 مصرف نمی‌کند؛ یال اصلی blocks حفظ شد.
- T0195 -> T0208: طبقه‌بندی hard_dependency و اعتبارسنجی شد. گزارش بحرانی/پوشش باید سناریوهای واقعی خطا، قطع دانلود، کمبود فضا، لغو عملیات و failure handling ساخته‌شده در T0195 را پوشش دهد؛ یال اصلی blocks حفظ شد.

## پیاده‌سازی A001
- در build variant دیباگ Android، پوشش آزمون‌های JVM و instrumentation فعال شد و jobهای CI taskهای تولید گزارش HTML را اجرا می‌کنند.
- در job مربوط به HostLifecycle، مجموعه‌آزمون Console app از runner پاک ساخته و زیر پوشش dotnet-coverage اجرا می‌شود؛ خروجی Cobertura و log شامل PASS marker ذخیره می‌شوند.
- اسکریپت tests/validate_coverage_artifacts.py گزارش Cobertura، HTML، JUnit XML، شمار آزمون‌ها و log بحرانی را اعتبارسنجی و خلاصه JSON تولید می‌کند.
- آزمون‌های واحد Python برای گزارش‌های معتبر/نامعتبر افزوده شد.

## نتیجه
پیاده‌سازی در commit جاری ثبت می‌شود. هنوز نتیجه CI این تغییرات دریافت نشده است؛ بنابراین T0208 در وضعیت in_progress باقی می‌ماند و تا تولید و اعتبارسنجی واقعی گزارش‌ها و بارگذاری artifactها تکمیل‌شده اعلام نمی‌شود.

## منابع فنی
- Android Gradle Plugin code coverage: https://developer.android.com/studio/test/coverage-report
- Microsoft dotnet-coverage: https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage
- dotnet-coverage 18.12.0: https://www.nuget.org/packages/dotnet-coverage/18.12.0

## A001 — نتیجهٔ نخستین CI روی V 3.0.949

- Quality Gate run 41928891108 اجرا شد: ۴۱۴ آزمون Python؛ ۴۱۳ موفق و یک شکست.
- شکست در آزمون موجود test_product_builds_publish_real_artifacts_and_fail_if_missing بود. این آزمون هنوز تعداد upload-artifactها را ۴ فرض می‌کرد، درحالی‌که دو artifact جدید پوشش Android و HostLifecycle تعداد را به ۶ افزایش داده‌اند.
- هر شش آزمون جدید tests/test_validate_coverage_artifacts.py موفق شدند.
- Product Build run 41928891017 در زمان checkpoint هنوز در حال اجرا بود؛ jobهای Windows، Android و HostLifecycle در جریان بودند.

## A002 — اصلاح قرارداد CI

آزمون test_reproducible_build_contract.py به‌روزرسانی شد تا شش artifact upload، چهار مسیر fail-if-missing، شش retention declaration، دو task پوشش Android و collector پوشش HostLifecycle را بررسی کند. اجرای این اصلاح در CI commit بعدی هنوز تأیید نشده است؛ بنابراین T0208 همچنان in_progress می‌ماند.

## A002 — نتایج CI تأییدشده روی V 3.0.950

- Step Three Quality Gate run 41929223701 موفق شد: هر ۴۱۴ آزمون Python، بررسی syntax، اعتبارسنجی ساختار گراف و بررسی evidenceهای ثبت‌شده موفق بودند.
- Product Build run 41929223424:
  - HostLifecycle job 126921549109: ۱۳/۱۳ گام موفق. پوشش خطی ۸۵٫۷۰٪ (۸٬۸۴۹ از ۱۰٬۳۲۵ خط)، پوشش شاخه‌ای ۶۵٫۵۸٪ (۲٬۴۸۵ از ۳٬۷۸۹ شاخه)، ۶۷ نشانگر PASS و یک SKIP. artifact شماره 13369539243 شامل Cobertura XML، خلاصه JSON و log کامل است.
  - Windows WPF job 126921549182: ۱۶/۱۶ گام موفق؛ artifact میزبان شماره 13370841360 با اندازهٔ ۱۴٬۲۸۱٬۶۲۷ بایت منتشر شد.
  - Android job 126921548656: در checkpoint آخر ۱۵/۲۹ گام موفق بود؛ SDK و ابزارها، پاک‌سازی Android، KtLint، ساخت APK پاک و بررسی حداقل مجوزها موفق شدند. آزمون JVM همراه تولید گزارش پوشش در حال اجرا بود؛ instrumentation، اعتبارسنجی JUnit/coverage و بارگذاری artifactهای Android هنوز تأیید نشده بودند.
- **وضعیت وظیفه:** T0208 همچنان in_progress است. تا زمانی که Android coverage HTML، JUnit XML، گزارش خلاصه و artifactهای APK/Keystore/coverage با موفقیت اعتبارسنجی و بارگذاری نشوند، وظیفه بسته نمی‌شود.


## A003 — پذیرش نهایی CI روی commit V 3.0.951

**commit بررسی‌شده:** `34eaca4b3a0ba5055705241cca14894b896078d2` روی شاخهٔ `main`  
**زمان اجراهای CI:** ۴ نوامبر ۲۰۲۶، ۲۳:۰۱:۲۰ تا ۲۳:۱۰:۲۸ UTC

### اجرای کنترل کیفیت
- Step Three Quality Gate run [41930272372](https://github.com/artinrahimii/project/actions/runs/41930272372) با موفقیت کامل شد.
- در این اجرا ۴۱۴ آزمون Python اجرا و همگی موفق شدند.
- `python -m compileall -q tools tests`، اعتبارسنجی ساختار گراف و اعتبارسنجی مسیرهای خروجی/شواهد نیز موفق شدند.

### اجرای ساخت محصول
- Product Build run [41930272240](https://github.com/artinrahimii/project/actions/runs/41930272240) با نتیجهٔ success تکمیل شد.
- Windows WPF job `126925093390`: تمام گام‌ها موفق؛ build پاک، بررسی C#، Protobuf، smoke test راه‌اندازی و کنترل‌های سیاست اجرا شدند.
- Android job `126925092909`: تمام گام‌ها موفق؛ KtLint، ساخت APK پاک، کنترل مجوزها، آزمون‌های JVM، instrumentation، تولید coverage، اعتبارسنجی گزارش و بارگذاری artifactها موفق شدند.
- HostLifecycle job `126925093261`: تمام گام‌ها موفق؛ آزمون پذیرش، جمع‌آوری Cobertura، اعتبارسنجی log بحرانی و بارگذاری artifact موفق شدند.
- Rendezvous container run [41930272348](https://github.com/artinrahimii/project/actions/runs/41930272348) نیز با موفقیت اجرا شد؛ کانتینر non-root و health/announcement/discovery/invalid-input smoke test پاس شدند.

### نتایج واقعی گزارش‌ها
- Android: دو گزارش HTML کامل با اندازه‌های ۱۱٬۶۵۱ و ۱۱٬۴۶۹ بایت اعتبارسنجی شدند. از ۳۴ فایل JUnit، مجموع ۲۲۴ مورد آزمون ثبت شد: ۲۲۳ موفق، ۱ مورد skip، صفر failure و صفر error.
- مورد skipشدهٔ Android آزمون `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan` است؛ اجرای آن به پارامترهای شبکهٔ واقعی workflow دستی نیاز دارد. این مورد را موفق محسوب نمی‌کنم و آزمون LAN واقعی همچنان در محدودهٔ پذیرش جداگانهٔ T0202 باقی می‌ماند.
- HostLifecycle: گزارش Cobertura معتبر با ۸٬۸۴۹ خط پوشش‌یافته از ۱۰٬۳۲۵ خط (۸۵٫۷۰٪) تولید شد؛ log آزمون ۶۷ نشانگر PASS و ۱ نشانگر SKIP داشت. مورد skipشده، آزمون مدل واقعی opt-in است که به مدل و تنظیمات محلی نیاز دارد؛ اجرای آن در این CI ادعا نمی‌شود.

### artifactهای تأییدشده
تمام artifactهای زیر به run شمارهٔ 41930272240 تعلق دارند و در زمان بررسی منقضی نشده بودند:
- Windows host: `13370524858`، اندازهٔ ۱۴٬۲۸۱٬۶۵۳ بایت.
- Android debug APK: `13371462924`، اندازهٔ ۲۸٬۹۷۲٬۴۷۷ بایت.
- Android instrumentation reports: `13371163313`، اندازهٔ ۸۲٬۲۱۵ بایت.
- Android unit-test reports: `13370878545`، اندازهٔ ۹۱٬۰۸۶ بایت.
- Android coverage و critical results: `13371418035`، اندازهٔ ۵٬۳۶۸ بایت.
- HostLifecycle coverage و critical results: `13371976216`، اندازهٔ ۱۱۰٬۹۲۰ بایت.

### نتیجهٔ پذیرش
معیار T0208 با شواهد CI احراز شد: خروجی واقعی، parser/validator، آزمون‌های منبع، گزارش‌های پوشش و نتایج آزمون‌های بحرانی در artifactهای قابل دریافت منتشر شده‌اند. بنابراین وضعیت وظیفه از `in_progress` به `done` تغییر می‌کند. دو مورد skip بالا به‌صورت صریح حفظ می‌شوند و به‌عنوان موفقیت end-to-end یا اجرای مدل واقعی تفسیر نمی‌شوند.

**محدودیت محیط این نوبت:** clone محلی به‌دلیل عدم resolve شدن `github.com` ممکن نشد؛ بنابراین آزمون محلی اجرا نشده است. پذیرش این نوبت بر اساس logها، jobها و artifactهای واقعی GitHub Actions روی commit دقیق بالا انجام شد.
