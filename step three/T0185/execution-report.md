# T0185 — حداقل‌سازی مجوزهای Android

- **وضعیت:** `in_progress`؛ manifest منبع فقط `android.permission.INTERNET` دارد. کنترل خودکار manifest ادغام‌شدهٔ APK به workflow اضافه شده، اما پذیرش exact-head CI این تغییر هنوز در انتظار است.
- **مبنای بررسی:** شاخهٔ `main`، commit `a2c5a33064c78744c1e8e7194ec9f9fa1d45ef19` — `V 3.0.867`.
- **تلاش:** `T0185-A001`؛ بازبینی وابستگی‌ها و افزودن کنترل مجوزهای منبع و APK.
- **تصمیم permission:** فقط `INTERNET` لازم است؛ کلاینت اتصال HTTPS به rendezvous و سوکت TCP/libp2p دارد. کد فعلی وضعیت شبکه را query نمی‌کند و API درخواست مجوز runtime ندارد. ذخیره‌سازی SQLCipher در فضای خصوصی اپ است و به مجوز storage خارجی نیاز ندارد.
- **Manifest:** هیچ مجوز جدیدی اضافه یا مجوز موجودی حذف نشد؛ یک توضیح کنار تنها declaration اضافه شد تا نیاز واقعی و install-time بودن آن روشن باشد.
- **کنترل اجرایی:** `tools/verify_android_permissions.py` با `aapt2 dump permissions` APK نهایی را با allowlist تک‌موردی مقایسه می‌کند. هر permission اضافی که از manifest یک dependency وارد APK شود، موجب شکست CI می‌شود.
- **آزمون‌های Python:** هشت مورد برای manifest منبع، parsing خروجی AAPT2، شناسایی uses-permission نسخه‌دار، allowlist دقیق، رد مجوزهای اضافی/مفقود و نبود API درخواست runtime طراحی شده‌اند. اجرای محلی harness اولیهٔ منطق policy/parser: ۷ آزمون موفق؛ آزمون جدید نبود API درخواست runtime در exact-head quality CI بررسی می‌شود.
- **وابستگی‌ها:** یال‌های `T0184 → T0185` و `T0177 → T0185` پس از بررسی شواهد به `sequencing_only` تغییر طبقه‌بندی یافتند؛ نوع `blocks` و ترتیب محافظه‌کارانه حفظ شده‌اند.
- **فایل‌های خروجی:** manifest Android، workflow محصول، verifier Python، آزمون‌های Python، `docs/security.md`، این گزارش و فایل شواهد.
- **مرز تکمیل:** تا اجرای موفق exact-head product workflow و مشاهدهٔ گزارش AAPT2، وظیفه `done` نمی‌شود.
