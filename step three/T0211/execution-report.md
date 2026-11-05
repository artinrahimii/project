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
