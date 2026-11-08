# بازبینی T0183 — محافظت هویت و اسرار محلی با Android Keystore

- مبنای شاخه main: `b69fbc20328af3f4273b4a137b649ca4cdf6e3bd`، پیام `V 3.1.96`
- مبنای کد محصول: `db2bfaa62c7816e9f06ed4e9e1a8d1d91cb822fd`، پیام `V 3.1.88`
- وضعیت پایه: `done`؛ حفظ شد.
- تصمیم: `re_review_required`
- وظایف به‌روزرسانی مرتبط: UPD-S1-007، UPD-S1-018

## پیاده‌سازی بررسی‌شده

- `AndroidKeyStoreProtectedBlobProtector` از envelope نسخه‌دار AES-256-GCM و کلید AES غیرقابل‌استخراج Keystore استفاده می‌کند.
- هویت libp2p در app-private preferences به‌صورت envelope محافظت‌شده نگهداری می‌شود؛ envelope و cleartext موقت پس از استفاده پاک‌سازی می‌شوند.
- کلیدهای identity و trust از aliasهای جدا استفاده می‌کنند.
- ذخیرهٔ trust نیازمند تأیید صریح کاربر است؛ revoke نیز پیاده‌سازی شده است.
- دادهٔ خراب یا کلید گمشده fail-closed است و باعث چرخش خاموش هویت نمی‌شود.

## آزمون و شواهد

- `AndroidKeyStorePersistenceTest.kt`: non-exportable بودن کلید، ماندگاری identity، جدایی کلیدها، tamper و missing-key را بررسی می‌کند.
- `AndroidPeerIdentityStoreTest.kt` و `AndroidPeerTrustStoreTest.kt`: ثبات peer ID، عدم اعتماد بدون تأیید و revoke را می‌سنجند.
- Product Build run [42510357567](https://github.com/artinrahimii/project/actions/runs/42510357567) شامل Keystore instrumentation موفق بود؛ Quality Gate run [42510357597](https://github.com/artinrahimii/project/actions/runs/42510357597) نیز موفق بود.

وضعیت پایه `done` حفظ شد؛ به‌دلیل اثر UPD-S1-018، بازاعتبارسنجی بعدی لازم است. این CI جایگزین upgrade روی دستگاه واقعی نیست.
