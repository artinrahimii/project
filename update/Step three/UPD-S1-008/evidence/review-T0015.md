# بازبینی محتوایی T0015 — پشته و پروژهٔ اجرایی Windows

## مبنای بررسی
- مخزن/شاخه: `artinrahimii/project` / `main`.
- HEAD بررسی‌شده: `a3fa1f54d99db809cc670ece1cfc244239f6a727` — `V 3.1.61`.
- snapshot کد محصولی که در CI آزموده شد: `c31ac02d39993965bd693b53f626d5cb09be7b2c` — `V 3.1.60`.
- commit `V 3.1.61` پس از آن فقط گراف Update، ledger reconciliation و گزارش بازبینی T0020 را تغییر داده است؛ فایل‌های پروژه و پیکربندی Windows در این بازبینی تغییر نکرده‌اند.
- اجرای محصول: [White-P product build run 42482227320](https://github.com/artinrahimii/project/actions/runs/42482227320).
- کنترل کیفیت همان snapshot: [Step Three quality run 42482227315](https://github.com/artinrahimii/project/actions/runs/42482227315).

## Task baseline و تصمیم
- شناسه: `T0015`.
- شرح: انتخاب C#، .NET LTS و رابط WPF و ایجاد پروژهٔ اجرایی Windows.
- وضعیت پایه و نهایی: `done → done`.
- پیش‌نیاز مستقیم: `T0014` با وضعیت `done`.
- بررسی زنجیرهٔ قبلی: `T0013` و `T0010` نیز `done` هستند؛ تغییری در وابستگی یا ترتیب قدیمی لازم نیست.
- وظایف Update مرتبط: `UPD-S1-001` و `UPD-S1-024`.
- تصمیم reconciliation: `unchanged`؛ `status_changed=false`.

## بررسی سورس و خروجی
- `src/Host.Windows/WhiteP.Host.Windows.csproj` یک پروژهٔ اجرایی WPF با `OutputType=WinExe`، `TargetFramework=net10.0-windows` و `UseWPF=true` است.
- `src/Host.Windows/App.xaml` و `App.xaml.cs` نقطهٔ ورود واقعی WPF و چرخهٔ startup برنامه را فراهم می‌کنند؛ `MainWindow.xaml` و `MainWindow.xaml.cs` پنجره و منطق اجرایی میزبان را پیاده می‌کنند.
- خط مبنای SDK در `global.json` و تصمیم پلتفرم در `docs/decisions/ADR-0001-platform-build-baselines.md` ثبت شده‌اند.
- workflow محصول از clone تمیز restore، clean، formatting verification، build، publish برای `win-x64` و بررسی فایل اجرایی خروجی را اجرا می‌کند.

## آزمون و نتیجهٔ CI
- Job ویندوز در run `42482227320` با نتیجهٔ `success` خاتمه یافت.
- مراحل مرتبط موفق: restore از clean checkout، clean solution، `dotnet format`، build میزبان، publish، بررسی خروجی publish، بررسی منبع Protobuf تولیدشده، smoke test راه‌اندازی WPF و آزمون‌های سیاست ابزار/ذخیرهٔ هویت و trust با DPAPI.
- مرحلهٔ ساخت و پذیرش installer ویندوز نیز در همان job موفق شد.
- Job مستقل HostLifecycle در همان run با نتیجهٔ `success` خاتمه یافت.
- Quality run `42482227315` نیز موفق بود؛ suite شامل 479 آزمون Python، compileall و هر دو graph guard را گذراند.
- نتیجه فقط انتخاب پشته، وجود پروژهٔ قابل build، راه‌اندازی WPF و کنترل‌های همان job را می‌پوشاند؛ اجرای مدل روی سخت‌افزار کاربر یا ارتباط فیزیکی Android-to-Windows از این آزمون نتیجه‌گیری نمی‌شود.

## مرز UPD-S1-024
هویت بصری مشترک، آیکون سکهٔ P و تم پنج‌رنگ هنوز معیار مستقل `UPD-S1-024` است. این وظیفه به دلیل وجود پروژهٔ WPF تکمیل‌شده، آن معیار را done نمی‌کند و هیچ ادعایی دربارهٔ اعمال آیکون/تم نهایی ندارد.

## نتیجه
معیارهای محدود T0015 دربارهٔ انتخاب C#/.NET LTS/WPF، وجود پروژهٔ اجرایی و build/startup قابل بررسی با سورس و CI تأیید شدند. تصمیم `unchanged` است و وضعیت پایهٔ `done` حفظ می‌شود. هیچ status یا dependency در گراف قدیمی تغییر نمی‌کند.
