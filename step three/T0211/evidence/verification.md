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


## A004 — Product Build نسخهٔ 3.0.962

- Run: https://github.com/artinrahimii/project/actions/runs/42020058706
- Android job: https://github.com/artinrahimii/project/actions/runs/42020058706/job/127220264790
- Quality Gate همان commit: https://github.com/artinrahimii/project/actions/runs/42020058599
- Quality Gate: 423 Python tests passed؛ compileall و هر دو graph_guard checks موفق؛ خطای گراف صفر و یک هشدار قدیمی metadata در T0001.
- Product Build: Windows WPF و HostLifecycle موفق؛ Android clean build، KtLint، permissions، JVM tests و connectedDebugAndroidTest/coverage موفق.
- Launcher smoke ناموفق: `adb shell am start -n com.whitep.client/.MainActivity` با `Error type 3` و `Activity class ... does not exist` خاتمه یافت. بنابراین T0211 تکمیل نیست و PID/resumed checks انجام نشدند.


## A005 — اصلاح مبتنی بر Package Manager (در انتظار CI)

- package با `adb shell pm path com.whitep.client` بررسی می‌شود.
- Activity واقعی MAIN/LAUNCHER با `cmd package resolve-activity --brief` استخراج و پس از کنترل package و suffix `MainActivity` اجرا می‌شود.
- PID و Activity فعال پس از اجرا بررسی می‌شوند.
- regression contract تغییر یافته است؛ هنوز نتیجهٔ اجرای آن در Quality Gate/Product Build جدید موجود نیست.

## A006 — Product Build و اصلاح نصب مجدد

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42023459980 — موفق، ۴۲۳ آزمون Python و بررسی‌های گراف.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42023460541
- Android job: https://github.com/artinrahimii/project/actions/runs/42023460541/job/127231737078
- Windows WPF و HostLifecycle jobs موفق شدند. Android APK build، KtLint، merged permission check، JVM tests، `connectedDebugAndroidTest` و coverage نیز موفق شدند.
- ۱۲ instrumentation test اجرا شدند؛ آزمون non-loopback LAN مطابق T0202 deferred skip شد.
- `adb shell pm path com.whitep.client | grep -F 'package:'` پس از instrumentation exit code 1 داد؛ بنابراین resolve/launch/PID/resumed بررسی نشدند.
- workflow اکنون همان APK ساخته‌شده را با `adb install -r` پس از instrumentation و پیش از package check نصب مجدد می‌کند. regression test ترتیب این سه مرحله را assert می‌کند.
- نتیجهٔ CI برای اصلاح A006 هنوز pending است؛ وضعیت T0211 همچنان `in_progress` است.


## A007 — Product Build و اصلاح shell isolation

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42026088287 — موفق؛ ۴۲۳ آزمون Python و graph guard موفق، ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطای گراف و یک هشدار قدیمی metadata.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42026088278
- Android job: https://github.com/artinrahimii/project/actions/runs/42026088278/job/127240545370
- Windows WPF و HostLifecycle موفق شدند. Android clean APK build، KtLint، merged permissions، JVM tests، connected instrumentation و coverage نیز موفق شدند.
- ۱۲ instrumentation case اجرا شدند؛ non-loopback LAN test طبق T0202 deferred skip شد. نصب مجدد APK با `adb install -r` موفق بود و `pm path` مسیر base.apk را نشان داد.
- launcher smoke به‌علت اجرای هر خط script در shell مستقل شکست خورد: متغیر `launch_activity` به خط بعدی منتقل نشد و `case` چندخطی parse نشد. اجرای Activity، PID و resumed بررسی نشد.
- workflow اکنون component واقعی MAIN/LAUNCHER را به فایل موقت می‌نویسد، وجود آن را جداگانه کنترل می‌کند و با `cat` همان فایل Activity را اجرا می‌کند. این اصلاح هنوز باید در CI تأیید شود؛ T0211 همچنان `in_progress` است.

