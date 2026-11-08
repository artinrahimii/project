# گزارش اجرای UPD-S1-004 — تعیین محدودهٔ Streaming

- مبنای کد و قرارداد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` — `V 3.1.38`
- commit تصمیم و regression: `71ddc79726691c4c437ea4d8f97ca9866618eb9c` — `V 3.1.39`
- تصمیم نسخهٔ نخست: **پاسخ کامل و یک‌باره؛ Streaming به نسخهٔ بعد موکول شد.**
- وضعیت: `in_progress` تا موفقیت quality gate روی commit اصلاحی V 3.1.40.

## ممیزی قرارداد و پیاده‌سازی

- `protocol/whitep/v1/messages.proto` یک `InferenceResponse` با `output_text` کامل، outcome نهایی، زمان تکمیل و شناسه‌های request/attempt/round تعریف می‌کند؛ پیام chunk/delta یا شمارهٔ ترتیب chunk وجود ندارد.
- `ModelCapability.supports_streaming` صرفاً metadata قابلیت است و قرارداد انتقال chunk را تعریف نمی‌کند.
- `LlamaCppServerBackend` قابلیت Streaming را `false` اعلام می‌کند؛ payload درخواست شامل model/messages/max_tokens است و گزینهٔ stream ندارد.
- `LlamaCppInferenceEngine.GenerateAsync` یک `Task<string>` کامل برمی‌گرداند و cancellation را به generation فعال منتقل می‌کند.
- انتقال Android یک پاسخ را برای هر request/attempt منتظر می‌ماند و آن را با peer احرازشده، round، model و deadline تطبیق می‌دهد. ViewModel پس از جمع‌آوری پاسخ‌های terminal متن کامل را به UI می‌رساند.
- `CancelRequest` به request و attempt دقیق وابسته است؛ موفقیت، خطا، timeout، لغو و ردشدن outcomeهای جدا دارند.

## تصمیم و اثر بر UI

نسخهٔ نخست متن تولیدشده را فقط پس از دریافت پاسخ موفق و correlate‌شده به گفت‌وگو اضافه می‌کند. وضعیت‌های در حال کشف، اتصال، پردازش، لغو، timeout و خطا مستقل می‌مانند. متن ناقص نباید به شکل پاسخ موفق نمایش داده شود. هیچ تغییر غیرضروری در schema پروتکل لازم نبود؛ قرارداد موجود حفظ شد.

Streaming برای نسخهٔ بعد deferred است. پیاده‌سازی آتی باید قرارداد chunk با ترتیب، پایان صریح، خطای terminal، backpressure، محدودیت اندازه، timeout، cancellation، disconnect، retry و تطبیق یکسان Android/Windows داشته باشد.

## خروجی‌ها

- `docs/decisions/ADR-0023-one-shot-inference-v1.md`: تصمیم معماری و معیارهای deferred streaming.
- `docs/decisions/README.md`: ثبت ADR-0023 همراه با مسیرهای source/test/workflow.
- `tests/test_inference_streaming_scope.py`: regression test برای schema یک‌باره، backend غیرفعال، پاسخ correlate‌شده در Android و cancellation.
- `update/Step three/UPD-S1-004/evidence/decision-review.md`: ممیزی و مرز پذیرش.

## اعتبارسنجی و اصلاح

- CI روی `V 3.1.39` (run [42463139418](https://github.com/artinrahimii/project/actions/runs/42463139418)) تعداد ۴۷۲ آزمون Python را اجرا کرد؛ چهار آزمون شکست خوردند. سه مورد ناشی از regex بیش‌ازحد escape‌شده/فرض نادرست دربارهٔ چندخطی بودن builder در آزمون جدید، و یک مورد ناشی از ثبت‌نشدن ADR در README بود. این شکست‌ها صریحاً حفظ شده و در این تلاش اصلاح شده‌اند.
- در `V 3.1.40` regexها و assertion builder اصلاح و ADR در register ثبت شده است؛ نتیجهٔ quality gate همان commit هنوز pending است. تا دریافت نتیجه، UPD-S1-004 `done` نیست.
- آزمون Android-to-Windows روی گوشی فیزیکی و میزبان Windows فیزیکی، محصول E2E و آزمون ارتقای درجا در این وظیفه اجرا نشده‌اند.
