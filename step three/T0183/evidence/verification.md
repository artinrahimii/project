# T0183-A002 تا A007 — شواهد و اعتبارسنجی

## snapshot
- مخزن/شاخه: `artinrahimii/project` / `main`
- مبنا: `88639fc553295c9b8a10b71b5eade12e1143ded8` (`V 3.0.852`)
- workflow instrumentation اکنون از فرمان `gradle --no-daemon --project-dir src/Client.Android connectedDebugAndroidTest` استفاده می‌کند. این اصلاح A006 مانع اجرای Gradle از ریشهٔ مخزن می‌شود.

## تاریخچهٔ شکست و اصلاح
- V848 run `41673696991`: ۸ تخلف KtLint در main source.
- V849 run `41675040018`: ۲ تخلف import در test source.
- V850 run `41675745738`: ۲ تخلف KtLint در Builder chain.
- V851 run `41676304428`: KtLint، assembleDebug و JVM tests موفق؛ emulator boot شد ولی test step به‌علت اجرای `cd` در shell مستقل Gradle را از root اجرا کرد. پیام واقعی: `Directory '/home/runner/work/project/project' does not contain a Gradle build.` این instrumentation tests را اجرا نکرد؛ شکست test assertion نبود.
- V852 quality run `41677809950`: 350 آزمون؛ 349 success و یک failure در `tests/test_android_keystore_local_secrets.py:79`. علت، assertion قدیمی بود که به فرمان قبلی workflow وابسته مانده بود. A007 انتظار را به فرمان دارای `--project-dir src/Client.Android` به‌روزرسانی می‌کند.
- V852 container run `41677809985`: success. Product run `41677809961` هنگام ثبت این شاهد هنوز در حال اجرا بود.

## مرز ادعا
تا اجرای واقعی connectedDebugAndroidTest با پنج سناریو و مشاهدهٔ report، هیچ instrumentation test به‌عنوان موفق ثبت نمی‌شود. emulator hardware-backed TEE/StrongBox دستگاه فیزیکی را اثبات نمی‌کند.