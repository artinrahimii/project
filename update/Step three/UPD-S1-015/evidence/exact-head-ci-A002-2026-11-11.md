# شواهد اجرای UPD-S1-015 — تلاش A002

## خط مبنای دقیق

- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit کد و آزمون‌های بازبینی‌شده: `ea374677019b50b07677a60c1e6c0df3a970c89b` — `V 3.1.203`
- تاریخ پایان بررسی workflowها: `2026-11-11T14:44:52Z`
- هیچ کد محصولی در این تلاش تغییر نکرد؛ این تلاش ممیزی و تأیید آزمون‌های موجود و ثبت شواهد دقیق CI است.

## شواهد رفتاری Android

آزمون‌های زیر در build همان commit اجرا و موفق شدند:

- `DistributedRequestTest.appliesThreeSequentialSingleHostAttemptsAsTheFirstReleaseMaximum`: سقف نسخهٔ نخست، یک میزبان فعال و حداکثر سه round/attempt را کنترل می‌کند.
- `DistributedRequestTest.rejectsNonPositiveDeadline` و `InferenceAttemptDispatcherTest.rejectsAnAlreadyExpiredRoundBeforeSubmitting`: deadline نامعتبر یا منقضی پیش از dispatch رد می‌شود.
- `InferenceAttemptDispatcherTest.rejectsDuplicateAttemptIdsBeforeSubmitting`: شناسهٔ attempt تکراری قبل از ارسال رد می‌شود.
- `RequestViewModelTest.showsFailedAndRetriesOnlyAfterTheUserRequestsItWithSameRequestAndFreshAttemptIds`: retry دستی، حفظ Request ID و تولید Attempt ID تازه را بررسی می‌کند.
- `RequestViewModelTest.successfulSingleHostRequestDoesNotOfferOrDispatchAnotherAttempt`: پس از موفقیت، تلاش اضافه ارسال نمی‌شود.
- `RequestViewModelTest.rejectsDuplicateResponseForTheSameRequestAndAttemptWithoutReplacingTheFirstCard`: پاسخ تکراری برای یک درخواست/تلاش پذیرفته نمی‌شود.
- آزمون‌های `InferenceAttemptDispatcherTest` سقف round/attempt، expiry و رعایت بودجهٔ باقی‌مانده هنگام retry را نیز اجرا می‌کنند.

## شواهد Windows

- `HostRequestCoordinatorTests` بررسی می‌کند deadline پیش از inference و هنگام تحویل پاسخ اعمال شود، خروجی دیررس تحویل نشود و cancellation به عملیات فعال برسد.
- `HostRequestCoordinatorConcurrencyTests` رقابت ۶۴ درخواست را اجرا می‌کند: فقط یک inference وارد می‌شود، ۶۳ درخواست Busy می‌گیرند و بیشینهٔ inference هم‌زمان یک باقی می‌ماند.
- `Program.cs` اجرای هر دو مجموعهٔ آزمون را در HostLifecycle harness فراخوانی می‌کند.

## CI روی همان commit

- [Step Three quality gate — run 43038776230](https://github.com/artinrahimii/project/actions/runs/43038776230): موفق؛ ۵۰۲ آزمون Python، compileall و هر دو graph guard در حالت ساختاری و repository-backed.
- [White-P product build — run 43038776527](https://github.com/artinrahimii/project/actions/runs/43038776527): موفق؛ Android lint، debug/release build، JVM unit tests، Keystore instrumentation؛ Windows WPF build/publish/startup smoke، HostLifecycle tests و installer acceptance.
- [Rendezvous container build/smoke — run 43038776377](https://github.com/artinrahimii/project/actions/runs/43038776377): موفق؛ فقط build و smoke محلی container، نه استقرار public endpoint.

تمام jobهای کیفیت و محصول در commit دقیق `ea374677019b50b07677a60c1e6c0df3a970c89b` با نتیجهٔ `success` خاتمه یافتند.

## نتیجه و مرز ادعا

معیارهای UPD-S1-015 دربارهٔ cardinality نسخهٔ نخست، retry دستی و محدود، correlation/idempotency پاسخ، deadline و جلوگیری از inference هم‌زمان با کد و آزمون‌های واقعی موجود و اجرای موفق CI تأیید شدند. کد سالم موجود حفظ شد و پیاده‌سازی تکراری ایجاد نشد.

این شواهد، آزمون انتهابه‌انتهای واقعی Android به Windows، اجرای دستگاه فیزیکی، یا بارگذاری مدل روی رایانهٔ کاربر نیست؛ آن موارد متعلق به UPD-S1-020 و همچنان مسدودند.
