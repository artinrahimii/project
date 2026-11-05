# شواهد اجرای T0211

- Product Build قبلی: https://github.com/artinrahimii/project/actions/runs/42007819870
- Android job: https://github.com/artinrahimii/project/actions/runs/42007819870/job/127178931797
- Debug APK artifact: https://github.com/artinrahimii/project/actions/runs/42007819870/artifacts/13405872201
- Unit reports: https://github.com/artinrahimii/project/actions/runs/42007819870/artifacts/13406620506
- Instrumentation reports: https://github.com/artinrahimii/project/actions/runs/42007819870/artifacts/13405447926
- Coverage: https://github.com/artinrahimii/project/actions/runs/42007819870/artifacts/13406635380

## شواهد تأییدشده
- Gradle assembleDebug clean، KtLint، حداقل permissionها، JVM unit tests، Android Keystore instrumentation و گزارش coverage موفق.
- JUnit summary: ۲۲۴ test case، ۲۲۳ passed، ۱ skipped، صفر failure و error.
- artifact APK: ۲۸٬۹۷۲٬۴۷۴ بایت.
- AndroidWindowsLanIntegrationTest non-loopback skip شد؛ این مورد با T0202 deferred سازگار است و قبولی LAN واقعی ادعا نمی‌شود.

## Explicit launch pending
- applicationId: com.whitep.client.
- Launcher activity: .MainActivity.
- workflow پس از instrumentation، MainActivity را صریحاً start و Status: ok، PID و resumed ActivityTaskManager record را کنترل می‌کند.
- این مرحله هنوز در CI اجرا نشده است؛ T0211 در حال اجرا باقی می‌ماند.


## A001 — نتیجهٔ QG و اصلاح actual_output_paths

- QG run 42010812129: ۴۲۳ Python tests موفق؛ graph structure ۲۳۰ task و ۴۲۲ edge معتبر، یک warning تاریخی.
- Graph evidence validation: ۷ خطای MISSING_OUTPUT_FILE فقط به این دلیل بود که actual_output_paths حاوی عبارت توضیحی/artifact references شده بود، نه مسیر literal tracked files.
- V 3.0.960 این مقادیر را از actual_output_paths حذف کرد و مسیرهای tracked مربوط به workflow، source و گزارش را نگه داشت؛ artifact IDs و URLs همچنان در evidence حاضرند.
- این خطا به معنی شکست APK build، instrumentation یا Windows installer acceptance نیست.
- Explicit MainActivity emulator launch در Product Build جدید همچنان لازم است؛ T0211 هنوز done نیست.
