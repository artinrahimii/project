# بازبینی محتوایی T0211 — ساخت، نصب و launch برنامهٔ Android

- مبنای بررسی: `a51627faafaa9b2c4c5299d81a465fa5ed61ffb6` — `V 3.1.106`
- وضعیت پایه: `done`؛ بدون تغییر.
- UPDهای مرتبط: `UPD-S1-021`, `UPD-S1-024`
- تصمیم: `re_review_required`؛ وضعیت بازبینی: `reviewed`.
- دلیل بازبینی مجدد آتی: بستهٔ نهایی و راهنمای Android در UPD-S1-021 و آیکون/هویت بصری در UPD-S1-024 باید روی خروجی نهایی بررسی شوند.

## کد و آزمون بررسی‌شده
- `.github/workflows/product-build.yml` — Blob `64fd1fb9695ae0aeb593cdb5b1499a9786c16f2a`
- `src/Client.Android/app/build.gradle.kts`
- `src/Client.Android/app/src/main/AndroidManifest.xml`
- `src/Client.Android/app/src/main/java/com/whitep/client/MainActivity.kt`
- `tests/test_reproducible_build_contract.py` — Blob `6662d35bf99118658bd54bae4c5bfabe5ba79ff4`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidKeyStorePersistenceTest.kt` — Blob `bc31ba216e5158cfa9c4693e527b401e1bb91439`

## شواهد اجرا روی HEAD جاری
- Product Build: https://github.com/artinrahimii/project/actions/runs/42516118989
- Android job: https://github.com/artinrahimii/project/actions/runs/42516118989/job/128839399364
- lint، clean debug APK، release signing smoke با کلید موقت، permission checks، JVM tests، Keystore instrumentation و coverage validation موفق شدند.
- 11 instrumentation test شروع شد؛ تست non-loopback LAN با نام `AndroidWindowsLanIntegrationTest.connectsToWindowsHostOverNonLoopbackLan` صریحاً `SKIPPED` شد.
- APK روی Emulator نصب شد؛ Package Manager launcher را resolve کرد؛ MainActivity اجرا شد؛ PID برابر 4319 و `topResumedActivity=com.whitep.client/.MainActivity` ثبت شدند.
- Artifact `whitep-android-debug-apk` با شناسهٔ 13631911820 و اندازهٔ ZIP برابر 28,979,404 بایت آپلود شد؛ گزارش‌های unit/instrumentation/coverage نیز بارگذاری شدند.

## محدودیت قطعی
این شواهد نصب و launch روی Emulator را اثبات می‌کند، نه ارتباط فیزیکی Android↔Windows، inference مدل روی Windows یا ذخیرهٔ پاسخ در Room پس از یک درخواست واقعی. تست LAN مربوطه skipped بود.

## نتیجه
ساخت، نصب و launch APK روی Emulator با شواهد exact-head تأیید شد. وضعیت پایهٔ `done` حفظ شد؛ بازبینی نهایی پس از UPD-S1-021 و UPD-S1-024 لازم است.
