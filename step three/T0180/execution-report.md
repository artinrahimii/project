# T0180 — پاک‌سازی payloadها و بافرهای موقت

**وضعیت:** `in_progress`؛ پیاده‌سازی A001 آمادهٔ اعتبارسنجی CI است.  
**مخزن/شاخه:** `artinrahimii/project` / `main`  
**نسخهٔ مبنا:** `1c329bd92f403ee380962dc7a237a3fce19bf84e` — `V 3.0.821`  
**تلاش:** `T0180-A001`  
**زمان شروع:** 2026-11-02T17:03:47+03:30

## بررسی پیش‌نیاز و یال‌ها

- `T0177` و `T0179` در گراف مبنا `done` بودند و خروجی/شواهد ثبت‌شدهٔ آن‌ها بررسی شد.
- یال‌های ورودی این وظیفه به `sequencing_only` اعتبارسنجی شدند؛ `type=blocks` و ترتیب اجرایی حفظ شدند.
- یال `T0180 → T0181` هنوز `unreviewed` است.

## تغییرات واقعی

- Windows: صفرکردن frame ارسالی، پیام ورودی در صورت شکست انتقال مالکیت به صف، پیام‌های باقی‌مانده در صف هنگام disposal، و بافرهای request/response در میزبان پس از پایان مصرف.
- Windows: افزودن helper برای پاک‌سازی آرایه‌های موقت پیام Protobuf مربوط به status/rejection/busy.
- Android: صفرکردن بافر قبلی، بافر ترکیبی و frameهای کامل decoder؛ پاک‌سازی وضعیت ناقص در پایان/بسته‌شدن stream و پاک‌سازی تمام frameها حتی اگر callback یکی را با خطا متوقف کند.
- افزودن آزمون JVM برای مشاهدهٔ واقعی backing array و آزمون‌های قراردادی Python.
- ثبت مرز تضمین و محدودیت‌ها در `docs/security.md`.

## فایل‌های تغییر‌یافته

- `src/Host.Windows/Networking/Libp2pPeerMessageProtocol.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/PeerMessageFrameCodec.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/PeerMessageFrameCodecTest.kt`
- `docs/security.md`
- `tests/test_sensitive_buffer_cleanup.py`
- `step three/project-graph.md`

## آزمون و پذیرش

آزمون‌های جدید اضافه شده‌اند، اما CI دقیق commit هنوز اجرا/بررسی نشده است. به همین دلیل T0180 در وضعیت `in_progress` می‌ماند و هیچ نتیجهٔ موفق آزمون ادعا نمی‌شود. معیار پذیرش نهایی، موفقیت Step Three quality gate، Android lint/compile/APK/JVM، Windows WPF/HostLifecycle و container checks روی همان commit است.

## محدودیت

صفرشدن کپی‌های بایتی تحت مالکیت مستقیم برنامه ادعا می‌شود؛ صفرشدن objectهای immutable، کپی‌های runtime، بافرهای داخلی کتابخانه/سیستم‌عامل، pagefile/swap یا crash dump اثبات نمی‌شود. frame خروجی Android تا تکمیل/شکست عملیات نوشتن حفظ می‌شود تا دادهٔ درحال‌ارسال تغییر نکند.
