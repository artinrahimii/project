# بازبینی T0164 — زمان‌سنج deadline هر round

- **Snapshot بررسی‌شده:** `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd` — `V 3.1.88`، شاخهٔ آنلاین `main`
- **وضعیت پایه:** `done`؛ بدون تغییر
- **تصمیم reconciliation:** `re_review_required`
- **وضعیت این بازبینی:** `reviewed`؛ بازاعتبارسنجی پس از UPD-S1-015 لازم است
- **وظایف Update مرتبط:** UPD-S1-003، UPD-S1-004، UPD-S1-015، UPD-S1-017

## کد و رفتار بررسی‌شده

- `InferenceRoundTimer.kt` از `java.time.Clock` تزریق‌پذیر استفاده می‌کند و پنجرهٔ زمانی هر round را با شروع مستقل می‌سازد.
- deadline مؤثر، زودترِ timeout پیکربندی‌شده و deadline مطلق درخواست است؛ timeout باقی‌مانده را می‌توان با بودجهٔ پاسخ caller محدود کرد.
- timeout پیش‌فرض ۶۰ ثانیه است و مقدار غیرمثبت رد می‌شود.
- runtime، پنجرهٔ همان round را برای attemptهای آن round نگه می‌دارد و timeout انتظار پاسخ را به زمان باقی‌مانده محدود می‌کند.

## آزمون و شواهد

- فایل آزمون: `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundTimerTest.kt`
- آزمون‌ها پیشروی clock تزریق‌شده، محدودشدن به deadline مطلق، deadline منقضی‌شده، bounded timeout و رد timeout نامعتبر را پوشش می‌دهند.
- Product Build دقیق snapshot بررسی‌شده: [run 42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567)؛ Android JVM unit-test stage موفق بود.
- آزمون محلی جداگانه در این محیط اجرا نشد؛ نتیجهٔ CI دقیق همان commit به‌عنوان شاهد استفاده شده است.

## تصمیم و مرز باز

وضعیت پایهٔ `done` حفظ شد؛ decision برابر `re_review_required` است، زیرا UPD-S1-015 باید پس از تثبیت retry ترتیبی، deadline، لغو و بودجهٔ تلاش را در مسیر نهایی دوباره تأیید کند. هیچ ادعایی دربارهٔ timeout شبکهٔ عمومی یا آزمون فیزیکی مطرح نیست.
