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
  - `src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt`: `86c2c211fd73b64ac973ca9567b5bfc5fd0735fc`.
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
