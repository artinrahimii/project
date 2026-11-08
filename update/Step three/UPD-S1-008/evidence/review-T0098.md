# بازبینی محتوایی T0098 — هماهنگ‌کنندهٔ درخواست با semaphore تک‌ظرفیتی

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
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs` — `04540b9be9a1aa8f89446bb35941931bf7180f59`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs` — `320a366c0e3888ce4cfc275c507fc6e2a944ce18`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorBusyTests.cs` — `7e238d0b86b6d9d69024ed92575b7c3089ac6454`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs` — `acedd7fd3064dc11933005e31cd9ae6aae4ba5f2`

## یافته‌ها
- HostRequestCoordinator با SemaphoreSlim(1,1) و WaitAsync(0) پذیرش غیرصفی را اعمال می‌کند؛ slot پس از success/failure/cancel آزاد می‌شود و درخواست هم‌زمان رد می‌شود.
- HostRequestAdmissionPolicy پیش از inference، participation، Ready state، model match، capacity state و deadline را fail-closed بررسی می‌کند.
- HostLifecycle روی exact V3.1.81 پیام PASS صریح دارد که از 64 درخواست هم‌زمان دقیقاً یک مورد پذیرفته و 63 مورد Busy شده‌اند؛ busy regressionها صف را رد و آزادشدن slot را کنترل می‌کنند.
- یال T0097→T0098 از نوع sequencing_only/validated و T0098→T0099 از نوع hard_dependency/validated است.

## علت بازبینی مجدد
- UPD-S1-013 هنوز قیود نهایی منابع و UPD-S1-023 انتخاب مدل/backend را کامل نکرده‌اند؛ admission و ظرفیت باید پس از اعمال این دو وظیفه دوباره روی مسیر نهایی آزموده شوند.

## یال‌های گراف قدیمی که حفظ شدند
- T0097 → T0098: sequencing_only/validated
- T0098 → T0099: hard_dependency/validated
- هیچ یال، status قدیمی یا تاریخچه‌ای در این بازبینی تغییر نکرد.

## شواهد CI دقیق V3.1.81
- Quality Gate: [42503265782](https://github.com/artinrahimii/project/actions/runs/42503265782) — passed.
- Rendezvous container build/smoke: [42503265712](https://github.com/artinrahimii/project/actions/runs/42503265712) — passed.
- HostLifecycle job: [42503265704](https://github.com/artinrahimii/project/actions/runs/42503265704) — passed; 67 PASS markers, one opt-in real-model SKIP, 85.88% line coverage.
- Windows/Android Product Build jobs were still in progress at review time.
- Historical task-specific runs are attributed to their original code revisions; no physical Android-to-Windows E2E, Room E2E, clean install or in-place upgrade was performed in this review.
