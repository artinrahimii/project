# بازبینی T0169 — انتخاب میزبان‌های جدید پس از round بدون پاسخ معتبر

- **مبنای بررسی روی main:** `744c5c4a8c794bd79f0073fb142b1679f4e965fe` — `V 3.1.90`
- **کد اجرایی بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ source انتخاب/dispatch در commitهای reconciliation بعدی تغییر نکرده است.
- **وضعیت پایهٔ legacy:** `done`؛ بدون تغییر
- **تصمیم:** `re_review_required`
- **وضعیت بازبینی:** `reviewed`؛ اختلاف سیاست با first-release باید پس از UPD-S1-015 حل نهایی شود
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## پیاده‌سازی مشاهده‌شده

- `HostCandidateSelector.selectNextRound` فقط پس از تکمیل round قبلی و نبود پاسخ معتبر، میزبان‌های قبلاً امتحان‌شده را حذف می‌کند؛ در صورت وجود پاسخ معتبر round جدید را متوقف می‌کند.
- آزمون‌ها انتخاب سه میزبان متمایز بعدی، سقف دور/کل تلاش، عدم انتخاب دوبارهٔ میزبان قبلی، retry allowlist و fail-closed در نبود تاریخچهٔ کامل را بررسی می‌کنند.
- `InferenceRoundResponseCollector` نتیجهٔ پایان round را با پاسخ‌ها و failureهای مجزا می‌سازد.
- اما مسیر production first-release در `RequestViewModel` retry را خودکار انجام نمی‌دهد: retry فقط با اقدام صریح کاربر آغاز می‌شود و حداکثر یک میزبان در هر round انتخاب می‌شود.

## آزمون و شواهد

- `HostCandidateSelectorTest.kt`: آزمون انتخاب سه میزبان بعدی و محدودیت‌های round/attempt را دارد.
- `InferenceRoundResponseCollectorTest.kt`: پایان collection قبل از انتخاب بعدی و رفتار پاسخ‌های معتبر/نامعتبر را پوشش می‌دهد.
- Product Build دقیق کد بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد.
- Quality Gate دقیق همان کد: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- آزمون محلی جداگانه در این محیط اجرا نشد.

## مغایرت و تصمیم

عنوان legacy از «سه میزبان متفاوت بعدی» سخن می‌گوید؛ سیاست first release که در کد production فعلی اعمال می‌شود، سه تلاش ترتیبی تک‌میزبانه را اجرا می‌کند، نه batch سه‌میزبانه و نه retry خودکار. generic selector هنوز mode سه‌میزبانه را برای سازگاری حفظ کرده است. این اختلاف بدون تصمیم UPD-S1-015 نباید با تغییر پنهانی graph یا کد حل شود. وضعیت پایه `done` حفظ شد و این وظیفه برای بازاعتبارسنجی پس از تصویب/اجرای سیاست جدید باز می‌ماند.
