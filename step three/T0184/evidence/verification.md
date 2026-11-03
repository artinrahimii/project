# T0184-A001 تا A003 — شواهد پیاده‌سازی و کنترل کیفیت

## snapshot و منبع

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پیاده‌سازی: `11eddfda0cc7f25a7c2ba9177d279960ce1a432c` — `V 3.0.854`.
- commit پایهٔ گراف: `9cf5e9b858f975b605c952ead3c4010fa78a9d11`.
- گراف پایه از Git Blob API دریافت شد؛ ۳٬۳۵۱٬۱۵۹ بایت UTF-8؛ SHA-1 قالب Git blob تطبیق داشت و JSON کامل parse شد.

## تصمیم و شواهد پیاده‌سازی

- Room قبلاً از open-helper استاندارد SQLite استفاده می‌کرد و رمزگذاری at-rest نداشت.
- T0183 رابط Keystore را فراهم می‌کند؛ پنج آزمون provider آن روی API 36 emulator در product run `41678108221` موفق شدند.
- T0184 از SQLCipher `4.20.0` و `SupportOpenHelperFactory` استفاده می‌کند.
- کلید تصادفی پایگاه فقط به‌صورت envelope محافظت‌شده توسط Android Keystore ذخیره می‌شود.
- مهاجرت plaintext با checkpoint و export به فایل جداگانه انجام می‌شود؛ integrity پیش از rename اتمی بررسی می‌شود.

## فایل‌های خروجی

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `tests/test_android_database_migration_contract.py`
- `tests/test_android_encrypted_storage_contract.py`
- `tests/test_android_context_capsule_persistence_contract.py`
- `tests/test_android_local_storage_boundary.py`
- `docs/security.md`
- `docs/decisions/ADR-0020-android-client-local-conversation-storage.md`
- `step three/T0184/execution-report.md`

## شکست CI و اصلاح

Quality run `41686907108`، روی commit `11eddfda0cc7f25a7c2ba9177d279960ce1a432c`، ۳۵۳ آزمون را اجرا کرد: ۳۵۰ موفق و سه شکست:
1. `test_android_context_capsule_persistence_contract.py` هنوز محل قدیمی `addMigrations` را در ClientDatabase.kt می‌خواست.
2. `test_android_local_storage_boundary.py` نیز همان فرض محل قدیمی را داشت.
3. تست ADR عبارت `not encrypted at rest` را به‌عنوان افشای محدودیت baseline جست‌وجو می‌کرد.

A003 دو assertion را به factory جدید منتقل کرد و ADR را با حفظ عبارت تاریخی، بدون ادعای پذیرش نهایی SQLCipher، اصلاح کرد. نتیجهٔ این اصلاح هنوز در CI تأیید نشده است.

## آزمون‌های T0184

چهار instrumentation test جدید هنوز در CI اجرا/تأیید نشده‌اند. هیچ نتیجه‌ای برای آن‌ها در `tests_passed` ثبت نشده است. Container run `41686907102` موفق شد؛ Product run `41686906946` در زمان ثبت این گزارش در حال اجرا بود.

**وضعیت:** `in_progress`؛ شکست‌های regression test مشخص و اصلاح شده‌اند، ولی اجرای مجدد و پذیرش runtime باقی مانده است.
