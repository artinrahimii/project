# T0145 evidence ledger

- Starting commit: [`V 3.0.523`](https://github.com/artinrahimii/project/commit/414f827b86d29244a7bc37e8e7268dc62b50e753).
- Source contract inspected: `MessageEntity.kt` exposes `MessageDao.observeForConversation` ordered by `createdAtEpochMillis ASC, messageId ASC`, plus Room upsert.
- Existing detail route inspected: `ClientDestination.kt` and `MainActivity.kt` pass the selected conversation ID into `conversationDetailScreen`.
- Storage/privacy contract inspected: `docs/decisions/ADR-0020-android-client-local-conversation-storage.md` specifies device-local Room message storage; ADR-0019/T0138 disclose the processing-host trust boundary.
- Added implementation: `ConversationMessagesViewModel.kt`, expanded `ConversationDetailScreen.kt`.
- Added tests: `ConversationMessagesViewModelTest.kt` (Robolectric + in-memory Room) and `tests/test_android_conversation_detail_contract.py`.
- Local validation limitation: no local Git clone/Gradle runner is available in this execution context. Exact-head GitHub Actions results will be recorded after completion.
- **Not claimed:** transmission to a processing host, assistant response generation, or independent-network P2P acceptance.
