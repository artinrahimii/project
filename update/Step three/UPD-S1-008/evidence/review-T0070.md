# بازبینی محتوایی T0070 — اتصال صفحهٔ کاتالوگ به فایل واقعی

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`، `UPD-S1-024`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/MainWindow.xaml` — `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `src/Host.Windows/Models/Catalog/OfflineModelCatalogStore.cs` — `tree hash not captured`
- `src/Host.Windows/Models/Catalog/ModelCatalogFileLoader.cs` — `2422dfc6584343a973af853ea83c7751d8e1b234`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs` — `ca9c4965f5cbf0fe8d40892a401b7e5657340412`
- `tests/test_model_catalog_ui.py` — `f2805cf663d09646aed932650eb745b7e9cde996`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`

## یافته‌ها
- WPF LoadCatalogButton مسیر OfflineModelCatalogStore را اجرا می‌کند؛ store فایل محلی را با ModelCatalogFileLoader/Parser اعتبارسنجی و cache معتبر offline را دوباره parse می‌کند. CatalogItems.ItemsSource از catalog.Models واقعی تغذیه و جزئیات source/license/size/checksum/backend نمایش داده می‌شود.
- SelectionChanged مقدار ActiveCatalogModelId را فقط به عنوان preference ذخیره می‌کند؛ کد صریحاً می‌گوید مدل را download/install یا در inference backend activate نمی‌کند. ConnectButton_Click هنوز ModelId.Text دستی لازم دارد و GeneratePromptAsync مقدار ثابت 1024 را می‌فرستد.
- UI contract tests اتصال list به catalog.Models، details، loader، strict UTF-8/byte limit، cache validation و backend allowlist را بررسی می‌کنند؛ HostLifecycle parser/loader acceptance نمونهٔ کاتالوگ را از disk می‌خواند. این صفحه metadata/catalog است، نه auto model selection.

## علت بازبینی مجدد
- UPD-S1-023 باید preference را به مدل واقعاً نصب‌شده/ready، backend resolver و max-output clamp وصل کند؛ UPD-S1-024 نیز UI صفحات مهم میزبان را از نظر هویت بصری و کنتراست تغییر می‌دهد. پس از هر دو باید re-review شود.

## شواهد و محدودیت CI
- Exact V3.1.79 Quality: [run 42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) passed.
- Exact V3.1.79 Rendezvous: [run 42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) passed.
- Product build [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) was still in progress during this review.
- Historical task-specific tests/builds are cited with their exact earlier runs in the legacy execution reports; they are not current V3.1.79 results.
- No physical Android-to-Windows E2E or installation/upgrade test was performed in this review.
