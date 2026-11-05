# گزارش اجرای T0211

## هدف و پیش‌نیاز
تولید APK واقعی Android از Gradle و نصب/اجرای آن روی emulator.
- T0208 و T0210 با شواهد CI معتبر تکمیل شده‌اند.
- یال‌های T0210 -> T0211 و T0208 -> T0211 به‌عنوان sequencing_only اعتبارسنجی شدند؛ APK از پروژهٔ Android و بدون مصرف artifact/API خاص آن پیش‌نیازها ساخته می‌شود. type اصلی blocks حفظ شده است.

## شواهد موجود از V 3.0.958
- Gradle clean، KtLint و assembleDebug موفق.
- merged APK permission check موفق.
- JVM unit tests و coverage generation موفق.
- connectedDebugAndroidTest و Android Keystore instrumentation روی emulator API 36 موفق.
- JUnit: ۳۴ فایل، ۲۲۴ آزمون، ۲۲۳ passed، ۱ skipped، صفر خطا و failure.
- APK با حجم ۲۸٬۹۷۲٬۴۷۴ بایت به‌عنوان artifact 13405872201 بارگذاری شد.
- مورد skipped، آزمون non-loopback LAN است؛ میزبان مستقل Windows/LAN در دسترس runner نبود و T0202 deferred باقی می‌ماند.

## اصلاح A001
بررسی workflow نشان داد instrumentation APK را نصب و تست می‌کند، اما launcher activity محصول را صریحاً اجرا نمی‌کند. اکنون پس از connectedDebugAndroidTest، workflow:
1. com.whitep.client را force-stop می‌کند.
2. com.whitep.client/.MainActivity را با adb am start -W اجرا می‌کند.
3. Status: ok و Component را بررسی می‌کند.
4. PID برنامه را کنترل می‌کند.
5. dumpsys activity را برای resumed بودن MainActivity بررسی می‌کند.
یک regression contract در tests/test_reproducible_build_contract.py اضافه شد تا این مراحل از workflow حذف نشوند.

## وضعیت
T0211 در حال اجراست؛ build و instrumentation شواهد موفق دارند، اما explicit launcher MainActivity smoke هنوز باید در CI جدید موفق شود. تا آن زمان done ثبت نمی‌شود.


## A001 — خطای Quality Gate در V 3.0.959 و اصلاح metadata

- Step Three Quality Gate run 42010812129 تعداد ۴۲۳ آزمون Python را اجرا کرد و همگی OK شدند.
- بررسی ساختار اولیهٔ graph: ۲۳۰ وظیفه و ۴۲۲ یال، بدون خطا و با یک warning تاریخی برای dependency غیرشناسه‌ای T0001.
- بررسی شواهد اجرای graph: ۷ خطای MISSING_OUTPUT_FILE صادر شد؛ علت، ثبت توضیحات و شناسه/مسیر artifactهای ephemeral به‌عنوان actual_output_paths بود، در حالی‌که graph_guard این فیلد را برای مسیر دقیق فایل موجود در checkout به‌کار می‌برد.
- این خطا شکست آزمون برنامه یا اجرای Android نبود؛ هیچ آزمون Python یا runtime task در QG شکست نخورد. اشکال فقط در ثبت مسیرهای واقعی گراف بود.
- اصلاح V 3.0.960: actual_output_paths در T0209/T0210/T0211 به مسیرهای دقیق tracked files محدود شد. شناسه‌های Windows installer و Android APK و لینک‌های CI در evidence باقی ماندند.
- T0211 همچنان in_progress است؛ explicit launcher activity check در CI جدید باید پاس شود.


## A002 — تشخیص شکست wrapper و اصلاح shell

