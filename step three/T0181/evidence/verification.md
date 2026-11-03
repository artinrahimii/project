# T0181-A001 — شواهد اجرا و بازبینی

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پیاده‌سازی: `0399100e6f2f63fbfa35bc8d178208608a920730` (`V 3.0.838`)
- اصلاح assertion: `V 3.0.839`
- فایل گراف snapshot مبنا: `step three/project-graph.md`، Blob `03f4331d08f31c17f22404d0880e675bb0ee8757`، 3,353,447 بایت UTF-8؛ Git Blob SHA-1 با محاسبه از بایت‌های کامل تطبیق داشت.
- GitHub Actions run `41632877816`، job `125906609458`: ۳۴۳ آزمون اجرا شد، ۳۴۲ موفق، ۱ شکست. خطا مربوط به assertion متنی در `tests/test_host_conversation_retention_policy.py` بود، نه اجرای کد ذخیره‌سازی.
- اصلاح: assertion رشته‌ای به `public static HostConversationRetentionPolicy Default` تغییر کرد تا قرارداد واقعی فایل سیاست را بررسی کند.
- دو یال ورودی T0181:
  - T0180 → T0181: خروجی‌های پاک‌سازی buffer انتقال و decoder در ذخیره‌سازی Windows مصرف نمی‌شوند.
  - T0177 → T0181: selector و retry budget اندروید در پیاده‌سازی Windows مصرف نمی‌شوند.
  طبقه‌بندی `sequencing_only` پیشنهاد/بازبینی شده، `type=blocks` و ترتیب اجرایی بدون تغییر می‌مانند.
- محدودیت: به‌روزرسانی Blob جدید گراف (۳٫۳۵ MB) از Git Data و Contents API در چند تلاش با خطای انتقال HTTP/2 قطع شد؛ گراف هنوز تغییر نکرده است.
- هیچ آزمون DPAPI، .NET یا WPF را صرفاً بر اساس وجود کد موفق اعلام نمی‌کنیم؛ اجرای دوبارهٔ CI ویندوزی لازم است.
