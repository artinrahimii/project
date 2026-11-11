# بازبینی پس از پذیرش UPD-S1-014 — T0122

- عنوان وظیفهٔ قدیمی: حذف میزبان منقضی یا قطع‌شده از discovery
- update_task_ids: UPD-S1-014
- commit کد بررسی‌شده: f9f4cc8ad57964dedaca985ccdc679fee5eb7718 — V 3.1.191
- وضعیت پایهٔ legacy: done؛ بدون تغییر
- decision پیشنهادی پس از بازبینی: unchanged
- review_status: reviewed
- status_changed: false

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/RendezvousPeerDirectoryClient.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/RendezvousPeerDirectoryClientTest.kt`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`

## آزمون‌های exact-head
- Android product workflow completed successfully (run 42999958352); Android full unit/instrumentation summary 237 passed, 1 skipped, 0 failed; Rendezvous smoke run 42999958177 passed.

## نتیجه
منطق انقضا و حذف metadata stale در directory و کلاینت وجود دارد؛ آزمون‌های دقیق commit و container smoke موفق‌اند. هیچ status legacy تغییر نمی‌کند.

این تصمیم فقط پس از ثبت هم‌زمان در reconciliation ledger و نشانگر update_impact گراف اولیه معتبر خواهد شد. هیچ status قدیمی بر اساس affected_task_ids تغییر داده نشده است.
