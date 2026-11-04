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
