# گزارش اجرای UPD-S1-013 — محدودیت منابع و آمادگی واقعی میزبان Windows

- وضعیت نهایی وظیفه: **done**
- commit پیاده‌سازی و مبنای آزمون: `991930f84f7a9af9fc839d4d4c7ee9b8a049758f` — `V 3.1.176`
- commit مستندسازی و همگام‌سازی وضعیت: `V 3.1.177` (پس از ثبت در Git)
- زمان شروع اجرا: ۱۰ نوامبر ۲۰۲۶، ۲۳:۱۹:۴۳ به وقت UTC+03:30
- زمان تأیید CI: ۱۰ نوامبر ۲۰۲۶، ۲۳:۲۸:۴۰ به وقت UTC+03:30

## ممیزی کد موجود و شکاف واقعی

کد موجود بازنویسی نشد. سرویس‌های اندازه‌گیری واقعی سخت‌افزار Windows، evaluator آمادگی، ذخیره‌سازی نسخه‌دار limitها، session بررسی سلامت backend، policy پذیرش درخواست و coordinator تک‌درخواستی از قبل وجود داشتند.

شکاف احرازشده در مسیر دریافت درخواست peer بود: مسیر شبکه از مقدار cached بولی `_capacityReady` استفاده می‌کرد؛ در نتیجه «ظرفیت ناکافی» و «ظرفیت نامشخص» را از هم جدا نمی‌کرد و الزام اندازه‌گیری تازه در مرز پذیرش را کامل نمی‌کرد.

## اصلاح واقعی

1. متد `EvaluateCurrentCapacityState` برای سنجش تازهٔ RAM و فضای آزاد درایو سیستم اضافه شد.
2. این متد از محدودیت‌های ذخیره‌شدهٔ مالک استفاده می‌کند، نه کنترل‌های WPF؛ بنابراین از thread پردازش peer به UI وابسته نیست.
3. نمایش UI و مسیر درخواست شبکه اکنون از همان ارزیابی ظرفیت استفاده می‌کنند.
4. پذیرش هر درخواست peer، ظرفیت را دوباره اندازه می‌گیرد و enum دقیق `Ready`، `Insufficient` یا `Unknown` را به policy می‌دهد.
5. ظرفیت قطعی ناکافی با `CapacityInsufficient` برمی‌گردد، حتی اگر lifecycle به Error منتقل شده باشد. ظرفیت نامعلوم همچنان fail-closed است.
6. آزمون C# برای جلوگیری از اجرای inference هنگام ظرفیت ناکافی و آزمون Python برای کنترل اتصال مسیر peer به ارزیابی تازه اضافه شدند.
7. کد coordinator و زیرساخت سخت‌افزار سالم حفظ شدند؛ coordinator همچنان حداکثر یک inference هم‌زمان می‌پذیرد و درخواست دوم را بدون صف Busy می‌کند.

## وضعیت چهار معیار پذیرش

1. **رد درخواست در نبود مشارکت، مدل آماده‌نشده یا ظرفیت ناکافی:** تأیید شد؛ policy و آزمون‌های HostLifecycle بررسی می‌شوند و مسیر شبکه وضعیت دقیق ظرفیت را دوباره می‌سنجد.
2. **سقف inference هم‌زمان:** تأیید شد؛ آزمون concurrency یک درخواست پذیرفته‌شده از ۶۴ درخواست هم‌زمان و ۶۳ پاسخ Busy ثبت کرد.
3. **شرایط برق/idle و محدودیت منابع:** RAM فیزیکی آزاد و فضای آزاد درایو سیستم اندازه‌گیری و اعمال می‌شوند. در کد فعلی API معتبر برای تشخیص AC/idle ارائه نشده و این شروط ادعا یا شبیه‌سازی نشده‌اند. GPU inventory، توان compute یا VRAM را ثابت نمی‌کند.
4. **ردها با کد قابل مشاهده و آزمون:** تأیید شد؛ ظرفیت ناکافی/نامعلوم به `ProtocolError RESOURCE_LIMIT` نگاشت می‌شوند؛ mapper acceptance نیز در HostLifecycle اجرا شد.

## آزمون‌های دقیق commit

مبنای تمام نتایج زیر همان commit پیاده‌سازی `V 3.1.176` است؛ CI نسخهٔ قبلی به‌عنوان شاهد این تغییر استفاده نشده است.

- [کیفیت و graph guards — run 42889844427](https://github.com/artinrahimii/project/actions/runs/42889844427): **۴۹۴ تست Python موفق**؛ compileall و دو حالت graph_guard برای هر دو گراف موفق. گراف به‌روزرسانی ۲۴ گره/۸۰ یال با صفر خطا و صفر هشدار؛ گراف قدیمی ۲۳۰ گره/۴۲۲ یال با صفر خطا و یک هشدار `UNMAPPED_DEPENDENCY` روی T0001. متن هشدار یک مقدار metadata توصیفی است، نه شناسهٔ task؛ بدون تغییر نگه داشته شد.
- [Build ویندوز و HostLifecycle — run 42889844413](https://github.com/artinrahimii/project/actions/runs/42889844413): job ویندوز WPF، restore/build تمیز، publish، startup smoke و پذیرش installer موفق. job HostLifecycle نیز موفق؛ ۶۸ نشانگر PASS و پوشش خطی ۸۶٫۱۵٪. یک آزمون اختیاری real-model در این job به‌دلیل opt-in بودن SKIP شد و به‌عنوان موفق شمارش نشده است.
- [Inference واقعی محلی — run 42889844288](https://github.com/artinrahimii/project/actions/runs/42889844288): اجرای واقعی مدل محلی llama.cpp موفق؛ cancellation مشاهده و فرایند متعلق به آزمون متوقف شد. این نتیجه اثبات Android-to-Windows با دستگاه فیزیکی نیست.
- [سرویس Rendezvous — run 42889844469](https://github.com/artinrahimii/project/actions/runs/42889844469): ساخت container و HTTP contract smoke موفق.

## محدودیت‌های خارج از این وظیفه

آزمون فیزیکی Android-to-Windows، نصب پاک روی دستگاه هدف و in-place upgrade در این وظیفه انجام نشده‌اند؛ این‌ها معیار مستقل UPD-S1-020 و UPD-S1-021 هستند. workflow Android در زمان ثبت این گزارش هنوز آزمون instrumentation را اجرا می‌کرد؛ buildهای debug/release قبلاً موفق شده بودند، اما نتیجهٔ نهایی job اندروید را به‌عنوان pass کلی اعلام نمی‌کنم.

## فایل‌های تغییر‌یافته در commit پیاده‌سازی

- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/test_host_resource_settings.py`

گزارش و شواهد این پوشه نیز در commit مستندسازی ثبت می‌شوند. وظیفه فقط بر اساس آزمون‌های exact-head بالا done شده است؛ این نتیجه به معنی تکمیل پروژهٔ ۲۴‌وظیفه‌ای نیست.
