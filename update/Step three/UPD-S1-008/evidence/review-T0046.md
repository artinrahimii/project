# بازبینی محتوایی T0046 — نمایش وضعیت واقعی میزبان

## خط مبنا و تصمیم
- commit منبع کد بررسی‌شده: `c933559b3be973f1cdf21e094b786c84742ca7ab` — `V 3.1.74`.
- وضعیت پایهٔ T0046: `done`.
- وظایف جدید مرتبط: `UPD-S1-012` و `UPD-S1-024`.
- تصمیم: `re_review_required`؛ وضعیت قدیمی تغییر نمی‌کند.

## بررسی پیاده‌سازی
- `HostStatusPresentation.FormatBackendStatus` وضعیت‌های Stopped/Starting/Ready/Busy/Error را مستقیماً از `HostStatus` قالب‌بندی می‌کند.
- `FormatNetworkStatus` endpoint محلی را از وضعیت مشاهده‌شدهٔ backend تفکیک می‌کند و صریحاً اعلام می‌کند که direct peer-to-peer transport در این build پیاده‌سازی نشده است؛ وضعیت backend را به‌عنوان P2P Online معرفی نمی‌کند.
- `MainWindow.RenderStatus` از وضعیت واقعی `_session.Status` و ارزیابی ظرفیت استفاده می‌کند. دکمهٔ Generate فقط وقتی فعال می‌شود که state برابر Ready و بررسی ظرفیت موفق باشد.
- متن UI اعلام می‌کند که وضعیت endpoint آخرین وضعیت مشاهده‌شده است؛ polling دائمی یا سلامت شبکهٔ P2P ادعا نمی‌شود.
- Blobهای بررسی‌شده: `HostStatusPresentation.cs` = `b18cf674cf5ade8bbf1e98695d5c9e6d8649ddab`؛ `MainWindow.xaml` = `202f3d0ea24bca658eb41271118ebf14e74dcc6b`؛ `MainWindow.xaml.cs` = `e2b2c3285f48fd8883b1781f5d98590eb6b77184`.

## آزمون و CI
- `tests/test_wpf_entrypoint.py` شامل آزمون‌های `test_status_panel_uses_live_backend_state_and_discloses_p2p_limit` و `test_window_status_starts_without_connecting_to_a_backend` است.
- Product CI run [42484937793](https://github.com/artinrahimii/project/actions/runs/42484937793) روی Blobهای یکسان code-behind و XAML، Windows WPF build و startup smoke را موفق اجرا کرد؛ HostLifecycle و Android APK نیز در همان run موفق شدند.
- Quality Gate دقیق V3.1.74 در run [42486010109](https://github.com/artinrahimii/project/actions/runs/42486010109) موفق شد.
- Product run جاری V3.1.74 [42486010134](https://github.com/artinrahimii/project/actions/runs/42486010134) هنگام آخرین مشاهده `in_progress` بود؛ نتیجهٔ نهایی آن ادعا نمی‌شود.

## وابستگی‌ها و مصرف‌کنندگان
- `T0045 → T0046`: `contract_or_interface_dependency/validated`؛ panel تنظیمات server/model ذخیره‌شده و اعتبارسنجی‌شده را مصرف می‌کند.
- `T0040 → T0046`: `sequencing_only/validated`؛ panel از HostStatus جلسهٔ backend استفاده می‌کند و خروجی parser پروتکل را مصرف نمی‌کند.
- فایل‌های XAML و code-behind مصرف‌کنندگان مستقیم وضعیت و کنترل‌های میزبان‌اند.

## چرا بازبینی مجدد لازم است؟
`UPD-S1-012` کنترل مشارکت و وضعیت‌های تازهٔ میزبان را تغییر می‌دهد؛ `UPD-S1-024` نیز رنگ‌ها، آیکون و صفحات اصلی میزبان و Android را عوض می‌کند. پس از اجرای هر دو، وضعیت‌های disabled/ready/busy/error، ظرفیت، کنتراست و عناصر رابط در اندازهٔ واقعی باید بازآزمایی شوند.

## خارج از محدودهٔ شواهد
هیچ ارتباط live P2P یا آزمون Android-to-Windows، inference مدل واقعی، نمایش پاسخ در Android، Room persistence، نصب فیزیکی یا ارتقای درجا در این بازبینی انجام نشده است.
