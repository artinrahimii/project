# بازبینی T0191 — لغو، deadline و exception در مرزهای async

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b`، `V 3.1.100`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- Update مرتبط طبق reconciliation: UPD-S1-017

## پیاده‌سازی بررسی‌شده

- `HostRequestCoordinator.TryExecuteAsync` ورودی و cancellation را بررسی می‌کند، deadline مطلق را در مرز پذیرش snapshot می‌گیرد و پیش از اجرای inference وضعیت آمادگی را دوباره ارزیابی می‌کند.
- semaphore با `WaitAsync(0)` حداکثر یک عملیات را می‌پذیرد؛ درخواست دوم `Busy` می‌گیرد و در صف منتظر نمی‌ماند.
- deadline monitor با linked CancellationTokenSource کار فعال را در موعد لغو می‌کند؛ نتیجهٔ دیرهنگام را به موفقیت تبدیل نمی‌کند.
- cancellation و failure، active token و semaphore را در مسیر finally آزاد می‌کنند؛ استثناها به خروجی typed یا caller منتقل می‌شوند.

## آزمون و شواهد

- `HostRequestCoordinatorFailureAcceptanceTests.cs`: duplicate admission، deadline منقضی، لغو حین inference، deadline فعال و failure/آزادسازی gate را پوشش می‌دهد.
- `HostRequestCoordinatorConcurrencyTests.cs`: ۶۴ درخواست هم‌زمان، دقیقاً یک اجرای inference و ۶۳ نتیجهٔ Busy را می‌سنجد.
- `HostRequestCoordinatorBusyTests.cs`: ۳۲ درخواست هم‌زمان را بدون صف و با یک invocation کنترل می‌کند.
- Product Build دقیق source [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) شامل HostLifecycle tests موفق بود؛ Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) نیز موفق بود.

وضعیت پایه `done` حفظ شد؛ تغییرات typed error/deadline در UPD-S1-017 نیازمند بازاعتبارسنجی مجدد هستند. اجرای محلی مستقل در این محیط ممکن نشد.
