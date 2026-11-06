# T0185 — حداقل‌سازی مجوزهای Android

- **وضعیت فعلی:** `done`؛ معیار پذیرش در exact-head V 3.0.871 احراز شد. ممیزی AAPT2 روی APK واقعی، آزمون‌های Android و quality gate موفق‌اند؛ جزئیات در A006 و فایل شواهد ثبت شده‌اند.
- **مبنای بررسی نهایی:** شاخهٔ `main`، commit `4f44de18e841bcf20e3096e80706e6f23b62206d` — `V 3.0.871`؛ product run `41735585097` و quality run `41735585127`.
- **تلاش:** `T0185-A001`؛ بازبینی وابستگی‌ها و افزودن کنترل مجوزهای منبع و APK.
- **تصمیم permission:** فقط `INTERNET` لازم است؛ کلاینت اتصال HTTPS به rendezvous و سوکت TCP/libp2p دارد. کد فعلی وضعیت شبکه را query نمی‌کند و API درخواست مجوز runtime ندارد. ذخیره‌سازی SQLCipher در فضای خصوصی اپ است و به مجوز storage خارجی نیاز ندارد.
- **Manifest:** هیچ مجوز جدیدی اضافه یا مجوز موجودی حذف نشد؛ یک توضیح کنار تنها declaration اضافه شد تا نیاز واقعی و install-time بودن آن روشن باشد.
- **کنترل اجرایی:** `tools/verify_android_permissions.py` با `aapt2 dump permissions` APK نهایی را با allowlist تک‌موردی مقایسه می‌کند. هر permission اضافی که از manifest یک dependency وارد APK شود، موجب شکست CI می‌شود.
- **آزمون‌های Python:** ۱۳ آزمون اختصاصی برای manifest منبع، applicationId، parsing مجوزهای AAPT2، سطح signature و رد مجوزهای اضافی/مفقود وجود دارد. همهٔ ۱۳ آزمون در Step Three quality gate دقیق V 3.0.869 با موفقیت اجرا شدند؛ در مجموع ۳۶۶ آزمون Python موفق بودند.
- **وابستگی‌ها:** یال‌های `T0184 → T0185` و `T0177 → T0185` پس از بررسی شواهد به `sequencing_only` تغییر طبقه‌بندی یافتند؛ نوع `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- **فایل‌های خروجی:** manifest Android، workflow محصول، verifier Python، آزمون‌های Python، `docs/security.md`، این گزارش و فایل شواهد.
- **مرز تکمیل:** این معیار در A006 با مشاهدهٔ خروجی AAPT2 از APK واقعی، اجرای Android JVM و instrumentation، و quality gate exact-head احراز شد؛ وضعیت فعلی `done` است.


## A002 — شکست واقعی ممیزی Manifest ادغام‌شده در V 3.0.868

- commit آزموده‌شده: f2883e4f07459e1dcb03d2c71971685ee28b7048؛ پیام V 3.0.868؛ product run 41731636396.
- Android KtLint و assembleDebug موفق شدند.
- مرحلهٔ Verify minimum permissions in merged Android APK شکست خورد: خروجی AAPT2 مجوز com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION را نیز نشان داد.
- پس از شکست، Android JVM unit tests، instrumentation و مراحل بعدی build اجرا نشدند؛ آن‌ها skipped هستند، نه passed.
- Windows WPF، HostLifecycle، Step Three quality gate و Rendezvous container موفق بودند. دروازهٔ کیفیت run 41731636477 شامل 361 آزمون Python موفق، compileall موفق، 230 وظیفه و 422 یال، صفر خطا و یک هشدار قدیمی T0001 بود.
- ریشه‌یابی: این مجوز از AndroidX Core می‌آید و برای حفظ رفتار non-exported برای dynamic receiver در نسخه‌های قدیمی‌تر Android تعریف می‌شود. AndroidX آن را با protectionLevel برابر signature تعریف و در manifest نهایی درخواست می‌کند. ردکردن بی‌قیدوشرط آن بیش از حد سخت‌گیرانه بود.
- **نتیجه:** A002 ناموفق ثبت می‌شود؛ T0185 همچنان in_progress است.



## A003 — اصلاح محدود و قابل‌آزمون allowlist

- تنها مجوزی که Manifest منبع مستقیماً درخواست می‌کند همچنان android.permission.INTERNET است.
- APK نهایی می‌تواند علاوه بر INTERNET فقط مجوز دقیقا applicationId-scoped زیر را داشته باشد: com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION.
- verifier با aapt2 dump permissions مجوزهای درخواستی APK و با aapt2 dump xmltree تعریف باینری permission را می‌خواند.
- پذیرش مجوز AndroidX فقط وقتی انجام می‌شود که تعریف دقیقاً یک‌بار وجود داشته و protectionLevel دقیقاً 0x2 (signature) باشد. مجوز اضافی دیگر، مجوز سفارشی دیگر، declaration مفقود/تکراری یا سطح حفاظتی متفاوت رد می‌شود.
- applicationId ثابت verifier با applicationId فایل build.gradle.kts تطبیق داده می‌شود.
- مجموعهٔ آزمون‌های متمرکز به 13 مورد افزایش یافته است؛ شامل سناریوی AndroidX مجاز و رد مجوز اضافی/غیرامضاشده.
- fixture محلی قالب AAPT2 xmltree مستندشده در ابزار AOSP parsed شد و مقدار signature برابر 0x2 تأیید شد. این fixture جایگزین اجرای واقعی verifier روی APK نیست.
- **نتیجه:** پیاده‌سازی اصلاح شد؛ نتیجهٔ exact-head CI هنوز pending است و T0185 done نیست.



## A004 — شکست واقعی parser در V 3.0.869

- commit آزموده‌شده: 43311fb5593ac6a9e10f553506458366135d9314؛ product run 41734543287.
- Android KtLint و assembleDebug موفق شدند؛ AAPT2 ممیزی مجوز شکست خورد.
- پیام خطا: The exact AndroidX dynamic-receiver permission must be declared exactly once in the packaged manifest.
- مجوز app-scoped در فهرست permissionهای APK وجود داشت، اما parser تعریف آن را از xmltree استخراج نکرد. Android JVM و instrumentation بعد از شکست skipped شدند.
- کیفیت run 41734543291 موفق شد: 366 آزمون Python، شامل 13 آزمون T0185؛ compileall موفق؛ graph guard صفر خطا و یک هشدار قدیمی T0001. Windows WPF، HostLifecycle و Rendezvous container نیز موفق بودند.
- تشخیص: parser فقط پیشوند کوتاه android: را می‌پذیرفت؛ قالب AAPT2 می‌تواند attribute را با URI کامل فضای نام Android چاپ کند. این ناسازگاری parser بود، نه مدرکی از مجوز اضافی جدید.

## A005 — پشتیبانی از هر دو قالب attribute در AAPT2

- XMLTREE_NAME و XMLTREE_PROTECTION_LEVEL اکنون هم پیشوند کوتاه android: و هم پیشوند کامل http://schemas.android.com/apk/res/android: را می‌پذیرند.
- پیام خطای عدم شناسایی declaration، فهرست declarationهای parse‌شده و سطح حفاظتی را نیز گزارش می‌کند تا شکست بعدی قابل‌تشخیص باشد.
- آزمون متمرکز جدید، قالب URI کامل را جدا از قالب کوتاه بررسی می‌کند؛ اکنون 14 آزمون اختصاصی وجود دارد.
- دو fixture محلی برای هر دو قالب موفق شدند؛ این نتیجه هنوز اجرای verifier روی APK واقعی نیست.
- product run قبلی V 3.0.870 در زمان ثبت A005 هنوز در مرحله‌های مقدماتی بود؛ نتیجهٔ آن در این گزارش موفق یا ناموفق فرض نشده است.
- **نتیجه:** parser اصلاح شده، T0185 همچنان in_progress است تا ممیزی APK واقعی و آزمون‌های Android در exact-head CI موفق شوند.



## A006 — پذیرش نهایی exact-head در V 3.0.871

- commit آزموده‌شده: `4f44de18e841bcf20e3096e80706e6f23b62206d`؛ product run `41735585097`؛ quality run `41735585127`.
- Android KtLint و `assembleDebug` موفق شدند.
- ممیزی واقعی AAPT2 روی APK ساخته‌شده موفق شد. خروجی permissionها دقیقاً `android.permission.INTERNET` و `com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION` بود؛ تعریف مجوز دوم در Manifest باینری با `protectionLevel=signature (0x2)` تأیید شد.
- آزمون Android JVM (`testDebugUnitTest`) و آزمون connected instrumentation (`connectedDebugAndroidTest`) موفق شدند.
- artifact شمارهٔ `13279121914` شامل ۹ آزمون instrumentation با صفر شکست و صفر مورد skip است: ۴ آزمون `EncryptedClientDatabaseAcceptanceTest` و ۵ آزمون `AndroidKeyStorePersistenceTest`.
- quality gate دقیق همان commit، ۳۶۷ آزمون Python را با موفقیت اجرا کرد؛ `compileall` و graph/output/evidence validation نیز موفق شدند. Graph guard تعداد ۲۳۰ وظیفه و ۴۲۲ یال را با صفر خطا و یک هشدار قدیمی و حفظ‌شده برای T0001 گزارش کرد.
- Windows WPF، HostLifecycle و Rendezvous container build/smoke نیز موفق شدند.
- **نتیجه:** اصلاح namespace کامل AAPT2 روی APK واقعی تأیید شد؛ `T0185` تکمیل‌شده است. دو تلاش ناموفق A002 و A004 در تاریخچه باقی می‌مانند و حذف نشده‌اند.
