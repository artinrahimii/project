# بازبینی پس از پذیرش UPD-S1-014 — T0123

- عنوان وظیفهٔ قدیمی: تعیین Online از heartbeat و HostStatus تازه و احرازشده
- update_task_ids: UPD-S1-014
- commit کد بررسی‌شده: f9f4cc8ad57964dedaca985ccdc679fee5eb7718 — V 3.1.191
- وضعیت پایهٔ legacy: done؛ بدون تغییر
- decision پیشنهادی پس از بازبینی: unchanged
- review_status: reviewed
- status_changed: false

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Application/HostInferenceSession.cs`
- `src/Shared.Contracts/ProtocolMessageValidator.cs`
- `src/Host.Infrastructure/ProtocolMessageParser.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt`

## آزمون‌های exact-head
- Android full JVM/instrumentation suite completed with 237 pass, 1 skip, 0 failures (run 42999958352); HostLifecycle 68 critical markers passed (same run).

## نتیجه
کشف میزبان از HostStatus تازه و احرازشده، timestamp، READY/BUSY و model capabilities استفاده می‌کند. Android suite و HostLifecycle دقیق commit موفق‌اند. این بازبینی، online flag ساده را معادل readiness تلقی نمی‌کند.

این تصمیم فقط پس از ثبت هم‌زمان در reconciliation ledger و نشانگر update_impact گراف اولیه معتبر خواهد شد. هیچ status قدیمی بر اساس affected_task_ids تغییر داده نشده است.
