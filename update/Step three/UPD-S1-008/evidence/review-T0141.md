# بازبینی محتوایی T0141

## خط مبنا و تصمیم
- مخزن: artinrahimii/project
- شاخه: main
- commit کد بررسی‌شده: c4b7076392dba6034db3d22200f8d9ab86419814 — V 3.1.86
- وضعیت پایه: done؛ حفظ شد؛ status_changed=false
- وظایف Update مرتبط: UPD-S1-009
- تصمیم: re_review_required
- review_status: reviewed
- review_commit_sha: c4b7076392dba6034db3d22200f8d9ab86419814

## یافته‌ها
ClientDatabase is a real Room database with conversation, message, request-state, inference-state, draft and capsule metadata entities. Conversation/message rows are app-local; request lifecycle rows hold status metadata rather than prompt, capsule or response. EncryptedClientDatabaseFactory opens Room through SQLCipher and wraps a per-database random key with the Android Keystore protector. File-backed tests reopen the database; Android instrumentation source tests fresh encrypted reopen, legacy plaintext export with row preservation, missing-key fail-closed behavior, integrity and sensitive-canary absence. ADR-0020 aligns with local-only persistence.

## فایل‌های بررسی‌شده
- src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/MessageEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/RequestStateEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/InferenceRequestStateEntity.kt
- src/Client.Android/app/src/main/java/com/whitep/client/data/EncryptedClientDatabaseFactory.kt
- docs/decisions/ADR-0020-android-client-local-conversation-storage.md
- src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseMigrationTest.kt
- tests/test_android_local_storage_boundary.py
- src/Client.Android/app/src/androidTest/java/com/whitep/client/data/EncryptedClientDatabaseAcceptanceTest.kt

## وابستگی‌ها
- T0141 -> T0142 contract_or_interface_dependency/validated
- T0138 -> T0141 sequencing_only/validated
- هیچ وضعیت پایه یا تاریخچه‌ای در گراف اولیه تغییر نکرد.

## CI و شواهد
- V3.1.86 Step Three Quality Gate: https://github.com/artinrahimii/project/actions/runs/42509600997 — موفق.
- V3.1.86 Rendezvous container: https://github.com/artinrahimii/project/actions/runs/42509601113 — موفق.
- V3.1.86 HostLifecycle: https://github.com/artinrahimii/project/actions/runs/42509601072 — موفق؛ Android و Windows jobs در زمان snapshot مبنا هنوز در حال اجرا بودند.
- V3.1.82 full Product Build: https://github.com/artinrahimii/project/actions/runs/42503829586 — Android، Windows و HostLifecycle موفق؛ Android گزارش 226 passed، 1 skipped، 0 failed از 227 tests داشت.

## اقدام بعدی
After UPD-S1-009, preserve this Room model and revalidate ordered history/status recovery; clean install and real installed-app upgrade with Keystore/trust state remain separate gates.

## محدودیت شواهد
این بازبینی اثبات نصب فیزیکی Android/Windows، پاسخ inference واقعی در Room، clean install یا in-place upgrade نیست.
