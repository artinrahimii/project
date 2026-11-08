# T0222 — ثابت کن گفت‌وگوها و capsule بعد از راه‌اندازی مجدد اندروید حفظ می‌شوند و capsule قابل‌ویرایش/حذف است.

- Reviewed at exact `main` source snapshot: `a9a12e8326d05e3b4aaeafb33eaeddb68e631c0e` (`V 3.1.108`).
- Baseline status: `not_started`; preserved without change (`status_changed=false`).
- Related update tasks: UPD-S1-009.
- Decision: `re_review_required`.

## Review finding

Room v5 and migration chain, conversation/message persistence, and context-capsule edit/save/delete are implemented; migration tests preserve records from a v1 DB. The current tests do not prove physical reboot, target-device in-place upgrade or Keystore/trust identity preservation.

## Source and evidence

- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationListViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/ui/conversations/ConversationContextCapsuleViewModel.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/RoomConversationContextCapsuleRepository.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseMigrationTest.kt`
- `tests/test_android_database_migration_contract.py`
- Exact-head Quality Gate: [42519571303](https://github.com/artinrahimii/project/actions/runs/42519571303) — passed.
- Exact-head Product Build: [42519571320](https://github.com/artinrahimii/project/actions/runs/42519571320) — passed.

- No source-only or CI-only result is represented as physical product acceptance.
- Review status means the effect was reviewed; it does not mean the legacy task is complete.

## Follow-up

Re-review after the related UPD-S1 implementation and required real-product acceptance. Preserve the baseline status until the legacy task's own acceptance criteria are satisfied.
