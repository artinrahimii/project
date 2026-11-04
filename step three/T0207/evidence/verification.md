# T0207 — شواهد راستی‌آزمایی

## نسخهٔ منبع و صحت گراف پایه

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پایه: `5f45fc2a72b9ad382d5823c0568686aeb07ef3c6` (`V 3.0.946`)
- tree پایه: `b03afff4f4d7e19b069c3e099dd3596e16274d0a`
- مسیر گراف: `step three/project-graph.md`
- Blob گراف پایه: `9224da6fec457ca0a53e2d04a8c3cc8808de5f19`
- اندازهٔ UTF-8 محتوای اصلی: 3,700,412 بایت؛ طول متن decode‌شده: 3,571,953 نویسه.
- SHA-1 شیء Git با سربرگ `blob 3700412\0` و کل بایت‌های اصلی محاسبه شد؛ با Blob درخت همان commit مطابق بود.
- JSON کامل parse شد: ۲۳۰ وظیفه، ۴۲۲ یال، صفر شناسهٔ تکراری و صفر endpoint حل‌نشده.

## CI دقیق

- Product Build run `41921424399` روی commit 943: هر سه job موفق؛ Android ۲۸/۲۸، Windows ۱۶/۱۶، HostLifecycle ۹/۹.
- Quality Gate `41921424419` و Rendezvous `41921424425`: موفق.
- Product Build روی commit 944، run `41922371821`: پنج خطای Robolectric setup ناشی از HTTP 429؛ ۱۸۶ تست از ۱۹۱ موفق.
- Product Build روی commit 945، run `41924457380`: ویندوز ۱۶/۱۶ و HostLifecycle ۹/۹ موفق؛ Android در KtLint با `Expected newline before '.'` شکست.
- Quality Gate روی commit 945، run `41924457621`: خطای شمارندهٔ تلاش ناموفق در graph guard.
- Quality Gate روی commit 946، run `41925547826`: ۴۰۷ از ۴۰۸ تست Python موفق؛ assertion تست mirror به‌علت تفاوت شکل تک‌خطی/چندخطی شکست.
- Product Build روی commit 946، run `41925547942`: Windows و HostLifecycle موفق؛ در آخرین بررسی Android از KtLint عبور کرده و APK assembly در جریان بود.
- Rendezvous روی commit 946، run `41925547878`: موفق.

## فایل‌های اصلاح‌شده

- `src/Client.Android/app/build.gradle.kts`: mirror runtime Robolectric قابل تنظیم؛ زنجیرهٔ providers مطابق KtLint.
- `tests/test_android_libp2p_transport_contract.py`: بررسی regex برای اجازهٔ whitespace در زنجیرهٔ چندخطی providers.
- `step three/T0207/execution-report.md`
- `step three/T0207/evidence/verification.md`
- `step three/project-graph.md`: تاریخچهٔ تلاش‌های A001 تا A004؛ A002 با `result=failed` و failed_attempt_count=1 ثبت شده است. T0207 هنوز `in_progress`.

## عدم ادعای تکمیل

تغییر تست در commit جاری هنوز CI نشده است. برای تکمیل، Quality Gate باید همهٔ تست‌های Python و هر دو graph guard را پاس کند؛ Product Build نیز باید clean build، lint، APK، JVM tests، Android Keystore instrumentation، generated Protobuf و artifact uploads را روی همان commit به پایان موفق برساند.


## checkpoint CI روی commit V 3.0.947

- Quality Gate run `41926077752`: موفق؛ اصلاح regex تست قرارداد در commit جاری پذیرفته شد.
- Rendezvous run `41926077686`: موفق.
- Product Build run `41926077657`: Windows job `126910709342` موفق؛ HostLifecycle job `126910709152` موفق.
- Android job `126910709306`: clean fixture build، Gradle clean، KtLint، APK build، permission policy و JVM tests همگی موفق؛ Keystore instrumentation در زمان ثبت checkpoint در حال اجرا بود و generated Protobuf check و artifact uploads هنوز انجام نشده بودند.
- گراف پس از ثبت این checkpoint همچنان T0207 را `in_progress` نگه می‌دارد؛ هیچ ادعای تکمیل زودهنگام ثبت نشده است.
