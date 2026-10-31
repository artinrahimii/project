# T0145 evidence ledger

- Start revision: [`V 3.0.523`](https://github.com/artinrahimii/project/commit/414f827b86d29244a7bc37e8e7268dc62b50e753).
- Implementation revision: [`V 3.0.524`](https://github.com/artinrahimii/project/commit/65560c8fe116ba024c0ece8fad5a2dd214fc78f1).
- Corrected code revision tested: [`V 3.0.525`](https://github.com/artinrahimii/project/commit/06aa9013393ccae0a46fe414a0088e37fe9a7926).
- Source contract: `MessageDao.observeForConversation` orders rows by `createdAtEpochMillis ASC, messageId ASC`; `MessageDao.upsert` persists a message locally.
- Dependency evidence: `ClientDestination.kt`, `MainActivity.kt`, `MessageEntity.kt`, ADR-0020 local-storage decision, and T0138's host-visibility disclosure.
- [V 3.0.525 product workflow passed](https://github.com/artinrahimii/project/actions/runs/40859606785): Android lint/build/unit tests, generated-source checks and APK upload; HostLifecycle; Windows WPF build/startup/security.
- [V 3.0.525 Step Three quality run](https://github.com/artinrahimii/project/actions/runs/40859606728): 221 Python tests passed, but graph guard found one metadata error (`T0145` summary attempt_count 2 vs 3 append-only history entries). This is corrected in V 3.0.526; quality rerun pending.
- T0145 incoming edges were evidence-reviewed and validated; outgoing T0145 → T0146 remains `unreviewed`.
- **Not claimed:** network delivery to a processing host, assistant response generation, or independent-network P2P acceptance.