- Product Build run 42011471555 روی commit `cca92b71ad138df5235e2546e4db8effd9a8caf8` در مرحلهٔ Android Keystore instrumentation شکست خورد.
- لاگ دقیق نشان داد action دستور script را با `/usr/bin/sh -c` اجرا کرده است؛ `set -euo pipefail` در این shell معتبر نیست و با `set: Illegal option -o pipefail` و exit code 2 متوقف شد. این خطا پیش از اجرای Gradle instrumentation رخ داد.
- clean APK assembly، KtLint، بررسی permissionهای merged APK و آزمون‌های JVM/coverage موفق بودند. cold boot ابتدا `adb: device offline` نشان داد، سپس ADB daemon بالا آمد؛ خطای خاتمه‌دهنده همان گزینهٔ نامعتبر shell بود.
- اصلاح workflow: استفاده از `set -eu` سازگار با POSIX؛ فرمان‌های pipeline باقی‌مانده در این script از exit status آخرین فرمان بهره می‌برند.
- regression contract در `tests/test_reproducible_build_contract.py` وجود `set -eu` و نبود `pipefail` را در script همین action بررسی می‌کند.
- این اصلاح هنوز از CI جدید عبور نکرده است. T0211 در وضعیت `in_progress` باقی می‌ماند تا instrumentation، اجرای MainActivity، PID و resumed activity همگی در Product Build جدید موفق شوند.


## A003 — اصلاح smoke test پس از instrumentation موفق

- Product Build run 42013760643 روی commit `bd0cbfc3843e2007acc3183b8f0840bfb87ca7e4` اصلاح POSIX shell را تأیید کرد: Gradle، clean debug APK، KtLint، بررسی permissionها، JVM tests و `connectedDebugAndroidTest createDebugAndroidTestCoverageReport` موفق شدند.
- آزمون `AndroidWindowsLanIntegrationTest` در runner به‌دلیل نیاز به میزبان LAN واقعی skip شد و همچنان ذیل T0202 تعویق‌یافته است.
- خطای باقی‌مانده بعد از BUILD SUCCESSFUL و پس از `adb shell am force-stop` رخ داد: فرمان `adb shell am start -W -n com.whitep.client/.MainActivity` که در command substitution اجرا می‌شد با exit code 1 برگشت. لاگ نشان می‌دهد action خطا را ثبت کرده، اما خروجی captureشدهٔ خود adb را نمایش نداده است؛ بنابراین علت داخلی آن بدون حدس ثبت می‌شود.
- اصلاح محدود: حذف `-W` و command substitution و اجرای مستقیم `adb shell am start -n ...` تا خروجی فرمان در لاگ wrapper قابل مشاهده باشد. کنترل PID و resumed Activity در دو فرمان مستقل و قابل مشاهدهٔ adb باقی مانده‌اند.
- regression contract اکنون شکل مستقیم start، نبود `-W`، بررسی PID با grep و بررسی resumed MainActivity را الزام می‌کند.
- وضعیت T0211 همچنان in_progress است؛ Product Build تازه باید start مستقیم، PID و resumed activity را تأیید کند.



## A004 — تشخیص شکست launcher در Product Build نسخهٔ 3.0.962

- Product Build run 42020058706 روی commit `8a5f575490dfd9aaf6c69a1a7e85a826d91321c8`: https://github.com/artinrahimii/project/actions/runs/42020058706
- Android job: https://github.com/artinrahimii/project/actions/runs/42020058706/job/127220264790
- Windows WPF و HostLifecycle jobs موفق شدند.
- Android clean APK build، KtLint، merged permission verification، JVM tests و `connectedDebugAndroidTest createDebugAndroidTestCoverageReport` به `BUILD SUCCESSFUL` رسیدند؛ آزمون non-loopback LAN طبق T0202 skip ماند.
- مرحلهٔ launcher با `adb shell am start -n com.whitep.client/.MainActivity` شکست خورد: `Error type 3` و `Activity class {com.whitep.client/com.whitep.client.MainActivity} does not exist.`
- Manifest مخزن `.MainActivity` را به‌عنوان MAIN/LAUNCHER معرفی می‌کند و namespace/applicationId/Kotlin package برابر `com.whitep.client` هستند؛ با وجود این، خروجی CI ثابت کرد کامپوننت hard-coded در محیط emulator پیدا نشده است.
- وضعیت T0211 تکمیل نیست؛ PID و resumed activity به مرحلهٔ تأیید نرسیدند.


