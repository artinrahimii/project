# T0183-A002 / A003 — شواهد و اعتبارسنجی

## snapshot

- مخزن/شاخه: `artinrahimii/project` / `main`
- commit: `0be00fb2f36f1a69fff8af31acc4339a76c7eb8e` (`V 3.0.848`)
- tree: `af57a771242c307a49418e17f4013932309a1b1b`
- مسیر گراف: `step three/project-graph.md`
- Blob گراف در همان tree: `b69a152faf7e1676a998cfd11a776997ef3b7c45` با اندازهٔ tree برابر 3,351,159 بایت.
- Blob API، 3,351,159 بایت UTF-8 و 3,224,186 نویسه برگرداند. SHA-1 Git از `blob 3351159\0` به‌علاوهٔ بایت‌های کامل محاسبه شد و با Blob ID در tree تطبیق داشت. JSON fenced به‌طور کامل parse شد: 230 node، 422 edge و بدون شناسهٔ تکراری یا ارجاع یال حل‌نشده.
- وضعیت گراف: 181 `done`، یک `in_progress`، 46 `not_started` و 2 `deferred_final`. T0183 در حال اجراست.

## شواهد CI

- run `41673697033`: Step Three quality gate موفق.
- run `41673696995`: Rendezvous container build and smoke موفق.
- run `41673696991`: White-P product build ناموفق.
- Windows WPF و HostLifecycle در product run موفق شدند.
- Android job `126048506024` در `ktlintMainSourceSetCheck` شکست خورد. هشت پیام واقعی KtLint در مسیر `src/Client.Android/app/src/main/java/com/whitep/client/security/AndroidKeyStoreProtectedBlobProtector.kt`:
  - line 16: blank line لازم بین دو اعلان
  - line 23: بازکردن و بستن پرانتز constructor باید چندخطی باشد (دو پیام)
  - line 84: expression تابع `additionalData` می‌تواند در همان خط امضا قرار بگیرد
  - line 91: expression چندخطی باید از خط بعد از `=` شروع شود
  - line 96: expression builder باید بعد از `=` در خط بعد آغاز و زنجیرهٔ dot به‌صورت چندخطی قالب‌بندی شود (دو پیام)
  - line 113: expression چندخطی باید از خط بعد از `=` آغاز شود
- به‌علت fail شدن lint، هیچ ادعایی دربارهٔ اجرای آزمون‌های Android یا ساخت APK برای این revision نمی‌شود.

## اصلاح

- Blob قبلی فایل Kotlin: `ec178640411d47a423bea380ea91bcb03d0fe6e3`
- Blob اصلاح‌شده: `dc41cc53a34de942e8bfdffca3c1acb968d908ea`
- اصلاح فقط whitespace/line wrapping و فاصلهٔ بین اعلان‌ها است؛ الگوریتم AES/GCM، اندازه‌ها، nonce، AAD، alias و قرارداد interface بدون تغییر باقی مانده‌اند.
- A002 به‌عنوان شکست واقعی KtLint حفظ می‌شود. A003 تا اجرای workflow دقیق commit اصلاحی در وضعیت partial می‌ماند.
