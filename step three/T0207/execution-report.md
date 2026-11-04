# T0207 — گزارش اجرای واقعی

## وضعیت

**در حال انجام؛ هنوز تکمیل نشده است.** ساخت پاک در commit `V 3.0.943` به‌طور کامل روی runnerهای تازه موفق شد، اما بازآزمایی‌های بعدی مشکلات محیطی و دو regression در پیکربندی/تست کشف کردند. اصلاحات بعدی هنوز باید با CI سبز تأیید شوند.

## وابستگی‌های ورودی

- `T0206 → T0207`: `sequencing_only` و اعتبارسنجی‌شده؛ build پاک از checkout تازه می‌آید و خروجی فنی T0206 را مصرف نمی‌کند. نوع اصلی `blocks` و ترتیب حفظ شده است.
- `T0195 → T0207`: `sequencing_only` و اعتبارسنجی‌شده؛ خروجی اختصاصی T0195 برای clean build ورودی نیست. نوع اصلی `blocks` و ترتیب حفظ شده است.

## پیاده‌سازی

فایل `.github/workflows/product-build.yml` در commit `ccaddf87dac832c838c9c270d20fc16e6ccf9e28` به clean صریح و build غیر incremental برای Windows، Android و HostLifecycle تغییر کرد.

پس از آن، بازآزمایی Android روی commit 944 پنج مورد از ۱۹۱ تست JVM را هنگام آماده‌سازی Robolectric و به‌علت HTTP 429 از `repo1.maven.org` متوقف کرد؛ ۱۸۶ تست دیگر موفق بودند. برای مقاوم‌سازی، commit 945 mirror قابل تنظیم Google Maven Central را با Gradle property و environment variable به runtime Robolectric افزود.

بازآزمایی commit 945 در Android KtLint به‌علت قرارگرفتن `providers.gradleProperty` در خط نامناسب شکست خورد. در commit 946، زنجیرهٔ Kotlin DSL به قالب چندخطی KtLint تغییر کرد. KtLint در run مربوط به commit 946 موفق شد؛ اما تست قراردادی Python هنوز شکل تک‌خطی قبلی را انتظار داشت و Quality Gate در همان تست متوقف شد (۴۰۷ از ۴۰۸ تست Python موفق).

در commit حاضر، assertion تست به regex فاصله‌پذیر تغییر کرده تا زنجیرهٔ چندخطی واقعی را بررسی کند. این اصلاح هنوز CI نشده است.

## شواهد CI

### commit `V 3.0.943` — اجرای کامل موفق
Product Build run [41921424399](https://github.com/artinrahimii/project/actions/runs/41921424399):
- Windows job `126894612727`: موفق، ۱۶/۱۶ مرحله.
- HostLifecycle job `126894612520`: موفق، ۹/۹ مرحله.
- Android job `126894611985`: موفق، ۲۸/۲۸ مرحله شامل JVM tests، Keystore instrumentation، Protobuf و APK.
- artifactها: [گزارش instrumentation](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13367776949)، [گزارش JVM](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13366814095)، [APK](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13367262823).
- Quality Gate run `41921424419` و Rendezvous run `41921424425`: موفق.

### commit `V 3.0.944`
Product Build run `41922371821`: ویندوز و HostLifecycle موفق؛ Android JVM با ۵ خطای Robolectric setup از ۱۹۱ تست روبه‌رو شد که همه HTTP 429 بودند؛ ۱۸۶ تست موفق.

### commit `V 3.0.945`
- Windows job `126905065862`: موفق، ۱۶/۱۶.
- HostLifecycle job `126905065861`: موفق، ۹/۹.
- Android job `126905065423`: شکست KtLint در `app/build.gradle.kts:13:14`؛ مراحل APK و تست‌ها skip شدند.
- Quality Gate run [41924457621](https://github.com/artinrahimii/project/actions/runs/41924457621): شکست graph guard به‌علت اختلاف failed_attempt_count و تاریخچه؛ معنای شمارنده بررسی و سابقه A002 اصلاح شد.

### commit `V 3.0.946`
- Quality Gate run `41925547826`: ۴۰۸ تست Python اجرا شد؛ ۴۰۷ موفق و تست قرارداد mirror شکست خورد، چون assertion شکل یک‌خطی را انتظار داشت.
- Product Build run `41925547942`: ویندوز و HostLifecycle هنوز در حال تکمیل بودند؛ Android در آخرین مشاهده از KtLint با موفقیت عبور کرده و به clean APK assembly رسیده بود.
- Rendezvous run `41925547878`: موفق.

## تاریخچه تلاش‌ها

- `T0207-A001`: clean build پیاده‌سازی و روی commit 943 کامل موفق؛ بازآزمایی 944 مشکل HTTP 429 را نشان داد.
- `T0207-A002`: mirror Robolectric اضافه شد؛ روی commit 945 در KtLint شکست خورد؛ Quality Gate همان commit نیز graph-history mismatch داشت.
- `T0207-A003`: قالب‌بندی Kotlin DSL اصلاح شد؛ KtLint روی commit 946 موفق، ولی تست قرارداد Python شکست خورد.
- `T0207-A004`: assertion تست Python برای زنجیرهٔ چندخطی اصلاح شد؛ CI هنوز اجرا نشده است.

تا موفقیت Quality Gate و هر سه job Product Build روی commit جدید، T0207 در وضعیت `in_progress` می‌ماند.


## checkpoint واقعی CI روی commit 947

در بازبینی مستقیم workflow runهای commit `3930ed7017ded5dea7786655590648920cb4ce1f`:

- [Step Three quality gate run 41926077752](https://github.com/artinrahimii/project/actions/runs/41926077752): موفق؛ آزمون‌های Python و بررسی ساختار/خروجی گراف سبز.
- [Rendezvous run 41926077686](https://github.com/artinrahimii/project/actions/runs/41926077686): موفق.
- [Product Build run 41926077657](https://github.com/artinrahimii/project/actions/runs/41926077657): Windows job `126910709342` و HostLifecycle job `126910709152` کامل و موفق.
- Android job `126910709306`: ساخت fixture، Gradle clean، KtLint، ساخت APK پاک، کنترل حداقل مجوزها و آزمون‌های JVM موفق شدند. در زمان checkpoint، Android Keystore instrumentation هنوز در حال اجرا بود و بررسی نهایی Protobuf و بارگذاری گزارش‌ها/APK در انتظار بودند.

**وضعیت پذیرش:** این checkpoint، موفقیت کامل T0207 را ثابت نمی‌کند. وظیفه تا پایان موفق Android instrumentation و همهٔ artifactها در وضعیت `in_progress` باقی می‌ماند.
