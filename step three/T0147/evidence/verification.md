# T0147 evidence ledger

- Start revision: [`V 3.0.531`](https://github.com/artinrahimii/project/commit/4f087b1edb10fc4624d02bf180e53f234acf96f9).
- Source model: `src/Client.Android/app/src/main/java/com/whitep/client/data/RequestStateEntity.kt` already defines all seven enum values and a Room Flow ordered by creation time.
- Actual database contract: `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt` exposes `requestStateDao()`; `ClientDatabaseTest.kt` already verifies persistence/cascade behavior.
- New implementation: `RequestStatusPresentation.kt` maps each persisted state to readable label/detail; `ConversationDetailScreen.kt` observes only real Room rows and shows an explicit empty state when no request records exist.
- Added tests: `RequestStatusPresentationTest.kt`, `ClientDatabaseTest.kt` coverage for all seven states, and `tests/test_android_request_status_contract.py`.
- Edge review: T0146 → T0147 and T0138 → T0147 are both validated as `sequencing_only` based on the actual contracts; their original `type=blocks` edges remain present.
- Exact-head product and quality CI are pending at the first implementation revision.
- **Not claimed:** live transport, actual state transitions, host delivery, response generation or independent-network P2P reachability.
