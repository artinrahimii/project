# بازبینی T0162 — انتخاب میزبان سازگار

- **Snapshot بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`
- **وضعیت پایه:** `done`؛ بدون تغییر
- **تصمیم reconciliation:** `re_review_required`
- **وضعیت این بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و رفتار بررسی‌شده

- `HostCandidateSelector.kt` candidateها را بر اساس شناسهٔ موقت یکتا، وضعیت runtime، ظرفیت اعلام‌شده و live status، سازگاری مدل و سقف‌های درخواست بررسی و به‌شکل deterministic مرتب می‌کند.
- `selectReachable` برای candidateهای واجد شرایط، status probe احرازشده را پیش از انتخاب اجرا می‌کند؛ هویت مبهم، وضعیت stale/untrusted، میزبان busy یا مدل ناسازگار کنار گذاشته می‌شود.
- selector عمومی هنوز تا سه میزبان را برای مسیر compatibility می‌پذیرد؛ این API را نباید با سیاست محصول first release یکی دانست.
- مسیر تولیدی `AndroidInferenceRequestRuntime.selectReachableHostsWithFeedback` تعداد میزبان را به `MAX_ACTIVE_HOSTS_PER_REQUEST=1` محدود می‌کند؛ `discoverAndSelectReachableHosts` مقدار دیگری جز یک را رد می‌کند و dispatch نیز بیش از یک میزبان را نمی‌پذیرد.

## آزمون و شواهد

- فایل آزمون: `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`
- آزمون‌ها انتخاب deterministic تا سه candidate در API عمومی، رد میزبان starting/busy/unavailable/full، رد هویت/نسخهٔ ناسازگار و محدودیت مدل/ظرفیت را پوشش می‌دهند.
- Product Build دقیق snapshot بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ job Android با موفقیت lint، build، JVM unit tests و Keystore instrumentation را گذراند.
- آزمون محلی جداگانه در این محیط اجرا نشد؛ نتیجهٔ CI دقیق همان commit به‌عنوان شاهد استفاده شده است.

## تصمیم و مرز باز

وضعیت legacy برابر `done` باقی ماند. تصمیم `re_review_required` ثبت می‌شود تا پس از اجرای UPD-S1-015، سازگاری selector عمومی، سیاست یک میزبان در محصول، retry ترتیبی و جلوگیری از broadcast در مسیر نهایی دوباره آزموده شوند. هیچ کد یا edge در این بازبینی تغییر نکرد.
