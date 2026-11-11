# شواهد مسدودی استقرار عمومی Rendezvous — UPD-S1-011 / تلاش A002

## خط مبنا

- commit بررسی‌شده: `1b91af90fe42e4a33237bf4065d8c98dd25bcb42` — `V 3.1.204`
- تاریخ ثبت بررسی: `2026-11-11T15:06:15.219Z`
- وضعیت: `blocked`

## شواهد مخزن

1. فایل `docs/deployment/rendezvous-service.md` آدرس `https://rendezvous.example.net/` را به‌صراحت placeholder و نه endpoint مستقر معرفی می‌کند.
2. همان راهنما نیاز به استقرار واقعی پشت ingress یا reverse proxy با TLS معتبر و محدودیت‌های provider-level را بیان می‌کند.
3. بخش Public reachability acceptance مستلزم اتصال یک peer از شبکه‌ای دیگر، دریافت announcement و برقراری اتصال authenticated به PeerId دقیق میزبان Windows است.
4. workflow `.github/workflows/rendezvous-container.yml` فقط image را build می‌کند، container را روی `127.0.0.1:18080` بالا می‌آورد و smoke test loopback اجرا می‌کند. هیچ مرحلهٔ deploy عمومی یا آزمون از شبکهٔ مستقل در آن وجود ندارد.
5. اجرای container workflow روی commit `1b91af90fe42e4a33237bf4065d8c98dd25bcb42` با run [43044517854](https://github.com/artinrahimii/project/actions/runs/43044517854) موفق شد؛ این نتیجه فقط build و loopback smoke است و deploy عمومی را ثابت نمی‌کند.
6. Android عمداً endpoint پیش‌فرض را خالی نگه می‌دارد تا URL حدسی یا ساختگی وارد محصول نشود؛ build-time property باید با origin واقعی و معتبر پیکربندی شود.

## مانع واقعی

در مخزن و ابزار متصل، public DNS origin، TLS ingress یا محیط/اعتبارنامهٔ deployment قابل‌استفاده برای انتشار واقعی این سرویس در دسترس نیست. بنابراین تنظیم یک URL فرضی یا ادعای discovery عمومی، گمراه‌کننده و ناامن خواهد بود.

## شرط رفع مسدودی

سرویس باید روی میزبان واقعی با origin عمومی HTTPS مستقر شود؛ سپس origin در build انتشار Windows و Android پیکربندی، و از یک شبکهٔ مستقل discovery، انقضای lease، احراز هویت PeerId و اتصال مستقیم میزبان آزموده شود. تا آن زمان UPD-S1-011 باید `blocked` بماند.

این نتیجه به معنی شکست container build یا quality CI نیست؛ جداسازی build موفق از استقرار و reachability واقعی عمدی است.
