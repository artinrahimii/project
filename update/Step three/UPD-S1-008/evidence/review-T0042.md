# بازبینی محتوایی T0042 — تفکیک لایه‌های میزبان ویندوز

## خط مبنا و وضعیت قدیمی
- شاخه: `main`.
- commit کد بررسی‌شده: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`.
- HEAD فعلی مخزن هنگام ثبت این بازبینی: `a259b29171131ce4ec17fecaadca7c24cc7523f2` — `V 3.1.71`؛ تغییرات آن نسبت به V3.1.70 در گزارش/گراف reconciliation بوده و فایل‌های کد/پروژهٔ مورد ممیزی در همان SHAهای ثبت‌شده باقی مانده‌اند.
- وضعیت پایهٔ `T0042`: `done`; پیش‌نیازهای `T0041` و `T0040` نیز `done`.
- نگاشت تأییدشده: `UPD-S1-001`, `UPD-S1-012`, `UPD-S1-024`.
- تصمیم این بازبینی: `re_review_required`; وضعیت پایه و تاریخچهٔ T0042 تغییر نمی‌کنند.

## بررسی ساختار واقعی پروژه‌ها
- Domain: `src/Host.Domain/WhiteP.Host.Domain.csproj`؛ بدون وابستگی project reference؛ شامل مدل/چرخهٔ عمر میزبان.
- Application: `src/Host.Application/WhiteP.Host.Application.csproj`؛ وابسته به Domain و Shared.Contracts؛ به Infrastructure concrete وابسته نیست.
- Infrastructure: `src/Host.Infrastructure/WhiteP.Host.Infrastructure.csproj`؛ وابسته به Application و Shared.Contracts؛ شامل backend adapter و parser.
- UI: `src/Host.Windows/WhiteP.Host.Windows.csproj`؛ WPF و referenceهای لایه‌های موردنیاز را مصرف می‌کند.
- قرارداد مشترک: `src/Shared.Contracts/WhiteP.Shared.Contracts.csproj`؛ مالک واحد تولید C# از `protocol/whitep/v1/messages.proto` است.
- `tests/test_host_layer_projects.py` سه آزمون ساختاری دارد: وجود پروژه‌ها و ثبت در solution، جهت وابستگی Application به abstraction، و تولید Protobuf فقط در Shared.Contracts.
- Blobهای فعلی پروژه‌ها هنگام بررسی: Domain `9d9dfba0836f1c5e2bd50245eeb4ea374afc7cca`; Application `229e1b222a349d1c4fc2c5b4eb0b81a65daa910b`; Infrastructure `f48808132365a0edc8e2fbc2c371116b7df2aac0`; Windows `3ce1023c24cfed4f5db8528cd4ece4ab3124bb28`; Shared.Contracts `3016d2abf624f95137aa27953cf5fc4b0cda2a8c`; layer tests `5a01e5ecfd7e4054cfcdbc1302bcc76a999a4681`.

## وابستگی‌ها و تصمیم
- `T0041 → T0042`: `hard_dependency/validated`; حفظ WPF entry point، MainWindow و smoke-test path لازم است. شواهد قبلی WPF smoke در run `39862296386`.
- `T0040 → T0042`: `contract_or_interface_dependency/validated`; انتقال parser/validator و تولید یکتای Protobuf به Shared.Contracts باید قرارداد و محدودیت‌ها را حفظ کند.
- `T0042 → T0043`: در گراف قدیمی همچنان `unreviewed`؛ این بازبینی آن را تغییر نمی‌دهد.
- کد لایه‌ها و آزمون ساختاری همچنان در مخزن وجود دارند. Quality CI دقیق V3.1.70 در [run 42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق شد.
- Product run دقیق V3.1.70 در [run 42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793)، Windows WPF build و HostLifecycle jobs را موفق کرد؛ Android client job در زمان ممیزی هنوز `in_progress` بود، پس کل workflow را successful اعلام نمی‌کنیم.
- Product build دقیق زمان تکمیل T0042 در [run 39907751958](https://github.com/artinrahimii/project/actions/runs/39907751958) با موفقیت کامل Windows، HostLifecycle و Android/Protobuf generation را تأیید کرده بود.

## چرا بازبینی مجدد لازم است؟
- `UPD-S1-012` هنوز `not_started` است و کنترل مشارکت اختیاری، حالت‌های وضعیت میزبان و رفتار توقف را در میزبان Windows تغییر خواهد داد.
- `UPD-S1-024` هنوز `not_started` است و آیکون، palette، منابع و صفحات UI هر دو برنامه را تغییر خواهد داد.
- این تغییرات لزوماً مرز پروژه‌های Domain/Application/Infrastructure را نقض نمی‌کنند؛ اما ممکن است فایل‌های UI، منابع و project configuration را تحت تأثیر قرار دهند. تا پس از اجرای این دو وظیفه، بازاعتبارسنجی نهایی مرزها و build هر دو سکو انجام نشود، تصمیم `unchanged` قطعی نیست.
- به همین دلیل، بازبینی فعلی `reviewed/re_review_required` است، نه pending و نه unchanged.

## موارد خارج از ادعا
- این بررسی اثبات load/execute مدل محلی، اتصال واقعی Android به Windows، پاسخ واقعی inference، Room persistence، نصب فیزیکی/clean install یا in-place upgrade نیست.
- Android job مربوط به Product run V3.1.70 در زمان ممیزی کامل نشده بود.
- وضعیت پایهٔ T0042 و تمام یال‌های گراف قدیمی دست‌نخورده باقی می‌مانند.
