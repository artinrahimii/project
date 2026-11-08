# بازبینی محتوایی T0047 — نگاشت امن پیام‌های خطا

## خط مبنا و تصمیم
- commit منبع بررسی‌شده: `c933559b3be973f1cdf21e094b786c84742ca7ab` — `V 3.1.74`.
- وضعیت پایهٔ T0047: `done`.
- وظیفهٔ به‌روزرسانی مرتبط: `UPD-S1-012`.
- تصمیم: `re_review_required`؛ وضعیت پایه و تاریخچهٔ T0047 تغییر نمی‌کند.

## پیاده‌سازی و جلوگیری از افشا
- `src/Host.Application/HostErrorPresentation.cs` خطاهای network/model/resource/validation/timeout/busy/cancel/internal را به code و پیام‌های ثابت و قابل‌فهم تبدیل می‌کند.
- `ToUserMessage(Exception)` متن خام exception را بازنمی‌گرداند؛ برای ArgumentException نیز راهنمای ثابت تنظیم server URL/model ID ارائه می‌شود.
- نگاشت typed protocol error شامل InvalidMessage، UnsupportedVersion، UnsupportedModel، HostBusy، RequestExpired، ResourceLimit و Internal است.
- در `MainWindow.xaml.cs` چند مسیر catch واقعی از این mapper استفاده می‌کنند: اتصال backend، شروع engine محلی، تولید پیام، به‌روزرسانی مدل، ذخیره تنظیمات و shutdown. متن exception/path مستقیماً در این مسیرها نمایش داده نمی‌شود.
- Blobهای بررسی‌شده: `HostErrorPresentation.cs` = `24a02d41725659e1143d14f9d4a3e3e2297d54a6`؛ `MainWindow.xaml.cs` = `e2b2c3285f48fd8883b1781f5d98590eb6b77184`.

## آزمون‌ها
- `tests/HostLifecycle.Tests/HostErrorPresentationAcceptanceTests.cs` ۱۳ نوع exception و چند ProtocolErrorCode را بررسی می‌کند؛ مسیرهایی مانند `C:\\private\\model.gguf` و `token.json` عمداً در ورودی تست هستند و خروجی کاربر باید آن‌ها را حذف کند.
- `tests/HostLifecycle.Tests/Program.cs` این مجموعه را از طریق `HostErrorPresentationAcceptanceTests.Run()` اجرا می‌کند.
- `tests/test_wpf_entrypoint.py` نیز اتصال UI به mapping امن و عدم استفاده از raw exception text را نگهبانی می‌کند.
- Blobهای آزمون: C# acceptance = `1b48d53a8e704bfa238d9a6230d68a3e7f5874ef`؛ harness = `e63132c55538e289e5f49840943e35349e758052`؛ Python WPF regression = `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`.
- Product CI run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) به‌طور کامل HostLifecycle و WPF build/smoke را روی همین Blobها موفق اجرا کرد. Quality Gate دقیق V3.1.74 در [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109) نیز موفق شد.
- Product run جاری V3.1.74 [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134) هنگام آخرین مشاهده `in_progress` بود و در این گزارش موفق اعلام نمی‌شود.

## بازبینی یال T0040 → T0047
- در گراف اولیه یال `blocks` از `T0040` به `T0047` در این commit هنوز `unreviewed` و بدون rationale/evidence بود.
- بررسی source نشان داد `HostErrorPresentation` از enum مشترک `ProtocolErrorCode` تولیدشده از `protocol/whitep/v1/messages.proto` استفاده می‌کند؛ اما هیچ call مستقیمی به `ProtocolMessageParser` یا `ProtocolMessageValidator` ایجادشده در T0040 ندارد.
- خود enum و تولید کد مشترک پیش از T0040 در قرارداد canonical وجود داشته‌اند؛ پس خروجی اختصاصی parser/validator در T0040، ورودی فنی T0047 نیست.
- نتیجهٔ دقیق: یال `T0040 → T0047` به‌عنوان `sequencing_only/validated` ثبت می‌شود؛ `type=blocks` و ترتیب محافظه‌کارانه حفظ می‌شوند، اما وابستگی فنی قطعی ادعا نمی‌شود.
- این تصمیم در گراف Step Three با یک رکورد جدید در `edge_change_log` ثبت شده و به بازبینی متاثر از `UPD-S1-012` پیوند می‌خورد.
- یال `T0046 → T0047` همچنان `contract_or_interface_dependency/validated` است؛ T0047 مرز presentation خطا را در همان سطح UI تکمیل می‌کند.

## چرا بازبینی مجدد لازم است؟
`UPD-S1-012` در پذیرش/رد درخواست‌های مشارکت میزبان، خطاهای جدید trust/readiness/opt-in را وارد مسیرهای UI می‌کند. پس از پیاده‌سازی باید mapping typed و عدم افشای جزئیات در همهٔ مسیرهای جدید دوباره آزموده شود.

## خارج از محدودهٔ شواهد
این بازبینی به‌تنهایی امنیت شبکه، مدیریت replay، هویت P2P، live inference، ارتباط فیزیکی Android-to-Windows، clean install یا in-place upgrade را اثبات نمی‌کند.
