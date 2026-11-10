# گزارش اجرای UPD-S1-013 — اندازه‌گیری ظرفیت و آمادگی میزبان Windows

- مخزن: `artinrahimii/project`
- شاخهٔ مبنا: `main`
- commit مبنا: `c7c2f86ae557d1b1983e896f7410eddc76a64796` — `V 3.1.175`
- شروع این تلاش: `2026-11-10T19:49:43Z`
- وضعیت فعلی: **in_progress**؛ منتظر CI دقیق commit این اصلاح هستیم.

## ممیزی خط مبنا

کد موجود بازنویسی نشد. اجزای زیر از commit مبنا بررسی شدند:

- `WindowsHostHardwareDiscovery`: اندازه‌گیری واقعی RAM فیزیکی آزاد و کل، نسخهٔ سیستم‌عامل، CPU و فضای آزاد/کل درایو سیستم در Windows.
- `HostReadinessEvaluator`: مقایسهٔ fail-closed RAM و فضای آزاد دیسک با محدودیت‌های پایدار مالک؛ مقدار نامعتبر یا اندازه‌گیری ناموجود Ready نمی‌شود.
- `HostInferenceSession`: health check واقعی backend، آزمون آمادگی پیش از اتصال و بازبینی readiness/قابلیت مدل قبل از inference.
- `HostRequestAdmissionPolicy`: بررسی مشارکت، وضعیت مدل، تطبیق Model ID، سقف خروجی مدل/پروتکل، ظرفیت و deadline.
- `HostRequestCoordinator`: `SemaphoreSlim(1,1)`، پذیرش بدون صف و پاسخ Busy به درخواست هم‌زمان؛ محدودیت حداکثر یک inference حفظ شد.
- `LocalHostSettingsStore`: نگهداری نسخه‌دار و اعتبارسنجی حداقل RAM و فضای آزاد دیسک.

## شکاف احرازشده

مسیر دریافت درخواست peer از مقدار cached بولی `_capacityReady` استفاده می‌کرد. این مقدار بین دو حالت متفاوت «ظرفیت ناکافی» و «ظرفیت نامشخص» تمایز نداشت و اندازه‌گیری را دقیقاً در مرز پذیرش درخواست تازه نمی‌کرد. در نتیجه، سیاست admission می‌توانست به‌جای `CapacityInsufficient` فقط `CapacityUnknown` ثبت کند و به مقدار قدیمی UI متکی بماند؛ هرچند `HostInferenceSession` پیش از اجرای مدل دوباره readiness را بررسی می‌کرد.

## اصلاح واقعی این تلاش

1. منطق اندازه‌گیری ظرفیت به متد مشترک `EvaluateCurrentCapacityState` منتقل شد.
2. متد از snapshot واقعی Windows و آستانه‌های ذخیره‌شدهٔ مالک استفاده می‌کند؛ به کنترل‌های WPF دسترسی ندارد و از thread پردازش peer قابل فراخوانی است.
3. UI همان ارزیابی مشترک را مصرف می‌کند.
4. مسیر درخواست شبکه در لحظهٔ دریافت درخواست، RAM/دیسک را دوباره می‌سنجد و enum دقیق `Ready`، `Insufficient` یا `Unknown` را به admission می‌دهد.
5. اگر ظرفیت به‌طور قطعی ناکافی باشد، policy آن را با کد typed `CapacityInsufficient` حتی در صورت انتقال lifecycle به Error گزارش می‌کند؛ وضعیت نامشخص همچنان fail-closed می‌ماند.
6. آزمون HostLifecycle برای rejection ناکافی‌بودن ظرفیت اضافه شد و آزمون قراردادی Python سیم‌کشی مسیر peer به ارزیابی تازه را کنترل می‌کند.

## محدودیت‌های سخت‌افزاری

- RAM و فضای دیسک توسط APIهای Windows اندازه‌گیری می‌شوند.
- پشتیبانی backend و مدل با health/capability واقعی بررسی می‌شوند.
- سقف هم‌زمانی ۱ حفظ شده است؛ هیچ درخواست دوم در صف قرار نمی‌گیرد.
- سیستم فعلی سنجش قابل اتکای وضعیت برق AC یا idle را در این مسیر پیاده‌سازی نکرده است؛ این محدودیت جعل یا به‌عنوان شرط اجراشده ادعا نمی‌شود.
- inventory کارت نمایشگر، اثبات توان پردازش GPU/VRAM نیست. کد موجود این محدودیت را صریحاً اعلام می‌کند.

## فایل‌های تغییر‌یافته

- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/test_host_resource_settings.py`
- `update/Step two/project-graph.md` (ثبت تلاش و وضعیت in_progress)

## اعتبارسنجی

- آزمون واحد admission برای `CapacityInsufficient` اضافه شد، اما در این محیط هنوز اجرا نشده است.
- آزمون قراردادی سیم‌کشی ظرفیت تازه اضافه شد، اما در این محیط هنوز اجرا نشده است.
- اجرای محلی .NET، Python و buildهای Windows/Android در نشست محلی انجام نشد؛ checkout شبکه‌ای GitHub در این runtime در دسترس نبود.
- CI خودکار روی commit این تلاش باید کیفیت، graph_guard، Windows WPF، Android و HostLifecycle را اعتبارسنجی کند؛ نتیجهٔ این CI هنوز ثبت نشده است.
- CI موفق commit مبنا `V 3.1.175` به‌عنوان اثبات این تغییر جدید استفاده نمی‌شود.

## موانع و ادامه

این تلاش مانع فنی دائمی ندارد؛ در انتظار CI دقیق commit جدید است. تا موفقیت آزمون‌های دقیق commit، وظیفه `in_progress` باقی می‌ماند و معیارهای UPD-S1-013 تکمیل‌شده محسوب نمی‌شوند.