## A005 — اصلاح launcher smoke با resolve واقعی Package Manager

- workflow اکنون ابتدا نصب بودن package را با `pm path` بررسی می‌کند، سپس کامپوننت MAIN/LAUNCHER نصب‌شده را با `cmd package resolve-activity` استخراج می‌کند.
- کامپوننت resolve‌شده باید متعلق به `com.whitep.client` و کلاس آن `MainActivity` باشد؛ سپس همان کامپوننت اجرا می‌شود و PID و resumed Activity به‌طور مستقل بررسی می‌شوند.
- regression contract متناظر در `tests/test_reproducible_build_contract.py` به‌روزرسانی شد.
- این اصلاح هنوز از Quality Gate و Product Build جدید عبور نکرده است؛ T0211 در `in_progress` باقی می‌ماند.

## A006 — نصب مجدد APK پیش از اعتبارسنجی launcher

- Quality Gate run 42023459980 روی commit `ca1aea857c4612d29ff1dd68e6941983ea699aa2` موفق شد؛ ۴۲۳ آزمون Python و کنترل‌های ساختار/شواهد گراف موفق بودند.
- Rendezvous container run 42023460015 موفق شد.
- Product Build run 42023460541: job ویندوز WPF و HostLifecycle موفق شدند؛ Android clean APK build، KtLint، بررسی permissionهای merged، آزمون‌های JVM، `connectedDebugAndroidTest` و تولید coverage موفق شدند.
- در instrumentation، ۱۲ آزمون اجرا شدند؛ آزمون non-loopback LAN به‌دلیل نیاز به میزبان مستقل، مطابق تعویق T0202، skip ماند.
- مرحلهٔ پس از instrumentation با `adb shell pm path com.whitep.client | grep -F 'package:'` و exit code 1 شکست خورد. در نتیجه Package Manager resolve، launch، PID و resumed Activity اجرا نشدند. این نتیجه ثابت نمی‌کند build APK شکست خورده است؛ فقط نصب‌بودن package در این نقطه اثبات نشد.
- اصلاح A006: نصب مجدد APK واقعی تولیدشده با `adb install -r src/Client.Android/app/build/outputs/apk/debug/app-debug.apk` بلافاصله پس از instrumentation و پیش از `pm path` اضافه شد. regression contract نیز ترتیب اجرای Gradle instrumentation، نصب مجدد، و بررسی package را الزام می‌کند.
- این اصلاح هنوز در CI اجرا نشده است؛ T0211 در وضعیت `in_progress` باقی می‌ماند و تا موفقیت package resolution، launch، PID و resumed Activity تکمیل نمی‌شود.


## A007 — اصلاح وابستگی به متغیر shell در launcher smoke

- Product Build run 42026088278: Android clean APK build، KtLint، merged permission check، JVM tests، `connectedDebugAndroidTest` و coverage موفق شدند؛ ۱۲ instrumentation test اجرا شدند و non-loopback LAN case مطابق T0202 deferred skip ماند.
- نصب مجدد APK واقعی با `adb install -r src/Client.Android/app/build/outputs/apk/debug/app-debug.apk` موفق بود و `adb shell pm path com.whitep.client` مسیر `base.apk` را برگرداند.
- شکست بعدی از shell isolation بود: emulator runner هر خط script را در `/usr/bin/sh -c` جداگانه اجرا می‌کند. متغیر `launch_activity` در خط بعدی در دسترس نبود؛ `printf` مقدار خالی چاپ کرد و خط مستقل `case` با `Syntax error: end of file unexpected (expecting ")")` شکست خورد. در نتیجه اجرای MainActivity، PID و resumed checks انجام نشدند.
- اصلاح A007: خروجی `cmd package resolve-activity` با فیلتر package و `MainActivity` در `/tmp/whitep-launcher-component.txt` ذخیره می‌شود؛ خط بعدی فایل را با `test -s` اعتبارسنجی می‌کند و فرمان اجرای Activity مقدار component را از همان فایل می‌خواند. هیچ متغیر shell بین خطوط نیاز نیست.
- regression contract ترتیب و محتوای این مراحل را بررسی می‌کند و استفاده از متغیر محلی و بلوک `case` چندخطی را رد می‌کند.
- اصلاح A007 هنوز در CI اجرا نشده است؛ T0211 در وضعیت `in_progress` باقی می‌ماند تا resolve، launch، PID و resumed Activity همگی در Product Build موفق شوند.

