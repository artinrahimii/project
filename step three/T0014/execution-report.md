# T0014 — گزارش اجرای واقعی

- وضعیت: در حال اجرا؛ Android SDK/build هنوز تأیید نشده است.
- تلاش‌ها: T0014-A001 تا T0014-A003.
- خروجی: `global.json` با .NET SDK `10.0.100` و `src/Client.Android/app/build.gradle.kts` با compile/target SDK برابر 36 و minSdk برابر 26.
- آزمون واقعی: ساخت WPF ویندوز در دو اجرای CI موفق شد. اجرای Android در تلاش دوم پیش از Gradle شکست خورد، چون `sdkmanager` در PATH نبود.
- اصلاح فعلی: workflow مسیر واقعی `sdkmanager` را در دایرکتوری SDK پیدا می‌کند و از همان مسیر برای پذیرش مجوزها و نصب platform/build-tools استفاده می‌کند.
- نتیجه: build موفق Android هنوز ادعا نمی‌شود؛ نتیجهٔ اجرای جدید CI باید بررسی شود.
