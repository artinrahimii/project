# شواهد تلاش A004 — UPD-S1-016

- مبنا: `41e5bd23dc9a49d4e1ddf05c51414af4225fcae7` — `V 3.1.180`.
- کیفیت exact-head: run [42896511950](https://github.com/artinrahimii/project/actions/runs/42896511950) اجراکنندهٔ ۴۹۶ تست بود؛ فقط یک تست شکست خورد.
- خطا: `test_one_round_dispatch_times_out_instead_of_replaying_a_host_submission` در `tests/test_request_retry_budget_boundary.py` سه درخواست را انتظار داشت اما dispatcher test هم‌اکنون چهار candidate (REJECTED، exception، FAILED و QUEUED) را می‌فرستد.
- Android ktlint + .NET fixture build: در run [42896511884](https://github.com/artinrahimii/project/actions/runs/42896511884) موفق؛ JVM/APK در حال اجرا بود.
- HostLifecycle در run 42896511884 موفق؛ Rendezvous در run [42896512209](https://github.com/artinrahimii/project/actions/runs/42896512209) موفق.
- اصلاح: assertion تعداد درخواست از ۳ به ۴.
- CI دقیق A004: pending؛ task همچنان `in_progress` است.