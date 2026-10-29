# T0149 evidence ledger

- **Attempt:** `T0149-A001`; result `partial`; no CI success claimed yet.
- **Production persistence contract:** `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt` opens the app-private Room database; `MessageEntity.kt` and `RequestStateEntity.kt` define conversation-scoped rows and ordered flows.
- **Screen consumption:** `ConversationDetailScreen.kt` observes messages and request states using Room-backed flows, while the message ViewModel persists user messages to Room.
- **New disk-backed regression:** `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt` opens a named on-disk database, seeds messages/request-state rows, closes it, reopens the same file, and asserts restored ordering and state metadata.
- **Repository contract check:** `tests/test_android_conversation_detail_contract.py` asserts the reopen test and file-backed database lifecycle exist.
- **Dependency reviews:** T0148 → T0149 and T0138 → T0149 were validated as sequencing-only. Their original `blocks` relations remain.
- **Not yet proven:** Kotlin formatting, compile/APK build, JVM/Robolectric execution, Python suite, graph guard, or exact-head CI for this attempt.
