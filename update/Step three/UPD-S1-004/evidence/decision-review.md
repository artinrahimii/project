# شواهد ممیزی و تکمیل UPD-S1-004

مبنای ممیزی کد: `2cc12c32cfc2663316c93064543e6ce62caaa5cb` (`V 3.1.38`). CI دقیق پذیرش: `380c89043d7e2cfce7bc50926419a1a682aaf0cb` (`V 3.1.40`).

## قرارداد canonical

- `protocol/whitep/v1/messages.proto`: `InferenceResponse` یک payload کامل `output_text` و outcome نهایی دارد؛ هیچ پیام chunk/delta/stream response یا شمارهٔ ترتیب قطعه تعریف نشده است.
- `ModelCapability.supports_streaming` یک metadata flag است، نه قرارداد wire برای streamed response.

## Windows

- `src/Host.Infrastructure/LlamaCppServerBackend.cs`: backend فعال `SupportsStreaming: false` را اعلام می‌کند و ChatCompletionRequest فاقد stream flag است.
- `src/Host.Windows/Inference/LlamaCppInferenceEngine.cs`: متد تولید یک `Task<string>` کامل دارد و cancellation به inference فعال منتقل می‌شود.
- `tests/HostLifecycle.Tests/HostPeerRuntimeCapabilitiesAcceptanceTests.cs`: عدم پشتیبانی backend از streamed responses را assert می‌کند.

## Android

- `Libp2pInferenceRequestTransport.awaitResponse` یک پاسخ pending را برای زوج request/attempt دریافت می‌کند و correlation peer، round، model و deadline را اعتبارسنجی می‌کند.
- `AndroidInferenceRequestRuntime` پاسخ terminal correlate‌شده را دریافت می‌کند؛ `RequestViewModel` تنها پس از تکمیل مسیر، output متن کامل را به UI می‌رساند.
- `cancelAttempts` پیام `CancelRequest` را با requestId و attemptId دقیق می‌فرستد.

## نتیجهٔ regression

چهار آزمون زیر در quality gate دقیق V 3.1.40 همگی PASS شدند:

- `test_wire_contract_is_one_terminal_response_not_a_chunk_protocol`
- `test_active_windows_backend_does_not_advertise_or_request_streaming`
- `test_android_waits_for_one_correlated_terminal_response_before_ui_update`
- `test_cancellation_remains_typed_and_bound_to_the_exact_attempt`

## نتایج کلی پذیرش

- [Quality gate 42463288660](https://github.com/artinrahimii/project/actions/runs/42463288660): ۴۷۲ آزمون Python موفق.
- [Product build 42463288704](https://github.com/artinrahimii/project/actions/runs/42463288704): Android، Windows و HostLifecycle موفق.
- [Rendezvous 42463288598](https://github.com/artinrahimii/project/actions/runs/42463288598): build/smoke موفق.
- Android: ۲۲۴ passed، صفر failed، یک skipped.
- HostLifecycle: ۶۷ critical pass markers، یک skip و ۸۵٫۷۶٪ line coverage.

## محدودیت پذیرش

CI و emulator جایگزین گوشی فیزیکی و میزبان Windows هدف نیستند. Android-to-Windows واقعی، clean install روی دستگاه هدف و in-place upgrade در این وظیفه اجرا نشده‌اند.
