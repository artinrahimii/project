# بازبینی T0192 — مرز retry توزیع‌شده و جلوگیری از replay پنهان

- مبنای main: `e06ceddfbbcf74a1116f4c10040e5080b61cca3b`، `V 3.1.100`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- Update مرتبط طبق reconciliation: UPD-S1-017

## پیاده‌سازی بررسی‌شده

- retry inference در Android و با بودجهٔ توزیع‌شدهٔ request کنترل می‌شود؛ timeout/failure پایین‌دست باعث replay خاموش inference نمی‌شود.
- fallback اتصال transport حداکثر ۸ آدرس یکتا را امتحان می‌کند و تمام آدرس‌ها باید همان PeerId مورد انتظار را پین کنند؛ این fallback اتصال است، نه اجرای مجدد inference.
- Host Busy با `retryable=false` و hint زمان retry برمی‌گردد؛ Android آن را به retry خودکار تبدیل نمی‌کند.
- `HostRequestResponseDeliveryGate` با claim اتمی از ارسال مجدد پاسخ پس از نتیجهٔ مبهم جلوگیری می‌کند؛ ارسال دوباره می‌تواند پاسخ تکراری بسازد.

## آزمون و شواهد

- `tests/test_request_retry_budget_boundary.py`: بودجهٔ سه تلاش first-release، dispatch، retry دستی، Busy غیرقابل retry و fallback هشت‌نشانی را از source کنترل می‌کند.
- `InferenceAttemptDispatcherTest.kt` و `HostRequestResponseDeliveryTests.cs`: تلاش‌های محدود، delivery race و suppression پاسخ تکراری را می‌سنجند.
- Product Build [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) و Quality Gate [42514411385](https://github.com/artinrahimii/project/actions/runs/42514411385) موفق بودند.

وضعیت پایه `done` حفظ شد؛ بازاعتبارسنجی پس از تغییرات UPD-S1-017 باقی است. آزمون شبکهٔ فیزیکی یا انتقال واقعی پاسخ در این بازبینی انجام نشد.
