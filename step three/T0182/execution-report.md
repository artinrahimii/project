# T0182 — پاک‌سازی فایل‌های موقت یتیم پس از راه‌اندازی مجدد

- وضعیت: `in_progress`؛ آزمون CI دقیق commit هنوز اجرا نشده است.
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit مبنا: `942ad2fbb3316d789bd0120abf2b39831e60520c` — `V 3.0.841`
- تلاش: `T0182-A001`
- شروع ثبت‌شده: `2026-11-03T12:29:30Z`

## بررسی پیش‌نیازها و یال‌ها

- `T0181 → T0182`: وابستگی فنی `hard_dependency`؛ T0181 ذخیره‌ساز و قرارداد نام‌گذاری فایل موقت `.<history-file>.<GUID-N>.tmp` را ایجاد می‌کند و T0182 همان خروجی را پاک‌سازی می‌کند.
- `T0177 → T0182`: وابستگی اجرایی صرف `sequencing_only`؛ انتخاب میزبان و بودجهٔ تلاش اندروید در پاک‌سازی فایل موقت میزبان Windows مصرف نمی‌شود.

در هر دو مورد `type=blocks` حفظ می‌شود. دلیل و شواهد در `edge_change_log` ثبت شده‌اند.

## پیاده‌سازی واقعی

- هنگام ساخت `WindowsHostConversationStore`، اسکن پوشهٔ اختصاصی history انجام می‌شود.
- فقط فایل هم‌مسیر با prefix دقیق نام history، پسوند `.tmp` و شناسهٔ ۳۲ نویسه‌ای GUID در قالب N بررسی می‌شود.
- فایل تازه‌تر از ۳۰ ثانیه حذف نمی‌شود تا در فاصلهٔ کوتاه close-to-rename با Save هم‌زمان تداخل ایجاد نشود.
- پیش از حذف، فایل به‌صورت انحصاری با `FileShare.None` باز می‌شود؛ فایل قفل‌شده از اسکن حذف می‌ماند.
- پوشه‌ها، reparse pointها، نام‌های نامعتبر، فایل‌های ذخیره‌ساز دیگر و خطاهای permission دست‌نخورده می‌مانند؛ خطای cleanup راه‌اندازی برنامه را متوقف نمی‌کند.
- آزمون runtime برای stale/recent/locked/malformed/unrelated file افزوده شد.

## فایل‌های تغییریافته

- `src/Host.Windows/Chat/WindowsHostConversationStore.cs`
- `tests/HostLifecycle.Tests/ChatConversationAcceptanceTests.cs`
- `tests/test_host_conversation_retention_policy.py`
- `docs/security.md`

## آزمون و محدودیت

در این commit فقط پیاده‌سازی و آزمون‌های جدید ثبت می‌شوند؛ نتیجهٔ آزمون هنوز ادعا نشده است. آزمون HostLifecycle روی runner ویندوزی، Step Three Python gate، Windows WPF و سایر workflowهای مرتبط باید روی SHA دقیق اجرا شوند.

## مرز پاک‌سازی

این تغییر فقط فایل‌های موقت متعلق به `WindowsHostConversationStore` را هدف می‌گیرد. حذف pagefile، crash dumps، OS backups، logs یا فایل‌های موقت سایر بخش‌ها در دامنهٔ T0182 نیست.
