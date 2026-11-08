# بازبینی T0187 — اسکن logها و فایل‌های موقت برای دادهٔ حساس

- مبنای main: `fc006d4c7f01de25c27346fc85670d1dc503e231`، `V 3.1.98`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی و آزمون بررسی‌شده

- آزمون Windows از canaryهای مصنوعی prompt، capsule و secret استفاده می‌کند و فایل‌های موقت/ذخیره‌سازی میزبان را جست‌وجو می‌کند؛ buffer فایل خوانده‌شده پس از اسکن zeroize می‌شود.
- Windows EventSource و Android logger فقط فیلدهای numeric allowlist را ثبت می‌کنند؛ آزمون canary نبود رشته‌های حساس را می‌سنجد.
- Android instrumentation فایل encrypted Room و logها را بررسی می‌کند؛ آزمون‌های source-contract نیز log sinkها و فایل‌های متعلق به برنامه را اسکن می‌کنند.
- دامنهٔ اسکن شامل pagefile، crash dump، backup، screenshot، logهای سیستم‌عامل یا ابزارهای ثالث نیست.

## شواهد

- `tests/test_sensitive_artifact_scanning.py`
- `tests/test_network_payload_logging.py`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/SafeDiagnosticLogAcceptanceTests.cs`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/SafeDiagnosticLogTest.kt`
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند.

وضعیت پایه حفظ شد؛ `re_review_required` بعد از تغییرات privacy/storage در UPD-S1-018 لازم است. اسکن‌ها به معنی forensic audit کل سیستم‌عامل نیستند.
