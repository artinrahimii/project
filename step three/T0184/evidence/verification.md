# T0184-A001 تا A011 — شواهد پیاده‌سازی و CI

## snapshot دقیق V3.0.860
- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit: `7d34810154bf3637e3eeccb51de2df51e4e4b947`; پیام `V 3.0.860`; tree `15104846e2993e1342745d6c0b65c5552a4a71cd`.
- گراف: Blob `ecfebd4adf633274b8979ebadc3de8aefa5b9c04`؛ اندازهٔ GitHub و UTF-8 برابر `3378931` بایت؛ Git SHA-1 قالب blob تطبیق یافت؛ JSON کامل parse و round-trip شد.
- factory: Blob `3d33cfdeba365d64d72305490a9e43eb1aace559`؛ اندازهٔ GitHub و UTF-8 برابر `12408` بایت؛ Git SHA-1 قالب blob تطبیق یافت.

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
- V3.0.860 quality `41693397321`: ۳۵۳ آزمون Python موفق؛ graph guard با ۲۳۰ task/۴۲۲ edge، صفر خطا و یک هشدار قدیمی T0001؛ container `41693397637` موفق.
- V3.0.860 product `41693397531`: Windows WPF و HostLifecycle موفق؛ test-source lint موفق؛ main-source lint چهار خطای indentation در factory داشت و build/JVM/instrumentation اجرا نشدند.

## خطای دقیق V3.0.859
`EncryptedClientDatabaseFactory.kt:70:21 Unexpected newline before '.'` در `.openHelperFactory(openHelperFactory)`. پس از شکست lint، `assembleDebug`، `testDebugUnitTest` و `connectedDebugAndroidTest` اجرا نشدند.

## A010
فقط `)` و `.openHelperFactory(openHelperFactory)` در یک خط قرار گرفتند؛ منطق رمزنگاری، ساخت کلید، ذخیرهٔ envelope، SQLCipher export، integrity check و rename تغییر نکردند. اجرای CI A010 هنوز موجود نیست.

## V3.0.860 lint failure و A011
در job Android `126117502709`، KtLint چهار مورد را مشخص کرد:
- خط 194: تورفتگی ۲۰ به‌جای ۲۴ فاصله.
- خط 195: تورفتگی ۲۴ به‌جای ۲۸ فاصله.
- خط 196: تورفتگی ۲۰ به‌جای ۲۴ فاصله.
- خط 197: تورفتگی ۱۶ به‌جای ۲۰ فاصله.
A011 فقط همین چهار خط را در lambda خروجی SQLCipher چهار فاصله به داخل منتقل می‌کند. این اصلاح هنوز CI نشده است.

## چهار آزمون اختصاصی T0184 — بدون نتیجهٔ pass
1. ایجاد پایگاه تازهٔ رمز‌شده و بازیابی conversation/message پس از reopen.
2. مهاجرت پایگاه plaintext و حفظ conversation/message/draft.
3. نگهداری کلید database فقط به‌شکل envelope محافظت‌شده با Keystore.
4. fail-closed هنگام فقدان کلید و عدم تغییر hash فایل پایگاه.

## مرز ادعا
تا اجرای موفق چهار scenario روی exact commit، هیچ runtime acceptance مربوط به SQLCipher ثبت نمی‌شود. آزمون T0183 نیز TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند.

## V 3.0.861 — exact-head failure و اصلاح A012

- commit: `6325ae0097bf262c02aa298a389481dc12db1625`.
- Product run: `41695659213`; Android job: `126125365784`.
- KtLint: passed.
- `assembleDebug`: failed at Kotlin compilation in `EncryptedClientDatabaseFactory.kt:248-250`.
  - The four-argument call was resolved against a non-password overload; `ByteArray` was seen where `CursorFactory` was expected, followed by mismatched `Int` and `DatabaseErrorHandler` parameters.
  - Exact-head logs show no APK, JVM unit-test, or connected instrumentation execution after compilation failed.
- Windows WPF and HostLifecycle: passed.
- Step Three quality run `41695659074`: passed.
- Rendezvous container run `41695659104`: passed.
- API reference: SQLCipher Android tag `v4.20.0`, `SQLiteDatabase.openDatabase(String, byte[], CursorFactory, int, SQLiteDatabaseHook)`.
- A012 adds the explicit fifth argument `null` for `SQLiteDatabaseHook` to select that overload.
- A012 verification is **pending**; no SQLCipher runtime acceptance is claimed.

## V 3.0.862 — exact-head build and emulator bootstrap failure

- Commit: `d740c10032d488c6d698aa4d43e78f6e1458d8b2` (`V 3.0.862`).
- Product run: `41697319573`; Android job: `126131159871`.
- Android KtLint: passed.
- Android `assembleDebug`: passed.
- Android `testDebugUnitTest`: passed. Downloaded artifact `13263131610` (51,122 bytes) reports 196 tests, 0 failures, 0 ignored.
- KVM udev setup: passed.
- Instrumentation step: failed before Gradle `connectedDebugAndroidTest` started. Emulator log: `Not enough space to create userdata partition. Available: 5861.69 MB, need 7372.80 MB`; `emulator-5554` did not appear in ADB; final error `Timeout waiting for emulator to boot`.
- Instrumentation report directory was not created; the configured upload step found no files. No T0184 instrumentation test result is claimed.
- Windows WPF and HostLifecycle jobs: passed.
- Step Three quality run `41697319725`: passed; 353 Python tests, compileall, graph guard 230 tasks/422 edges, 0 errors, 1 existing T0001 metadata warning.
- Rendezvous container run `41697319454`: passed.
- A013 changes the emulator options to include `-partition-size 4096` and removes the already-used one-time .NET `bin`/`obj` fixture outputs before emulator startup, then records available disk with `df -h`.
- The workflow adjustment is pending exact-head CI. No SQLCipher instrumentation acceptance is claimed.
