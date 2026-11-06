# T0190 — شواهد گزارش وضعیت بدون محتوای پیام

## Snapshot و اعتبارسنجی مبنا
- Branch: `main`.
- Base commit: `bcec43249466d88015c2807701f9ec873b7caa27` (`V 3.0.890`).
- Graph path: `step three/project-graph.md`.
- Graph Blob SHA: `5e228afd442578fdbe4fea79e5c56cda62db219f`; اندازهٔ ثبت‌شده در GitHub برابر ۳٬۴۹۰٬۳۶۸ بایت بود و متن کامل JSON با موفقیت parse شد.
- Node/edge counts: ۲۳۰ / ۴۲۲.
- T0188 و T0189 در این snapshot `done` هستند.
- ورودی‌های T0190 هر دو `blocks` اصلی خود را حفظ می‌کنند و به `sequencing_only / validated` با rationale/evidence ثبت‌شده تغییر طبقه‌بندی داده‌اند.

## شواهد مسئلهٔ موجود در کد
- `src/Host.Application/HostStatusPresentation.cs`: نسخهٔ قبلی `FormatBackendStatus` مقدار `status.Message` را مستقیماً به خروجی UI الحاق می‌کرد.
- `src/Host.Windows/MainWindow.xaml.cs`: مسیر `BackendCapabilityStatus` برای `HostState.Error` متن `status.Message` را نمایش می‌داد.
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`: نسخهٔ قبلی `networkStatusMessage` و `hostAvailabilityMessage` را مستقیم در ناحیهٔ وضعیت رندر می‌کرد.

## پیاده‌سازی و آزمون‌های تغییر یافته
- Windows presenter اکنون فقط از `HostState` و متن ثابت استفاده می‌کند؛ `HostStatus.Message` در status presenter خوانده نمی‌شود.
- Windows capability error copy اکنون ثابت است و جزئیات آزاد backend را نمایش نمی‌دهد.
- Android presenter جدید `toNetworkInferenceStatusPresentation()` فقط وضعیت شبکه/peer و چرخهٔ inference را به برچسب‌های ثابت تبدیل می‌کند.
- Android UI دیگر `networkStatusMessage` و `hostAvailabilityMessage` را در status panel رندر نمی‌کند.
- آزمون Android یک prompt/status-message sentinel را وارد می‌کند و assert می‌کند که هیچ‌کدام از دو خروجی status آن را بازتاب نمی‌دهند.
- آزمون Windows همان sentinel را در `HostStatus.Message` برای همهٔ stateها قرار می‌دهد و عدم حضور آن در backend/network presentation را assert می‌کند.
- Python source contracts تضمین می‌کنند مسیرهای UI به نمایش متن آزاد بازنگشته‌اند.

## وضعیت آزمون
- آزمون محلی در این محیط اجرا نشده است.
- نتیجهٔ KtLint، Android JVM، Windows HostLifecycle، WPF build و Step Three quality gate برای commit نهایی هنوز pending است.
- تا دریافت CI exact-head، T0190 تکمیل‌شده تلقی نمی‌شود.


## Exact-head CI reconciliation for V 3.0.891

- Step Three quality gate run `41837229188`: passed.
- Rendezvous container run `41837229166`: passed.
- Windows WPF job `126599163618`: passed (C# format, host build, generated Protobuf, WPF startup, agent-tool checks and DPAPI/peer trust).
- Windows HostLifecycle job `126599162902`: failed only at the prior case-sensitive assertion that expected capitalized `Ready`, while the new fixed state-only label is `ready`. The failure was at `Program.cs:1072`; newly added sentinel redaction assertions were not reached.
- Android job `126599163603`: KtLint failed due one extra blank line before the final class brace in `RequestStatusPresentationTest.kt:71`. Android APK, JVM and Keystore instrumentation were skipped.
- Real inference job `41837229151`: pinned llama.cpp binary and GGUF verification succeeded; the shared HostLifecycle harness failed at the same status capitalization assertion before the live model acceptance test could produce a response. No inference success/failure is claimed.

## A002 correction snapshot

- Updated the legacy HostStatus ready-state assertion to compare `ready` with `StringComparison.OrdinalIgnoreCase`; the production presenter remains static and never reads `HostStatus.Message`.
- Removed the single extra blank line at the end of `RequestStatusPresentationTest.kt` reported by KtLint.
- No protocol, status mapping, redaction, inference, or networking logic changed.
- Exact-head Step Three, HostLifecycle, Android lint/APK/JVM/instrumentation and real-inference results are pending.


## Exact-head CI reconciliation for V 3.0.892

- Rendezvous container run `41837994420`: passed.
- Windows HostLifecycle job `126601806755`: passed, including T0190's structured-state/no-HostStatus.Message sentinel assertion and corrected Ready assertion.
- Windows WPF job `126601807304`: passed, including build, startup, generated Protobuf, agent-tool and DPAPI/peer-trust checks.
- Real inference run `41837994496` / job `126601806330`: passed. The log explicitly states live local llama.cpp inference succeeded, active generation cancellation was observed, and the owned process stopped cleanly.
- Step Three quality gate run `41837994512`: Python test suite ran 391 tests and passed; graph guard failed only with `FAILED_ATTEMPT_COUNT_MISMATCH: T0190: summary=1, history=0` because A001 had not been classified as failed in the history.
- Android job `126601807116`: KtLint reported one needless blank line in `RequestStatusPresentation.kt:51`; APK/JVM/instrumentation did not execute.

## A003 correction snapshot
- Removed the needless blank line in `RequestStatusPresentation.kt` reported by KtLint.
- Marked T0190-A001 as `failed`, matching its exact-head HostLifecycle/Android acceptance failure; summary `failed_attempt_count=1` now matches history. T0190-A002 remains `partial`.
- No production status behavior or privacy boundary changed.
- Exact-head graph guard, Android lint/build/tests and live-inference rerun remain pending.


## Final acceptance evidence — V 3.0.893

- **Branch/commit:** `main` / `ba0f5dd57eb5b11dda9aabe3cd56430e939b561e`.
- **Graph/Python gate:** run `41838724150` passed; 391 Python tests passed; graph_guard accepted 230 nodes/422 edges and the reconciled T0190 failed-attempt summary/history.
- **Rendezvous:** run `41838723576` passed.
- **Windows HostLifecycle:** job `126604339681` passed; log includes `backend and local-endpoint status uses structured state only, excludes HostStatus message content` and the safe-error mapping assertions.
- **Windows WPF:** job `126604339078` passed, including build/startup, agent-tool restrictions and DPAPI/peer-trust validation.
- **Android:** job `126604339686` passed KtLint, APK build, merged-manifest permission policy and Android JVM tests, including the status presentation tests.
- **Real model proof:** on V 3.0.892, run `41837994496` recorded `real local llama.cpp inference succeeded`; the response was received, active generation cancellation was observed, and the application-owned process stopped cleanly. The only V 3.0.893 code adjustment after this was removal of a whitespace-only blank line in RequestStatusPresentation.kt.
- **Broader workflow caveat:** on V 3.0.893, Android Keystore instrumentation remained in progress at the final observation; instrumentation report upload, Protobuf generated source verification, and APK artifact upload were not yet confirmed. This does not invalidate T0190's direct HostLifecycle/Android JVM status-presentation acceptance, but the overall product build is not being claimed green.

## Completion decision
- Windows status presenter uses only `HostState` and fixed copy; no `HostStatus.Message` is surfaced.
- Android status panel displays network and inference labels derived from structured lifecycle/peer state; free-form network/host availability messages are not rendered in the status panel.
- Prompt and inference responses remain in their dedicated input/result areas and are not reclassified as status.
- Regression checks passed on both client platforms. T0190 is `done`.
