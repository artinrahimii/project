# بازبینی مجدد T0020 — یکتایی شناسهٔ ADR

بازبینی قبلی T0020 در V3.1.61 وضعیت `unchanged/reviewed` ثبت کرده بود؛ اما دو تصمیم مستقل هر دو شناسهٔ ADR-0003 داشتند: `ADR-0003-local-inference-backend.md` و `ADR-0003-tcp-request-framing.md`. آزمون موجود یکتایی شناسه را enforce نمی‌کرد، پس شواهد قبلی حفظ و این بازبینی دوباره pending شد.

اصلاح انجام‌شده:
- ADR-0003 برای تصمیم قدیمی‌تر local inference حفظ شد.
- TCP framing به ADR-0024 منتقل شد.
- مسیر قدیمی TCP به‌عنوان redirect تاریخی باقی ماند تا پیوندهای T0111/T0114 نشکنند.
- رجیستر به ADR-0024 پیوند می‌دهد.
- تست traceability یکتایی IDهای رجیستر و فایل‌های واقعی ADR را بررسی می‌کند و redirect را تصمیم مستقل نمی‌شمارد.

وضعیت پایهٔ T0020 در گراف قدیمی `done` باقی می‌ماند؛ هیچ وضعیت/یال/نشانگر قدیمی تغییر نمی‌کند. وضعیت reconciliation تا CI دقیق اصلاح: `pending/re_review_required`.

V3.1.63 quality CI پیش از این اصلاح اجرا شده و قبولی regression جدید را اثبات نمی‌کند. اجرای suite کامل، compileall و دو graph guard روی commit اصلاحی ضروری است. هیچ آزمون live inference، Android فیزیکی، Android-to-Windows، clean install یا in-place upgrade در این بازبینی انجام نشده است.


## نتیجهٔ نهایی آزمون روی commit اصلاحی

- commit: `365c734e178013fc6ea9499bf08855e91f5d0cbe` — `V 3.1.64`.
- quality run دقیق commit: [42483596179](https://github.com/artinrahimii/project/actions/runs/42483596179) — **success**.
- regression یکتایی ADR، آزمون‌های Python، compileall، Update graph guards و Step Three graph/output-evidence guards موفق شدند.
- تصمیم reconciliation نهایی T0020: `unchanged`; وضعیت بازبینی: `reviewed`; وضعیت پایهٔ قدیمی `done` حفظ شد.
- هیچ ادعایی دربارهٔ live inference، Android-to-Windows یا آزمون دستگاه واقعی در این بررسی وجود ندارد.
