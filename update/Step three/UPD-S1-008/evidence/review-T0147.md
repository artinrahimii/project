# بازبینی محتوایی T0147

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 01c2374e37b8e8d874ea06d1498991adeeefe15d — V 3.1.87
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-004, UPD-S1-009, UPD-S1-010
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 01c2374e37b8e8d874ea06d1498991adeeefe15d

## یافته‌ها
RequestStateEntity stores typed local lifecycle status and correlation metadata. RequestStatusPresentation maps states such as queued/contacting/waiting/partial/completed/failed/cancelled into user-facing labels and status cards; ConversationDetailScreen observes request states for the selected conversation through Room. Tests exercise the mapping and ViewModel transitions. However the current chat composer remains a preview without an active host transport, so these persisted/status presentation types are not evidence of a completed real request lifecycle; UPD-S1-004/010 must connect real host outcomes.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/data/RequestStateEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestStatusPresentation.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/request/RequestViewModel.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestStatusPresentationTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/request/RequestViewModelTest.kt
- tests/test_android_request_architecture.py

## وابستگی‌های مستقیم
- T0146 -> T0147 sequencing_only/validated
- T0138 -> T0147 sequencing_only/validated
- T0147 -> T0148 contract_or_interface_dependency/validated
- وضعیت پایه و تاریخچه تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164070 — موفق.
- Rendezvous container V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164017 — موفق.
- HostLifecycle V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164037 — موفق؛ Android/Windows jobs هنوز در حال اجرا بودند.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — هر دو پلتفرم موفق؛ 226 pass، 1 skip، 0 failed از 227 آزمون Android.

## اقدام بعدی
After UPD-S1-004/009/010, test every real runtime outcome (processing, complete, failed, timeout, cancelled, retry and busy) from transport through Room and the same conversation UI.

## محدودیت شواهد
هیچ دستگاه Android فیزیکی متصل به Windows نصب‌شده، inference مدل واقعی در این chat، clean install یا in-place upgrade در این بازبینی اجرا نشده است.
