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
