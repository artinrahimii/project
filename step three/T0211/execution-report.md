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