## A007 — نتیجهٔ واقعی Product Build نسخهٔ 3.0.965

- Quality Gate run 42027995142 موفق شد: ۴۲۳ آزمون Python، کنترل ساختار و کنترل شواهد گراف با ۲۳۰ وظیفه و ۴۲۲ یال؛ صفر خطای گراف و یک هشدار تاریخی T0001.
- Product Build run 42027993509: Windows WPF و HostLifecycle موفق شدند. در Android، clean APK build، KtLint، بررسی permissionهای merged، آزمون‌های JVM و coverage موفق شدند.
- `connectedDebugAndroidTest createDebugAndroidTestCoverageReport` به `BUILD SUCCESSFUL` رسید و ۱۲ آزمون روی emulator خاتمه یافتند. نصب مجدد APK با `adb install -r` موفق بود؛ `pm path` مسیر `base.apk` را برگرداند؛ Package Manager مقدار `com.whitep.client/.MainActivity` را resolve کرد؛ `am start` نیز `Starting: Intent` گزارش کرد.
- کنترل فوری `adb shell pidof com.whitep.client` خروجی نداشت و با exit code 1 شکست خورد. در نتیجه کنترل resumed Activity اجرا نشد. لاگ این اجرا logcat را در زمان شکست ثبت نکرد؛ بنابراین هنوز نمی‌توان نتیجه گرفت برنامه crash کرده یا صرفاً در نمونه‌برداری فوری آماده نبوده است.
- T0211 تکمیل نیست. A007 از حالت partial به failed تغییر کرد؛ اصلاح A008 یک انتظار محدود ۳۰ثانیه‌ای برای PID و resumed Activity اضافه می‌کند و در صورت timeout، ۳۰۰ خط آخر logcat را ثبت می‌کند.

## A008 — انتظار محدود و تشخیص timeout در emulator launcher

- workflow برای هر بررسی، حلقهٔ retry مستقل و تک‌خطی دارد تا با اجرای جداگانهٔ هر خط توسط emulator-runner سازگار باشد.
- process با `pidof` حداکثر ۳۰ ثانیه بررسی می‌شود. اگر PID ظاهر نشود، logcat ثبت و job شکست داده می‌شود.
- Activity Manager نیز حداکثر ۳۰ ثانیه برای resumed شدن MainActivity بررسی می‌شود؛ در timeout، logcat ثبت می‌شود.
- regression contract برای وجود هر دو حلقه، timeout diagnostics، تعداد logcat dumps و ترتیب launch → PID → resumed اضافه شد.
- نتیجهٔ A008 هنوز pending CI است؛ T0211 همچنان `in_progress` است و T0212 شروع نمی‌شود.

## T0211-A009 — تکمیل پذیرش launcher در V 3.0.966

