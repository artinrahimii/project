# T0207 — شواهد راستی‌آزمایی

## نسخهٔ منبع و یکپارچگی گراف پایه

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پایهٔ این به‌روزرسانی: `37ed932657824ecb2eac6e532f5f8a5bb766d764` (`V 3.0.945`)
- tree همان commit: `9058689ce8ab083aebe2fc90c91f53a957f5e036`
- مسیر گراف: `step three/project-graph.md`
- Blob گراف پایه: `a0d7e556f2f48b8165028074fff97b93570765d7`
- اندازهٔ محتوای اصلی UTF-8: 3,697,173 بایت؛ طول متن decode‌شده: 3,568,728 نویسه.
- SHA-1 شیء Git با سربرگ `blob 3697173\0` و تمام بایت‌های اصلی محاسبه شد و با Blob فوق مطابقت داشت.
- JSON کامل parse شد: ۲۳۰ task node، ۴۲۲ edge، بدون شناسهٔ تکراری و بدون endpoint معلق.

## نتایج مرتبط

- [Product Build 41921424399، commit 943](https://github.com/artinrahimii/project/actions/runs/41921424399): هر سه job موفق؛ ویندوز ۱۶/۱۶، HostLifecycle ۹/۹، اندروید ۲۸/۲۸.
- [Quality Gate 41921424419](https://github.com/artinrahimii/project/actions/runs/41921424419): موفق.
- [Rendezvous 41921424425](https://github.com/artinrahimii/project/actions/runs/41921424425): موفق.
- Product Build روی commit 944 (`41922371821`): ویندوز و HostLifecycle موفق؛ Android JVM پنج خطای Robolectric setup به‌دلیل HTTP 429 و ۱۸۶ تست موفق.
- Product Build روی commit 945 (`41924457380`):
  - Windows job `126905065862`: موفق، ۱۶/۱۶.
  - HostLifecycle job `126905065861`: موفق، ۹/۹.
  - Android job `126905065423`: ناموفق در KtLint: `src/Client.Android/app/build.gradle.kts:13:14 Expected newline before '.'`; مراحل بعدی Android اجرا نشدند.
- [Quality Gate 41924457621](https://github.com/artinrahimii/project/actions/runs/41924457621): شکست در graph guard با `FAILED_ATTEMPT_COUNT_MISMATCH`. validator به‌طور مشخص تعداد رکوردهای تاریخچه با `result == "failed"` را با summary مقایسه می‌کند؛ رکورد A002 اکنون مطابق نتیجهٔ CI `failed` است.

## اصلاح حاضر

- فایل اصلاح‌شده: `src/Client.Android/app/build.gradle.kts`.
- زنجیرهٔ providers اکنون با خط ادامهٔ جداگانه آغاز می‌شود تا قاعدهٔ KtLint برای newline پیش از نقطه رعایت شود.
- Blob جدید فایل Kotlin DSL: `7b8324063e7244da8ca79d148bdd2641c9520209`.
- تلاش A002 به‌عنوان شکست واقعی ثبت شده و A003 برای اصلاح/بازآزمایی فعال است.
- Blob جدید گراف در همین commit اعمال خواهد شد؛ وضعیت task همچنان `in_progress` باقی می‌ماند.

**عدم ادعای موفقیت:** اصلاح mirror و تغییر قالب‌بندی هنوز روی runner اجرا نشده‌اند. پذیرش نهایی T0207 منوط به سبزشدن Quality Gate و هر سه job Product Build روی commit جدید است.
