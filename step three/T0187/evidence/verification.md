# T0187 — شواهد بازبینی و اجرای آزمون حساسیت log/temporary artifact

## Snapshot
- شاخه: `main`; مبنا: `e528d6d9e9daeb5a4c9540dfefe7d136770b90af` (`V 3.0.876`).
- گراف از Blob دقیق tree دریافت و SHA-1 Git Blob آن با `step three/project-graph.md` تطبیق داده شد؛ JSON کامل با ۲۳۰ node و ۴۲۲ edge parse شد.
- وضعیت قبل از اجرا: ۱۸۵ done، ۲ deferred_final، ۴۳ not_started؛ T0187 not_started.

## شواهد پیش‌نیازها و یال‌ها
- T0186: disclosure consent در Android و Windows، آزمون‌های `test_peer_inference_privacy_disclosure.py` و WPF/HostLifecycle در وضعیت done.
- T0177: `InferenceAttemptDispatcher`/`HostCandidateSelector` و آزمون‌های بودجه تلاش در وضعیت done.
- T0186 → T0187 و T0177 → T0187: type=blocks حفظ شد؛ طبقه‌بندی sequencing_only / validated شد؛ دلیل و فهرست شواهد در graph edge fields و edge_change_log ثبت شد.
- T0187 → T0188 هنوز unreviewed است.

## مبنای فنی بررسی‌شده
- Windows `SafeDiagnosticLog` فقط enumهای عددی و مدت زمان را می‌پذیرد و EventSource فقط سه مقدار عددی صادر می‌کند.
- Android `SafeDiagnosticLog.formatRecord` فقط schema/event/outcome/duration را تولید می‌کند.
- `WindowsHostConversationStore.Save` ابتدا `_protector.Protect(plaintext)` را صدا می‌زند؛ سپس فایل موقت را می‌سازد و فقط `protectedPayload` را می‌نویسد؛ plaintext و ciphertext bufferها در finally صفر می‌شوند.
- Android `EncryptedClientDatabaseFactory` از `sqlcipher_export` به فایل موقت مهاجرت می‌کند، مقصد رمز‌شده را validate می‌کند، سپس rename اتمی انجام می‌دهد و در خطا temp/sidecar را حذف می‌کند.

## فایل‌های تغییر‌یافته
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/SafeDiagnosticLogTest.kt`
- `tests/test_sensitive_artifact_scanning.py`
- `docs/security.md`
- `step three/T0187/execution-report.md`
- `step three/T0187/evidence/verification.md`
- `step three/project-graph.md`

## نتیجهٔ آزمون
- در زمان ایجاد snapshot، هیچ آزمون CI exact-head اجرا نشده بود؛ همهٔ نتیجه‌های وابسته به commit جدید pending هستند.
- اسکن‌ها فقط markerهای ساختگی را در artifactهای test-owned بررسی می‌کنند؛ ادعای اسکن کل filesystem یا دادهٔ خارج از کنترل برنامه مطرح نمی‌شود.
