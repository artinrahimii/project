# گزارش اجرای UPD-S1-004 — تعیین محدودهٔ Streaming

- مبنای کد و قرارداد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` — `V 3.1.38`
- پیش‌نیازها: UPD-S1-001 و UPD-S1-002 در گراف جاری `done` هستند.
- تصمیم نسخهٔ نخست: **پاسخ کامل و یک‌باره؛ Streaming به نسخهٔ بعد موکول شد.**
- وضعیت این تلاش: `in_progress` تا زمان موفقیت آزمون regression در CI همان commit اجرایی.

## ممیزی قرارداد و پیاده‌سازی

- `protocol/whitep/v1/messages.proto` یک `InferenceResponse` با `output_text` کامل، outcome نهایی، زمان تکمیل و شناسه‌های request/attempt/round تعریف می‌کند؛ پیام chunk/delta یا شمارهٔ ترتیب chunk وجود ندارد.
- فیلد `ModelCapability.supports_streaming` صرفاً metadata قابلیت است و به‌تنهایی قرارداد انتقال chunk را تعریف نمی‌کند.
- `LlamaCppServerBackend` قابلیت Streaming را `false` اعلام می‌کند؛ payload درخواست OpenAI-compatible شامل model/messages/max_tokens است و گزینهٔ stream ندارد.
- `LlamaCppInferenceEngine.GenerateAsync` یک `Task<string>` کامل برمی‌گرداند و cancellation را به generation فعال منتقل می‌کند.
- انتقال Android یک پاسخ را برای هر request/attempt منتظر می‌ماند و آن را با peer احرازشده، round، model و deadline تطبیق می‌دهد. ViewModel پس از جمع‌آوری پاسخ‌های terminal متن کامل را به UI می‌رساند.
- `CancelRequest` همچنان به request و attempt دقیق وابسته است؛ موفقیت، خطا، timeout، لغو و ردشدن در قرارداد outcomeهای جدا دارند.

## تصمیم و اثر بر UI

نسخهٔ نخست متن تولیدشده را فقط پس از دریافت پاسخ موفق و correlate‌شده به گفت‌وگو اضافه می‌کند. وضعیت‌های در حال کشف، اتصال، پردازش، لغو، timeout و خطا مستقل می‌مانند. متن ناقص نباید به شکل پاسخ موفق نمایش داده شود. هیچ تغییر غیرضروری در schema پروتکل یا رفتار سالم فعلی لازم نبود؛ قرارداد موجود حفظ شد.

Streaming برای نسخهٔ بعد deferred است. پیش از فعال‌سازی آتی باید پروتکل chunk با ترتیب، پایان صریح، خطای terminal، backpressure، محدودیت اندازه، timeout، cancellation، disconnect، retry و تطبیق یکسان Android/Windows تصویب و آزموده شود.

## خروجی‌های این تلاش

- `docs/decisions/ADR-0023-one-shot-inference-v1.md`: تصمیم معماری و معیارهای deferred streaming.
- `tests/test_inference_streaming_scope.py`: آزمون regression برای قرارداد یک‌باره، غیرفعال‌بودن Streaming در backend فعلی، انتظار پاسخ correlate‌شده در Android و حفظ cancellation.
- `update/Step three/UPD-S1-004/evidence/decision-review.md`: شواهد ممیزی کد/قرارداد و مرز پذیرش.
- هیچ فیلد یا پیام Protobuf تغییر نکرده است؛ فایل canonical موجود فقط ممیزی و حفظ شده است.

## اعتبارسنجی

- آزمون‌های موجود در مبنای `V 3.1.38`، از جمله Python quality، Protobuf contract، Android transport contract، Android unit tests و HostLifecycle tests در CI موفق بوده‌اند.
- آزمون تازهٔ `test_inference_streaming_scope.py` در commit این تلاش افزوده شده و تا زمان اجرای quality gate همان commit، نتیجهٔ آن **pending** است؛ بنابراین این وظیفه هنوز `done` نیست.
- آزمون Android-to-Windows روی گوشی فیزیکی و Windows میزبان فیزیکی، آزمون محصول UPD-S1-020 و آزمون ارتقای درجا در این تلاش اجرا نشده‌اند.
