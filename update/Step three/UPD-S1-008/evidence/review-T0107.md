# بازبینی محتوایی T0107

## خط مبنا و تصمیم
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit کد بررسی‌شده: `63ae554809f1d1bc6217b0ad48632f416f3db0f2` — `V 3.1.82`
- وضعیت پایه: `done`؛ حفظ شد؛ `status_changed=false`
- وظایف Update مرتبط: `UPD-S1-004`، `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-017`، `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- review_commit_sha: `63ae554809f1d1bc6217b0ad48632f416f3db0f2`

## یافته‌های کد و آزمون
Failure acceptance covers repeated active admission returning Busy without invoking a second callback, expiry before execution, in-flight cancellation including a late callback return and inference exception without successful late delivery. Peer lifecycle cancellation correlation and replay protection are separate layers. Exact CI records the duplicate/deadline/cancel/failure PASS marker.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Requests/HostRequestCoordinator.cs`
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorFailureAcceptanceTests.cs`
- `tests/HostLifecycle.Tests/HostRequestCoordinatorTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0106 -> T0107 sequencing_only/validated
- T0097 -> T0107 sequencing_only/validated
- T0107 -> T0108/T0109/T0110/T0111/T0113/T0114/T0115 sequencing_only/validated
- T0107 -> T0112 contract_or_interface_dependency/validated
- هیچ یال، وضعیت پایه یا تاریخچهٔ قدیمی در این بازبینی تغییر نکرد.

## CI دقیق
- Product Build: [run 42503829586](https://github.com/artinrahimii/project/actions/runs/42503829586) — موفق؛ jobهای Android، Windows WPF و HostLifecycle.
- Step Three Quality Gate: [run 42503829570](https://github.com/artinrahimii/project/actions/runs/42503829570) — موفق.
- HostLifecycle: ۶۷ PASS، پوشش خطی ۸۵٫۸۹٪؛ یک آزمون real-model اختیاری SKIP شد.

## دلیل بازبینی مجدد
UPD-S1-004/017 affect request/response/streaming and transport contracts; UPD-S1-005/013/023 affect retries/capacity/model. Run the full failure matrix on the final Android-to-Windows path.

## مرز شواهد
این بازبینی، کد و آزمون CI را ثبت می‌کند؛ اثبات اجرای فیزیکی Android-to-Windows، inference مدل واقعی یا clean install/in-place upgrade محسوب نمی‌شود. نشانگر `update_impact.review_status=pending` در گراف اولیه تا بسته‌شدن کل reconciliation حفظ می‌شود؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.
