# بازبینی محتوایی T0073 — آزمون parser، checksum، backend و URLهای ناامن

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`
- `tests/test_model_catalog_parser.py` — `6810a2d783b35197994f08e97012e96ec40b2816`
- `tests/test_model_catalog_schema.py` — `tree hash not captured`
- `src/Host.Windows/Models/Catalog/ModelCatalogParser.cs` — `ca9c4965f5cbf0fe8d40892a401b7e5657340412`
- `src/Host.Windows/Models/Catalog/model-catalog-v1.schema.json` — `57a8beac9317c8767c4409d9ce16dd9ccde8c3ac`

## یافته‌های کد و آزمون
- C# acceptance و Python source tests ورودی معتبر/نامعتبر، checksum اشتباه، backend و format پشتیبانی‌نشده، duplicate model ID و JSON property، URL غیر HTTPS/خارج allowlist، license ناموجود/نامشخص و محدودیت اندازه/تعداد را پوشش می‌دهند.
- parser 2MB و سقف 10,000 entry دارد، duplicate properties را رد و allowlist source/license را از trusted configuration می‌گیرد؛ parser خودش HTTP fetch یا Process.Start نمی‌کند.
- Exact V3.0.227 Quality run40122299470 و HostLifecycle parser harness run40122299496 job120956288798 به‌عنوان evidence تاریخی ثبت شده‌اند؛ آن‌ها اجرای parser tests هستند، نه آزمون auto-selection مدل در UI.

## علت بازبینی مجدد
- UPD-S1-023 resolver مدل و سقف خروجی را اضافه می‌کند؛ آزمون‌ها باید بعد از اتصال واقعی catalog -> installed model -> ready backend گسترش یابند.

## شواهد CI دقیق V3.1.80
- Quality Gate: [run 42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) — passed.
- Rendezvous container build/smoke: [run 42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) — passed.
- HostLifecycle job: [run 42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) — passed; 67 PASS markers, 85.89% line coverage, one optional real-model integration SKIP.
- Product jobs for Windows and Android were still in progress during this review; no full Product Build success is claimed.
- Historical exact task-specific runs are described above and in the old task report; they are not current V3.1.80 results.
- No physical Android-to-Windows E2E, clean install, in-place upgrade, actual model download/activation, or target-device visual contrast review was performed in this attempt.
