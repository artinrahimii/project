# T0185 — حداقل‌سازی مجوزهای Android

- **وضعیت:** `in_progress`؛ manifest منبع فقط INTERNET را درخواست می‌کند. اجرای exact-head نسخهٔ V 3.0.868 نشان داد AndroidX Core یک مجوز app-scoped از نوع signature را نیز در manifest ادغام‌شده قرار می‌دهد؛ verifier قبلی آن را به‌اشتباه رد کرد. نسخهٔ A003 این مورد خاص را همراه با بررسی سطح signature می‌پذیرد؛ پذیرش CI نسخهٔ اصلاح‌شده هنوز در انتظار است.
- **مبنای بررسی:** شاخهٔ `main`، commit `a2c5a33064c78744c1e8e7194ec9f9fa1d45ef19` — `V 3.0.867`.
- **تلاش:** `T0185-A001`؛ بازبینی وابستگی‌ها و افزودن کنترل مجوزهای منبع و APK.
- **تصمیم permission:** فقط `INTERNET` لازم است؛ کلاینت اتصال HTTPS به rendezvous و سوکت TCP/libp2p دارد. کد فعلی وضعیت شبکه را query نمی‌کند و API درخواست مجوز runtime ندارد. ذخیره‌سازی SQLCipher در فضای خصوصی اپ است و به مجوز storage خارجی نیاز ندارد.
- **Manifest:** هیچ مجوز جدیدی اضافه یا مجوز موجودی حذف نشد؛ یک توضیح کنار تنها declaration اضافه شد تا نیاز واقعی و install-time بودن آن روشن باشد.
- **کنترل اجرایی:** `tools/verify_android_permissions.py` با `aapt2 dump permissions` APK نهایی را با allowlist تک‌موردی مقایسه می‌کند. هر permission اضافی که از manifest یک dependency وارد APK شود، موجب شکست CI می‌شود.
- **آزمون‌های Python:** هشت مورد برای manifest منبع، parsing خروجی AAPT2، شناسایی uses-permission نسخه‌دار، allowlist دقیق، رد مجوزهای اضافی/مفقود و نبود API درخواست runtime طراحی شده‌اند. اجرای محلی harness اولیهٔ منطق policy/parser: ۷ آزمون موفق؛ آزمون جدید نبود API درخواست runtime در exact-head quality CI بررسی می‌شود.
- **وابستگی‌ها:** یال‌های `T0184 → T0185` و `T0177 → T0185` پس از بررسی شواهد به `sequencing_only` تغییر طبقه‌بندی یافتند؛ نوع `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- **فایل‌های خروجی:** manifest Android، workflow محصول، verifier Python، آزمون‌های Python، `docs/security.md`، این گزارش و فایل شواهد.
- **مرز تکمیل:** تا اجرای موفق exact-head product workflow و مشاهدهٔ گزارش AAPT2، وظیفه `done` نمی‌شود.


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

