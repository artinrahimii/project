# بازبینی محتوایی T0214 — UPD-S1-008

## مبنا و وضعیت
- مخزن/شاخه: `artinrahimii/project` / `main`
- commit دقیق بررسی: `fe489b7f141a9a969f550b851093e3fcf45c8a19` — `V 3.1.67`
- وظایف به‌روزرسانی مرتبط: `UPD-S1-008` و `UPD-S1-021`
- وضعیت پایه و فعلی T0214: `done`؛ هیچ تغییر وضعیتی انجام نشد.
- وابستگی‌های قدیمی T0214: `T0208` و `T0213`؛ هر دو در گراف قدیمی `done` هستند.
- تصمیم reconciliation: `re_review_required`؛ پس از تکمیل UPD-S1-021 باید مستندات، بسته‌ها و آزمون ارتقا دوباره بررسی شوند.

## بررسی README، ابزارها و مصرف‌کنندگان build
- فرمان‌های README برای .NET restore/build/lifecycle/publish، Android lint/assembleDebug و آزمون‌های Python با workflowهای واقعی تطبیق داده شدند.
- `tests/test_readme_build_commands.py` فرمان‌های README را با workflowهای محصول و کیفیت تطبیق می‌دهد.
- `tests/test_reproducible_build_contract.py` نسخه‌های ابزار، ورودی‌های Android SDK، فرمان‌های build، artifactهای واقعی، installer ویندوز و راه‌اندازی MainActivity در emulator را بررسی می‌کند.
- `.github/workflows/product-build.yml` از checkout تمیز restore/clean/build/publish واقعی Windows، تولید installer، smoke test، ساخت APK، آزمون‌های JVM و instrumentation، تولید کد Protobuf و upload artifactها را اجرا می‌کند.
- `.github/workflows/quality.yml` آزمون‌های Python، compileall و هر دو graph guard را اجرا می‌کند.
- `README.md`، دو آزمون بالا و workflowهای جاری در commit بررسی‌شده با فرمان‌های build واقعی هماهنگ بودند؛ مسیرهای ساخت تکراری یا فرمان غیرموجود در دامنهٔ T0214 مشاهده نشد.

## شواهد build و package در CI
- Product build run [42482672544](https://github.com/artinrahimii/project/actions/runs/42482672544) روی commit V3.1.61 با موفقیت کامل شد؛ jobهای Windows، Android و HostLifecycle همگی موفق بودند.
- خروجی‌های artifact همان run:
  - `whitep-windows-installer` — artifact ID `13618361311`، اندازهٔ آرشیو `60,988,953` بایت، digest آرشیو GitHub `sha256:2a1426a2633cec22d2bd3b2c2f9438d3d773d8d89b25362202a5b1b12cc52b4f`.
  - `whitep-android-debug-apk` — artifact ID `13618342255`، اندازهٔ آرشیو `28,979,403` بایت، digest آرشیو GitHub `sha256:8c25ca78ee1be06bd1ab0070500935d6cd1f862c8bfc6b66bda26d35a9d62847`.
  - `whitep-windows-host` — artifact ID `13617964208`، اندازهٔ آرشیو `101,573,061` بایت، digest آرشیو GitHub `sha256:a2909d8b7dd8f36c00b2b0f31af5bbd18cdf40120855d87f3aed28d6c0bcad34`.
  - `whitep-windows-publish` — artifact ID `13617734842`، اندازهٔ آرشیو `10,023,222` بایت، digest آرشیو GitHub `sha256:011773872ff5950a8ef61cdc55a577fafbff800c4f90d95a601c97ca51e40046`.
- digestهای بالا متعلق به آرشیوهای artifact بارگذاری‌شده در GitHub هستند؛ آن‌ها را checksum مستقل فایل درون APK یا installer معرفی نمی‌کنم.
- Quality workflow دقیق V3.1.67، run [42484306209](https://github.com/artinrahimii/project/actions/runs/42484306209)، موفق بود و آزمون‌های README/build-contract و graph guardها را در همین commit اجرا کرد.
- Product build دقیق V3.1.67، run [42484306205](https://github.com/artinrahimii/project/actions/runs/42484306205)، هنوز `in_progress` بود؛ نتیجهٔ نهایی آن ادعا نمی‌شود.

## تصمیم و محدودیت پذیرش
- T0214 در محدودهٔ اجرای فرمان‌های README از clean checkout و تولید/آزمون artifactهای CI، معیار تاریخی خود را پاس می‌کند؛ وضعیت پایهٔ `done` حفظ شد.
- تصمیم `re_review_required` لازم است، چون `UPD-S1-021` بسته‌های نهایی، checksum محصول، مجوزهای وابستگی، راهنمای نصب هماهنگ و آزمون ارتقای درجا را به دامنه می‌افزاید.
- CI نشان‌دهندهٔ build و تست در runner و emulator است؛ نصب فیزیکی تمیز روی دستگاه کاربر و ارتقای in-place اندروید/ویندوز با حفظ هویت، تنظیمات و داده‌ها در این بازبینی اجرا نشده‌اند.
- `review_status=reviewed` نشان‌دهندهٔ بازبینی فعلی است؛ `decision=re_review_required` الزام بازبینی مجدد پس از اجرای UPD-S1-021 را نگه می‌دارد.
