# بازبینی T0179 — log ساختاریافته با allowlist و بدون محتوای حساس

- مبنای main: `93a6b094ac3114780b02a821c553b13cf3ed2f4d` — `V 3.1.94`
- source code baseline: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-007، UPD-S1-018

## کد و رفتار

- API مشترک Windows فقط enumهای event/outcome و مدت زمان غیرمنفی را می‌پذیرد؛ API رشته‌ای برای request، response، peer، model، address، key، exception یا prompt ندارد.
- Android نیز فقط event/outcome code و duration را می‌پذیرد.
- هر دو logger سقف ۸۰ رکورد در هر process دارند؛ schema فقط کدهای عددی و زمان را ثبت می‌کند.
- transportهای Android و Windows از همین eventهای ثابت استفاده می‌کنند، نه محتوای payload.
- اسکن log sinkها در آزمون Python، فراخوانی‌های رایج Console/Debug/Trace/ILogger/Android Log را در شاخه‌های اصلی source بررسی و ارجاع به fieldهای حساس را رد می‌کند.

## آزمون و شواهد

- Android `SafeDiagnosticLogTest.kt`: schema allowlist، duration نامعتبر، ثابت‌بودن کدها و نبود canaryهای مصنوعی prompt/capsule/secret را بررسی می‌کند.
- Windows `SafeDiagnosticLogAcceptanceTests.cs`: EventSource واقعی را فعال می‌کند و سه فیلد numeric را دریافت و نبود canary را کنترل می‌کند.
- `tests/test_network_payload_logging.py`: APIها و log sinkهای محصول را اسکن می‌کند.
- Product Build دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Quality Gate دقیق source [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) هر دو موفق شدند.

## تصمیم

وضعیت پایه `done` حفظ شد و `re_review_required` برای آزمون دوباره پس از تغییرات احتمالی UPD-S1-018 و تکمیل ممیزی end-to-end لاگ‌ها باقی ماند. این آزمون‌ها شواهد source/CI هستند، نه تحلیل forensic از کل فایل‌های log دستگاه فیزیکی.
