# بازبینی محتوایی T0071 — جزئیات مجوز، منبع، اندازه و محدودیت مدل

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `5cba6051583091d8fbfb2cc849a2aa812c983d53` — `V 3.1.80`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-023`، `UPD-S1-024`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/MainWindow.xaml` — `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `src/Host.Windows/Models/Catalog/ModelCatalogModels.cs` — `08f254b5506bd0e1ac0557c6b446acfb609eaf6d`
- `tests/test_model_catalog_ui.py` — `f2805cf663d09646aed932650eb745b7e9cde996`

## یافته‌های کد و آزمون
- پنل جزئیات WPF پس از انتخاب ModelCatalogEntry واقعی، نام/شناسه، فرمت، اندازه، URL منبع، وضعیت و شناسهٔ license، URL مجوز، backendهای پشتیبانی‌شده، checksum و محدودیت‌های استفاده را نمایش می‌دهد.
- لیبل مجوز بین review_required، restricted و ادعای open_source تمایز می‌گذارد؛ حتی وضعیت verified را تأیید مستقل حقوقی جلوه نمی‌دهد. UI و Python contract test وجود این فیلدها و عدم overclaim را assert می‌کنند.
- تاریخچهٔ exact source commit 33f6ada3bddda0aad99c5322b2960454be0afc35، Quality run 40115099003 و Windows Product run 40115099007 را موفق ثبت کرده است؛ هیچ model download یا inference در این task ادعا نشده.

## علت بازبینی مجدد
- UPD-S1-023 اتصال کاتالوگ به انتخاب مدل/backend فعال را تغییر می‌دهد و UPD-S1-024 UI میزبان را از نظر palette/icon/contrast بازطراحی می‌کند؛ پس از هر دو باید جزئیات واقعی و rendered UI دوباره بررسی شوند.

## شواهد CI دقیق V3.1.80
- Quality Gate: [run 42502980246](https://github.com/artinrahimii/project/actions/runs/42502980246) — passed.
- Rendezvous container build/smoke: [run 42502980194](https://github.com/artinrahimii/project/actions/runs/42502980194) — passed.
- HostLifecycle job: [run 42502980229](https://github.com/artinrahimii/project/actions/runs/42502980229) — passed; 67 PASS markers, 85.89% line coverage, one optional real-model integration SKIP.
- Product jobs for Windows and Android were still in progress during this review; no full Product Build success is claimed.
- Historical exact task-specific runs are described above and in the old task report; they are not current V3.1.80 results.
- No physical Android-to-Windows E2E, clean install, in-place upgrade, actual model download/activation, or target-device visual contrast review was performed in this attempt.
