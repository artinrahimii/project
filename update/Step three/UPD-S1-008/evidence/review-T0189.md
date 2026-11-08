# بازبینی T0189 — خطاهای typed و پیام‌های پایدار

- مبنای main: `fc006d4c7f01de25c27346fc85670d1dc503e231`، `V 3.1.98`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-017

## پیاده‌سازی بررسی‌شده

- پروتکل مشترک enum خطاهای InvalidMessage، UnsupportedVersion، UnsupportedModel، HostBusy، RequestExpired، ResourceLimit و Internal را تعریف می‌کند.
- Windows استثناهای محلی را به دسته‌های پایدار map می‌کند و جزئیات exception، مسیر و token را به پیام کاربر منتقل نمی‌کند.
- Windows response mapper خطای typed را به requestId/attemptId/roundId متصل می‌کند.
- Android parser پاسخ typed، busy و unsupported-model را parse می‌کند، correlation را الزام می‌کند و متن غیرقابل‌اعتماد سمت میزبان را به پیام کاربر تبدیل نمی‌کند.
- UI از کد پایدار برای نمایش خطای قابل‌فهم استفاده می‌کند.

## آزمون و شواهد

- `RequestErrorCodeTest.kt`، `RemoteRequestErrorParserTest.kt` و `InferenceRoundResponseCollectorTest.kt`
- `HostErrorPresentationAcceptanceTests.cs`
- `tests/test_request_error_codes.py`
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند.

وضعیت پایه `done` حفظ شد؛ به دلیل تغییرات runtime/error در UPD-S1-017، `re_review_required` است. اتصال واقعی پیام خطا در همان گفت‌وگوی Room باید دوباره آزموده شود.
