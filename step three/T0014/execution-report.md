# T0014 — گزارش اجرای واقعی

- وضعیت: در حال اجرا؛ Android SDK/build هنوز تأیید نشده است.
- تلاش‌ها: T0014-A001 و T0014-A002.
- مبنا: `bc8f9fd537a28bd64042f2e1b48b435902b90dbf` (`V 3.0.23`).
- خروجی: `global.json` با .NET SDK `10.0.100` و `src/Client.Android/app/build.gradle.kts` با compile/target SDK برابر 36 و minSdk برابر 26.
- شاهد آزمون: در run [39640743619](https://github.com/artinrahimii/project/actions/runs/39640743619)، ساخت WPF ویندوز موفق شد. آماده‌سازی Android پیش از اجرای Gradle شکست خورد، چون اکشن setup-android بستهٔ منسوخ `tools` را درخواست کرد.
- اصلاح: workflow به‌جای اکشن مشکل‌دار، بسته‌های `platforms;android-36`, `build-tools;36.0.0` و `platform-tools` را صریح نصب می‌کند.
- نتیجه: build موفق Android یا تکمیل T0014 هنوز ادعا نمی‌شود؛ اجرای جدید CI باید بررسی شود.
