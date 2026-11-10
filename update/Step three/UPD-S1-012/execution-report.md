# UPD-S1-012 — گزارش اجرای وظیفه

**وضعیت:** `done`  
**مبنای ممیزی:** `main` در commit `5aea044ec035aa9328ba5cc4f09f5df7b8537c74` (`V 3.1.144`)  
**عملیات منبع:** `modify`  
**نتیجه:** پیاده‌سازی تولیدی موجود بررسی شد؛ شکاف پذیرش مشاهده‌شده‌ای که نیازمند تغییر کد باشد باقی نماند. از ایجاد listener یا مسیر موازی خودداری شد.

## دامنه و تصمیم

مسیر واقعی مشارکت میزبان در WPF از `NetworkParticipationToggle` به `PeerAnnouncementHostLifecycle` متصل است. این مسیر با پیش‌فرض خاموش آغاز می‌شود و فقط پس از اقدام مالک، بررسی آمادگی backend و ظرفیت، پذیرش هشدار حریم خصوصی و تأیید صریح هویت همتای مشخص شروع می‌شود.

در خاموش‌سازی، سرویس ابتدا پذیرش درخواست جدید را می‌بندد، سپس چرخهٔ دریافت/انتشار را لغو می‌کند، درخواست‌های فعال را تخلیه می‌کند و منابع انتقال را آزاد می‌کند. خاموش‌کردن مشارکت، موتور یا فایل مدل محلی را حذف نمی‌کند.

## معیارهای پذیرش

1. **پیش‌فرض خاموش:** کنترل XAML بدون حالت فعال اولیه تعریف شده و سازندهٔ پنجره وضعیت «مشارکت غیرفعال؛ listener باز نیست» را نمایش می‌دهد. هیچ فراخوانی خودکاری برای شروع سرویس در سازنده وجود ندارد.
2. **رضایت صریح مالک:** فعال‌سازی از تغییر کنترل کاربر آغاز می‌شود؛ هشدار حریم خصوصی با پاسخ پیش‌فرض No نمایش داده می‌شود؛ تأیید PeerId ناشناخته نیز مستقل و صریح است. رد هرکدام کنترل را خاموش نگه می‌دارد و سرویس ساخته/فعال نمی‌شود.
3. **عدم پذیرش در حالت خاموش:** سرویس تولیدی فقط پس از فعال‌سازی صریح شروع می‌شود. `StopAsync` مقدار فعال‌بودن را پیش از لغو receive loop صفر می‌کند و مسیر دریافت، وضعیت غیرفعال را پیش از ثبت/اجرای handler بررسی می‌کند.
4. **توقف و حفظ مدل:** خاموش‌کردن، `PeerAnnouncementHostLifecycle.DisposeAsync` را اجرا می‌کند؛ کارهای فعال لغو و تخلیه می‌شوند. این مسیر `DisconnectAsync` یا حذف مدل محلی را فراخوانی نمی‌کند. خاموش‌سازی پنجره نیز transition فعال‌سازی/غیرفعال‌سازی را لغو و منتظر پایان آن می‌ماند.

## کد و آزمون‌های بررسی‌شده

- `src/Host.Windows/MainWindow.xaml`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/Networking/Libp2pPeerTransport.cs`
- `src/Shared.Security/PeerTrustStore.cs`
- `src/Host.Application/HostReadinessEvaluator.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/ParticipationNetworkListenerLifecycleTests.cs`
- `tests/HostLifecycle.Tests/ParticipationActivationRecoveryTests.cs`
- `tests/test_network_listener_exposure.py`

## آزمون‌های واقعی اجراشده در CI روی همین commit

- دستور `dotnet build tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release --no-incremental` موفق شد.
- دستور مستند `dotnet run --project tests/HostLifecycle.Tests/HostLifecycle.Tests.csproj --configuration Release` موفق شد. log شامل PASS برای رد حالت غیرفعال، توقف/آزادسازی listener، کشف metadata، lifecycle میزبان، اعتماد همتا، احراز هویت libp2p، جلوگیری از replay، لغو درخواست و تخلیهٔ درخواست‌های فعال است.
- آزمون Python کامل: `python -m unittest discover -s tests -v`، تعداد 491 آزمون، نتیجه `OK`. این مجموعه شامل regression guardهای واقعی اتصال WPF و ترتیب shutdown است.
- ساخت محصول ویندوز، انتشار، آزمون smoke واقعی شروع WPF روی Windows runner، ساخت installer و آزمون‌های lifecycle موفق شدند.
- quality gate و هر دو graph guard در CI روی commit مبنا موفق شدند.

## مرز شواهد و موارد خارج از ادعا

آزمون `PeerHostLifecyclePublishesAndServesTrustedPeerAsync` از دو هویت libp2p مجزا، transport رمزگذاری‌شده، سرویس rendezvous loopback و backend HTTP آزمایشی برای آزمون correlation، replay، status و cancellation استفاده می‌کند. این آزمون، آزمون فیزیکی Android-to-Windows یا اجرای مدل واقعی روی میزبان کاربر نیست. همچنین screenshot/installer CI جایگزین نصب و اجرای مستقل روی رایانهٔ کاربر نیست.

بنابراین این گزارش فقط تکمیل رفتار مشارکت اختیاری میزبان در دامنهٔ UPD-S1-012 را ثبت می‌کند؛ آزمون واقعی Android-to-Windows، مدل محلی واقعی در مسیر انتها‌به‌انتها، نصب تمیز و ارتقای درجا همچنان gateهای جداگانه‌اند.

## فایل‌های تغییر‌یافته

در این وظیفه کد محصول تغییر نکرد؛ دو فایل گزارش/شواهد این وظیفه ایجاد شدند. کد و آزمون‌های موجود به علت انطباق با معیارهای این وظیفه حفظ شدند.
