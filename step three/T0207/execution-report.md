# T0207 — گزارش اجرای واقعی

## وضعیت فعلی

**در حال انجام؛ هنوز تکمیل نشده است.** پیاده‌سازی build پاک در `V 3.0.943` روی runnerهای تازه در Product Build کامل موفق شد. اما بازآزمایی‌های بعدی دو مشکل مستقل نشان دادند؛ اصلاح آینهٔ Maven در commit `V 3.0.945` هنوز با CI سبز تأیید نشده است.

## وابستگی‌های ورودی

- `T0206 → T0207`: اعتبارسنجی‌شده با طبقه‌بندی `sequencing_only`. ساخت پاک از checkout تازه انجام می‌شود و API، schema، fixture یا artifact اختصاصی T0206 را مصرف نمی‌کند. نوع اصلی یال `blocks` و ترتیب محافظه‌کارانه حفظ شده است.
- `T0195 → T0207`: اعتبارسنجی‌شده با طبقه‌بندی `sequencing_only`. build پاک، خروجی اختصاصی T0195 را مصرف نمی‌کند. نوع اصلی یال `blocks` و ترتیب حفظ شده است.

## پیاده‌سازی واقعی

فایل `.github/workflows/product-build.yml` در commit `ccaddf87dac832c838c9c270d20fc16e6ccf9e28` اصلاح شد:
- ویندوز: clean صریح solution و build غیر incremental در Release.
- اندروید: clean و build مجدد fixture دات‌نت، Gradle clean، lint و build APK از خروجی پاک.
- HostLifecycle: clean پروژهٔ آزمون پیش از اجرای suite کامل.

در ادامهٔ بازآزمایی، CI در تاریخچهٔ Robolectric به دلیل HTTP 429 از `repo1.maven.org` هنگام دریافت `android-all-instrumented` شکست موقت داشت (۵ شکست class setup از ۱۹۱ تست؛ ۱۸۶ تست دیگر موفق). برای مقاوم‌سازی این وضعیت، commit `37ed932657824ecb2eac6e532f5f8a5bb766d764` ویژگی‌های runtime مستند Robolectric را برای آینهٔ Google Maven Central، با امکان override از طریق Gradle property یا environment variable، اضافه کرد.

بازآزمایی همان commit خطای قالب‌بندی Kotlin DSL را آشکار کرد: `app/build.gradle.kts:13:14 Expected newline before '.'`. در commit حاضر، زنجیرهٔ providers به قالبی تبدیل شد که فراخوانی `gradleProperty` را در خط ادامهٔ جداگانه قرار می‌دهد. این اصلاح هنوز در CI اجرا نشده است.

## نتایج CI و شواهد

### پیاده‌سازی build پاک — commit `V 3.0.943`

Product Build run [41921424399](https://github.com/artinrahimii/project/actions/runs/41921424399) با موفقیت کامل شد:
- Windows job `126894612727`: ۱۶ از ۱۶ مرحله موفق؛ clean و Release build، Protobuf، WPF smoke، کنترل‌های agent tool و DPAPI.
- HostLifecycle job `126894612520`: ۹ از ۹ مرحله موفق؛ clean و اجرای کامل آزمون‌ها.
- Android job `126894611985`: ۲۸ از ۲۸ مرحله موفق؛ Gradle clean، lint، APK، JVM tests، Keystore instrumentation، بررسی Protobuf و بارگذاری artifactها.
- گزارش instrumentation: [artifact 13367776949](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13367776949).
- گزارش آزمون JVM: [artifact 13366814095](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13366814095).
- APK دیباگ: [artifact 13367262823](https://github.com/artinrahimii/project/actions/runs/41921424399/artifacts/13367262823).
- [Quality Gate 41921424419](https://github.com/artinrahimii/project/actions/runs/41921424419) و [Rendezvous 41921424425](https://github.com/artinrahimii/project/actions/runs/41921424425): موفق.

### بازآزمایی روی commit `V 3.0.944`

Product Build run `41922371821`: ویندوز و HostLifecycle موفق؛ Android JVM شامل ۱۹۱ تست بود که ۵ مورد در bootstrap مربوط به Robolectric و به علت HTTP 429 شکست خوردند؛ ۱۸۶ تست موفق بودند. این خطاها پیش از اجرای بدنهٔ پنج test class رخ دادند.

### بازآزمایی روی commit `V 3.0.945`

- Windows job `126905065862`: موفق، ۱۶ از ۱۶.
- HostLifecycle job `126905065861`: موفق، ۹ از ۹.
- Android job `126905065423`: ناموفق در `ktlintKotlinScriptCheck`؛ مرحلهٔ lint خطای `Expected newline before '.'` را در خط ۱۳ گزارش کرد. build APK، JVM tests و instrumentation اجرا نشدند.
- Quality Gate run [41924457621](https://github.com/artinrahimii/project/actions/runs/41924457621): به دلیل ناسازگاری مقدار `failed_attempt_count` با تعداد تاریخچهٔ تلاش‌های دارای `result=failed` شکست خورد. معنای validator بررسی شد و سابقهٔ تلاش A002 در گراف با نتیجهٔ واقعی `failed` ثبت گردید.

## تلاش‌ها و اقدام بعدی

- `T0207-A001`: پیاده‌سازی clean build؛ CI دقیقاً روی commit پیاده‌سازی موفق شد، اما بازآزمایی بعدی روی 944 خطای خارجی HTTP 429 را آشکار کرد.
- `T0207-A002`: افزودن mirror قابل تنظیم برای Robolectric؛ روی 945 در lint شکست خورد و در validator نیز ناسازگاری سابقهٔ اجرا آشکار شد.
- `T0207-A003`: اصلاح قالب‌بندی Kotlin DSL و ثبت سازگار تلاش ناموفق A002 در تاریخچه؛ **CI هنوز در انتظار اجرا است**.

تا زمانی که Quality Gate و Product Build روی commit حاضر موفق نشوند، وضعیت T0207 به `done` تغییر نمی‌کند.
