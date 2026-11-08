# بازبینی محتوایی T0148

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 01c2374e37b8e8d874ea06d1498991adeeefe15d — V 3.1.87
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-009
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 01c2374e37b8e8d874ea06d1498991adeeefe15d

## یافته‌ها
ConversationMessagesViewModel receives SavedStateHandle and stores the unsent composer draft under a stable saved-state key; a new ViewModel created with the restored handle recovers the draft. The acceptance test validates draft restoration and message ordering. The test uses a restored SavedStateHandle rather than driving an Android process kill/relaunch on a physical device, so the platform process-death acceptance is not claimed.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationMessagesViewModelTest.kt
- tests/test_android_conversation_detail_contract.py

## وابستگی‌های مستقیم
- T0147 -> T0148 contract_or_interface_dependency/validated
- T0138 -> T0148 sequencing_only/validated
- T0148 -> T0149 sequencing_only/validated
- وضعیت پایه و تاریخچه تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164070 — موفق.
- Rendezvous container V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164017 — موفق.
- HostLifecycle V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164037 — موفق؛ Android/Windows jobs هنوز در حال اجرا بودند.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — هر دو پلتفرم موفق؛ 226 pass، 1 skip، 0 failed از 227 آزمون Android.

## اقدام بعدی
After UPD-S1-009, retain SavedStateHandle draft recovery and verify on Android instrumentation that drafts survive Activity recreation and OS process recreation without duplicating sent messages.

## محدودیت شواهد
هیچ دستگاه Android فیزیکی متصل به Windows نصب‌شده، inference مدل واقعی در این chat، clean install یا in-place upgrade در این بازبینی اجرا نشده است.
