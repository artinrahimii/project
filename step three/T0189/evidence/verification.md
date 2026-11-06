# T0189 — شواهد کد خطای پایدار

## Snapshot و دریافت گراف
- شاخه: `main`
- commit مبنا: `42c872b6620abcd81a553213fa957f6846c4fbdd` (`V 3.0.884`)
- مسیر گراف: `step three/project-graph.md`
- Blob استخراج‌شده از درخت همان commit: `90778fe8c66796d3be2094037e19931f47c9d1bc`
- Blob API محتوای کامل را برگرداند؛ طول پاسخ ۳٬۳۳۷٬۸۶۳ نویسه بود. بازفرستادن همان محتوای اصلی UTF-8 به GitHub Blob API، همان Blob SHA را بازگرداند. JSON کامل parse شد: ۲۳۰ وظیفه و ۴۲۲ یال.
- وضعیت T0188 در این snapshot: `done`; T0189: `not_started`.

## بازبینی پیش‌نیاز
- یال `T0188 → T0189`: بر اساس خروجی واقعی T0188 و قراردادهای مصرف‌کننده، رابطه فنی مستقیم وجود ندارد؛ T0189 رابط یا API تولیدشده توسط T0188 را مصرف نمی‌کند. بنابراین `sequencing_only / validated` مناسب است. نوع اصلی `blocks` و ترتیب اصلی حفظ می‌شوند.
- شواهد: `step three/T0188/execution-report.md`، `step three/T0188/evidence/verification.md`، `docs/security.md`، `tests/test_threat_model_documentation.py`.

## قرارداد و پیاده‌سازی بررسی‌شده
- schema موجود در `protocol/whitep/v1/messages.proto` شامل `ProtocolErrorCode` و پیام‌های `ProtocolError`، `HostBusy` و `UnsupportedModel` است.
- `NetworkInferenceRequestResponseMapper` در Windows این پیام‌ها را برای خطاهای validation، version، expiration، capacity، model mismatch، busy و internal می‌سازد.
- Android transport قبلاً فقط `InferenceResponse` را parse می‌کرد؛ این تغییر، error envelopeهای موجود را با requestId و هویت peer احراز‌شده تطبیق می‌دهد.
- پیام متن آزاد peer و متن exception، source of truth برای کد نیستند و در UI نمایش داده نمی‌شوند.
- ستون `RequestStateEntity.errorCode` از قبل وجود داشت؛ schema یا شمارهٔ migration تغییر نکرده است.

## فایل‌های تغییر‌یافته/ایجادشده
- `src/Host.Application/HostErrorPresentation.cs`
- `tests/HostLifecycle.Tests/HostErrorPresentationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/errors/RequestErrorCode.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RemoteRequestErrorParser.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceRoundResponseCollector.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/HostResponseCards.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceRoundResponseCollectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/errors/RequestErrorCodeTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RemoteRequestErrorParserTest.kt`
- `tests/test_request_error_codes.py`
- `step three/T0189/execution-report.md`
- `step three/T0189/evidence/verification.md`
- `step three/project-graph.md`

## آزمون و محدودیت
- آزمون‌های Windows و Android در source اضافه شده‌اند؛ در محیط این نوبت محلی اجرا نشده‌اند.
- نتایج CI دقیق commit حاوی این تغییرات هنوز pending است.
- این تغییر کدهای پایدار و نمایش امن را پوشش می‌دهد؛ سنجش دسترس‌پذیری واقعی اینترنت، صحت مدل نصب‌شده یا موفقیت inference end-to-end را جایگزین نمی‌کند.


## Exact-head CI reconciliation for V 3.0.885

- Step Three quality gate run 41829441636: passed.
- Rendezvous container run 41829441657: passed.
- Windows WPF job 126572504180: passed.
- HostLifecycle job 126572504604: failed at ParticipationActivationRecoveryTests.FailedRollbackAddsSafeRecoveryGuidanceAsync; the broad IOException-to-RESOURCE_LIMIT mapping displaced the expected safe generic fallback.
- Android job 126572504616: KtLint failed on import ordering in RequestErrorCodeTest.kt at line 3. The build and test steps behind this lint gate did not execute in this job.
- Real local inference run 41829441646: stopped at the same HostLifecycle failure; live model acceptance was not reached, so this is not evidence of a llama.cpp runtime failure.

## A002 correction snapshot

- Generic IOException now maps to INTERNAL unless a more specific subtype already maps to NETWORK_UNAVAILABLE, MODEL_UNAVAILABLE or RESOURCE_LIMIT.
- HostErrorPresentationAcceptanceTests explicitly verifies that generic IOException maps to INTERNAL and that supplied exception details are not exposed.
- Kotlin imports are reordered with standard java imports after regular application/library imports.
- The correction commit and exact-head CI results are pending; task status remains in_progress.


