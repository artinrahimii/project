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
