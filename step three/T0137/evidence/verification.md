# T0137 — شواهد و اعتبارسنجی

## دریافت و اعتبارسنجی گراف مبنا
- Branch: `main`; commit مبنا: `V 3.0.480` — `9a3ad37c0534832002a9aa96b52db6863838f746`.
- Tree: `7a62d234d584aa6c8f0514a494a1d252ed209abd`; graph Blob: `9cbefc74b737fb81eab1c1621168d2725bb56517`.
- اندازهٔ محتوای اصلی: **2,328,976 بایت UTF-8**؛ طول رشته: **2,206,971 نویسه**.
- SHA-1 صحیح شیء Git (نوع + طول بایتی + NUL + بایت‌های کامل شامل حصار Markdown) با Blob ID در درخت تطبیق داشت؛ JSON کامل parse شد: **229 وظیفه / 418 یال فعال**.

## تلاش A001 — خطای کامپایل حفظ‌شده
- `V 3.0.479` در HostLifecycle job `123011004076` شکست خورد: `CS1061` به‌علت نبود `using Google.Protobuf;` و `CS0103` به‌علت نبودن لینک صریح `PeerResponseCorrelationValidator.cs` در `HostLifecycle.Tests.csproj`. Assertions جدید در آن commit اجرا نشدند.
- [لاگ شکست A001](https://github.com/artinrahimii/project/actions/runs/40743489739/job/123011004076).

## تلاش A002 — اصلاح و شواهد پذیرش موفق
- `V 3.0.480` اصلاح کرد: namespace افزونه‌های Protobuf به آزمون امنیتی افزوده شد و `PeerResponseCorrelationValidator.cs` به فهرست Compile صریح پروژهٔ آزمون لینک شد.
- [HostLifecycle integration PASS](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419409): شامل آزمون‌های جدید tampered request/replay/forged response و آزمون‌های قبلی identity mismatch، plaintext-only و secure-channel-version mismatch.
- [Windows WPF build and DPAPI identity/trust PASS](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419686).
- [Android APK/lint/Protobuf PASS](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419135).
- [Real local inference and cancellation PASS](https://github.com/artinrahimii/project/actions/runs/40744170628/job/123013419660).
- [Graph/Python quality gate PASS](https://github.com/artinrahimii/project/actions/runs/40744170564/job/123013418399).

## محدوده و ریسک
- Replay cache فقط در حافظه است و پس از restart پاک می‌شود؛ replay بین restartها دفع‌شده فرض نمی‌شود.
- آزمون loopback/CI جایگزین آزمون `T0128` روی دو شبکهٔ مستقل نیست؛ آن وظیفه هنوز NOT RUN است.