## Exact-head CI reconciliation for V 3.0.886
- Step Three quality gate run 41831155750: passed.
- Rendezvous container run 41831155720: passed.
- Windows WPF job 126578385121: passed.
- HostLifecycle job 126578385038: failed the existing T0047 assertion because ArgumentException presentation must retain actionable "server URL and model ID" guidance.
- Android job 126578384349: failed KtLint import ordering in ConversationDetailScreen.kt; APK/JVM/instrumentation did not execute.
- Real local inference run 41831155932: pinned llama.cpp binary and GGUF download/checks completed, but the run stopped at the same T0047 HostLifecycle assertion before live inference acceptance.

## A003 correction snapshot
- ArgumentException remains VALIDATION_FAILED while using fixed safe settings guidance mentioning the server URL and model ID; supplied exception text/path is not rendered.
- HostErrorPresentationAcceptanceTests explicitly checks the actionable safe guidance and redaction.
- ConversationDetailScreen imports are sorted according to KtLint's reported ordering.
- A003 has not yet run on an exact head; T0189 remains in_progress.


## Exact-head CI reconciliation for V 3.0.887
- Step Three quality gate run 41831823959: passed.
- Rendezvous container run 41831823935: passed.
- Windows WPF job 126580651445: passed.
- HostLifecycle job 126580651477: passed, including the previously failing participation-recovery and safe-error-mapping assertions.
- Real local inference job 126580651317: passed. The log states: "real local llama.cpp inference succeeded; active generation cancellation was observed and the owned process stopped cleanly."
- Android job 126580651061: failed only in ktlintMainSourceSetCheck, with three reported formatting violations in RequestErrorCode.kt:61, Libp2pInferenceRequestTransport.kt:612, and RequestViewModel.kt:880. Android APK, JVM tests and instrumentation were not run.

## A004 correction snapshot
- Added the trailing comma before the enum-entry semicolon in RequestErrorCode.kt.
- Moved the multi-line filter expression in Libp2pInferenceRequestTransport.kt to the next line after the assignment.
- Placed RequestViewModel.safeMessage's short expression on the signature line.
- No business logic, protocol contract, error classification, redaction rule or test assertion was weakened.
- Exact-head lint/build/JVM/instrumentation results for A004 are pending; T0189 remains in_progress.


## Exact-head CI reconciliation for V 3.0.888
- Commit: `f7d01de9bc9a628f9b0cde50ba9060354ac5908c`.
- Step Three quality gate run `41832537397`: passed.
- Rendezvous container run `41832537373`: passed.
- Product build run `41832537324`: HostLifecycle job `126583060656` passed; Windows WPF job `126583060973` passed; Android job `126583060958` failed at `ktlintMainSourceSetCheck`.
- KtLint reported unexpected indentation in `Libp2pInferenceRequestTransport.kt` lines 614–621 (expected indentation columns 20/24/16 as appropriate). APK, merged manifest permission verification, JVM tests and Android Keystore instrumentation were skipped.

## A005 correction snapshot
- Adjusted only indentation of the body and closing brace in the `pendingAttempts.entries.filter` lambda in `Libp2pInferenceRequestTransport.kt`.
- Request ID, authenticated peer, controller identity, ephemeral host ID and model ID predicates are unchanged.
- No protocol schema, error mapping, redaction rule or test assertion changed.
- Exact-head KtLint/build/JVM/instrumentation results are pending; T0189 remains `in_progress`.


## Final exact-head acceptance — V 3.0.889

- **Branch / commit:** `main` / `b4477b678906bb17f0c37be4970adf5500e811e6`.
- **Step Three quality gate:** run `41834044390` — success.
- **Rendezvous container:** run `41834044297` — success.
- **Product build:** run `41834044374` — success.
- **Windows HostLifecycle job `126588272463`:** success. Log assertions confirm stable Windows error categories and protocol-code mapping without exception details, plus safe actionable user guidance.
- **Windows WPF job `126588272363`:** success, including build, generated Protobuf, startup smoke test, agent-tool policy/capability restrictions, DPAPI host identity and peer-trust persistence checks.
- **Android job `126588271859`:** success. Kotlin lint, debug APK, merged-manifest permission policy, JVM unit tests, Android Keystore instrumentation, generated Java/Kotlin Protobuf sources and APK artifact upload all passed.
- **Keystore instrumentation:** `9/9` tests completed; `0` skipped; `0` failed.
- **Artifact evidence:** APK and instrumentation/JVM report artifacts were successfully uploaded by the product workflow.
- **Scope caveat:** the dedicated real local inference workflow was not triggered for this commit. The HostLifecycle log marks the opt-in model integration test as `SKIP`; no real-inference claim is made for `V 3.0.889`. The T0189 acceptance gates are the error mapping, safe presentation, Android parsing/UI, and associated Windows/Android tests, all of which passed.
- **Decision:** T0189 acceptance criteria are satisfied; mark `done`.
