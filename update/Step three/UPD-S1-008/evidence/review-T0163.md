# بازبینی T0163 — شناسهٔ تلاش و dispatch قابل‌تطبیق

- **Snapshot بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`
- **وضعیت پایه:** `done`؛ بدون تغییر
- **تصمیم reconciliation:** `re_review_required`
- **وضعیت این بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و رفتار بررسی‌شده

- `InferenceAttemptDispatcher.kt` برای هر submission شناسهٔ attempt مستقل می‌سازد؛ پیش از اولین ارسال، payloadهای همهٔ candidateهای انتخاب‌شده را اعتبارسنجی می‌کند تا رد محلی باعث ارسال نیمه‌کارهٔ یک round نشود.
- شناسهٔ request در round مشترک است و هر attempt شناسهٔ مستقل دارد. پیشرفت درخواست تاریخچهٔ شناسه‌های موقت میزبان را نگه می‌دارد.
- `Libp2pInferenceRequestTransport.kt` attemptهای منتظر را با زوج requestId/attemptId نگه می‌دارد، peer احرازشده را با peer انتخابی تطبیق می‌دهد و correlation پاسخ را پیش از پذیرش بررسی می‌کند.
- محدودیت یک میزبان در مسیر first-release runtime بالادست dispatcher اعمال می‌شود؛ وجود dispatcher generic به‌تنهایی مجوز ارسال هم‌زمان به چند میزبان نیست.

## آزمون و شواهد

- آزمون اصلی: `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`
- آزمون‌های transport: `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`
- Product Build دقیق snapshot بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Android JVM unit-test stage و سایر مراحل Android موفق شدند.
- کنترل‌های Windows HostLifecycle در همان Product Build، run 42510357567، موفق شدند. این شاهد، آزمون دستگاه Android فیزیکی یا تحویل inference زنده از میزبان واقعی نیست.
- آزمون محلی جداگانه در این محیط اجرا نشد؛ نتیجهٔ CI دقیق همان commit به‌عنوان شاهد استفاده شده است.

## تصمیم و مرز باز

وضعیت legacy `done` و تاریخچهٔ قبلی حفظ شد. تصمیم `re_review_required` باقی است تا UPD-S1-015، UPD-S1-017 و آزمون‌های انتهابه‌انتهای UPD-S1-019/020 شناسه‌ها، retry، timeout، پاسخ دیرهنگام و dispatch نهایی را با سیاست first release دوباره تأیید کنند. هیچ ادعای تحویل موفق پاسخ واقعی به Android مطرح نمی‌شود.
