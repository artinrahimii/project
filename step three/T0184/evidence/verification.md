# T0184-A001 تا A005 — شواهد پیاده‌سازی و CI

## snapshotهای معتبر

- V3.0.854: `11eddfda0cc7f25a7c2ba9177d279960ce1a432c`.
- V3.0.855: `7678df818d5911701a2ecb41e3aa87b0a2ef6d47`.
- گراف مبنا: `b69a152faf7e1676a998cfd11a776997ef3b7c45`، ۳٬۳۵۱٬۱۵۹ بایت UTF-8؛ تطبیق Git Blob SHA-1 موفق و JSON کامل parse شد.

## پیاده‌سازی

- SQLCipher `4.20.0` از طریق Room `SupportOpenHelperFactory`.
- passphrase تصادفی ۳۲ بایتی، envelope محافظت‌شده توسط Android Keystore و alias مجزای database.
- export از SQLite plaintext با WAL checkpoint، `sqlcipher_export()`، حفظ نسخه/شناسهٔ برنامه، بررسی integrity و rename اتمی.
- چهار instrumentation scenario برای create/reopen، migration، protected key envelope و missing-key fail-closed.

## سابقهٔ CI

- Quality run `41686907108` روی V3.0.854: ۳۵۰ از ۳۵۳ تست موفق؛ سه قرارداد قدیمی که پس از انتقال migration به factory و اصلاح ADR نیازمند به‌روزرسانی بودند.
- Product run `41686906946`: Windows و HostLifecycle موفق؛ Android KtLint شکست خورد. build، JVM و instrumentation اجرا نشدند.
- Quality run `41687478161` روی V3.0.855: موفق.
- Container run `41687478388` روی V3.0.855: موفق.
- Product run `41687478163` هنگام ثبت این گزارش در حال اجرا بود.
- KtLint چهار مورد را در فایل instrumentation مشخص کرد؛ A005 آن‌ها را اصلاح کرد. هنوز نتیجهٔ CI اصلاح A005 وجود ندارد.

## فایل‌های پیاده‌سازی و آزمون

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `tests/test_android_database_migration_contract.py`
- `tests/test_android_encrypted_storage_contract.py`
- `tests/test_android_context_capsule_persistence_contract.py`
- `tests/test_android_local_storage_boundary.py`
- `docs/security.md` و ADR-0020

## مرز پذیرش

چهار instrumentation test جدید T0184 هنوز نتیجهٔ pass ندارند. آزمون‌های موفق Keystore مربوط به T0183 هستند و جایگزین آزمون رمزگذاری Room نمی‌شوند. T0184 به همین علت `in_progress` باقی است.
