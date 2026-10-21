# T0014 — گزارش اجرای واقعی

- وضعیت: در حال اجرا؛ Android build هنوز تأیید نشده است.
- تنظیمات واقعی: `global.json` روی .NET SDK `10.0.100`؛ Android compile/target SDK برابر 36 و minSdk برابر 26.
- شواهد: ساخت WPF ویندوز در runهای [39640743619](https://github.com/artinrahimii/project/actions/runs/39640743619) و [39641116809](https://github.com/artinrahimii/project/actions/runs/39641116809) موفق بود.
- شکست‌های Android ثبت‌شده: تلاش اول اکشن SDK بستهٔ منسوخ `tools` را خواست؛ تلاش دوم `sdkmanager` را در PATH نیافت؛ تلاش سوم workflow به‌علت YAML نامعتبر اصلاً job ایجاد نکرد.
- اصلاح فعلی: پیدا کردن `sdkmanager` در زیر `ANDROID_HOME` و اجرای فرمان نصب به‌شکل YAML block scalar.
- نتیجه: تا اجرای واقعی و موفق build اندروید، T0014 تکمیل‌شده محسوب نمی‌شود.
