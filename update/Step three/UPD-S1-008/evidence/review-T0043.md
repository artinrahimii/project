# بازبینی محتوایی T0043 — مدل وضعیت چرخهٔ عمر میزبان

## خط مبنا
- commit کد بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`; HEAD فعلی هنگام ثبت: `d22f96dfaf1997593ac3907f2870713804247cb9` — `V 3.1.72`.
- وضعیت پایهٔ T0043: `done`; پیش‌نیازهای T0042 و T0040 نیز `done`.
- نگاشت جدید: `UPD-S1-012`.
- تصمیم: `re_review_required`; وضعیت پایهٔ قدیمی و تاریخچه حفظ می‌شوند.

## پیاده‌سازی و آزمون
- مدل Domain در `src/Host.Domain/HostLifecycle.cs` تعریف شده است.
- enum دقیقاً پنج وضعیت دارد: `Stopped`, `Starting`, `Ready`, `Busy`, `Error`.
- جدول انتقال معتبر محدود است: Stopped→Starting؛ Starting→Ready/Error/Stopped؛ Ready→Busy/Error/Stopped؛ Busy→Ready/Error؛ Error→Starting/Stopped.
- `Transition` پیام خالی/فاصله‌ای را رد می‌کند، transition نامعتبر را با `InvalidOperationException` رد می‌کند و پیام معتبر را trim می‌کند.
- `tests/HostLifecycle.Tests/Program.cs` آزمون‌های initial Stopped، عبور از پنج state، مسیرهای recovery، transitionهای غیرمجاز، پیام trim‌شده و پیام خالی را اجرا می‌کند. همان فایل همچنین ماتریس انتقال T0044 را به‌صورت exhaustive بررسی می‌کند.
- Blobهای کد lifecycle، Program.cs، پروژهٔ Domain و پروژهٔ آزمون در HEAD فعلی با commit کد Product CI یعنی V3.1.70 یکسان‌اند:
  - HostLifecycle.cs: `22e09c2867509ea6a3b4efac87f6dc7646e35ae3`
  - HostLifecycle.Tests Program.cs: `e63132c55538e289e5f49840943e35349e758052`
  - HostLifecycle.Tests.csproj: `63c666374f7a7bf7f14a9c8acda26748d60e5172`
  - Host.Domain.csproj: `9d9dfba0836f1c5e2bd50245eeb4ea374afc7cca`
- Quality CI دقیق V3.1.70 در [run 42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق شد.
- Product CI دقیق همان کد در [run 42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) با نتیجهٔ success هر سه job را تکمیل کرد: Windows WPF، Android APK و HostLifecycle unit/integration.
- گزارش اولیهٔ T0043 یک کیفیت‌گیت ناموفق به دلیل metadata شواهد گراف ثبت کرده بود؛ این خطای source/build نبود. Quality run جدید روی گراف اصلاح‌شده موفق شده و Product run فعلی نیز سه job را سبز کرده است.
- اجرای قبلی اختصاصی T0043 نیز [run 39910955252](https://github.com/artinrahimii/project/actions/runs/39910955252) را برای HostLifecycle، Windows و Android موفق ثبت کرده بود.

## چرا بازبینی مجدد لازم است؟
- `UPD-S1-012` هنوز `not_started` است و کنترل مشارکت، توقف فوری پذیرش درخواست جدید، و وضعیت‌های خاموش/روشن/متصل/آماده/مشغول/خطا را در محصول ادغام خواهد کرد.
- مدل وضعیت و state machine فعلی پایهٔ مناسبی دارد، اما رفتار opt-in و توقف listener ممکن است انتقال‌ها/حالت‌های دیگری را لازم کند.
- بنابراین وضعیت بازبینی فعلی `reviewed` و تصمیم `re_review_required` است تا پس از اجرای UPD-S1-012 دوباره آزموده شود؛ نه `unchanged` و نه `pending`.

## محدودهٔ ادعا
- CI build و آزمون واحد/یکپارچه را روی runnerهای GitHub اثبات می‌کند؛ نصب روی دستگاه فیزیکی، مدل محلی واقعی، Android-to-Windows P2P inference، clean install و in-place upgrade در این بازبینی انجام نشده است.
- وضعیت پایهٔ T0043 و یال‌های گراف اولیه تغییر نکرده‌اند.
