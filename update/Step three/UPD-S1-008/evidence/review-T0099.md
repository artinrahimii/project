# بازبینی محتوایی T0099 — رد fail-fast درخواست دوم با وضعیت Busy

## خط مبنا و تصمیم
- شاخه: `main`
- commit بررسی‌شده: `08ba01194adc2acc37593a2586eb79104c7dd650` — `V 3.1.81`
- وضعیت پایهٔ legacy: `done`؛ بدون تغییر
- update_task_ids: `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- `status_changed=false`

## فایل‌ها و Blob SHA
- `src/Host.Windows/Requests/HostRequestCoordinator.cs` — `a06529ed485c4f48352caf7f87c5f76397948131`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs` — `320a366c0e3888ce4cfc275c507fc6e2a944ce18`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs` — `7e238d0b86b6d9d69024ed92575b7c3089ac6454`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs` — `acedd7fd3064dc11933005e31cd9ae6aae4ba5f2`

## یافته‌ها
- در حالت slot اشغال، Coordinator نتیجهٔ Busy را فوراً برمی‌گرداند؛ callback درخواست دوم اجرا و وارد صف نمی‌شود. Cancellation token حفظ می‌شود و پس از آزادشدن slot درخواست جدید پذیرفته می‌شود.
- HostRequestCoordinatorBusyTests درخواست‌های هم‌زمان را بدون queue بررسی می‌کند؛ HostRequestAdmissionTests مدل not-ready/mismatch، capacity Unknown/Insufficient و deadline expired را پیش از inference رد می‌کنند.
- تاریخچهٔ T0099 Product run 40309351792 و Quality run 40309351849 را موفق ثبت کرده؛ revalidation admission context در V3.0.313 run 40312672878 نیز پیام PASS دارد.
- یال‌های T0098→T0099 و T0099→T0100 از نوع hard_dependency/validated و T0097→T0099 از نوع sequencing_only/validated باقی می‌مانند.

## علت بازبینی مجدد
- UPD-S1-013 و UPD-S1-023 هنوز سیاست ظرفیت و مدل/backend را تغییر می‌دهند؛ پس از اتصال آن‌ها Busy/admission/cancellation باید دوباره بازآزمایی شود.

## یال‌های گراف قدیمی که حفظ شدند
- T0097 → T0099: sequencing_only/validated
- T0098 → T0099: hard_dependency/validated
- T0099 → T0100: hard_dependency/validated
- هیچ یال، status قدیمی یا تاریخچه‌ای در این بازبینی تغییر نکرد.

## شواهد CI دقیق V3.1.81
- Quality Gate: [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) — passed.
- Rendezvous container build/smoke: [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) — passed.
- HostLifecycle job: [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) — passed; 67 PASS markers, one opt-in real-model SKIP, 85.88% line coverage.
- Windows/Android Product Build jobs were still in progress at review time.
- Historical task-specific runs are attributed to their original code revisions; no physical Android-to-Windows E2E, Room E2E, clean install or in-place upgrade was performed in this review.
