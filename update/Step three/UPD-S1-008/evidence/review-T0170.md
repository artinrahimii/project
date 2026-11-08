# بازبینی T0170 — سقف دور و تلاش نسخهٔ نخست

- **مبنای بررسی روی main:** `744c5c4a8c794bd79f0073fb142b1679f4e965fe` — `V 3.1.90`
- **کد اجرایی بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`؛ source محصول در commitهای مستنداتی بعدی ثابت است.
- **وضعیت پایهٔ legacy:** `done`؛ بدون تغییر
- **تصمیم:** `re_review_required`
- **وضعیت بازبینی:** `reviewed`؛ بازاعتبارسنجی بعد از UPD-S1-015/017 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## پیاده‌سازی مشاهده‌شده

- `FirstReleaseInferenceLimits`: حداکثر سه round، یک میزبان فعال در هر round و سه attempt کل.
- `DistributedRequest.create` بودجهٔ نسخهٔ نخست را بر مبنای maxRounds می‌سازد و progress تاریخچهٔ ordered شناسه‌های host را ذخیره می‌کند.
- `RequestViewModel.executeRequestRound` در production سقف انتخاب میزبان را با MAX_ACTIVE_HOSTS_PER_REQUEST=1 محدود می‌کند، hostهای قبلی را exclude می‌کند و retry را فقط در پاسخ به action دستی کاربر انجام می‌دهد.
- `InferenceAttemptDispatcher` و `HostCandidateSelector` generic هنوز سقف سازگاری سه میزبان در هر round/نه attempt کل را پوشش می‌دهند؛ آن سقف عمومی با policy first-release یکسان نیست.

## آزمون و شواهد

- `DistributedRequestTest.kt`: three sequential single-host attempts را با سه round، یک host فعال و سه attempt کل می‌سنجد؛ مسیر generic compatibility نیز جداگانه تا ۹ attempt را آزمایش می‌کند.
- `HostCandidateSelectorTest.kt` و `InferenceAttemptDispatcherTest.kt`: انتخاب چند میزبان عمومی، exclusions، سقف‌ها و fail-closed رفتارهای compatibility را پوشش می‌دهند.
- Product Build دقیق کد بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق شد.
- Quality Gate دقیق همان کد: [run 42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق شد.
- اجرای تست محلی در این محیط ممکن نشد؛ CI دقیق کد بررسی‌شده استفاده شد.

## تصمیم و مرز باز

وضعیت پایهٔ `done` حفظ شد؛ `re_review_required` ثبت می‌شود تا UPD-S1-015/017 سقف‌ها، retry، timeout، حالت busy و تفاوت compatibility/first-release را در flow نهایی و آزمون‌های E2E تأیید کنند. هیچ کد یا edge قدیمی در این بازبینی تغییر داده نشد.
