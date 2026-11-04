# T0185 — بررسی مجوزهای Android و شواهد اجرا

## Snapshot و دریافت فایل‌ها

- مخزن/شاخه: `artinrahimii/project` / `main`.
- commit مبنا: `a2c5a33064c78744c1e8e7194ec9f9fa1d45ef19`؛ پیام `V 3.0.867`.
- Git Trees API برای tree `bd0a8402f7b38f8cad969c7632fd1ceb73bf861e` کامل بود؛ `truncated=false`.
- Blobهای فایل‌های تصمیم‌ساز با UTF-8 bytes و قالب Git Blob SHA-1 از طریق درخت همان commit تطبیق داده شدند:
  - `src/Client.Android/app/src/main/AndroidManifest.xml`: `df3fa237e07fdf2929270692b1643ff1082d313f`.
  - `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`: `7483f9d0e2bb448594a4daba30e21f8670d6e0bc`.
  - `src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt`: `376edde0a9f0cc00fc797a9961fc6e2d1f369677`.
  - `src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt`: `e32db4d192879f3d77cd46603d3c60ceb1b5c2db`.
  - `src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt`: `99f69fbadc0270aec5ed6464c3f4cbb383d2417f`.
  - `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`: `86c2c211fd73b64ac973ca9566d269c5fd0735fc`.
  - `.github/workflows/product-build.yml`: `f90aff07a86103da147b4e58159094b80f46e0af`.
  - `docs/security.md`: `86df99eac2fbe9c638ba81e91ed3136b578f26e9`.

## تصمیم مجوز و وابستگی‌ها

- تنها declaration منبع `android.permission.INTERNET` است.
- `RendezvousPeerDirectoryClient` از HTTP(S) برای دریافت metadata استفاده می‌کند؛ `Libp2pInferenceRequestTransport` ارتباط مستقیم TCP/Noise را برقرار می‌کند. هر دو به بازکردن سوکت شبکه نیاز دارند.
- `INTERNET` در Android با protection level برابر `normal` تعریف شده و در گروه install-time قرار دارد؛ نیازی به runtime permission prompt ندارد.
- جست‌وجوی مخزن برای declarationهای permission و APIهای درخواست runtime permission فقط همین declaration را در manifest پیدا کرد؛ موردی از `requestPermissions`، `checkSelfPermission` یا `ActivityResultContracts.RequestPermission` پیدا نشد.
- `ClientDatabase` از application context و `whitep-client.db` استفاده می‌کند؛ `EncryptedClientDatabaseFactory` آن را با SQLCipher و کلید محافظت‌شده در Android Keystore باز می‌کند. فایل در فضای خصوصی اپ است؛ مجوز shared/external storage لازم نیست.
- کد selection/dispatch در T0177 هیچ API مجوز/وضعیت شبکه‌ای مصرف نمی‌کند.

## بازبینی یال‌ها

- `T0184 → T0185`: از `unreviewed` به `sequencing_only` و `validated` تغییر یافت؛ خروجی ذخیره‌سازی خصوصی T0184 برای رد نیاز به storage permission مدرک زمینه‌ای است، اما هیچ قرارداد permission از آن مصرف نمی‌شود.
- `T0177 → T0185`: از `unreviewed` به `sequencing_only` و `validated` تغییر یافت؛ انتخاب میزبان و بودجه تلاش، خروجی موردنیاز permission minimization نیست.
- هر دو یال اصلی `blocks` و ترتیب اجرا حفظ شدند؛ تغییر فقط طبقه‌بندی و ثبت دلیل/شواهد است.

## پیاده‌سازی و اعتبارسنجی افزوده‌شده

- `tools/verify_android_permissions.py` می‌خواند source AndroidManifest.xml را parse می‌کند و خروجی `aapt2 dump permissions` را از APK ساخته‌شده استخراج می‌کند؛ مجموعهٔ مؤثر باید دقیقاً شامل `android.permission.INTERNET` باشد.
- `tests/test_android_minimum_permissions.py` هشت آزمون parser و policy دارد.
- `.github/workflows/product-build.yml` پس از `assembleDebug`، APK را با AAPT2 بررسی می‌کند؛ مجوزی که توسط manifest وابستگی وارد شده باشد نیز در این مرحله رد می‌شود.
- اجرای محلی harness اولیهٔ منطق policy/parser: ۷ passed. این اجرای محلی جایگزین GitHub Actions یا بازرسی APK نهایی نیست.
- **CI exact-head:** pending؛ تا مشاهدهٔ موفقیت مرحلهٔ AAPT2 و کل product workflow، T0185 در `in_progress` باقی می‌ماند.

