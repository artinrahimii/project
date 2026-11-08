# بازبینی T0182 — پاک‌سازی فایل موقت یتیم پس از راه‌اندازی مجدد

- مبنای شاخه main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd`، پیام `V 3.1.96`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی بررسی‌شده

- سازندهٔ `WindowsHostConversationStore` پاک‌سازی فایل‌های موقت یتیم را در startup آغاز می‌کند.
- فقط فایل‌های سطح اول با prefix مخصوص، پسوند `.tmp`، شناسهٔ GUID با قالب دقیق و عمر دست‌کم ۳۰ ثانیه کاندید حذف‌اند.
- فایل‌های تازه، directory، reparse point، فایل قفل‌شده یا غیرقابل‌دسترسی حذف نمی‌شوند.
- شکست enumeration یا دسترسی، startup را متوقف نمی‌کند.

## آزمون و شواهد

- `ChatConversationAcceptanceTests.cs`: پاک‌سازی فایل یتیم و حفظ فایل موقت تازه/فعال را آزمایش می‌کند.
- `test_host_conversation_retention_policy.py`: اجرای پاک‌سازی در startup و شروط نام، سن، lock و reparse point را کنترل می‌کند.
- Product Build run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) موفق بودند.

وضعیت پایهٔ `done` حفظ شد. این شواهد تضمین پاک‌سازی forensic تمام cacheها یا فایل‌های سیستم‌عامل نیست.
