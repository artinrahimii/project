# بازبینی T0198 — انتخاب میزبان، round، timeout، deadline و idempotency

- مبنای main: `473c5964e49e82a17c963304cc82fbd4f6e23a70`، `V 3.1.102`
- اجرای تاریخی تست‌های این وظیفه: `a074815979aff371650c5101c514989f42180968`، `V 3.0.922`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- UPDهای مرتبط: UPD-S1-019

## کد و تست فعلی بررسی‌شده

- `HostCandidateSelectorTest.kt`: انتخاب deterministic حداکثر سه میزبان سازگار، وضعیت ready، مدل، ظرفیت، protocol، exclusion میزبان‌های قبلی، allowlist retry و سقف budget.
- `InferenceRoundTimerTest.kt`: clock تزریق‌پذیر، deadline مطلق، timeout محدودشده و رد deadline منقضی.
- `InferenceAttemptDispatcherTest.kt`: attempt برای هر میزبان منتخب، duplicate host/attempt، round limit، سقف سه میزبان و نه attempt در compatibility و جلوگیری از dispatch پس از expiry.
- `DistributedRequestTest.kt`: شناسهٔ مستقل request، progress مرتب و محدودیت first-release سه تلاش متوالی تک‌میزبانه.
- `InferenceRoundResponseCollectorTest.kt`: zero/one/three response، timeout، response دیرهنگام و cancellation caller.
- `RequestViewModelTest.kt`: duplicate response رد می‌شود و retry دستی requestId را حفظ و attemptId تازه می‌سازد.

## شواهد اجرا

- اجرای Android JVM مربوط به source تاریخی [Product Build run 41886746018](https://github.com/artinrahimii/project/actions/runs/41886746018)، job 126773137271، موفق ثبت شده است.
- گزارش run می‌گوید Android JVM unit tests موفق بوده؛ اما Product Build روی main جاری در زمان بازبینی هنوز در حال اجرا بود و اجرای جدید را به‌عنوان pass گزارش نمی‌کنم.
- اجرای محلی در این محیط انجام نشد؛ امکان clone مستقیم GitHub به‌علت DNS وجود نداشت.

## مغایرت و تصمیم

کد فعلی first-release runtime یک میزبان در هر round و حداکثر سه تلاش ترتیبی را اجرا می‌کند، درحالی‌که selector/dispatcher generic سقف سه میزبان در round و نه تلاش compatibility را حفظ می‌کنند. تست collector شامل صفر، یک و سه پاسخ است؛ تست مستقل دقیقاً دو پاسخ معتبر در فهرست فعلی پیدا نشد. وضعیت قدیمی `done` حفظ شد؛ به‌دلیل تغییر policy و نیاز به تست دو پاسخ و بازاعتبارسنجی UPD-S1-019، تصمیم `re_review_required` است.
