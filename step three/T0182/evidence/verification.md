# T0182-A002 — شواهد نهایی آزمون و اعتبارسنجی

## Snapshot منبع و اعتبار گراف

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit منبع آزمون: `f21dc02702f654f51e8cae699aa592f9a5399fac` (`V 3.0.842`)
- tree منبع: `4e293600bd13221155388dc243ed65e433b80140`
- Blob منبع گراف: `1dda65c32ed0c394ffb760d388004433a614c642`
- اندازهٔ اصلی گراف: ۳٬۳۳۵٬۵۸۰ بایت UTF-8؛ طول متن ۳٬۲۰۸٬۶۳۴ نویسه.
- Blob SHA-1 با قالب Git `blob <length>\0<content>` از تمام بایت‌های اصلی محاسبه شد و با SHA استخراج‌شده از tree دقیقاً برابر بود. JSON کامل parse شد: ۲۳۰ node و ۴۲۲ edge.
- همهٔ actual output/evidence paths در tree همان commit وجود داشتند.

## شواهد runtime

HostLifecycle job `125919924364` در product run `41636799070` موفق شد و خروجی صریح زیر را ثبت کرد:

`PASS: in-memory conversation, explicit retention consent, Windows DPAPI history and orphaned temporary-file cleanup.`

آزمون جدید stale file، recent file، فایل قفل‌شده با handle انحصاری، GUID نامعتبر و فایل متعلق به store دیگر را بررسی می‌کند.

## سایر workflowهای exact-head

- Step Three quality run `41636799068`: ۳۴۴ آزمون Python؛ compileall موفق؛ graph guard با ۰ خطا و ۱ هشدار metadata قدیمی T0001.
- Container run `41636799141`: production build و non-root HTTP smoke موفق.
- Windows WPF job `125919923959`: C# formatting، Release build با ۰ warning/۰ error، MainWindow startup smoke و DPAPI identity/trust checks موفق.
- Android job `125919924294`: KtLint، debug APK، JVM unit tests، generated Protobuf Java/Kotlin verification و artifact upload موفق.
- Real local inference integration run `41636799166`: موفق.

## Artifactها

- Windows host: `13231511016`، ۱۴٬۲۷۵٬۴۳۳ بایت.
- Android debug APK: `13230882593`، ۱۹٬۵۳۱٬۹۸۲ بایت.
- Android JVM unit-test report: `13231970969`، ۵۱٬۲۲۰ بایت.

## ثبت تلاش‌ها

- `T0182-A001`: پیاده‌سازی cleanup و آزمون‌های جدید؛ commit `f21dc02702f654f51e8cae699aa592f9a5399fac`؛ نتیجهٔ اولیه `partial` تا زمان CI.
- `T0182-A002`: اعتبارسنجی exact-head روی همان commit؛ همهٔ workflowهای مرتبط موفق؛ نتیجه `succeeded`.

## وضعیت یال‌ها

- `T0181 → T0182`: `hard_dependency`، validated؛ type=blocks حفظ شده است.
- `T0177 → T0182`: `sequencing_only`، validated؛ type=blocks حفظ شده است.
