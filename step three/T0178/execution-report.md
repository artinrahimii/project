# T0178 — پیش‌فرض عدم ذخیرهٔ پایدار متن درخواست و پاسخ در میزبان

- **وضعیت نهایی:** `done`
- **شاخه:** `main`
- **commit پیاده‌سازی:** `1bd292f8daddc2466177aa48844b84c7a8603db3` — `V 3.0.813`
- **commit اصلاح آزمون و ثبت نتیجه:** `e2ee8f70ac70b0680cc9be03e7e219747980a9fe` — `V 3.0.814`
- **تلاش‌ها:** A001 ناموفق در کنترل کیفیت به دلیل assertion قدیمی؛ A002 موفق پس از اصلاح آزمون منبع.

## پیاده‌سازی واقعی

- سیاست immutable `HostConversationRetentionPolicy.Default` با دامنهٔ `ProcessSessionOnly` افزوده شد؛ `PersistsPromptAndResponse` برابر false است.
- `ChatConversation` سیاست را دریافت می‌کند و محتوای مکالمه را فقط در مجموعهٔ حافظه‌ای نشست جاری نگه می‌دارد.
- `MainWindow` سیاست پیش‌فرض را به‌صورت صریح به مکالمه تزریق می‌کند.
- آزمون HostLifecycle، سیاست غیرپایدار، تزریق آن و شروع خالی یک مکالمهٔ مستقل را بررسی می‌کند.
- آزمون Python و `docs/security.md` مرز ادعا را محافظت و تشریح می‌کنند.
- خطای آزمون منبع قدیمی WPF در تلاش A001 اصلاح شد؛ سابقهٔ شکست حذف نشده است.

## بررسی وابستگی

یال `T0177 → T0178` با بررسی کد و خروجی‌ها `sequencing_only` ثبت شد. نوع `blocks` و ترتیب گراف حفظ شده‌اند؛ از برچسب `blocks` به‌تنهایی وابستگی فنی استنتاج نشد.

## شواهد آزمون دقیق commit

- [Step Three quality gate — run 41439813467](https://github.com/artinrahimii/project/actions/runs/41439813467): **موفق**؛ ۳۳۲ آزمون Python پاس شدند، بررسی syntax اجرا شد و graph guard برای ۲۳۰ وظیفه و ۴۲۲ یال، صفر خطا گزارش کرد. یک هشدار قدیمی دربارهٔ دادهٔ متنی غیرشناسه‌ای در metadata وظیفهٔ T0001 باقی است.
- [White-P product build — run 41439813544](https://github.com/artinrahimii/project/actions/runs/41439813544): **موفق**؛ ساخت و smoke test ویندوز، کنترل‌های امنیتی و DPAPI، lint و APK اندروید، آزمون‌های JVM، تولید Protobuf و آزمون‌های HostLifecycle همگی پاس شدند.
- [Rendezvous container build and smoke test — run 41439813434](https://github.com/artinrahimii/project/actions/runs/41439813434): **موفق**.

## محدودیت صریح

این وظیفه ذخیره‌سازی پایدار تحت کنترل برنامهٔ میزبان را پوشش می‌دهد؛ صفرسازی حافظه، pagefile/swap، crash dump، ابزارهای خارجی یا حذف کامل داده از logها را اثبات نمی‌کند. این موارد در T0179 و T0180 و کنترل‌های مستقل بررسی می‌شوند. همچنین Android طبق ADR-0020 تاریخچهٔ محلی خود را در Room نگه می‌دارد. بنابراین تکمیل T0178 به معنی تأیید کامل `AC-PRIV-01` نیست.

## فایل‌ها و خروجی‌ها

- `src/Host.Application/Chat/HostConversationRetentionPolicy.cs`
- `src/Host.Application/Chat/ChatConversation.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `tests/test_wpf_entrypoint.py`
- `docs/security.md`
- `step three/T0178/evidence/verification.md`
