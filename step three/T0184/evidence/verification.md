# T0184-A001 — شواهد پیاده‌سازی و وضعیت اعتبارسنجی

## snapshot گراف

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit پایه: `9cf5e9b858f975b605c952ead3c4010fa78a9d11`
- tree پایه: `3518804be7a98ddac216830980e181ab9512945c`
- Blob گراف: `b69a152faf7e1676a998cfd11a776997ef3b7c45`؛ ۳٬۳۵۱٬۱۵۹ بایت UTF-8؛ تطبیق SHA-1 قالب Git blob موفق و JSON کامل parse شد.

## شواهد معماری و کد

- Room پیش از این از open-helper استاندارد SQLite استفاده می‌کرد و ADR-0020 رمزگذاری at-rest را انجام‌نشده می‌دانست.
- T0183 در `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt` پیاده‌سازی Keystore را ایجاد کرده است؛ پنج آزمون instrumentation آن روی emulator API 36 در product run `41678108221` موفق شدند.
- T0184 از SQLCipher `4.20.0` و `SupportOpenHelperFactory` استفاده می‌کند و Room از مسیر factory جدید باز می‌شود.
- کلید تصادفی پایگاه فقط به‌صورت envelope Keystore-protected نگهداری می‌شود؛ هیچ passphrase خامی به source یا preferences نوشته نمی‌شود.
- مهاجرت plaintext با checkpoint و export فایل جداگانه انجام می‌شود و قبل از rename اتمی، integrity مقصد بررسی می‌شود.

## فایل‌های خروجی T0184

- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- `tests/test_android_database_migration_contract.py`
- `tests/test_android_encrypted_storage_contract.py`
- `docs/security.md`
- `docs/decisions/ADR-0020-android-client-local-conversation-storage.md`
- `step three/T0184/execution-report.md`
- `step three/T0184/evidence/verification.md`

## نتایج آزمون

کدهای T0184 در محیط محلی Gradle اجرا نشده‌اند. چهار instrumentation test و دو Python contract test به مخزن اضافه می‌شوند؛ اجرای آن‌ها روی commit نهایی هنوز **انجام‌نشده** است. بنابراین `tests_passed` در گراف T0184 خالی می‌ماند و وضعیت `in_progress` است.

## موارد ریسک برای CI

- سازگاری نسخهٔ SQLCipher با API مربوط به Room و emulator.
- اجرای واقعی ATTACH/export/checkpoint روی فایل SQLite.
- اثبات حفظ schema/data در مهاجرت و fail-closed بودن کلید مفقود.
- اجرای KtLint و بررسی اینکه workflow instrumentation این چهار سناریو را همراه سایر تست‌ها اجرا کند.
