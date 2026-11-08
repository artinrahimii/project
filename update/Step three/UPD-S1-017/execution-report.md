# گزارش اجرای UPD-S1-017 — قرارداد مشترک پیام و چرخهٔ درخواست

## وضعیت این تلاش
- وضعیت: `in_progress`؛ کد و آزمون‌ها تغییر کرده‌اند، اما exact-commit CI هنوز اجرا/تأیید نشده است.
- Attempt: `UPD-S1-017-20261108114610`
- Repository/branch: `artinrahimii/project` / `main`
- Baseline: `66f21b572b2766433d1d9eed40e98bf7c8ebba73` — `V 3.1.48`
- زمان آغاز: `2026-11-08T11:46:10.532Z`
- Operation: `modify`؛ schema و runtimeهای موجود حفظ و فقط شکاف قرارداد/نگاشت خطا اصلاح شد.

## شکاف‌های احرازشده
1. `InferenceResponse` شناسه‌های request/attempt/round داشت اما error code typed نداشت؛ Android `FAILED` را به `INTERNAL` عمومی تبدیل می‌کرد و `HOST_BUSY` و `RESOURCE_LIMIT` از بین می‌رفتند.
2. پیام‌های جانبی `ProtocolError`, `HostBusy`, `UnsupportedModel` فقط به request ID متصل بودند؛ Android transport آن‌ها را با requestId به چند attempt pending اعمال می‌کرد.
3. parser سمت Android برای خطاهای جانبی attempt و round را الزام نمی‌کرد؛ این با retry ترتیبی و round جدید سازگار نبود.
4. timeout داخلی backend با deadline مطلق درخواست باید تفکیک می‌شد؛ هیچ پاسخ دیررس یا دارای completion timestamp ساختگی نباید ارسال شود.

## اصلاحات اجرایی
- Protobuf به‌صورت additive توسعه یافت: `InferenceResponse.error_code = 10`; فیلدهای `attempt_id` و `round_id` در شماره‌های جدید برای `ProtocolError`, `HostBusy`, `UnsupportedModel`.
- validator مشترک C# بازهٔ enum و سازگاری error_code با outcome terminal را بررسی می‌کند؛ پاسخ success/cancelled کد خطا نمی‌گیرد.
- Windows مسیر واقعی peer response را به کدهای typed برای Busy، ModelMismatch، ResourceLimit، InvalidMessage، backend timeout و Internal متصل کرد.
- اگر deadline منطقی تمام شده باشد، میزبان هیچ پاسخ دیررس نمی‌فرستد؛ lifecycle timeout بعد از deadline را suppress می‌کند تا timer اصلی Android وضعیت نهایی را ثبت کند.
- پیام‌های خطای جانبی ساخته‌شده توسط mapper، request/attempt/round را حفظ می‌کنند.
- Android parser فقط خطایی را می‌پذیرد که هر سه شناسهٔ nonblank داشته باشد؛ transport آن را فقط به همان AttemptKey، round، peer احرازشده، stream controller و model مرتبط می‌کند. خطای stale یا نامنطبق فقط unmatched می‌شود.
- RequestViewModel کد typed را در کارت همان attempt نمایش می‌دهد و کد ناسازگار با outcome را رد می‌کند.
- Streaming قطعه‌ای اضافه نشده است؛ پاسخ کامل terminal نسخهٔ نخست حفظ می‌شود.

## فایل‌های تغییر‌یافته
- `protocol/whitep/v1/messages.proto`
- `src/Shared.Contracts/ProtocolMessageValidator.cs`
- `src/Host.Windows/Networking/PeerResponseCorrelationValidator.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/Networking/NetworkInferenceRequestResponseMapper.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RemoteRequestErrorParser.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RemoteRequestErrorParserTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt`
- `tests/HostLifecycle.Tests/NetworkInferenceRequestResponseMapperAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/RequestIdentityIndependenceAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`
- `tests/test_message_validation.py`
- `tests/test_request_error_codes.py`
- `tests/test_android_libp2p_transport_contract.py`
- `update/Step three/UPD-S1-017/execution-report.md`
- `update/Step three/UPD-S1-017/evidence/protocol-error-correlation.md`
- `update/Step two/project-graph.md`

## آزمون‌های لازم و وضعیت
- Windows HostLifecycle و validator مشترک: pending exact-commit CI.
- Android parser و RequestViewModel unit tests: pending exact-commit CI.
- Python protocol/error/transport contracts: pending exact-commit CI.
- quality.yml و product-build.yml: pending.
- آزمون فیزیکی Android-to-Windows، clean install و in-place upgrade در این وظیفه اجرا نشده‌اند.

## محدودیت
کد و آزمون‌ها به فایل‌های واقعی محصول و test suiteهای موجود افزوده شده‌اند؛ تا اجرای موفق exact-commit CI، هیچ معیار پذیرشی تکمیل‌شده اعلام نمی‌شود.
