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

## V 3.0.863 — workflow rejected before jobs

- Commit: `d9a2889743f26571fa80fea9b0d4d7be54519575` (`V 3.0.863`).
- GitHub Actions check for `.github/workflows/product-build.yml`: failed; no workflow jobs were created.
- Commit diff shows the new cleanup step was not indented as a child of the Android `steps` list (`shell` and `run` were at root indentation).
- A014 fixes YAML indentation for the cleanup step and restores the emulator-runner step as a sibling. The `-partition-size 4096` setting remains.
- Exact-head validation is pending; no app/test pass is claimed from V 3.0.863.

## V 3.0.864 — workflow dispatch after indentation correction

- Commit: `3f41808e3995110b746f8c538e1bc5fc585f195f` (`V 3.0.864`).
- Product run `41700515325` created Android, Windows WPF, and HostLifecycle jobs; the YAML/workflow dispatch failure from V 3.0.863 no longer occurred.
- HostLifecycle completed successfully. Android KtLint passed; Android `assembleDebug` was still running at the verification checkpoint. The four instrumentation tests had not started.

## A015 — correct Android userdata disk sizing

- The exact pinned `reactivecircus/android-emulator-runner` action commit `a421e43855164a8197daf9d8d40fe71c6996bb0d` declares a `disk-size` input. Its `src/emulator-manager.ts` maps that input to `disk.dataPartition.size=<value>`.
- Replaced the ineffective emulator CLI `-partition-size 4096` option with `disk-size: 4096M` under the action's `with` block.
- Preserved safe cleanup of the already-consumed `tests/HostLifecycle.Tests/bin` and `obj` outputs before AVD creation and a `df -h "$HOME"` diagnostic.
- Static source contract review: passed. Exact-head GitHub Actions, AVD boot and SQLCipher instrumentation tests: pending. No runtime acceptance is claimed.


## V 3.0.865 — اجرای واقعی instrumentation و شکست مهاجرت plaintext

- Commit: `e2e13532785b7f02ada7864e0eed9cb84dfee97e` (`V 3.0.865`); product run `41701047926`; Android job `126144093268`.
- Emulator با تنظیم `disk-size: 4096M` boot شد؛ `sys.boot_completed=1`. بنابراین مانع ساخت userdata قبلی برطرف شده است.
- Android KtLint: passed. `assembleDebug`: passed. `testDebugUnitTest`: passed. Windows WPF و HostLifecycle: passed. Step Three quality gate run `41701047886` و Rendezvous container run `41701048006`: passed.
- `connectedDebugAndroidTest` نه تست را شروع کرد. آزمون `legacyPlaintextRoomDatabaseIsExportedAndExistingRowsArePreserved` شکست خورد: `SQLiteCantOpenDatabaseException` هنگام `ATTACH DATABASE` برای `t0184-legacy-plaintext.db.sqlcipher-migrating`، کد ۱۴. سایر سناریوها را بدون گزارش جزئی جداگانه، pass مستقل اعلام نمی‌کنیم.
- بررسی سورس SQLCipher Android `v4.20.0` وجود `CREATE_IF_NECESSARY` را تأیید می‌کند. upstream issue #259 هم کاربرد این flag برای ساخت فایل مقصد ATTACH را توصیف می‌کند.

## A016 — اصلاح flag ایجاد فایل مقصد

- در `EncryptedClientDatabaseFactory.kt`، بازکردن منبع plaintext از `OPEN_READWRITE` به ترکیب `OPEN_READWRITE or CREATE_IF_NECESSARY` تغییر یافت تا SQLCipher اجازهٔ ایجاد فایل مقصد ATTACH را داشته باشد.
- این اصلاح بر پایهٔ شکست واقعی exact-head V 3.0.865 است؛ هنوز runtime CI روی commit اصلاح‌شده اجرا نشده است.
- **مرز ادعا:** T0184 همچنان `in_progress` است. هیچ‌کدام از چهار معیار instrumentation اختصاصی T0184 به‌عنوان گروه کامل پذیرفته نشده‌اند تا exact-head بعدی همه را سبز کند.
