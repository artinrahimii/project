# T0184-A001 تا A009 — شواهد پیاده‌سازی و CI

## snapshot
- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit بررسی‌شده: `e7affffd1c05736338c2caf0f1149baaec38b207` — `V 3.0.858`.
- گراف: Blob `2cbe8897a7f991ebe24e4ecd5e8825fb55e0caa5`، 3372375 بایت UTF-8؛ اندازهٔ اعلام‌شدهٔ GitHub 3372375 بایت؛ Git SHA-1 قالب blob تطبیق یافت و JSON کامل parse و round-trip شد.

## فایل‌های کد و آزمون
- `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt`
- regression tests برای migration، encrypted storage، capsule persistence و local storage boundary
- `docs/security.md` و ADR-0020

## شواهد CI
- V3.0.854 quality `41686907108`: ۳۵۰/۳۵۳؛ سه شکست در assertionهای قدیمی.
- V3.0.855 quality `41687478161`: ۳۵۳ آزمون موفق؛ container `41687478388` موفق؛ Android product در lint متوقف شد.
- V3.0.856 quality `41688070305`: ۳۵۳ آزمون موفق؛ container `41688070068` موفق؛ test-source lint با خطای import/chain.
- V3.0.857 quality `41688934109`: ۳۵۳ آزمون موفق؛ container `41688934150` موفق؛ Windows و HostLifecycle موفق؛ Android lint با یک خطای import.
- V3.0.858 quality `41689552284`: ۳۵۳ آزمون موفق؛ graph guard ۲۳۰ task/۴۲۲ edge، صفر خطا و یک هشدار metadata قدیمی T0001.
- V3.0.858 container `41689552377`: build و non-root HTTP smoke موفق.
- V3.0.858 product `41689552411`: Windows WPF و HostLifecycle موفق؛ فایل test source از KtLint عبور کرد؛ main source lint روی factory با سه خطا شکست خورد.

## A009 — اصلاح factory
1. import دارای alias به انتهای فهرست importها منتقل شد.
2. `Room.databaseBuilder` به خط continuation منتقل و زنجیرهٔ Room به فراخوانی‌های یک‌خطی/چندخطی استاندارد تفکیک شد.
3. `source.rawQuery` به خط continuation منتقل شد.

هیچ تغییر runtime در الگوریتم encryption/export/key handling در این patch وجود ندارد؛ این اصلاح برای عبور formatter است. نتیجهٔ CI A009 در زمان ثبت این گزارش هنوز موجود نیست.

## چهار آزمون اختصاصی SQLCipher — هنوز بدون نتیجهٔ pass
1. ایجاد پایگاه تازهٔ رمز‌شده و بازیابی مکالمه/پیام پس از reopen.
2. مهاجرت پایگاه plaintext و حفظ conversation/message/draft.
3. ذخیرهٔ کلید پایگاه فقط به‌صورت envelope محافظت‌شده با Keystore.
4. fail-closed با کلید مفقود و عدم تغییر hash فایل پایگاه.

## مرز ادعا
تا اجرای موفق چهار سناریو روی exact commit، هیچ پذیرش runtime مربوط به SQLCipher ثبت نمی‌شود. آزمون Keystore در T0183 نیز سخت‌افزار TEE/StrongBox گوشی فیزیکی را اثبات نمی‌کند.
