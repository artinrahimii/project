# بازبینی T0161 — مدل دامنهٔ DistributedRequest

- **Snapshot بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`
- **وضعیت پایه:** `done`؛ بدون تغییر
- **تصمیم reconciliation:** `re_review_required`
- **وضعیت این بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015 همچنان لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و رفتار بررسی‌شده

- `DistributedRequest.kt`: شناسهٔ موقت درخواست، deadline مثبت، تعداد دور و وضعیت درخواست را نگه می‌دارد؛ شناسه‌ها با UUID ساخته می‌شوند.
- `DistributedRequestProgress`: تعداد دورها، تلاش‌ها و تاریخچهٔ مرتب‌شدهٔ شناسه‌های میزبان را ثبت می‌کند؛ شناسهٔ خالی و تکرار در یک dispatch را رد می‌کند.
- سیاست first release در `FirstReleaseInferenceLimits` برابر سه دور، یک میزبان هم‌زمان و حداکثر سه تلاش کل است. محدودیت عمومی سه میزبان در `MultiHostCompatibilityLimits` صرفاً برای سازگاری generic باقی مانده است.
- runtime تولیدی Android در `AndroidInferenceRequestRuntime.kt` سقف یک میزبان را در مسیر انتخاب و dispatch اعمال می‌کند؛ بنابراین API سازگاری سه‌میزبانه به‌تنهایی سیاست first release نیست.

## آزمون و شواهد

- فایل آزمون: `src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt`
- آزمون‌های موجود، شناسهٔ UUID متمایز، deadline و round limit، ثبت تاریخچهٔ تلاش‌ها، رد شناسهٔ خالی و سقف first release را پوشش می‌دهند.
- Product Build دقیق snapshot بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ job Android با موفقیت lint، build، JVM unit tests و Keystore instrumentation را گذراند.
- آزمون محلی جداگانه در این محیط اجرا نشد؛ نتیجهٔ CI دقیق همان commit به‌عنوان شاهد استفاده شده است.

## تصمیم و مرز باز

وضعیت قدیمی `done` حفظ شد و هیچ کد یا وابستگی تغییر نکرد. تصمیم `re_review_required` باقی می‌ماند، زیرا UPD-S1-015 هنوز سیاست نهایی تخصیص میزبان/retry را در مسیر محصول پیاده‌سازی و دوباره اعتبارسنجی نکرده است. این بازبینی، اثبات E2E فیزیکی یا پاسخ مدل واقعی نیست.
