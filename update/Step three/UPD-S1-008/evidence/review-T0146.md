# بازبینی محتوایی T0146

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: 01c2374e37b8e8d874ea06d1498991adeeefe15d — V 3.1.87
- وضعیت پایهٔ legacy: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-009, UPD-S1-010
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: 01c2374e37b8e8d874ea06d1498991adeeefe15d

## یافته‌ها
ConversationMessageValidator rejects blank/whitespace and over-limit message content before the Room write path; ConversationMessagesViewModel validates user input before creating a user-role MessageEntity in the selected conversation. Unit tests cover blank, over-limit, valid content and persistence behavior. The UI also checks the message length before enabling send. This is local-message validation, not protocol prompt validation or host transport validation; UPD-S1-009/010 may change the final send path.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/domain/ConversationMessageValidator.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationMessagesViewModel.kt
- src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationDetailScreen.kt
- src/Client.Android/app/src/test/java/com/whitep/client/domain/ConversationMessageValidatorTest.kt
- src/Client.Android/app/src/test/java/com/whitep/client/ui/conversations/ConversationMessagesViewModelTest.kt
- tests/test_android_conversation_detail_contract.py

## وابستگی‌های مستقیم
- T0145 -> T0146 contract_or_interface_dependency/validated
- T0138 -> T0146 sequencing_only/validated
- T0146 -> T0147 sequencing_only/validated
- وضعیت پایه و تاریخچه تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164070 — موفق.
- Rendezvous container V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164017 — موفق.
- HostLifecycle V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164037 — موفق؛ Android/Windows jobs هنوز در حال اجرا بودند.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — هر دو پلتفرم موفق؛ 226 pass، 1 skip، 0 failed از 227 آزمون Android.

## اقدام بعدی
After UPD-S1-009/010, rerun blank, boundary, oversized UTF-8 and valid-message cases through the actual conversation composer and authenticated request runtime.

## محدودیت شواهد
هیچ دستگاه Android فیزیکی متصل به Windows نصب‌شده، inference مدل واقعی در این chat، clean install یا in-place upgrade در این بازبینی اجرا نشده است.
