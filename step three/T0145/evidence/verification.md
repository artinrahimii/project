# T0145 evidence ledger

- Start revision: [`V 3.0.523`](https://github.com/artinrahimii/project/commit/414f827b86d29244a7bc37e8e7268dc62b50e753).
- Initial implementation revision: [`V 3.0.524`](https://github.com/artinrahimii/project/commit/65560c8fe116ba024c0ece8fad5a2dd214fc78f1).
- Source contract: `MessageDao.observeForConversation` orders rows by `createdAtEpochMillis ASC, messageId ASC`; `MessageDao.upsert` persists a message locally.
- Dependency evidence inspected: `ClientDestination.kt`, `MainActivity.kt`, `MessageEntity.kt`, ADR-0020 local storage decision, and the T0138 host-visibility disclosure.
- [Exact V 3.0.524 Step Three quality gate passed](https://github.com/artinrahimii/project/actions/runs/40859323839).
- [Exact V 3.0.524 Android product job failed at ktlint](https://github.com/artinrahimii/project/actions/runs/40859323728): `ConversationDetailScreen.kt:166:13` function name `MessageCard` must start with lowercase. APK compilation and JVM tests were skipped; HostLifecycle tests passed.
- A002 changes the helper function to `messageCard`; exact-head CI results are pending.
- **Not claimed:** network delivery to a processing host, assistant response generation, or independent-network P2P acceptance.