## منابع رسمی Android

- Permission protection levels and install-time/runtime behavior: https://developer.android.com/guide/topics/permissions/overview
- `INTERNET` permission is normal and allows opening network sockets: https://developer.android.com/reference/android/Manifest.permission#INTERNET
- AAPT2 `dump permissions` inspects permissions extracted from the APK manifest: https://developer.android.com/tools/aapt2


## A002 — exact-head AAPT2 result

- Exact commit: f2883e4f07459e1dcb03d2c71971685ee28b7048; V 3.0.868; product workflow 41731636396.
- KtLint and assembleDebug passed.
- AAPT2 permission audit failed with the only unexpected entry: com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION.
- Android JVM and connected instrumentation steps were skipped as downstream steps; do not count them as successes.
- Windows WPF, HostLifecycle, quality gate and container workflows passed. Quality run 41731636477 completed 361 Python tests, compileall and graph/output validation; 230 nodes, 422 edges, zero errors, one pre-existing T0001 warning.
- Root cause checked against the AndroidX Core manifest maintained in the Android Open Source Project mirror: it declares com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION with protectionLevel=signature and a matching uses-permission. This mechanism protects dynamic receivers on older Android versions.
- Previous evidence correction: the source blob for InferenceAttemptDispatcherTest.kt is 86c2c211fd73b64ac973ca9566d269c5fd0735fc; an earlier evidence line contained a transcription typo.

## A003 — exact validation rules now implemented

- Source manifest request allowlist: INTERNET only.
- Packaged APK request allowlist: INTERNET plus exactly com.whitep.client.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION.
- The verifier invokes aapt2 dump permissions and aapt2 dump xmltree --file AndroidManifest.xml.
- The binary manifest must declare that exact application-scoped permission exactly once with numeric protection level 0x2 (signature).
- Any other packaged permission, any other custom permission declaration, a missing/duplicate declaration or protection level other than signature is rejected.
- applicationId is fixed to com.whitep.client and tested against app/build.gradle.kts.
- Added 13 focused tests, including expected AndroidX signature permission acceptance and rejection of ACCESS_NETWORK_STATE, arbitrary app-scoped permission, missing declaration and weak protection.
- A local parser fixture matching AOSP's AAPT2 xmltree attribute format passed. This is a parser fixture only, not a claim that the actual APK has passed the revised verifier.
- Exact-head V 3.0.869 product and quality workflows are pending. Keep T0185 in_progress until both confirm the output.



## A003 — نتیجهٔ دقیق CI کیفیت V 3.0.869

- Product workflow: 41734543287؛ در زمان ثبت این گزارش هنوز in_progress بود و Android job در assembleDebug قرار داشت. مرحلهٔ AAPT2 ممیزی permission هنوز اجرا نشده بود.
- Step Three quality gate: 41734543291 — success.
- کل آزمون‌های Python: 366 passed.
- هر 13 آزمون اختصاصی T0185 در همین اجرا passed شدند، شامل پذیرش دقیق AndroidX signature permission و رد مجوزهای دیگر/ضعیف‌تر.
- Python compileall موفق شد.
- Graph guard: 230 tasks / 422 edges، صفر خطا و یک هشدار قدیمی T0001.
- نتیجه: آزمون‌های کد verifier و graph-quality پذیرفته شده‌اند؛ این نتیجه جایگزین بررسی Manifest باینری APK نیست. T0185 همچنان in_progress باقی می‌ماند.



## A004 — exact-head AAPT2 declaration parser failure (V 3.0.869)

- Product run 41734543287 completed failure.
- KtLint and APK assembly passed.
- AAPT2 permission audit found the app-scoped permission request, but the parser reported that the matching permission declaration was not found.
- Android JVM and connected instrumentation were skipped after the failed audit.
- The Step Three quality gate 41734543291 passed 366 Python tests, including all 13 prior T0185 policy tests; compileall passed; graph guard showed 230 tasks / 422 edges, zero errors and one pre-existing T0001 warning.
- Diagnosis: the AAPT2 XML-tree parser matched only the short android: attribute prefix. AAPT2 can render Android attributes using the full namespace URI.

