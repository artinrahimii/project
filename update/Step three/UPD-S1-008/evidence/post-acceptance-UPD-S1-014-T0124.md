# بازبینی پس از پذیرش UPD-S1-014 — T0124

- عنوان وظیفهٔ قدیمی: انتخاب میزبان بر اساس reachability، readiness، ظرفیت و مدل
- update_task_ids: UPD-S1-014, UPD-S1-023
- commit کد بررسی‌شده: f9f4cc8ad57964dedaca985ccdc679fee5eb7718 — V 3.1.191
- وضعیت پایهٔ legacy: done؛ بدون تغییر
- decision پیشنهادی پس از بازبینی: unchanged
- review_status: reviewed
- status_changed: false

## فایل‌ها و مصرف‌کنندگان بررسی‌شده
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt`
- `tests/test_android_host_candidate_selector_contract.py`

## آزمون‌های exact-head
- Quality Gate: 496/496 Python tests pass (run 42999958123). Android product build and unit/instrumentation suite passed (run 42999958352); HostLifecycle and rendezvous smoke passed (runs 42999958352 and 42999958177).

## نتیجه
selector پس از غربالگری میزبان‌های قابل‌قبول بر اساس activeRequests/maxConcurrentRequests واقعی مرتب می‌کند و برای تساوی از PeerId پایدار استفاده می‌کند؛ metric ناموجود RTT/throughput حدس زده نمی‌شود. سقف first-release یک میزبان در هر round حفظ شده است.

این تصمیم فقط پس از ثبت هم‌زمان در reconciliation ledger و نشانگر update_impact گراف اولیه معتبر خواهد شد. هیچ status قدیمی بر اساس affected_task_ids تغییر داده نشده است.
