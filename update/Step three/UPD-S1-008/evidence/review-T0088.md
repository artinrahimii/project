# بازبینی محتوایی T0088 — صفحهٔ گفت‌وگوی محلی میزبان

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `08ba01194adc2acc37593a2586eb79104c7dd650` — `V 3.1.81`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-010`، `UPD-S1-024`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/MainWindow.xaml` — `202f3d0ea24bca658eb41271118ebf14e74dcc6b`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/test_wpf_entrypoint.py` — `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`

## یافته‌ها
- MainWindow.xaml دارای پیام‌های چت، ورودی متن و کنترل ارسال است؛ MainWindow.xaml.cs مسیر ارسال را به HostInferenceSession متصل می‌کند و پیام user/assistant را جداگانه در UI نمایش می‌دهد.
- تاریخچهٔ T0088، build و startup smoke واقعی WPF، HostLifecycle و Android/Protobuf build را در Product run 40179954088 و Quality run 40179954129 روی source commit وظیفه ثبت کرده است.
- یال T0087→T0088 از نوع sequencing_only/validated و T0088→T0089 از نوع contract_or_interface_dependency/validated است؛ rationale/evidence قبلی حفظ می‌شوند.

## علت بازبینی مجدد
- این task صفحهٔ چت میزبان Windows را پوشش می‌دهد، نه چت Android و Room. UPD-S1-010 چرخهٔ Android را به runtime واقعی متصل و UPD-S1-024 هویت بصری صفحات را تغییر می‌دهد؛ پس از هر دو باید صفحه دوباره بازبینی شود.

## یال‌های گراف قدیمی که حفظ شدند
- T0087 → T0088: sequencing_only/validated
- T0088 → T0089: contract_or_interface_dependency/validated
- هیچ یال، status قدیمی یا تاریخچه‌ای در این بازبینی تغییر نکرد.

## شواهد CI دقیق V3.1.81
- Quality Gate: [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) — passed.
- Rendezvous container build/smoke: [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) — passed.
- HostLifecycle job: [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) — passed; 67 PASS markers, one opt-in real-model SKIP, 85.88% line coverage.
- Windows/Android Product Build jobs were still in progress at review time.
- Historical task-specific runs are attributed to their original code revisions; no physical Android-to-Windows E2E, Room E2E, clean install or in-place upgrade was performed in this review.
