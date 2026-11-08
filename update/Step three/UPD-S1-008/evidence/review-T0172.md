# بازبینی T0172 — رد پاسخ دیرهنگام و عدم انتساب به درخواست جدید

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` — `V 3.1.92`
- snapshot کد محصول بررسی‌شده: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد بررسی‌شده

- `InferenceResponseCorrelationValidator.matches` پاسخ را با requestId، attemptId، roundId، شناسهٔ میزبان، modelId و deadline تطبیق می‌دهد.
- هم timestamp تکمیل اعلام‌شده توسط میزبان و هم زمان دریافت محلی باید مثبت و حداکثر برابر deadline مطلق درخواست باشند.
- `Libp2pInferenceRequestTransport.awaitResponse` پاسخ را از pending attempt مشخص با زوج requestId/attemptId می‌گیرد و در نهایت همان ورودی pending را از map حذف می‌کند؛ هویت peer احراز‌شده و فیلدهای correlation در مسیر دریافت نیز بررسی می‌شوند.

## آزمون و شواهد

- `InferenceResponseCorrelationValidatorTest.kt`: پاسخ همان request/attempt/round/host/model را می‌پذیرد؛ پاسخ درخواست قبلی، round دیگر، completion بعد deadline و receive بعد deadline را رد می‌کند.
- `RequestViewModelTest.kt`: پاسخ درخواست قبلی پس از شروع request جدید را رد می‌کند.
- `tests/test_android_inference_response_correlation_contract.py`: قرارداد round/request/attempt و آزمون‌های مرتبط را کنترل می‌کند.
- Product CI دقیق کد: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Quality CI [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.
- اجرای محلی در این محیط انجام نشد.

## تصمیم

وضعیت پایهٔ `done` تغییر نکرد. به‌دلیل تغییرات احتمالی retry و dispatch در UPD-S1-015 و نیاز به آزمون E2E، تصمیم `re_review_required` باقی می‌ماند. هیچ پاسخ دیرهنگام در این بازبینی به‌عنوان یک پاسخ واقعی از Windows فیزیکی آزمایش نشده است.
