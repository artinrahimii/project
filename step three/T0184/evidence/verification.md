# T0184-A001 تا A007 — شواهد پیاده‌سازی و CI

## snapshot منبع
- مخزن/شاخه: `artinrahimii/project` / `main`.
- آخرین commit بررسی‌شده: `5a0e424c391dfd2f3665bf8d84ae462188287e58` — `V 3.0.856`.
- گراف در همان commit: Blob `67b25eee8f518a730b259d0f2b6e1539899a68c1`، `3366362` بایت UTF-8؛ اندازهٔ اعلام‌شدهٔ GitHub نیز `3366362` بایت و Git Blob SHA-1 تطبیق دارد؛ JSON کامل parse و round-trip شد.

## فایل‌های پیاده‌سازی و آزمون
- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- آزمون‌های contract مربوط به migration، encrypted storage، capsule persistence و local storage boundary
- `docs/security.md` و ADR-0020.

## CI دقیق
- V3.0.854 quality `41686907108`: ۳۵۰ از ۳۵۳؛ سه شکست قرارداد قدیمی.
- V3.0.855 quality `41687478161`: ۳۵۳ آزمون موفق؛ graph guard با ۲۳۰ task/۴۲۲ edge و صفر خطا.
- V3.0.855 container `41687478388`: موفق.
- V3.0.855 product `41687478163`: Android KtLint شکست؛ Android build و test اجرا نشد.
- V3.0.856 quality `41688070305`: ۳۵۳ آزمون موفق؛ compileall و evidence checks موفق.
- V3.0.856 container `41688070068`: موفق.
- V3.0.856 product `41688070251`: Windows WPF و HostLifecycle موفق؛ Android job `126099017248` به KtLint رسید و شکست خورد.

## خطای دقیق V3.0.856
KtLint یک import-order diagnostic و هشت chain-continuation diagnostic در فایل instrumentation اعلام کرد. به همین علت ساخت APK، JVM tests و چهار connected instrumentation tests skipped شدند. A007 importها را مطابق `ktlint_official` مرتب می‌کند و زنجیره‌های method را با operatorهای هم‌تراز بازنویسی می‌کند. A007 هنوز CI تأییدکننده ندارد.

## سناریوهای instrumentation ـ هنوز بدون نتیجهٔ pass
1. پایگاه تازه رمز‌شده و بازیابی conversation/message پس از reopen.
2. مهاجرت پایگاه plaintext و حفظ conversation/message/draft.
3. نگهداری کلید پایگاه فقط به‌شکل envelope محافظت‌شده با Keystore.
4. fail-closed با کلید مفقود و عدم تغییر hash فایل encrypted DB.

## مرز ادعا
تا اجرای موفق چهار scenario روی exact commit، هیچ runtime acceptance مربوط به SQLCipher ثبت نمی‌شود. تست Android Keystore در T0183، TEE/StrongBox دستگاه فیزیکی را هم اثبات نمی‌کند.
