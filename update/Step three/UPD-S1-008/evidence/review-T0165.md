# بازبینی T0165 — تطبیق پاسخ با request/attempt/round/host

- **Snapshot بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`
- **وضعیت پایه:** `done`؛ بدون تغییر
- **تصمیم reconciliation:** `re_review_required`
- **وضعیت این بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و رفتار بررسی‌شده

- قرارداد `protocol/whitep/v1/messages.proto` شناسه‌های request، attempt و round را در پیام درخواست و پاسخ حمل می‌کند؛ پاسخ همچنین modelId، host identity و زمان پایان را نگه می‌دارد.
- `InferenceResponseCorrelationValidator.kt` در Android تطبیق دقیق request/attempt/round/host/model و نیز deadline و زمان دریافت محلی را کنترل می‌کند.
- `Libp2pInferenceRequestTransport.kt` پیش از پذیرش پاسخ، peer احرازشده را با host ذخیره‌شده در attempt تطبیق می‌دهد و validator را اجرا می‌کند.
- `PeerResponseCorrelationValidator.cs` در Windows request/attempt/round/model، هویت میزبان و deadline را پیش از ارسال پاسخ بررسی می‌کند؛ `PeerAnnouncementHostLifecycle.cs` این validator را برای پاسخ‌های production، Busy و rejection فراخوانی می‌کند.

## آزمون و شواهد

- Android: `src/Client.Android/app/src/test/java/com/whitep/client/network/InferenceResponseCorrelationValidatorTest.kt`
- Windows: `tests/HostLifecycle.Tests/PeerMessageSecurityAcceptanceTests.cs` و مجموعهٔ HostLifecycle در CI
- Product Build دقیق snapshot بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Android JVM/Keystore instrumentation و Windows HostLifecycle acceptance jobs موفق شدند.
- آزمون محلی جداگانه در این محیط اجرا نشد؛ نتیجهٔ CI دقیق همان commit به‌عنوان شاهد استفاده شده است.

## تصمیم و مرز باز

وضعیت legacy `done` حفظ شد و هیچ کد یا dependency تغییر نکرد. تصمیم `re_review_required` باقی می‌ماند تا تغییرات بعدی UPD-S1-015/017 و آزمون‌های واقعی UPD-S1-019/020 دوباره correlation، پاسخ دیرهنگام، retry و وضعیت‌های terminal را اعتبارسنجی کنند. این بررسی جایگزین آزمون Android فیزیکی به میزبان Windows و ذخیرهٔ پاسخ در Room نیست.
