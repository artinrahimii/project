# بازبینی T0196 — آزمون state machine، admission و ظرفیت

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b`، `V 3.1.100`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- Updateهای مرتبط طبق reconciliation: UPD-S1-005، UPD-S1-013، UPD-S1-019، UPD-S1-023

## پیاده‌سازی و آزمون بررسی‌شده

- `HostCapacityAcceptanceTests.cs` کمبود RAM، دیسک ناکافی و backend ناسازگار را `Insufficient`، و نبود اندازه‌گیری یا دادهٔ متناقض را `Unknown` می‌کند؛ هیچ وضعیت غیر Ready مجاز به ادعای اجرای inference نیست.
- `HostRequestAdmissionTests.cs` مشارکت خاموش، میزبان stopped/busy، model mismatch، capacity unknown/insufficient، deadline صفر، منقضی و خارج از محدوده را پیش از invocation رد می‌کند.
- `HostRequestCoordinatorBusyTests.cs` ۳۲ درخواست موازی را به Busy می‌فرستد؛ `HostRequestCoordinatorConcurrencyTests.cs` از ۶۴ contender فقط یک inference را می‌پذیرد.
- Product test runner به‌صورت مستقیم HostCapacity، Admission، Coordinator و Session concurrency acceptance suites را اجرا می‌کند.

## شواهد

- `tests/HostLifecycle.Tests/HostCapacityAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorConcurrencyTests.cs`
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند.

وضعیت پایه `done` حفظ شد؛ بعد از تغییرات host readiness/capacity و concurrency در UPD-S1-005/013/019/023 بازآزمایی لازم است. این شواهد آزمون ظرفیت سیستم فیزیکی کاربر نیست.
