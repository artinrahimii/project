# T0014 — گزارش اجرای واقعی

- وضعیت گره: `in_progress؛ build ویندوز و Android موفق‌اند، اما پیش‌نیازهای گرافی `T0013` و `T0010` باید جداگانه تأیید شوند.
- تنظیمات واقعی: `global.json` روی .NET SDK `10.0.100`؛ Android compile/target SDK برابر 36 و minSdk برابر 26.
- شواهد: [White-P product build — run 39641644577](https://github.com/artinrahimii/project/actions/runs/39641644577) موفق است؛ job ویندوز `119366422112` و job Android `119366421861` هر دو موفق‌اند.
- job Android گام‌های پیدا کردن `sdkmanager`، پذیرش مجوزها، نصب `platforms;android-36` و `build-tools;36.0.0` و ساخت debug APK را با موفقیت گذراند.
- شکست‌های قبلی (بستهٔ منسوخ، مسیر sdkmanager و YAML نامعتبر) در تاریخچه حفظ شده‌اند.
- نتیجه: شواهد build برای هر دو پلتفرم موجود است؛ اما به‌دلیل پیش‌نیازهای تکمیل‌نشدهٔ T0013 و T0010، وظیفه هنوز `done` نمی‌شود.
