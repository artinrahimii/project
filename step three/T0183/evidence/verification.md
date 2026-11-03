# T0183-A002 تا A004 — شواهد lint و اصلاح

## snapshot
- مخزن/شاخه: `artinrahimii/project` / `main`
- snapshot قبل از اصلاح: `0be00fb2f36f1a69fff8af31acc4339a76c7eb8e` (`V 3.0.848`)
- commit اصلاح main source: `6af58b498c536db0ae6788099e44195ff1ce7809` (`V 3.0.849`)

## خطاهای واقعی
- run `41673696991`, Android job `126048506024`: `ktlintMainSourceSetCheck` در فایل `AndroidKeyStoreProtectedBlobProtector.kt` با هشت violation شکست خورد. A003 این ۸ مورد را از نظر قالب‌بندی اصلاح کرد.
- در run `41675040018`، مرحلهٔ main-source lint عبور کرد و سپس `ktlintTestSourceSetCheck` شکست خورد. فقط دو violation گزارش شد:
  - `AndroidPeerIdentityStoreTest.kt:3:1`
  - `AndroidPeerTrustStoreTest.kt:3:1`
  هر دو گزارش می‌گویند importها باید lexicographic باشند و گروه‌های import نباید با خط خالی از هم جدا شوند.
- اصلاح A004، importهای Android را پیش از com قرار می‌دهد و فاصلهٔ اضافی بین importها را حذف می‌کند؛ بدنهٔ آزمون‌ها بدون تغییر است.

## نتایج قطعی تا این snapshot
- run `41675040020`: Step Three quality gate موفق.
- run `41675040018`: HostLifecycle job `126053271139` موفق؛ Android lint در test source شکست؛ بنابراین APK/JVM/instrumentation این run هنوز موفق ثبت نمی‌شوند.
- در زمان نوشتن این شواهد، CI پس از اصلاح A004 هنوز اجرا نشده است.
