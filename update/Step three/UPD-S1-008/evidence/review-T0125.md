# بازبینی محتوایی T0125

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-003, UPD-S1-014, UPD-S1-015
- تصمیم reconciliation: re_review_required
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
ADR-0022 approved the first-release product policy: one selected host in each round and at most three sequential attempts over three rounds, initiated only by explicit manual retry. requestId and absolute deadline are preserved; attemptId and roundId are fresh; previous attempts are excluded and no concurrent broadcast occurs. FirstReleaseInferenceLimits and AndroidInferenceRequestRuntime enforce one active selected peer per round. Generic MultiHostCompatibilityLimits and the old three-host selector/dispatcher tests remain available for compatibility/future mode, not as permission for first-release product broadcast. The legacy T0125 requirement to select three hosts therefore needs re-review against the approved single-host product policy; the old baseline status remains unchanged.

## فایل‌های بررسی‌شده
- docs/decisions/ADR-0022-single-host-sequential-retry.md
- src/Client.Android/app/src/main/java/com/whitep/client/requests/DistributedRequest.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/InferenceAttemptDispatcher.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/DistributedRequestTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/InferenceAttemptDispatcherTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt
- tests/test_android_host_candidate_selector_contract.py
- tests/test_android_inference_attempt_dispatcher_contract.py

## وابستگی‌های مستقیم
- T0124 -> T0125: contract_or_interface_dependency / validated
- T0115 -> T0125: sequencing_only / validated
- T0125 -> T0126: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-003/014/015 must revalidate the product selector-to-runtime-to-dispatcher/UI path under the single-host policy. Do not re-enable generic three-host batching in first release without a separate approved decision.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.
