# بازبینی محتوایی T0108

## خط مبنا و تصمیم
- مخزن: `artinrahimii/project`
- شاخه: `main`
- commit کد بررسی‌شده: `63ae554809f1d1bc6217b0ad48632f416f3db0f2` — `V 3.1.82`
- وضعیت پایه: `done`؛ حفظ شد؛ `status_changed=false`
- وظایف Update مرتبط: `UPD-S1-012`، `UPD-S1-017`
- تصمیم: `re_review_required`
- review_status: `reviewed`
- review_commit_sha: `63ae554809f1d1bc6217b0ad48632f416f3db0f2`

## یافته‌های کد و آزمون
ParticipationGatedNetworkListener checks its predicate before binding; inactive startup does not bind and active startup binds and exposes the endpoint. The actual WPF host starts PeerAnnouncementHostLifecycle only after explicit toggle, backend/capacity readiness, privacy consent and peer trust. The separate TCP listener fixture is not the same object as the production peer service. Exact CI records the inactive/active listener PASS marker.

## فایل‌های بررسی‌شده
- `src/Host.Windows/Networking/ParticipationGatedNetworkListener.cs`
- `src/Host.Windows/Networking/PeerAnnouncementHostLifecycle.cs`
- `src/Host.Windows/MainWindow.xaml.cs`
- `tests/HostLifecycle.Tests/HostNetworkListenerParticipationTests.cs`
- `tests/HostLifecycle.Tests/Program.cs`

## وابستگی‌ها
- T0107 -> T0108 sequencing_only/validated
- T0108 -> T0109 hard_dependency/validated
- هیچ یال، وضعیت پایه یا تاریخچهٔ قدیمی در این بازبینی تغییر نکرد.

## CI دقیق
- Product Build: [run 42503829586](https://github.com/artinrahimii/project/actions/runs/42503829586) — موفق؛ jobهای Android، Windows WPF و HostLifecycle.
- Step Three Quality Gate: [run 42503829570](https://github.com/artinrahimii/project/actions/runs/42503829570) — موفق.
- HostLifecycle: ۶۷ PASS، پوشش خطی ۸۵٫۸۹٪؛ یک آزمون real-model اختیاری SKIP شد.

## دلیل بازبینی مجدد
UPD-S1-012/017 modify transport participation; verify opt-in remains enforced on the final production libp2p listener and recovery path.

## مرز شواهد
این بازبینی، کد و آزمون CI را ثبت می‌کند؛ اثبات اجرای فیزیکی Android-to-Windows، inference مدل واقعی یا clean install/in-place upgrade محسوب نمی‌شود. نشانگر `update_impact.review_status=pending` در گراف اولیه تا بسته‌شدن کل reconciliation حفظ می‌شود؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.
