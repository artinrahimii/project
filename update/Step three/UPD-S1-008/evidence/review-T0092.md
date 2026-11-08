# بازبینی محتوایی T0092 — نمایش خطا به‌صورت پیام system، نه پاسخ موفق مدل

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `08ba01194adc2acc37593a2586eb79104c7dd650` — `V 3.1.81`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-010`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Application/HostErrorPresentation.cs` — `24a02d41725659e1143d14f9d4a3e3e2297d54a6`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/HostLifecycle.Tests/Program.cs` — `tree SHA not captured`
- `tests/test_wpf_entrypoint.py` — `55a9892c1ad76e1e3ad935e064465dcc6f1cee2d`

## یافته‌ها
- HostErrorPresentation دسته‌های خطا و متن امن کاربر را جدا می‌کند؛ در مسیر Generate، خطا یا لغو به پیام System می‌رود و به‌عنوان Assistant response نمایش داده نمی‌شود.
- Regression tests در تاریخچهٔ T0092 مسیر خطا را از پاسخ موفق جدا می‌کنند؛ Quality run 40232583373 و Product run 40232583374 برای کد/آزمون مبنا موفق ثبت شده‌اند.
- یال T0091→T0092 از نوع contract_or_interface_dependency/validated و یال‌های T0087→T0092 و T0092→T0093 از نوع sequencing_only/validated هستند.

## علت بازبینی مجدد
- این شواهد مربوط به WPF میزبان است. UPD-S1-010 باید typed error mapping قابل‌فهم در Android را بسازد و retry/cancel/disconnect را بدون نمایش خطا به‌صورت پاسخ موفق مدیریت کند؛ سپس بازبینی مجدد لازم است.

## یال‌های گراف قدیمی که حفظ شدند
- T0087 → T0092: sequencing_only/validated
- T0091 → T0092: contract_or_interface_dependency/validated
- T0092 → T0093: sequencing_only/validated
- هیچ یال، status قدیمی یا تاریخچه‌ای در این بازبینی تغییر نکرد.

## شواهد CI دقیق V3.1.81
- Quality Gate: [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) — passed.
- Rendezvous container build/smoke: [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) — passed.
- HostLifecycle job: [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) — passed; 67 PASS markers, one opt-in real-model SKIP, 85.88% line coverage.
- Windows/Android Product Build jobs were still in progress at review time.
- Historical task-specific runs are attributed to their original code revisions; no physical Android-to-Windows E2E, Room E2E, clean install or in-place upgrade was performed in this review.
