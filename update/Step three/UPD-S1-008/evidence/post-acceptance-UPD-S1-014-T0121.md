# بازبینی پس از پذیرش UPD-S1-014 — T0121

- عنوان وظیفهٔ قدیمی: اعلان حضور میزبان با TTL و تمدید دوره‌ای
- update_task_ids: UPD-S1-014
- commit کد بررسی‌شده: f9f4cc8ad57964dedaca985ccdc679fee5eb7718 — V 3.1.191
- وضعیت پایهٔ legacy: done؛ بدون تغییر
- decision پیشنهادی پس از بازبینی: unchanged
- review_status: reviewed
- status_changed: false

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Host.Windows/Networking/PeerAnnouncementLeasePublisher.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Rendezvous.Service/PeerAnnouncementDirectory.cs`
- `src/Rendezvous.Service/PeerAnnouncementServiceHost.cs`
- `tests/HostLifecycle.Tests/PeerAnnouncementClientAcceptanceTests.cs`
- `tests/rendezvous_container_smoke.py`

## آزمون‌های exact-head
- HostLifecycle 14/14 steps passed (run 42999958352, job 130453350033); Rendezvous container build/smoke 6/6 steps passed (run 42999958177, job 130453350139).

## نتیجه
چرخهٔ lease/TTL و renewal در کد فعلی حفظ شده و این delta آن را تغییر نداده است. HostLifecycle و container smoke دقیق commit موفق‌اند. استقرار عمومی مستقل و network probe جداگانه لازم است.

این تصمیم فقط پس از ثبت هم‌زمان در reconciliation ledger و نشانگر update_impact گراف اولیه معتبر خواهد شد. هیچ status قدیمی بر اساس affected_task_ids تغییر داده نشده است.
