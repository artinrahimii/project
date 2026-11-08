# بازبینی محتوایی T0124

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit دقیق کد بررسی‌شده: 07b21e2762ea9c03157de9f6c6a379eea07919de — V 3.1.84
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-014, UPD-S1-023
- تصمیم reconciliation: re_review_required
- review_status: reviewed
- review_commit_sha: 07b21e2762ea9c03157de9f6c6a379eea07919de

## یافته‌های کد و آزمون
HostCandidateSelector.selectReachable probes metadata candidates in stable PeerId order using authenticated live HostStatus, rejects stale, untrusted, unreachable, non-Ready, capacity-exhausted or model-incompatible peers and enforces deterministic compatibility checks. JVM tests cover stable order, identity mismatch, not-ready/full hosts and incompatible models. Confirmed gap: the selector receives an already-built InferenceRequest and validates its modelId and maxOutputTokens; it does not select a model from the catalog or compute the output-token limit. UPD-S1-023 requires that resolver before host selection.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/requests/HostCandidateSelector.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AuthenticatedHostDiscovery.kt
- src/Client.Android/app/src/main/java/com/whitep/client/requests/AndroidInferenceRequestRuntime.kt
- src/Client.Android/app/src/main/java/com/whitep/client/network/Libp2pInferenceRequestTransport.kt
- src/Client.Android/app/src/test/java/com/whitep/client/requests/HostCandidateSelectorTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/network/Libp2pInferenceRequestTransportTest.kt
- tests/test_android_host_candidate_selector_contract.py

## وابستگی‌های مستقیم
- T0123 -> T0124: contract_or_interface_dependency / validated
- T0115 -> T0124: sequencing_only / validated
- T0124 -> T0125: contract_or_interface_dependency / validated
- T0124 -> T0174: contract_or_interface_dependency / validated
- هیچ وضعیت پایه یا تاریخچهٔ وظیفهٔ قدیمی تغییر نکرد؛ یال‌های بررسی‌شده بازنویسی نشدند.

## CI دقیق
- Step Three Quality Gate V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641717 — موفق.
- Rendezvous container V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641758 — موفق.
- Product Build V3.1.84: https://github.com/artinrahimii/project/actions/runs/42508641574 — HostLifecycle و Windows WPF موفق؛ Android در زمان snapshot در حال اجرا بود.
- HostLifecycle transport/metadata acceptance V3.1.83: https://github.com/artinrahimii/project/actions/runs/42508306341 — موفق.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — موفق؛ کدهای محصول بررسی‌شده پس از آن تغییر نکرده‌اند.

## اقدام بعدی
UPD-S1-023 must provide deterministic model selection and output-token clamping before candidate selection. Revalidate catalog, installed model, backend readiness and capabilities together.

## مرز شواهد
این بازبینی منبع کد و آزمون CI میزبانی‌شده را ثبت می‌کند؛ اثبات شبکهٔ عمومی مستقل، NAT traversal، relay runtime، دستگاه Android فیزیکی متصل به میزبان Windows نصب‌شده، clean install یا in-place upgrade نیست. نشانگر update_impact.review_status در گراف اولیه تا تکمیل کل reconciliation بدون تغییر باقی می‌ماند؛ وضعیت واقعی این بازبینی فقط در JSON reconciliation ثبت شده است.
