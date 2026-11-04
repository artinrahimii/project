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
