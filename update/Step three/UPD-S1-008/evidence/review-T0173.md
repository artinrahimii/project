# بازبینی T0173 — idempotency پاسخ تکراری

- مبنای main: `9e67a8fa942495f7e1702116747bccfe5d4c6d99` — `V 3.1.92`
- snapshot کد محصول بررسی‌شده: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد
- تصمیم reconciliation: `re_review_required`
- وضعیت بازبینی: `reviewed`
- Updateهای مرتبط: UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد بررسی‌شده

- `RequestViewModel.recordValidatedResponse` با lock، زوج `(requestId, attemptId)` را قبل از افزودن کارت کنترل می‌کند؛ response تکراری برای همان attempt رد می‌شود و محتوای کارت اول جایگزین نمی‌شود.
- retry از همان peer attemptId تازه دارد و از کارت تلاش قبلی متمایز می‌ماند.
- `Libp2pInferenceRequestTransport` pending response را با زوج request/attempt نگه می‌دارد و پس از پایان انتظار آن را حذف می‌کند؛ سرور Windows نیز replay guard محدودشده بر peer/request/attempt دارد.

## آزمون و شواهد

- `RequestViewModelTest.kt`: پاسخ تکراری با متن متفاوت را رد می‌کند و کارت اول را حفظ می‌کند؛ پاسخ stale به request جدید هم رد می‌شود.
- `tests/test_android_host_response_cards_contract.py`: هویت کارت، جلوگیری از تکرار و renderer را کنترل می‌کند.
- Product CI دقیق source: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد؛ Quality CI [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق بود.
- آزمون محلی جداگانه اجرا نشد.

## محدودیت و تصمیم

deduplication کارت‌ها در state موقت ViewModel و در بازهٔ عمر آن اعمال می‌شود؛ این بازبینی تضمین persistence/idempotency پاسخ پس از process restart در همان Room conversation را ثابت نمی‌کند. وضعیت پایه `done` حفظ شد و `re_review_required` تا اتصال نهایی پاسخ و persistence در UPD-S1-010/019/020 باقی می‌ماند.
