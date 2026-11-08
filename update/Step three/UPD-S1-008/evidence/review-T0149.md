# بازبینی محتوایی T0149

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
ClientDatabaseTest creates a file-backed Room database, writes conversation/message/request-state data, closes the database instance, reopens the same file and verifies message ordering and persisted status recovery. SQLCipher database instrumentation separately checks encrypted-file open/reopen, legacy plaintext conversion, integrity and fail-closed missing-key behavior. Closing and reopening a DB instance is valid storage evidence, but does not prove a clean install or a full installed-app upgrade while preserving Android Keystore and peer trust identity.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/MessageEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/RequestStateEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt
- src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt
- tests/test_android_local_storage_boundary.py
- src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt

## وابستگی‌های مستقیم
- T0148 -> T0149 sequencing_only/validated
- T0138 -> T0149 sequencing_only/validated
- T0149 -> T0150 contract_or_interface_dependency/validated
- وضعیت پایه و تاریخچه تغییر نکرد.

## CI دقیق
- Quality Gate V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164070 — موفق.
- Rendezvous container V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164017 — موفق.
- HostLifecycle V3.1.87: https://github.com/artinrahimii/project/actions/runs/42510164037 — موفق؛ Android/Windows jobs هنوز در حال اجرا بودند.
- Full Android/Windows Product Build V3.1.82: https://github.com/artinrahimii/project/actions/runs/42503829586 — هر دو پلتفرم موفق؛ 226 pass، 1 skip، 0 failed از 227 آزمون Android.

## اقدام بعدی
After UPD-S1-009 and UPD-S1-022, rerun file-backed and encrypted reopen tests; separately complete the required installed-app upgrade snapshot with conversation IDs, message order, schema, Keystore identity and peer trust preserved.

## محدودیت شواهد
هیچ دستگاه Android فیزیکی متصل به Windows نصب‌شده، inference مدل واقعی در این chat، clean install یا in-place upgrade در این بازبینی اجرا نشده است.
