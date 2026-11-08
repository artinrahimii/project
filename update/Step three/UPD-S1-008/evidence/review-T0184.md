# بازبینی T0184 — رمزگذاری پایگاه Room و حفاظت کلید

- مبنای شاخه main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd`، پیام `V 3.1.96`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی بررسی‌شده

- `EncryptedClientDatabaseFactory` دیتابیس Room را با SQLCipher باز می‌کند.
- کلید تصادفی دیتابیس با Android Keystore محافظت و در app-private preferences به‌صورت envelope نگهداری می‌شود؛ در نبود کلید برای فایل encrypted، سیستم fail-closed می‌کند.
- مهاجرت دیتابیس plaintext قدیمی، integrity و WAL checkpoint را بررسی می‌کند، با `sqlcipher_export` به فایل encrypted موقت منتقل می‌کند، version و application ID را حفظ می‌کند و پس از integrity check و بستن منبع، فایل را اتمی جایگزین می‌کند.
- Room schema نسخهٔ ۵ و migrationهای نسخه‌دار دارد.

## آزمون و شواهد

- `EncryptedClientDatabaseAcceptanceTest.kt`: ایجاد/reopen دیتابیس رمزگذاری‌شده، حفظ ردیف‌های نسخهٔ plaintext قبلی، حفاظت کلید و رفتار fail-closed را در Android instrumentation می‌سنجد.
- contract testها migration، encrypted storage، capsule persistence و مرز ذخیره‌سازی محلی را بررسی می‌کنند.
- Product Build run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) شامل Android instrumentation موفق بود؛ Quality Gate run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.

وضعیت پایه `done` حفظ شد. UPD-S1-010/018 ممکن است schema و محتوای گفتگو را متأثر کنند؛ بنابراین upgrade نهایی باید دوباره آزمایش شود. این بازبینی اثبات in-place upgrade فیزیکی نیست.
