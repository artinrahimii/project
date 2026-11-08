# بازبینی محتوایی T0100

## خط مبنا و تصمیم
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit کد بررسی‌شده: `63ae554809f1d1bc6217b0ad48632f416f3db0f2` — `V 3.1.82`
- وضعیت پایه: `done`؛ حفظ شد؛ `status_changed=false`
- وظایف Update مرتبط: `UPD-S1-005`، `UPD-S1-013`، `UPD-S1-023`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- review_commit_sha: `63ae554809f1d1bc6217b0ad48632f416f3db0f2`

## یافته‌های کد و آزمون
MainWindow.HandlePeerInferenceMessageAsync builds HostRequestAdmissionContext from the active peer service, session state, requested/configured model IDs, capacity readiness and deadline, then calls the production coordinator before GenerateAsync. The policy rejects inactive participation, non-Ready/mismatched model, unknown/insufficient capacity and invalid/expired deadline; the coordinator rechecks admission before inference. Exact CI records the admission PASS marker.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Requests/HostRequestAdmissionPolicy.cs`
- `src/Host.Windows/Requests/HostRequestCoordinator.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/HostRequestAdmissionTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0099 -> T0100 hard_dependency/validated
- T0097 -> T0100 sequencing_only/validated
- T0100 -> T0101 sequencing_only/validated
- هیچ یال، وضعیت پایه یا تاریخچهٔ قدیمی در این بازبینی تغییر نکرد.

## CI دقیق
- Product Build: [run 42503829586](https://github.com/artinrahimii/project/actions/runs/42503829586) — موفق؛ jobهای Android، Windows WPF و HostLifecycle.
- Step Three Quality Gate: [run 42503829570](https://github.com/artinrahimii/project/actions/runs/42503829570) — موفق.
- HostLifecycle: ۶۷ PASS، پوشش خطی ۸۵٫۸۹٪؛ یک آزمون real-model اختیاری SKIP شد.

## دلیل بازبینی مجدد
UPD-S1-013 changes resource admission and UPD-S1-023 changes model/backend resolution; rerun admission against the final measured resource/model snapshot.

## مرز شواهد
این بازبینی، کد و آزمون CI را ثبت می‌کند؛ اثبات اجرای فیزیکی Android-to-Windows، inference مدل واقعی یا clean install/in-place upgrade محسوب نمی‌شود. نشانگر `update_impact.review_status=pending` در گراف اولیه تا بسته‌شدن کل reconciliation حفظ می‌شود؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.
