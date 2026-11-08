# شواهد ممیزی UPD-S1-004

مبنای ممیزی کد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` (`V 3.1.38`).

## قرارداد canonical

- `protocol/whitep/v1/messages.proto`: `InferenceResponse` یک بدنهٔ کامل با `output_text` و outcome نهایی دارد. هیچ پیام chunk/delta/stream response در schema فعلی تعریف نشده است.
- `ModelCapability.supports_streaming` یک بیت capability است؛ ترتیب، framing، terminal chunk، backpressure و انتقال جزئی پاسخ را تعریف نمی‌کند.

## Windows

- `src/Host.Infrastructure/LlamaCppServerBackend.cs`: snapshot مدل فعال `SupportsStreaming: false` برمی‌گرداند.
- ساختار `ChatCompletionRequest` فقط model، messages و max_tokens دارد؛ گزینهٔ stream ارسال نمی‌کند.
- `src/Host.Windows/Inference/LlamaCppInferenceEngine.cs`: API تولید، `Task<string>` کامل دارد و cancellation را برای generation فعال اعمال می‌کند.
- `tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs`: صریحاً عدم پشتیبانی backend فعلی از streamed responses را assert می‌کند.

## Android

- `Libp2pInferenceRequestTransport.awaitResponse` یک پاسخ pending را دریافت می‌کند، سپس entry همان request/attempt را پاک می‌کند.
- پاسخ با PeerId احرازشده، requestId، attemptId، roundId، model و deadline بررسی می‌شود.
- `AndroidInferenceRequestRuntime` برای هر attempt همین پاسخ correlate‌شده را می‌گیرد؛ `RequestViewModel` پس از جمع‌آوری نتایج terminal، output کامل را به گفت‌وگو متصل می‌کند.
- `cancelAttempts` پیام typed `CancelRequest` را با requestId و attemptId دقیق می‌فرستد.

## آزمون‌ها

- منابع موجود: `tests/test_proto_contract.py`، `tests/test_android_libp2p_transport_contract.py` و `tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs`.
- آزمون regression جدید: `tests/test_inference_streaming_scope.py`.
- فرمان CI برای آزمون جدید: `python -m unittest discover -s tests -p test_inference_streaming_scope.py -v`.
- کیفیت CI روی مبنای قبلی موفق بوده است؛ نتیجهٔ اجرای آزمون تازه روی commit این تلاش هنوز pending است.

## مرز شواهد

این تصمیم به معنی موفقیت آزمون واقعی Android-to-Windows نیست. تست transport/CI موجود به‌تنهایی جای گوشی فیزیکی و میزبان Windows هدف را نمی‌گیرد. هیچ ادعایی دربارهٔ Streaming زنده، محصول end-to-end فیزیکی یا clean/in-place upgrade ارائه نمی‌شود.
