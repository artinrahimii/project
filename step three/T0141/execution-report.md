# T0141 execution report — accepted by exact-head CI

- **Task:** Create the Android-local Room database for conversations, messages and request status.
- **Final status:** `done` after exact commit `V 3.0.505` (`882a73162acd26c0410b3863f65d39b211262a28`) passed all required quality/product CI jobs. The code commit is [visible on GitHub](https://github.com/artinrahimii/project/commit/882a73162acd26c0410b3863f65d39b211262a28).
- **Step Three quality gate:** PASS — [run 40840501577](https://github.com/artinrahimii/project/actions/runs/40840501577); 208 Python tests passed, Python syntax passed, graph guard validated 229 tasks / 417 edges with zero errors and one preserved warning for T0001's free-text prerequisite.
- **Android product workflow:** PASS — [run 40840501606](https://github.com/artinrahimii/project/actions/runs/40840501606). Android `ktlintCheck`, `assembleDebug`, `testDebugUnitTest`, generated Protobuf Java/Kotlin source checks and APK upload all passed. The three Room acceptance cases cover ordered conversation/message/request-state persistence, cascade deletion, and draft restoration while keeping non-empty request preparation explicitly `TRANSPORT_UNAVAILABLE`.
- **Windows product job:** PASS — Release formatting/build, WPF startup smoke, generated Protobuf C# and required agent/identity/trust gates passed in the same product workflow.
- **Host lifecycle job:** PASS — unit and integration tests passed in the same product workflow.
- **Attempts:** A001–A004 remain recorded as failed formatting attempts with their exact CI diagnostics; A005 is recorded as succeeded. Summary counts: 5 attempts, 4 failed, last attempt succeeded.
- **Dependency audit:** `T0140 → T0141` remains a validated contract/interface dependency. `T0138 → T0141` is validated as sequencing-only; the original `type=blocks` and conservative edge order were kept, and the evidence/rationale remain in `edge_change_log`.
- **Privacy boundary:** Room stores conversation/messages and the active draft on-device in app-private storage. Request-state rows omit prompt/capsule/response bodies; deleting a conversation cascades to message/request-state rows. This database is **not encrypted at rest** by this task. T0142 owns schema migration tests and T0184 owns encrypted-storage work. ADR-0019's processing-host visibility disclosure and the unverified Android-to-Windows P2P runtime gates remain unchanged.
- **Local environment limitation:** local `git ls-remote` failed due hostname resolution, so no local Gradle command was claimed. Acceptance is based on the exact-head GitHub Actions runs cited above.

## Actual deliverables

- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientDatabase.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ConversationEntity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/MessageEntity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/RequestStateEntity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/ClientRequestDraftEntity.kt`
- `src/Client.Android/app/src/main/java/com/whitep/client/data/RoomClientRequestRepository.kt`
- `src/Client.Android/app/src/test/java/com/whitep/client/data/ClientDatabaseTest.kt`
- `tests/test_android_local_storage_boundary.py`
- `docs/decisions/ADR-0020-android-client-local-conversation-storage.md`
- `docs/decisions/README.md`
