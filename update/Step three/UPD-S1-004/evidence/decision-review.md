# شواهد ممیزی UPD-S1-004

مبنای ممیزی کد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` (`V 3.1.38`).

## قرارداد و backend

- `protocol/whitep/v1/messages.proto`: یک `InferenceResponse` با بدنهٔ کامل و outcome نهایی؛ بدون پیام chunk/delta.
- `ModelCapability.supports_streaming` فقط metadata capability است.
- `LlamaCppServerBackend` مقدار `SupportsStreaming: false` را گزارش می‌کند و payload درخواستش stream flag ندارد.
- `LlamaCppInferenceEngine.GenerateAsync` یک `Task<string>` دارد.
- `HostPeerRuntimeCapabilitiesAcceptanceTests` عدم پشتیبانی backend فعال از streamed responses را صریحاً بررسی می‌کند.

## Android

- `Libp2pInferenceRequestTransport.awaitResponse` یک پاسخ pending را می‌گیرد و کلید همان request/attempt را در پایان پاک می‌کند.
- پاسخ با peer احرازشده، requestId، attemptId، roundId، model و deadline بررسی می‌شود.
- `AndroidInferenceRequestRuntime` پاسخ correlate‌شدهٔ هر attempt را جمع‌آوری می‌کند؛ `RequestViewModel` خروجی کامل را بعد از پایان این مسیر ثبت می‌کند.
- `cancelAttempts` پیام typed `CancelRequest` را برای requestId و attemptId همان تلاش ارسال می‌کند.

## آزمون و CI

- آزمون regression جدید `tests/test_inference_streaming_scope.py` به کنترل کیفیت اضافه شده است.
- [V 3.1.39 quality gate — run 42463139418](https://github.com/artinrahimii/project/actions/runs/42463139418): ۴۷۲ آزمون؛ ۴ شکست در test جدید و ADR registry. علت‌ها در گزارش اجرا ثبت شدند.
- در V 3.1.40 دو نوع خطای آزمون (escapeهای regex و شکل چندخطی CancelRequest builder) اصلاح و فایل ADR-0023 در `docs/decisions/README.md` ثبت شد.
- نتیجهٔ CI اصلاحیه هنوز pending است؛ هیچ موفقیتی پیش از سبزشدن CI اعلام نمی‌شود.