- commit آزموده‌شده: `16c7a5f9de7728e2614fb084a1c35a423b308854` با پیام `V 3.0.966`.
- Quality Gate run [42045953315](https://github.com/artinrahimii/project/actions/runs/42045953315) موفق شد: ۴۲۳ آزمون Python، compileall، اعتبارسنجی ساختار گراف با ۲۳۰ وظیفه و ۴۲۲ یال، و کنترل خروجی‌ها/شواهد.
- Product Build run [42045953382](https://github.com/artinrahimii/project/actions/runs/42045953382) با موفقیت کامل شد؛ jobهای Android، Windows WPF/installer و HostLifecycle همگی موفق بودند.
- Android job: [127308807324](https://github.com/artinrahimii/project/actions/runs/42045953382/job/127308807324). ساخت clean APK با Gradle، KtLint، بررسی حداقل permissionهای APK ادغام‌شده، آزمون‌های JVM، instrumentation و اعتبارسنجی coverage موفق شدند.
- خلاصهٔ JUnit: ۳۴ گزارش، ۲۲۴ آزمون، ۲۲۳ موفق، ۱ مورد skip، صفر error و صفر failure. مورد skip، آزمون شبکهٔ non-loopback است که طبق تصمیم مرحلهٔ پایانی T0202 به تعویق افتاده؛ این run اثبات‌کنندهٔ LAN واقعی نیست.
- artifact واقعی APK با نام `whitep-android-debug-apk` بارگذاری شد: [artifact 13423874796](https://github.com/artinrahimii/project/actions/runs/42045953382/artifacts/13423874796)، اندازهٔ ثبت‌شدهٔ artifact برابر ۲۸٬۹۷۲٬۴۷۵ بایت.
- نصب مجدد با `adb install -r` خروجی `Success` داد؛ `pm path` مسیر `base.apk` را برگرداند؛ Package Manager کامپوننت واقعی `com.whitep.client/.MainActivity` را resolve کرد؛ `am start` خروجی `Starting: Intent` داد.
- PID واقعی برنامه `4521` مشاهده شد و ActivityManager مقدار `topResumedActivity=... com.whitep.client/.MainActivity` را ثبت کرد. workflow پیام صریح PASS برای resolve، launch، PID و resumed activity نوشت.

### ارزیابی معیار پذیرش
- تولید APK قابل نصب: موفق.
- نصب APK روی emulator: موفق.
- resolve شدن launcher واقعی: موفق.
- اجرای MainActivity و مشاهدهٔ process زنده: موفق.
- مشاهدهٔ resumed MainActivity: موفق.
- آزمون‌ها و coverage مرتبط: موفق؛ یک آزمون مستقل LAN مطابق T0202 عمداً skip شد.

**نتیجه:** A008 با CI واقعی تأیید شد؛ T0211 اکنون `done` است. این نتیجه به معنی انتشار عمومی GitHub Release یا تأیید شبکهٔ واقعی Android↔Windows نیست. وظیفهٔ منطقی بعدی T0212 است؛ یال‌های ورودی آن، یعنی T0211→T0212 و T0208→T0212، هنوز `unreviewed` هستند و باید پیش از شروع T0212 بررسی شوند.
## A010 — اصلاح پیوند شواهد آزمون در Graph Guard

- پس از commit V 3.0.967، Quality Gate run https://github.com/artinrahimii/project/actions/runs/42048420336 تعداد ۴۲۳ آزمون Python و compileall را با موفقیت گذراند.
- مرحلهٔ Graph Guard یک خطا ثبت کرد: DONE_WITHOUT_TEST_FILE_EVIDENCE. علت دقیق این بود که فهرست evidence برای T0211 به workflow، artifact و فایل‌های production اشاره می‌کرد، اما مسیر فایل منبع آزمون را شامل نمی‌شد.
- این خطا شکست build، نصب یا اجرای Android نبود؛ شواهد واقعی A009 برای نصب APK، PID و resumed MainActivity در Product Build run 42045953382 همچنان معتبر است.
- قرارداد tools/graph_guard.py منبع آزمون را از مسیرهای tests/، test/ یا source setهای Android با پسوند آزمون می‌شناسد. دو فایل موجود و واقعی به شواهد T0211 افزوده شدند: tests/test_reproducible_build_contract.py و src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidKeyStorePersistenceTest.kt.
- تلاش A010 به‌عنوان شکست اعتبارسنجی metadata ثبت شد؛ تاریخچهٔ موفقیت A009 و شش failure قبلی حذف یا بازنویسی نشدند. باید Quality Gate commit اصلاحی را مجدداً بررسی کند.

وضعیت قابلیت T0211: شواهد build/install/launcher موفق‌اند و وضعیت وظیفه done باقی است؛ کنترل گراف در A010 به علت نبود مسیر test source در metadata شکست خورد و اکنون این پیوند اصلاح شده است.
