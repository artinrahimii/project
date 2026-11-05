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


## A002 — شکست قبلی و اصلاح

- Product Build run 42011471555: https://github.com/artinrahimii/project/actions/runs/42011471555
- Android job: https://github.com/artinrahimii/project/actions/runs/42011471555/job/127191303809
- نتیجه: APK clean build، lint، permission verification و JVM tests موفق؛ Android Keystore instrumentation به‌دلیل shell mismatch اجرا نشد.
- لاگ job: action script را با `/usr/bin/sh -c` اجرا کرد و `set -euo pipefail` باعث `set: Illegal option -o pipefail` شد.
- اصلاح ثبت‌شده در این تلاش: POSIX `set -eu` و regression test برای جلوگیری از برگشت `pipefail`.
- اجرای مجدد CI و بررسی صریح MainActivity/PID/resumed هنوز pending است؛ بنابراین این شواهد به‌تنهایی معیار پذیرش T0211 را تکمیل نمی‌کنند.


## A003 — Product Build پس از اصلاح shell

- Product Build: https://github.com/artinrahimii/project/actions/runs/42013760643
- Android job: https://github.com/artinrahimii/project/actions/runs/42013760643/job/127199003648
- Instrumentation artifact: https://github.com/artinrahimii/project/actions/runs/42013760643/artifacts/13408547431
- شواهد لاگ: clean APK build، KtLint، merged permissions، JVM tests/coverage، اجرای `connectedDebugAndroidTest` و `createDebugAndroidTestCoverageReport` به `BUILD SUCCESSFUL` رسیدند.
- آزمون non-loopback LAN از `AndroidWindowsLanIntegrationTest` skip شد؛ این بخش هنوز به میزبان LAN مستقل نیاز دارد و در T0202 deferred است.
- پس از موفقیت instrumentation، `adb shell am force-stop com.whitep.client` موفق بود؛ فرمان launch قبلی `adb shell am start -W -n com.whitep.client/.MainActivity` با exit code 1 برگشت و باعث شکست job شد. چون خروجی capture شده در لاگ wrapper ظاهر نشده، علت درونی adb نامشخص باقی می‌ماند.
- اصلاح A003: اجرای مستقیم `adb shell am start -n com.whitep.client/.MainActivity` و بررسی مستقل PID و resumed MainActivity؛ این تغییر هنوز به CI نیاز دارد.
