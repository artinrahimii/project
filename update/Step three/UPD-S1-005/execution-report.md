# گزارش اجرای UPD-S1-005 — سیاست رضایت و ظرفیت میزبان

- مبنای اجرای کد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` — `V 3.1.38`
- مبنای ممیزی قبلی: `d027e72f3968ef7c8c826f889042eb67a8533fa4` — `V 3.1.36`
- وضعیت پس از CI دقیق مبنا: **تمام چهار معیار پذیرش این وظیفه تأیید شده‌اند.**

## ممیزی و شکاف احرازشده

میزبان قبلاً مشارکت شبکه را به‌صورت پیش‌فرض خاموش می‌کرد، opt-in مالک، readiness backend، افشای حریم خصوصی با پاسخ پیش‌فرض No، تأیید PeerId، اندازه‌گیری زندهٔ RAM و فضای آزاد درایو سیستم، رد حالت Unknown/Insufficient و سقف یک inference هم‌زمان داشت. شکاف احرازشده این بود که حدهای RAM و فضای دیسک در تنظیمات پایدار نبودند و پس از restart باقی نمی‌ماندند.

## اصلاح واقعی

- تنظیمات میزبان به schema نسخهٔ ۲ ارتقا یافتند و `requiredModelMemoryGiB` و `minimumFreeDiskGiB` را ذخیره می‌کنند.
- schema نسخهٔ ۱ با مقادیر سازگار ۴ GiB RAM و ۱ GiB فضای آزاد دیسک بارگذاری می‌شود؛ اولین Save بعدی آن را به schema نسخهٔ ۲ می‌برد.
- مقدارها باید finite، مثبت و در سقف تعریف‌شده باشند. تنظیمات نامعتبر یا ذخیره‌سازی ناموفق باعث پذیرش ظرفیت نامعلوم نمی‌شود.
- WPF مقدارها را در شروع بازیابی و هنگام ویرایش/پیش از اتصال، inference محلی و hosting اعتبارسنجی و ذخیره می‌کند.
- اندازه‌گیری سخت‌افزار فقط RAM آزاد فیزیکی و فضای آزاد درایو سیستم را مبنای تصمیم قرار می‌دهد. فهرست GPU صرفاً inventory است؛ محدودیت CPU/GPU/VRAM/idle/AC ادعا یا جعل نشده است.
- listener شبکه در حالت opt-out خاموش می‌ماند و با توقف مشارکت، admission/listener متوقف می‌شود.
- coordinator با ظرفیت `SemaphoreSlim(1,1)` و پذیرش غیرصفی، درخواست هم‌زمان دوم را Busy می‌کند.

## آزمون و CI دقیق

روی commit `2cc12c32cfc2663316c93064543e6ce62caaa5cb`، پیام `V 3.1.38`، هر چهار workflow موفق شدند:

1. [Step Three quality gate — run 42459356154](https://github.com/artinrahimii/project/actions/runs/42459356154): آزمون‌های خودکار Python، compileall و graph_guard برای هر دو گراف (ساختار و مسیرهای خروجی/شواهد) موفق.
2. [White-P product build — run 42459356153](https://github.com/artinrahimii/project/actions/runs/42459356153): ساخت تمیز WPF، smoke startup، installer acceptance؛ lint و debug/release APK اندروید؛ آزمون‌های JVM و Android Keystore instrumentation؛ آزمون‌های HostLifecycle و coverage؛ همهٔ jobها موفق.
3. [Real local inference — run 42459356156](https://github.com/artinrahimii/project/actions/runs/42459356156): llama.cpp CPU build 11443 با archive SHA-256 برابر `98303a5a688a606d4a32522e47ed7cafa0ddff04d3be56ab927e25d3b1ea45c6`؛ executable SHA-256 برابر `4cdaae426a798b49717e3ce9b8a7756a16355c83c812d9d59bc99e26ae6d3968`؛ مدل GGUF ثابت‌شده در revision `a6b7826ed969c3926a98359e39912af137b464cb` با حجم 105,454,144 بایت و SHA-256 برابر `8030f04528538d47bda434f6f0bdf3952c40a58123e4d5e755332f23731a8684`. آزمون اجرای inference واقعی و cancellation موفق شد.
4. [Rendezvous container build/smoke — run 42459356182](https://github.com/artinrahimii/project/actions/runs/42459356182): ساخت و smoke test سرویس موفق.

شواهد ظرفیت در HostLifecycle شامل رد ظرفیت Unknown/Insufficient، fail-closed هنگام مشارکت خاموش و پذیرش دقیقاً یک درخواست از ۶۴ درخواست هم‌زمان (۶۳ پاسخ Busy، حداکثر یک inference فعال) است. آزمون settings نیز round-trip، migration از schema v1، ذخیرهٔ مقادیر RAM/disk و رد مقدارهای نامعتبر را پاس کرد.

اشکال‌های دو assertion قدیمی در V 3.1.37 در commit جاری V 3.1.38 اصلاح شدند و CI دقیق commit جدید سبز است؛ سوابق شکست تاریخی حذف نشده‌اند.

## وضعیت معیارهای پذیرش

1. مشارکت پیش‌فرض خاموش و فعال‌سازی با اقدام آگاهانهٔ مالک: تأیید شد.
2. فقط محدودیت‌های اندازه‌گیری‌پذیر و enforce‌شده (RAM و فضای آزاد دیسک): تأیید شد؛ محدودیت GPU/CPU/idle/AC ادعا نمی‌شود.
3. شروط عدم پذیرش، readiness و توقف مشارکت: آزموده و تأیید شد.
4. سقف یک inference هم‌زمان: با آزمون رقابت ۶۴ درخواست تأیید شد.

## محدودیت آزمون

CI روی runnerهای میزبانی‌شده انجام شده است. آزمون گوشی فیزیکی Android با Windows فیزیکی هدف، Android-to-Windows E2E، نصب تمیز محصول روی دستگاه کاربر، و in-place upgrade هیچ‌یک در این وظیفه اجرا نشده‌اند و برای وظایف پذیرش مربوط باقی می‌مانند. این محدودیت‌ها مانع تکمیل معیارهای مستقل UPD-S1-005 نیستند.
