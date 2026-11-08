# بازبینی T0166 — پاسخ مستقل هر میزبان و کارت UI

- **مبنای بررسی روی main:** `744c5c4a8c794bd79f0073fb142b1679f4e965fe` — `V 3.1.90`
- **کد اجرایی بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ تغییرات بعدی تا این مبنا فقط reconciliation، گزارش و گراف بوده و source محصول این فایل‌ها را تغییر نداده است.
- **وضعیت پایهٔ legacy:** `done`؛ بدون تغییر
- **تصمیم:** `re_review_required`
- **وضعیت بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015/017 و اتصال نهایی Chat لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## پیاده‌سازی مشاهده‌شده

- `RequestViewModel.kt` یک `HostResponseCardState` با requestId، attemptId، hostId، modelId، outcome، متن خروجی و خطا نگه می‌دارد.
- `recordValidatedResponse` تطبیق request/attempt/host/model، شناسهٔ round، timestamp، outcome و سازگاری error code را بررسی می‌کند؛ سپس با قفل و کلید یکتای `(requestId, attemptId)` کارت را اضافه می‌کند و پاسخ تکراری را نمی‌پذیرد.
- `HostResponseCards.kt` برای هر عنصر یک Compose Card مجزا، با key برابر requestId و attemptId، تولید می‌کند.
- `MainActivity.kt` renderer را واقعاً در صفحهٔ «درخواست جدید» فراخوانی می‌کند.

## آزمون و شواهد

- `RequestViewModelTest.kt`: کارت مستقل برای دو attempt، رد پاسخ تکراری، رد tuple نامعتبر و پاک‌کردن کارت‌های موقت را پوشش می‌دهد.
- `tests/test_android_host_response_cards_contract.py`: state، renderer، مصرف MainActivity و آزمون ViewModel را کنترل می‌کند.
- Product Build دقیق کد بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ job Android شامل lint، build، JVM tests و Keystore instrumentation موفق شد.
- Quality Gate دقیق همان کد: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- آزمون محلی مستقل در این محیط اجرا نشد؛ شواهد فوق از CI دقیق کد بررسی‌شده است.

## محدودیت و تصمیم

کارت‌ها در `MutableStateFlow` و state موقت ViewModel نگهداری می‌شوند و صفحهٔ نمایش آن‌ها صفحهٔ درخواست جدید است. این پیاده‌سازی به‌تنهایی ذخیرهٔ پاسخ در پیام همان گفت‌وگو یا Room را ثابت نمی‌کند. وضعیت پایهٔ legacy حفظ شد؛ تصمیم `re_review_required` باقی است تا پس از UPD-S1-010، پاسخ به همان conversation/message وصل و persistence واقعی آزموده شود. این بازبینی، Android-to-Windows فیزیکی یا live model inference نیست.
