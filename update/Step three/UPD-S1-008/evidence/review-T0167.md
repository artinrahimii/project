# بازبینی T0167 — نمایش پاسخ‌ها بدون ادغام خودکار

- **مبنای بررسی روی main:** `744c5c4a8c794bd79f0073fb142b1679f4e965fe` — `V 3.1.90`
- **کد اجرایی بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ source مربوطه در commitهای مستنداتی بعدی تغییر نکرده است.
- **وضعیت پایهٔ legacy:** `done`؛ بدون تغییر
- **تصمیم:** `re_review_required`
- **وضعیت بازبینی:** `reviewed`؛ بازاعتبارسنجی بعد از سیاست نهایی UPD-S1-015 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## پیاده‌سازی مشاهده‌شده

- `HostResponseCards.kt` هر response را جداگانه پیمایش می‌کند و برای هر پاسخ یک Card مجزا با هویت request/attempt می‌سازد.
- متن خروجی، میزبان، مدل، outcome و اطلاعات attempt جدا نمایش داده می‌شوند؛ هیچ concatenation یا merge خودکار پاسخ‌ها در renderer وجود ندارد.
- `RequestViewModel.kt` از attempt ID به‌عنوان بخش هویت کارت استفاده می‌کند و تکرار یک attempt را رد می‌کند.

## آزمون و شواهد

- `RequestViewModelTest.kt`: دو attempt را به دو کارت جدا نگه می‌دارد و پاسخ تکراری را رد می‌کند.
- `tests/test_android_multi_host_response_rendering_contract.py`: renderer و جداسازی خروجی متن را کنترل می‌کند.
- Product Build دقیق کد بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Android build و unit-test job موفق شد.
- Quality Gate دقیق همان کد: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- آزمون محلی جداگانه در این محیط اجرا نشد.

## محدودیت و تصمیم

نمایش چند کارت در UI generic است؛ مسیر اجرایی first release در حال حاضر فقط یک میزبان فعال در هر round انتخاب می‌کند. بنابراین این کد به‌تنهایی اثبات دریافت چند پاسخ واقعی هم‌زمان در محصول نیست. status پایه `done` حفظ شد و `re_review_required` باقی ماند تا پس از UPD-S1-015 سیاست مجاز multi-host/compatibility و مصرف‌کنندهٔ واقعی نهایی شود.
