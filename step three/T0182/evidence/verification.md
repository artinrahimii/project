# T0182-A001 — شواهد و تصمیم‌های اجرایی

- baseline: `V 3.0.841` / `942ad2fbb3316d789bd0120abf2b39831e60520c`.
- T0181 در گراف `done` با CI کامل بود و ذخیره‌ساز موجود `src/Host.Windows/Chat/WindowsHostConversationStore.cs` فایل موقت را با نام `.<history-file>.<GUID-N>.tmp` می‌ساخت.
- آزمون‌های موجود T0181 round-trip DPAPI و fail-closed روی فایل اصلی تاریخچه را پوشش می‌دادند، اما فایل موقت باقی‌مانده از crash را پوشش نمی‌دادند.
- T0177 خروجی selector/retry-budget Android دارد و WindowsHostConversationStore آن‌ها را مصرف نمی‌کند.

## تغییرات

- پاک‌سازی در constructor ذخیره‌ساز فراخوانی می‌شود؛ فایل‌های قدیمی‌تر از ۳۰ ثانیه کاندید هستند.
- نام فایل با prefix تاریخچه و GUID-N دقیق اعتبارسنجی می‌شود.
- قبل از حذف، file attributes و reparse-point بررسی و سپس exclusive-open انجام می‌شود؛ locked file حذف نمی‌شود.
- آزمون runtime حالت stale، recent، active locked، malformed GUID و unrelated store name را بررسی می‌کند.
- Python source-contract و docs security نیز به‌روزرسانی شده‌اند.

## وضعیت آزمون

- تست runtime و quality در این مرحله هنوز اجرا نشده‌اند؛ تا دریافت CI، T0182 `in_progress` می‌ماند.
- GitHub commit و CI exact SHA باید در تلاش بعدی به execution_history افزوده شوند.