## A007 — نتیجهٔ CI در V 3.0.965

- Quality Gate run 42027995142: موفق، ۴۲۳ آزمون Python و validation گراف.
- Product Build run 42027993509: Windows WPF و HostLifecycle موفق. Android clean APK، KtLint، merged permissions، JVM tests و coverage موفق؛ Gradle instrumentation نیز `BUILD SUCCESSFUL` و ۱۲ test completed گزارش کرد.
- APK با `adb install -r` نصب شد؛ `pm path` مسیر base.apk را نشان داد؛ Package Manager کامپوننت `com.whitep.client/.MainActivity` را resolve کرد و `am start` خروجی `Starting: Intent` داد.
- بلافاصله پس از launch، `pidof` خروجی نداشت و exit code 1 داد؛ پس از آن job متوقف شد و resumed Activity بررسی نشد. از این لاگ به‌تنهایی crash یا startup delay قابل تشخیص نیست.
- لاگ‌های اجرا: https://github.com/artinrahimii/project/actions/runs/42027993509 و Android job: https://github.com/artinrahimii/project/actions/runs/42027993509/job/127246936494

## A008 — اصلاح تشخیص process و Activity

- PID و resumed Activity هرکدام با retry محدود تا ۳۰ ثانیه بررسی می‌شوند.
- در هر timeout، `adb logcat -d -t 300` اجرا می‌شود تا علت واقعی failure در CI قابل تشخیص باشد.
- این اصلاح هنوز با CI تأیید نشده و هیچ ادعای launch موفق نهایی ثبت نمی‌شود.

## A009 — شواهد نهایی launcher و نصب APK در V 3.0.966

- Quality Gate: https://github.com/artinrahimii/project/actions/runs/42045953315 — موفق؛ ۴۲۳ Python tests، compileall، graph validation (۲۳۰ task / ۴۲۲ edge) و recorded-output/evidence validation.
- Product Build: https://github.com/artinrahimii/project/actions/runs/42045953382 — موفق؛ Android، Windows WPF/installer و HostLifecycle jobs.
- Android job: https://github.com/artinrahimii/project/actions/runs/42045953382/job/127308807324 — همهٔ مراحل موفق.
- APK artifact: https://github.com/artinrahimii/project/actions/runs/42045953382/artifacts/13423874796 — `whitep-android-debug-apk`, اندازهٔ artifact: ۲۸٬۹۷۲٬۴۷۵ بایت.

### خروجی واقعی emulator
- `adb install -r src/Client.Android/app/build/outputs/apk/debug/app-debug.apk` → `Success`.
- `adb shell pm path com.whitep.client` → مسیر نصب‌شدهٔ `base.apk`.
- Package Manager → `com.whitep.client/.MainActivity`.
- `adb shell am start -n ...` → `Starting: Intent { cmp=com.whitep.client/.MainActivity }`.
- `adb shell pidof com.whitep.client` → PID برابر `4521`.
- ActivityManager → `topResumedActivity=ActivityRecord{... com.whitep.client/.MainActivity ...}`.
- workflow → `PASS: installed Android MainActivity launcher was resolved, launched, and verified with a live process and resumed activity.`

### نتایج آزمون
- JUnit: ۲۲۴ آزمون در ۳۴ گزارش؛ ۲۲۳ passed، یک skipped، صفر errors و صفر failures.
- یک مورد skip همان `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan` است که برای پذیرش شبکهٔ واقعی به ورودی‌های runner فیزیکی نیاز دارد و به‌سبب تعویق T0202 اجرا نشد. این موضوع خارج از معیار launcher این وظیفه است و LAN واقعی را تأیید نمی‌کند.

**وضعیت T0211: done.** تولید، نصب و راه‌اندازی MainActivity روی emulator با شواهد CI واقعی تأیید شد. انتشار عمومی Release در این وظیفه ادعا نمی‌شود؛ signing و checksum در وظایف بعدی قرار دارند.