## A005 — both AAPT2 namespace representations

- Parser now supports the short android: prefix and full http://schemas.android.com/apk/res/android: prefix for both android:name and android:protectionLevel.
- The focused suite now contains 14 tests, with a distinct full-URI AAPT2 XML-tree fixture.
- Local fixture validation passed for both namespace forms, extracting the app-scoped permission name and signature protection level 0x2.
- The actual APK audit on V 3.0.871 has not run yet. Product run 41735036654 for V 3.0.870 was still active before this correction and is not counted as success.



## A006 — شواهد exact-head V 3.0.871 و اعتبارسنجی فایل‌ها

- شاخه: `main`؛ commit مورد آزمون `4f44de18e841bcf20e3096e80706e6f23b62206d`؛ tree همان commit: `678da0157659f3a14b02d5efca463c9e5283d9da`.
- فایل گراف `step three/project-graph.md` از Blob API با SHA `7468c269cf7efcf00ac91215b55a436c995d4af2` دریافت شد. متن ۳٬۲۹۰٬۴۱۰ نویسه و ۳٬۴۱۷٬۵۴۷ بایت UTF-8 بود؛ اندازهٔ tree نیز ۳٬۴۱۷٬۵۴۷ بایت بود. Git Blob SHA-1 محاسبه‌شده روی `blob <size>\0<bytes>` دقیقاً با Blob ID برابر شد. حصار Markdown حفظ و JSON داخلی کامل parse شد: ۲۳۰ node و ۴۲۲ edge.
- فایل Manifest منبع: `src/Client.Android/app/src/main/AndroidManifest.xml`؛ Blob `36db9e70d041f4c380557ec1212c100295bfce13`؛ تنها declaration منبع `android.permission.INTERNET` است.
- verifier: `tools/verify_android_permissions.py`؛ Blob `411bbada506f2d39aafea167fdc51282766de2ba`. آزمون‌ها: `tests/test_android_minimum_permissions.py`؛ Blob `8103c265f6e6e16f57b54f43ee13d986217b5246`. Workflow: `.github/workflows/product-build.yml`؛ Blob `cadaeb0e2dbb848b9ef5b41ddff423783d8dd756`.
- Product run `41735585097`, Android job `126263830821`: KtLint، `assembleDebug`، مرحلهٔ `Verify minimum permissions in merged Android APK`، `testDebugUnitTest` و `connectedDebugAndroidTest` همگی موفق شدند. لاگ AAPT2 خروجی دقیق دو permission و تأیید signature برای مجوز app-scoped را ثبت می‌کند.
- Instrumentation artifact `13279121914`: گزارش HTML خلاصه ۹ آزمون، صفر failure، صفر skipped؛ تمام ۴ آزمون دیتابیس رمزگذاری‌شده و ۵ آزمون ماندگاری Android Keystore موفق.
- Quality run `41735585127`, job `126263830327`: `Ran 367 tests ... OK`؛ compileall موفق؛ graph guard: ۲۳۰ task / ۴۲۲ edge، صفر error، یک هشدار قدیمی `UNMAPPED_DEPENDENCY` مربوط به T0001 که بدون تغییر حفظ شد.
- Product workflow همچنین Windows WPF، HostLifecycle و Rendezvous container build/smoke را با موفقیت تکمیل کرد.
- یال‌های `T0184 → T0185` و `T0177 → T0185` پیش‌تر با شواهد به `sequencing_only` / `validated` طبقه‌بندی شده بودند؛ نوع `blocks` و ترتیب اصلی حفظ شده است. یال `T0185 → T0186` همچنان `unreviewed` باقی مانده و برای شروع T0186 باید همراه با یال `T0177 → T0186` بازبینی شود.
- **نتیجه:** شواهد واقعی exact-head معیار پذیرش T0185 را تأیید می‌کند؛ وضعیت `done` ثبت شد. Blob IDs بالا از tree همان commit استخراج شده‌اند؛ اعتبارسنجی Blob گراف در همین نوبت به‌طور مستقل با SHA-1 Git انجام شد.
