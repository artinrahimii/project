# T0184-A001 تا A008 — شواهد پیاده‌سازی و CI

## snapshot منبع
- مخزن/شاخه: `artinrahimii/project` / `main`.
- آخرین commit بررسی‌شده: `361ac9d1468602486b09256fc4c0c3419a30d046` — `V 3.0.857`.
- گراف: Blob `c212247ceecef4df84fdd6e0dab72323ecaa53aa`، `3369922` بایت UTF-8؛ اندازهٔ اعلام‌شدهٔ GitHub `3369922` بایت؛ Git Blob SHA-1 تطبیق دارد و JSON کامل parse و round-trip شد.

## فایل‌های پیاده‌سازی و آزمون
- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- آزمون‌های contract مربوط به migration، encrypted storage، capsule persistence و local storage boundary
- `docs/security.md` و ADR-0020.

## CI دقیق
- V3.0.854 quality `41686907108`: ۳۵۰ از ۳۵۳؛ سه شکست قرارداد قدیمی.
- V3.0.855 quality `41687478161`: ۳۵۳ آزمون موفق؛ container `41687478388` موفق؛ Android product به KtLint نرسید.
- V3.0.856 quality `41688070305`: ۳۵۳ آزمون موفق؛ container `41688070068` موفق؛ KtLint Android یک import و هشت chain error گزارش کرد.
- V3.0.857 quality `41688934109`: ۳۵۳ آزمون موفق؛ graph guard ۲۳۰ task و ۴۲۲ edge با صفر خطا و یک هشدار قدیمی T0001؛ container `41688934150` موفق. Windows WPF و HostLifecycle product موفق؛ Android KtLint فقط یک import-ordering error داشت.

## اصلاح‌های قالب‌بندی
- A005: انتقال alias import به انتهای فهرست و اصلاح اولیهٔ style.
- A007: اصلاح تمام chain-continuation diagnostics؛ در CI V857 دیگر chain error گزارش نشد.
- A008: انتقال `kotlinx.coroutines` به گروه عمومیِ الفبایی قبل از `org.junit` مطابق `ktlint_official`. اجرای A008 هنوز CI ندارد.

## چهار scenario اختصاصی instrumentation — بدون نتیجهٔ pass
1. پایگاه تازهٔ رمز‌شده و بازیابی conversation/message پس از reopen.
2. مهاجرت پایگاه plaintext و حفظ conversation/message/draft.
3. نگهداری کلید پایگاه فقط به‌شکل envelope محافظت‌شده با Keystore.
4. fail-closed با کلید مفقود و عدم تغییر hash فایل encrypted DB.

## مرز ادعا
تا اجرای موفق چهار scenario روی exact commit، هیچ runtime acceptance مربوط به SQLCipher ثبت نمی‌شود. تست Android Keystore در T0183، TEE/StrongBox دستگاه فیزیکی را نیز اثبات نمی‌کند.
