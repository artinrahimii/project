# بازبینی محتوایی T0063 — لغو inference و توقف process اختصاصی

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `bc9882a8ad1fcd29b6a2e42e31509a76e926f2f6` — `V 3.1.78`
- وضعیت پایهٔ legacy: `done`؛ حفظ شد.
- update_task_ids: `UPD-S1-023`
- decision: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Inference/LocalInferenceProcessLauncher.cs` — `10ba72549cb8cd94e5d116813ce0554fdcfc13a1`
- `src/Host.Windows/MainWindow.xaml.cs` — `e2b2c3285f48fd8883b1781f5d98590eb6b77184`
- `tests/HostLifecycle.Tests/Program.cs` — `e63132c55538e289e5f49840943e35349e758052`
- `.github/workflows/real-inference.yml` — `6c3a6aa183c24c5ad3493d7c89f8bd291a4bddc4`
- `step three/T0063/execution-report.md` — `7f21c57f1dab12c7da9ac842a09c7902cbc0ed09`

## یافته‌های فنی
- MainWindow مالک LocalInferenceProcessLauncher است و token عملیات generation را به HostInferenceSession می‌دهد؛ در مسیر managed local engine امکان توقف process اختصاصی دارد.
- آزمون opt-in واقعی generation دوم را شروع می‌کند، in-flight ماندن پس از 150ms را الزام، token را cancel و OperationCanceledException را بررسی می‌کند؛ سپس process متعلق به test را stop و endpoint را unavailable می‌سنجد.
- تاریخچهٔ T0063-A004 exact commit 694b0fcf847a9a2087af292910112c9c041d86df را با active generation cancellation و توقف تمیز process گزارش می‌کند؛ jobهای Windows/Android/quality همان commit نیز موفق بوده‌اند.

## علت بازبینی مجدد
- این پذیرش تاریخی مربوط به managed local inference است. پس از تغییر catalog/backend composition در UPD-S1-023 باید cancellation و process ownership دوباره آزموده شوند.

## شواهد و محدودیت CI
- Exact V3.1.78 Quality: [run 42502412081](https://github.com/artinrahimii/project/actions/runs/42502412081) passed.
- Exact V3.1.78 Rendezvous: [run 42502412120](https://github.com/artinrahimii/project/actions/runs/42502412120) passed.
- Product build [42502412102](https://github.com/artinrahimii/project/actions/runs/42502412102) was still in progress during this review.
- Historical live local-model evidence: run 40077575618 (T0061), 40078502151 (T0062), and 40092245411 (T0063/T0064) on older exact implementation commits; these are not V3.1.78 results.
- T0065 is a source-level provider boundary check, not packet capture or runtime egress proof.
- No physical Android-to-Windows E2E or install/upgrade test was performed in this review.
