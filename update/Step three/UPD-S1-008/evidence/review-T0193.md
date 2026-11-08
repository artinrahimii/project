# بازبینی T0193 — توقف پذیرش و shutdown امن inference host

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b`، `V 3.1.100`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- Update مرتبط طبق reconciliation: UPD-S1-012

## پیاده‌سازی بررسی‌شده

- `PeerAnnouncementHostLifecycle.StopAsync` زیر قفل lifecycle ابتدا `_active=0` را ثبت می‌کند، سپس receive-loop را از طریق lifetime token لغو می‌کند، تمدید lease را متوقف، transport را disconnect و receive-loop را await/dispose می‌کند.
- مسیر پردازش request، cancellation token متناظر با request/attempt و lifetime میزبان را بررسی می‌کند؛ بعد از لغو، پاسخ موفق دیرهنگام را ارسال نمی‌کند.
- `HostInferenceSession.DisposeCoreAsync` ابتدا shutdown را علامت می‌زند و token فعال را لغو می‌کند؛ سپس operation gate را می‌گیرد و فقط بعد از خروج عملیات فعال backend را dispose می‌کند.
- `MainWindow_Closing` پذیرش تعامل جدید را می‌بندد، operation و peer participation را لغو می‌کند، سرویس peer را await/dispose و فقط process متعلق به برنامه را متوقف می‌کند.

## آزمون و شواهد

- `HostInferenceSessionConcurrencyTests.cs`: disconnect، انتظار تا پایان inference، لغو shutdown، عدم انتشار نتیجهٔ دیرهنگام، drain و dispose backend را آزمایش می‌کند.
- مجموعهٔ `HostLifecycle.Tests` شامل مسیرهای cancellation و peer service است و در Product Build اجرا می‌شود.
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) موفق بود؛ Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) نیز موفق بود.

## محدودیت و تصمیم

تست‌ها lifecycle session و HostLifecycle را پوشش می‌دهند، اما خاموش‌شدن یک میزبان Windows نصب‌شده هم‌زمان با peer درخواست‌کننده و شبکهٔ واقعی در این نوبت انجام نشده است. وضعیت پایه `done` و تصمیم `re_review_required` حفظ شد.
