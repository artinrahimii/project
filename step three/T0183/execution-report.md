# T0183 — استفاده از Android Keystore برای اسرار و کلیدهای محلی

- **وضعیت:** `in_progress`؛ منتظر اجرای دقیق CI و آزمون instrumentation روی Android emulator.
- **شاخه:** `main`
- **commit مبنا:** `0f1089e1890c24846ac9ebf324609951645dd962` — `V 3.0.843`
- **تلاش:** `T0183-A001`
- **زمان:** `2026-11-03`

## پیش‌نیازها و یال‌ها

- `T0182` در graph برابر `done` است؛ گزارش و خروجی واقعی Windows conversation store و آزمون پاک‌سازی orphan temp بررسی شدند. T0182 فقط چرخهٔ عمر فایل تاریخچهٔ میزبان Windows را پاک‌سازی می‌کند و خروجی آن توسط Android Keystore مصرف نمی‌شود. یال `T0182 → T0183` به `sequencing_only` اعتبارسنجی می‌شود؛ `type=blocks` و ترتیب محافظه‌کارانه حفظ می‌شوند.
- `T0177` در graph برابر `done` است؛ گزارش، selector و dispatcher اندروید بررسی شدند. انتخاب میزبان و بودجهٔ تلاش درخواست در مسیر ساخت/محافظت کلید محلی مصرف نمی‌شوند. یال `T0177 → T0183` نیز `sequencing_only` می‌شود؛ `type=blocks` و ترتیب حفظ می‌شوند.
- خروجی مشترک این task، قرارداد موردنیاز T0184 برای wrap کردن کلید رمزگذاری دیتابیس است. یال `T0183 → T0184` به‌عنوان `contract_or_interface_dependency` ثبت می‌شود؛ helper قابل‌استفادهٔ مجدد `ProtectedBlobProtector` و پیاده‌سازی Android Keystore در همین task ایجاد می‌شود و T0184 باید از آن برای حفاظت از کلید دیتابیس استفاده کند.

## پیاده‌سازی

- منطق حفاظت AES-GCM از فایل identity به مسیر مستقل `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt` منتقل و به interface مشترک `ProtectedBlobProtector` تبدیل شد.
- هر purpose یک alias مستقل دارد: `com.whitep.client.peer_identity.v1` و `com.whitep.client.peer_trust.v1`. کلید AES-256 به‌صورت non-exportable در Android Keystore ایجاد و بازیابی می‌شود؛ entry اشتباه، الگوریتم نادرست یا کلید exportable رد می‌شود.
- envelope دارای نسخه، nonce تصادفی ۹۶ بیتی و tag احراز اصالت ۱۲۸ بیتی است. نسخه و alias نیز در AAD قرار دارند. اندازهٔ داده و envelope محدود است.
- مسیر تولید هویت و فهرست اعتماد از helper مشترک استفاده می‌کنند؛ فرمت رکوردهای فعلی و نام SharedPreferences حفظ شده است. خطای decrypt، envelope خراب یا کلید گمشده fail-closed می‌ماند و هویت قبلی rotate نمی‌شود.
- آزمون JVM قبلی با protector تستی حفظ شد. پنج آزمون instrumentation جدید، provider واقعی AndroidKeyStore را برای پایداری، non-exportability، جداسازی alias، حذف کلید و tamper rejection بررسی می‌کنند.
- workflow محصول، اجرای `connectedDebugAndroidTest` روی Android API 36 emulator و artifact گزارش instrumentation را اضافه می‌کند.
- ADR-0021 و security docs مرز اطمینان و محدودیت emulator را توضیح می‌دهند.

## فایل‌های تغییر‌یافته

- `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerIdentityStore.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/network/AndroidPeerTrustStore.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerIdentityStoreTest.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/network/AndroidPeerTrustStoreTest.kt`
- `src/Client.Android/app/src/androidTest/java/com/whitep/client/network/AndroidKeyStorePersistenceTest.kt`
- `src/Client.Android/app/build.gradle.kts`
- `.github/workflows/product-build.yml`
- `tests/test_android_keystore_local_secrets.py`
- `tests/test_android_libp2p_transport_contract.py`
- `tests/test_decision_traceability.py`
- `docs/decisions/ADR-0021-android-libp2p-peer-transport.md`
- `docs/decisions/README.md`
- `docs/security.md`
- `step three/T0183/execution-report.md`
- `step three/T0183/evidence/verification.md`
- `step three/project-graph.md`

## آزمون و محدودیت پذیرش

- پنج سناریوی instrumentation به workflow محصول اضافه شده‌اند؛ اجرای آن‌ها هنوز در این گزارش موفق اعلام نشده است.
- آزمون‌های JVM فعلی همچنان fake-protector هستند و جایگزین instrumentation واقعی نیستند.
- API 36 emulator فقط provider integration و non-exportability قابل‌مشاهده از API را نشان می‌دهد؛ TEE/StrongBox روی دستگاه فیزیکی را اثبات نمی‌کند.
- Room database encryption، migration و حفاظت کلید دیتابیس در T0184 باقی می‌مانند. T0183 به‌تنهایی هیچ ادعایی دربارهٔ encryption-at-rest پایگاه Room ندارد.
- تا سبز شدن quality gate، build APK، Android instrumentation، Windows WPF و HostLifecycle دقیق commit، وضعیت T0183 `in_progress` است.
