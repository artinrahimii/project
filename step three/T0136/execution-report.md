# T0136 — گزارش اجرای وظیفه

- **وضعیت:** `done`
- **نسخهٔ کد/آزمون تأییدشده:** [V 3.0.477 — `2b64a70c59c1d02c9d30d6ef510e6952b89cdb14`](https://github.com/artinrahimii/project/commit/2b64a70c59c1d02c9d30d6ef510e6952b89cdb14).
- **روش اجرا:** بررسی دقیق پیاده‌سازی و اجرای مجموعهٔ HostLifecycle در GitHub Actions روی همان commit؛ برای این وظیفه کد جایگزین یا مولد تکراری ساخته نشد، چون پیاده‌سازی امن و آزمون اجرایی مربوطه از قبل وجود داشت و در CI اجرا شد.

## خروجی واقعی بررسی‌شده

- `src/Host.Windows/Requests/SecureRequestIdGenerator.cs` برای هر فراخوانی ۱۶ بایت را با `RandomNumberGenerator.GetBytes` تولید می‌کند و شناسه را به صورت `req_` + ۳۲ رقم hex کوچک برمی‌گرداند. این مسیر از هش یا digest ثابت برای ساخت شناسه استفاده نمی‌کند.
- `tests/HostLifecycle.Tests/SecureRequestIdGeneratorTests.cs`، ثبت‌شده در `tests/HostLifecycle.Tests/Program.cs`، ۴۰۹۶ شناسه تولید می‌کند و قالب و عدم تکرار داخل نمونه را بررسی می‌کند.
- آزمون روی commit دقیق `V 3.0.477` در HostLifecycle اجرا شد و لاگ صریحاً ثبت کرد: `PASS: generated 4096 temporary request IDs using a cryptographic RNG; format and sample uniqueness checks passed.`

## بازبینی وابستگی‌ها

- `T0135 → T0136`: `sequencing_only` و `validated`؛ یال به عنوان ترتیب محافظه‌کارانه در گراف فعال باقی ماند.
- `T0128 → T0136`: `sequencing_only` و `validated`؛ چون اجرای دو رایانه/شبکهٔ مستقل هیچ API تولید شناسهٔ امن یا قرارداد لازم برای این وظیفه ایجاد نمی‌کند، از یال‌های فعال حذف شد. دلیل و شواهد در `edge_change_log` باقی است. این تغییر به معنای اجرای `T0128` نیست.

## محدودیت ادعا

تست ۴۰۹۶ نمونه، عدم برخورد جهانی را اثبات ریاضی نمی‌کند؛ فقط آزمون رگرسیون و قالب/عدم تکرار در همین نمونه است. انتخاب CSPRNG مستقیماً در سورس مشاهده شد و آزمون روی CI موفق شد. وظیفهٔ بعدی `T0137` است؛ یال‌های مرتبط با آن قبل از شروع باید بازبینی شوند.
