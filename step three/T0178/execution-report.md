# T0178 — پیش‌فرض عدم ذخیرهٔ پایدار متن درخواست و پاسخ در میزبان

- **وضعیت فعلی:** `in_progress`؛ پیاده‌سازی آماده است، اما CI دقیق هنوز همهٔ گردش‌ها را تأیید نکرده است.
- **commit پایه:** `08431f8eabb2a6010be6cd3f8cd149ec8928acbe` (`V 3.0.812`).
- **commit پیاده‌سازی:** `1bd292f8daddc2466177aa48844b84c7a8603db3` (`V 3.0.813`)، شاخهٔ `main`.
- **Attempt A001:** `2026-11-02T11:47:11.868Z` تا `2026-11-02T11:49:03.912Z` — `failed`.
- **Attempt A002:** از `2026-11-02T11:49:03.912Z` — `partial`؛ CI دقیق commit جدید در انتظار است.

## وابستگی

یال `T0177 → T0178` از روی خروجی واقعی وظایف بررسی شد. آزمون انتخاب میزبان و سقف تلاش، خروجی/قراردادی مصرفی برای سیاست نگهداری میزبان تولید نمی‌کند؛ بنابراین طبقه‌بندی به `sequencing_only` تغییر کرد. نوع `blocks` و ترتیب اجرایی اصلی حفظ شده‌اند. ثبت دلیل و شواهد در `edge_change_log` گراف انجام شده است.

## پیاده‌سازی

- سیاست immutable با دامنهٔ `ProcessSessionOnly` و `PersistsPromptAndResponse == false` اضافه شد.
- `ChatConversation` سیاست را دریافت می‌کند و محتوای مکالمه را در مجموعهٔ حافظه‌ای نشست نگه می‌دارد.
- `MainWindow` سیاست پیش‌فرض را صریحاً تزریق می‌کند.
- آزمون runtime میزبان، پیش‌فرض غیرپایدار، استفاده از سیاست و شروع خالی مکالمهٔ مستقل را می‌سنجد.
- آزمون Python جدید و `docs/security.md` مرز ادعا را کنترل می‌کنند و ذخیره‌سازی Android را از سیاست میزبان جدا می‌دانند.

## تاریخچهٔ تلاش‌ها

### A001 — شکست آزمون کیفیت

- commit: `1bd292f8daddc2466177aa48844b84c7a8603db3` (`V 3.0.813`).
- گردش [Step Three quality gate](https://github.com/artinrahimii/project/actions/runs/41439579310) شکست خورد: از ۳۳۲ آزمون پایتون، یک مورد ناموفق بود.
- علت: `tests/test_wpf_entrypoint.py:124` هنوز مقداردهی قدیمی `new ChatConversation()` را مطالبه می‌کرد؛ کد جدید به‌درستی سیاست پیش‌فرض را صریح تزریق می‌کند.
- این شکست در تاریخچه نگه داشته شد؛ آن را موفق یا نادیده‌گرفته‌شده ثبت نمی‌کنم.

### A002 — اصلاح آزمون منبع

- آزمون قدیمی به‌روزرسانی شد تا `new(HostConversationRetentionPolicy.Default)` را بررسی کند.
- نتیجهٔ گردش‌های جدید پس از ثبت commit این تلاش باید بررسی شود؛ هنوز نتیجهٔ موفقی برای A002 ادعا نشده است.

## محدودیت دامنه

این وظیفه فقط ذخیره‌سازی پایدار تحت کنترل برنامهٔ میزبان را هدف می‌گیرد. صفرسازی حافظه، pagefile/swap، crash dump، ابزارهای خارجی، logهای ساختاریافته، پاک‌سازی بافرهای موقت یا پذیرش عملیاتی کامل `AC-PRIV-01` را اثبات نمی‌کند. logها در `T0179` و پاک‌سازی بافرها در `T0180` جداگانه‌اند. Android طبق ADR-0020 سابقهٔ محلی خود را در Room نگه می‌دارد.

## فایل‌های واقعی این وظیفه

- `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`
- `src/Host.Application/Chat/ChatConversation.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `tests/test_wpf_entrypoint.py`
- `docs/security.md`
- `step three/T0178/execution-report.md`
- `step three/T0178/evidence/verification.md`
- `step three/project-graph.md`

## نتیجهٔ آزمون فعلی

- A001: یک شکست کیفیت به علت assertion قدیمی، ثبت شد.
- A002: در زمان ایجاد این گزارش هنوز آزمون اجرا/تأیید نشده است.
- وضعیت وظیفه تا تأیید اجرای دقیق commit بعدی `in_progress` می‌ماند.
