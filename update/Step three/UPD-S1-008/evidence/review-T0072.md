# بازبینی محتوایی T0072 — هشدار واضح برای مجوز ناموجود یا نامعتبر

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs` — `08f254b5506bd0e1ac0557c6b446acfb609eaf6d`
- `src/Host.Windows/MainWindow.xaml` — `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `tests/HostLifecycle.Tests/ModelCatalogParserAcceptanceTests.cs` — `a89672b4a75531b034dd48c319def651a58b8801`
- `tests/test_model_catalog_ui.py` — `f2805cf663d09646aed932650eb745b7e9cde996`

## یافته‌های کد و آزمون
- ModelLicenseMetadata.RequiresReview برای statusهای review_required/unknown و classification unknown true است؛ ReviewLabel برای missing/unknown هشدار می‌دهد و برای open_source اعلام می‌کند این فقط ادعای کاتالوگ است، نه بررسی حقوقی مستقل.
- یک ستون Review state در ListView وضعیت را بدون انتخاب سطر نیز نشان می‌دهد؛ acceptance در C# و Python مسیر missing/unknown/declared verified را بررسی می‌کند تا UI مجوز را overclaim نکند.
- تاریخچه exact commit 3ee8fffe1624c17addfb9e8f07744d4c6a7e18d1، HostLifecycle run40122299496 job120956288798، WPF job120956288933 و real-inference run40122299478 را موفق ثبت کرده است.

## علت بازبینی مجدد
- UPD-S1-023 سیاست catalog/model activation را تغییر می‌دهد؛ هشدار مجوز باید روی مدل انتخابی واقعی، مسیر update/download و هر نمایش جدید دوباره بررسی شود. این برچسب خودش اجازهٔ استفاده یا redistribution نیست.

## شواهد CI دقیق V3.1.80
- Quality Gate: [run 42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) — passed.
- Rendezvous container build/smoke: [run 42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) — passed.
- HostLifecycle job: [run 42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) — passed; 67 PASS markers, 85.89% line coverage, one optional real-model integration SKIP.
- Product jobs for Windows and Android were still in progress during this review; no full Product Build success is claimed.
- Historical exact task-specific runs are described above and in the old task report; they are not current V3.1.80 results.
- No physical Android-to-Windows E2E, clean install, in-place upgrade, actual model download/activation, or target-device visual contrast review was performed in this attempt.
