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


## تلاش A002 — بازاعتبارسنجی آزمون‌ها روی commit دقیق

- commit کد و آزمون‌های اجراشده: `ea374677019b50b07677a60c1e6c0df3a970c89b` — `V 3.1.203`.
- کیفیت: run `43038776230` موفق؛ ۵۰۲ آزمون Python، compileall و اعتبارسنجی هر دو گراف.
- محصول: run `43038776527` موفق؛ Android lint/JVM/build/Keystore instrumentation و Windows WPF/HostLifecycle/installer.
- شواهد رفتاری deadline، سقف تلاش، retry دستی، Request ID پایدار، Attempt ID تازه، پاسخ تکراری، بودجهٔ retry و concurrency در فایل `evidence/exact-head-ci-A002-2026-11-11.md` ثبت شده‌اند.
- نتیجه: معیارهای پذیرش UPD-S1-015 با کد موجود و آزمون‌های واقعی تأیید شدند؛ هیچ تغییر غیرضروری به منطق محصول اعمال نشد. وضعیت این وظیفه به `done` ارتقا یافت.
- محدودیت: این نتیجه آزمون فیزیکی Android-to-Windows یا اجرای مدل روی دستگاه کاربر محسوب نمی‌شود؛ آن‌ها در UPD-S1-020 جداگانه مسدود هستند.
