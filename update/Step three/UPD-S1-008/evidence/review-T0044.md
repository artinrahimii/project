# بازبینی محتوایی T0044 — ماتریس انتقال وضعیت میزبان

## مبنا و نتیجهٔ اولیه
- source/test commit: `8e9aacc84a37e14f04baf48ccd691b59d5de59df` — `V 3.1.70`.
- reconciliation base: `fd15be99a60d34fc4f08cc21857b436f1e747884` — `V 3.1.73`.
- وضعیت پایهٔ T0044: `done`; پیش‌نیاز T0043 و T0040 نیز `done`.
- تصمیم فعلی: `re_review_required`; وضعیت پایه و سوابق قدیمی حفظ می‌شوند.

## ماتریس پیاده‌سازی‌شده
کلاس `src/Host.Domain/HostLifecycle.cs` یک ماتریس صریح و محدود برای پنج حالت دارد:
- `Stopped → Starting`
- `Starting → Ready | Error | Stopped`
- `Ready → Busy | Error | Stopped`
- `Busy → Ready | Error`
- `Error → Starting | Stopped`

انتقال خارج از ماتریس با `InvalidOperationException` رد می‌شود؛ پیام خالی رد و پیام غیرخالی trim می‌شود. آزمون‌های `tests/HostLifecycle.Tests/Program.cs` ماتریس را با زوج‌های وضعیت بررسی می‌کنند، انتقال‌های مجاز و غیرمجاز، state اولیه، مسیر recovery و حفظ message را ارزیابی می‌کنند.

## وابستگی و شواهد CI
- `T0043 → T0044`: `hard_dependency/validated`; state machine باید پیش از ماتریس انتقال تعریف شده باشد.
- `T0040 → T0044`: `sequencing_only/validated`; این یال ترتیب اجرای مرحله است، نه قرارداد مستقیم بین APIها.
- HostLifecycle source و test harness روی commit بررسی‌شده V3.1.70 همان Blobهای اجراشده در Product CI هستند.
- Quality CI دقیق V3.1.70 در [run 42484937846](https://github.com/artinrahimii/project/actions/runs/42484937846) موفق شد.
- Product CI دقیق همان commit در [run 42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) هر سه job Windows WPF، Android APK و HostLifecycle را با موفقیت تکمیل کرد.

## چرا re-review لازم است؟
`UPD-S1-012` هنوز `not_started` است و consent/opt-in، توقف فوری پذیرش درخواست و کنترل participation را به همین lifecycle اضافه خواهد کرد. بنابراین ماتریس کنونی پایهٔ معتبر است، اما ممکن است transitionهای تازه یا تغییرات sequencing لازم شود. تا پس از اجرای UPD-S1-012 و اجرای مجدد آزمون کامل ماتریس، T0044 را `unchanged` نهایی نمی‌کنیم.

## مرز شواهد
CI و آزمون‌های HostLifecycle روی runnerهای GitHub موفق‌اند. این مورد اثبات نصب روی میزبان فیزیکی، اجرای مدل محلی، Android-to-Windows P2P، clean install یا in-place upgrade نیست. وضعیت پایهٔ T0044 و تمام یال‌های گراف قدیمی دست‌نخورده‌اند.
