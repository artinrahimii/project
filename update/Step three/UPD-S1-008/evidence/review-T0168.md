# بازبینی T0168 — جمع‌آوری پاسخ تا پایان پنجرهٔ مشترک round

- **مبنای بررسی روی main:** `744c5c4a8c794bd79f0073fb142b1679f4e965fe` — `V 3.1.90`
- **کد اجرایی بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ source محصول این مسیر در commitهای گراف/گزارش بعدی ثابت است.
- **وضعیت پایهٔ legacy:** `done`؛ بدون تغییر
- **تصمیم:** `re_review_required`
- **وضعیت بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015/017 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## پیاده‌سازی مشاهده‌شده

- `InferenceRoundResponseCollector.collect` شناسهٔ host و attempt را بررسی می‌کند و uniqueness attemptها را می‌سنجد.
- برای هر attempt صف‌شده، انتظار async جداگانه با زمان باقی‌مانده از همان `InferenceRoundTimer.Window` اجرا می‌کند؛ `awaitAll` تا پایان همهٔ انتظارها ادامه می‌یابد.
- timeout، submission failure/rejection، نبود پنجره و خطای پاسخ برای هر attempt جداگانه ثبت می‌شود؛ پاسخ معتبر یک attempt موجب لغو انتظار sibling نمی‌شود.
- cancellation واقعی caller از timeout تفکیک شده و exceptionها به failureهای typed تبدیل می‌شوند.
- runtime همان پنجرهٔ round را با زوج requestId/attemptId نگه می‌دارد و به collector می‌دهد.

## آزمون و شواهد

- `InferenceRoundResponseCollectorTest.kt`: دریافت پاسخ دوم بعد از اولین پاسخ، timeout یک attempt بدون لغو پاسخ دیگر، صفر/یک/سه پاسخ، حفظ ترتیب dispatch، deadline منقضی و cancellation caller را پوشش می‌دهد.
- `tests/test_android_inference_round_response_collector_contract.py`: استفاده از awaitAll، round window مشترک و آزمون‌های کلیدی را کنترل می‌کند.
- Product Build دقیق کد بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Android JVM job موفق شد.
- Quality Gate دقیق همان کد: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- آزمون محلی مستقل در این محیط انجام نشد.

## محدودیت و تصمیم

collector قابلیت generic چند attempt را دارد؛ اما runtime و ViewModel نسخهٔ نخست در هر round تنها یک میزبان dispatch می‌کنند. بنابراین تست collector با چند peer معادل multi-host production E2E نیست. وضعیت پایهٔ `done` حفظ شد و تصمیم `re_review_required` به دلیل تغییر سیاست در UPD-S1-015/017 و ضرورت E2E واقعی باقی است.
