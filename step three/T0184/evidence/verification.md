# T0184-A001 تا A010 — شواهد پیاده‌سازی و CI

## snapshot دقیق V3.0.859
- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit: `1e14ea896eaff3d8c24399156b8c79c7ba53308a`; پیام `V 3.0.859`; tree `da09f6f408009631257bea09e4bb8ecdedddaf36`.
- گراف: Blob `17e2f250d883769fb03450ac84d40712d960f88d`؛ اندازهٔ GitHub و UTF-8 برابر `3375671` بایت؛ Git SHA-1 قالب blob تطبیق یافت؛ JSON کامل parse و round-trip شد.
- factory: Blob `df158ad4ac2fbf5ae96ba57592d4b9c1f1886892`؛ اندازهٔ GitHub و UTF-8 برابر `12429` بایت؛ Git SHA-1 قالب blob تطبیق یافت.

## کد واقعی و آزمون‌ها
- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- regression tests مربوط به Room migration، encrypted storage و local storage boundary
- `docs/security.md` و ADR-0020.

## CI exact-head
- V3.0.858 quality `41689552284`: ۳۵۳ آزمون موفق؛ graph guard ۲۳۰ task و ۴۲۲ edge، صفر خطا و یک هشدار metadata قدیمی T0001.
- V3.0.858 container `41689552377`: build و non-root HTTP smoke موفق.
- V3.0.858 product `41689552411`: Windows WPF و HostLifecycle موفق؛ test-source lint موفق؛ main-source KtLint در factory سه ایراد گزارش کرد.
- V3.0.859 quality `41691220553`: ۳۵۳ آزمون Python موفق؛ compileall و graph/evidence validation موفق.
- V3.0.859 container `41691220755`: build و non-root HTTP smoke موفق.
- V3.0.859 product `41691220625`: Windows WPF و HostLifecycle موفق؛ Android test-source lint موفق؛ main-source KtLint یک خطا در line 70 factory گزارش کرد.

## خطای دقیق V3.0.859
`EncryptedClientDatabaseFactory.kt:70:21 Unexpected newline before '.'` در `.openHelperFactory(openHelperFactory)`. پس از شکست lint، `assembleDebug`، `testDebugUnitTest` و `connectedDebugAndroidTest` اجرا نشدند.

## A010
فقط `)` و `.openHelperFactory(openHelperFactory)` در یک خط قرار گرفتند؛ منطق رمزنگاری، ساخت کلید، ذخیرهٔ envelope، SQLCipher export، integrity check و rename تغییر نکردند. اجرای CI A010 هنوز موجود نیست.

## چهار آزمون اختصاصی T0184 — بدون نتیجهٔ pass
1. ایجاد پایگاه تازهٔ رمز‌شده و بازیابی conversation/message پس از reopen.
2. مهاجرت پایگاه plaintext و حفظ conversation/message/draft.
3. نگهداری کلید database فقط به‌شکل envelope محافظت‌شده با Keystore.
4. fail-closed هنگام فقدان کلید و عدم تغییر hash فایل پایگاه.

## مرز ادعا
تا اجرای موفق چهار scenario روی exact commit، هیچ runtime acceptance مربوط به SQLCipher ثبت نمی‌شود. آزمون T0183 نیز TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند.
