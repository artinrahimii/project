# T0137 — گزارش اجرای وظیفه

- **وضعیت:** `done`؛ معیارهای پذیرش مربوط به آزمون‌های امنیتی محلی/loopback و آزمون‌های همتای احرازشده با CI دقیق تأیید شده‌اند.
- **Attemptهای ثبت‌شده:** `T0137-A001` شکست کامپایل (حفظ‌شده در تاریخچه)؛ `T0137-A002` اصلاح و تأیید موفق.
- **Commit پیاده‌سازی اولیه:** [`V 3.0.479` — `80fa3a12c7387a1bfb5c8e39e9f1463cfd58b425`](https://github.com/artinrahimii/project/commit/80fa3a12c7387a1bfb5c8e39e9f1463cfd58b425).
- **Commit اصلاح و آزمون:** [`V 3.0.480` — `9a3ad37c0534832002a9aa96b52db6863838f746`](https://github.com/artinrahimii/project/commit/9a3ad37c0534832002a9aa96b52db6863838f746).

## خروجی پیاده‌سازی
- replay guard محدود در حافظه که کلید را بر اساس PeerId احرازشده + request_id + attempt_id ثبت می‌کند، ورودی‌های منقضی را پاک می‌کند و هنگام پر بودن ظرفیت، کلید زنده را حذف نمی‌کند و fail-closed می‌شود.
- مرز میزبان درخواست Protobuf نامعتبر را پیش از اجرای handler رد می‌کند؛ هویت transport را با PeerId مورد انتظار تطبیق می‌دهد؛ پاسخ خروجی را از نظر حد اندازه، قالب Protobuf و تطبیق RequestId/AttemptId/ModelId/Host identity بررسی می‌کند؛ replay دقیق را بدون اجرای دوبارهٔ inference رد می‌کند.
- آزمون‌های جدید tampering، replay-key semantics/capacity/expiry، پاسخ جعلی/نامنطبق و loopback واقعی `InferenceRequest`/`InferenceResponse` به runner افزوده شده‌اند.

## CI دقیق نسخهٔ اصلاحی — همه موفق
- [HostLifecycle unit/integration](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419409) — PASS؛ لاگ صریح: `PASS: tampered request rejection, bounded request replay fencing, forged response correlation rejection, and authenticated-peer transport checks.`
- [Windows WPF build/startup/DPAPI](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419686) — PASS.
- [Android lint/APK/Protobuf](https://github.com/artinrahimii/project/actions/runs/40744170733/job/123013419135) — PASS.
- [Real local llama.cpp/GGUF inference and cancellation](https://github.com/artinrahimii/project/actions/runs/40744170628/job/123013419660) — PASS؛ inference واقعی موفق شد، cancellation مشاهده شد و فرایند متعلق به تست تمیز متوقف شد.
- [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/40744170564/job/123013418399) — PASS.

## مرز ادعا
- `T0137` برای آزمون‌های محلی، parser/response correlation، replay guard، peer authentication و authenticated loopback تأیید شد.
- `T0128` (end-to-end روی دو رایانه یا دو شبکهٔ مستقل) همچنان `not_started` و NOT RUN است؛ این نتایج محلی ادعای موفقیت آن را ایجاد نمی‌کنند.
- Replay guard در حافظهٔ فرایند است؛ با restart پاک می‌شود. بنابراین ضد-replay بین restartها یا ذخیرهٔ پایدار deduplication ادعا نمی‌شود.
