# بازبینی محتوایی T0069 — parser fail-closed و URL allowlist کاتالوگ

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `da866157596ac17ff91eba21555e8821bc06878e` — `V 3.1.79`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs` — `ca9c4965f5cbf0fe8d40892a401b7e5657340412`
- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs` — `08f254b5506bd0e1ac0557c6b446acfb609eaf6d`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json` — `57a8beac9317c8767c4409d9ce16dd9ccde8c3ac`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`
- `tests/test_model_catalog_parser.py` — `6810a2d783b35197994f08e97012e96ec40b2816`

## یافته‌ها
- Parser سقف ۲MB/10,000 مدل دارد، duplicate properties و duplicate model IDs را رد می‌کند، format/backend و SHA-256 را validate و HTTPS URL را به DNS allowlist بدون credentials/port/fragment محدود می‌کند.
- License status/classification را fail-closed می‌گیرد؛ مدل بدون license را review_required/unknown می‌کند و open_source را بدون status verified + identifier + URL نمی‌پذیرد. Parser خودش network fetch ندارد.
- C# acceptance و Python source contracts invalid schema، duplicate ID/property، checksum نامعتبر، backend پشتیبانی‌نشده، URL ناامن، license ناموجود، oversize و allowlist خالی را پوشش می‌دهند؛ runs 40113006243 و 40113006228 موفق‌اند.

## علت بازبینی مجدد
- UPD-S1-023 باید parser output را به resolver deterministic و readiness واقعی متصل کند؛ parser فقط metadata را validate می‌کند و نصب یا فعال‌بودن مدل را ثابت نمی‌کند.

## شواهد و محدودیت CI
- Exact V3.1.79 Quality: [run 42502772895](https://github.com/artinrahimii/project/actions/runs/42502772895) passed.
- Exact V3.1.79 Rendezvous: [run 42502773013](https://github.com/artinrahimii/project/actions/runs/42502773013) passed.
- Product build [42502772903](https://github.com/artinrahimii/project/actions/runs/42502772903) was still in progress during this review.
- Historical task-specific tests/builds are cited with their exact earlier runs in the legacy execution reports; they are not current V3.1.79 results.
- No physical Android-to-Windows E2E or installation/upgrade test was performed in this review.
