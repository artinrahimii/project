# گزارش اجرای UPD-S1-015 — cardinality میزبان و retry ترتیبی

- مبنا: `40cdea13743b35487ca1b6365cb881b181a5ede1` — `V 3.1.200`
- تاریخ: 2026-11-11T14:06:20.090Z
- وضعیت تلاش: `in_progress`
- عملیات: `modify`؛ بازاستفاده از منطق موجود، بدون scheduler موازی.

## تصمیم معتبر

سیاست first-release: یک میزبان فعال در هر round، حداکثر سه round و سه تلاش کل؛ retry فقط با اقدام دستی کاربر. سقف generic سه میزبان/round صرفاً برای سازگاری است و مجوز broadcast در محصول نیست.

## منطق موجود بازبینی‌شده

- `FirstReleaseInferenceLimits`: یک میزبان فعال، سه round و سه تلاش کل.
- `AndroidInferenceRequestRuntime`: انتخاب را به یک میزبان محدود می‌کند و dispatch چندمیزبانه را رد می‌کند.
- `RequestViewModel`: request ID منطقی را حفظ، attempt ID تازه تولید، میزبان‌های قبلی را حذف و retry را فقط از `retryFailedAttempts` انجام می‌دهد.
- قبل از round بعدی، attempt قبلی باید terminal/timeout/cancel شده باشد؛ deadline و بودجهٔ کل کنترل می‌شوند.
- فقط میزبان‌های ناموفق هدف retry هستند؛ broadcast به همهٔ همتاهای آنلاین انجام نمی‌شود.
- response correlation به request/attempt/round/model/host متصل است و Windows در ارسال مبهم پاسخ را replay نمی‌کند.

## آزمون‌های موجود و جدید

- `tests/test_android_inference_attempt_budget_acceptance.py`
- `tests/test_request_retry_budget_boundary.py`
- `DistributedRequestTest.kt`, `InferenceAttemptDispatcherTest.kt`, `RequestViewModelTest.kt`, `Libp2pInferenceRequestTransportTest.kt`
- سناریوی JVM: `showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds`.
- آزمون قراردادی جدید: `tests/test_update_s1_015_retry_policy.py`.

در این بررسی شکاف قطعی جدیدی در منطق محصول پیدا نشد؛ کد سالم موجود حفظ شد. وضعیت تا اجرای کیفیت، Android JVM و HostLifecycle روی commit دقیق، `in_progress` می‌ماند.
