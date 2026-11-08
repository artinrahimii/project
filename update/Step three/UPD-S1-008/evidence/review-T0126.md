# بازبینی محتوایی T0126

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 7082162e902e1c7e32718e0af687f8f62b7d92dc — V 3.1.85
- وضعیت پایه: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-003, UPD-S1-014, UPD-S1-015
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 7082162e902e1c7e32718e0af687f8f62b7d92dc

## یافته‌ها
HostSelectionFeedback couples the exact selected candidate list to UI feedback. Zero compatible hosts blocks dispatch and says the request was not sent; a positive count allows dispatch and reports any shortfall. The legacy compatibility form defaults to three hosts, and its tests cover zero through three. The first-release product path instead enforces one active host per round through FirstReleaseInferenceLimits and AndroidInferenceRequestRuntime; RequestViewModel dispatches with maxHosts=1. One reachable host is therefore sufficient in first release, while zero blocks the request. The old three-host shortage rule must be reconciled with the approved one-host decision rather than changing the legacy status silently.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/requests/HostSelectionFeedback.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/HostSelectionFeedbackTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/AuthenticatedHostDiscoveryTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt

## وابستگی‌ها
- T0125 -> T0126: contract_or_interface_dependency / validated
- T0115 -> T0126: sequencing_only / validated
- T0126 -> T0127: sequencing_only / validated
- T0126 -> T0174: contract_or_interface_dependency / validated
- هیچ وضعیت پایه، تاریخچه یا یال قدیمی تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179813 — موفق.
- Rendezvous V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179886 — موفق.
- HostLifecycle V3.1.85: https://github.com/artinrahimii/project/actions/runs/42509179878 — موفق؛ یک آزمون real-model با وضعیت SKIP دارد.
- Windows و Android product jobs در زمان این snapshot هنوز در حال اجرا بودند.
- Full Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ کدهای محصول بررسی‌شده بعد از آن تغییر نکرده‌اند.

## اقدام بعدی
Reconcile UI feedback and legacy three-host compatibility with the approved one-host first-release policy after UPD-S1-003/014/015.

## مرز شواهد
این بازبینی اثبات آزمون public independent network، inference مدل واقعی یا in-place upgrade نیست. T0128 با وضعیت deferred_final باقی می‌ماند.
